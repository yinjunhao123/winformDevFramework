using HslCommunication;
using Microsoft.Extensions.Logging;
using PLCBasic;
using PLCBasic.IRepository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.Common;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC通信服务
    /// 管理多个PLC客户端的连接池，提供统一的PLC读写接口
    /// 支持异常重试、连接健康检查和自动重连
    /// </summary>
    public class PlcCommunicationService : IPlcCommunicationService
    {
        /// <summary>
        /// PLC客户端连接池（线程安全）
        /// Key: PlcCode, Value: PLC客户端实例
        /// </summary>
        private readonly ConcurrentDictionary<string, HslPlcClient> _plcClients = new ConcurrentDictionary<string, HslPlcClient>();

        /// <summary>
        /// PLC配置数据仓储
        /// </summary>
        private readonly IPLC_ConfigRepository _plcConfigRepository;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<PlcCommunicationService> _logger;

        /// <summary>
        /// 初始化锁对象
        /// </summary>
        private readonly object _initLock = new object();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// 健康检查定时器
        /// </summary>
        private Timer _healthCheckTimer;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        private const int MaxRetryCount = 3;

        /// <summary>
        /// 重试间隔（毫秒）
        /// </summary>
        private const int RetryDelayMs = 100;

        /// <summary>
        /// 健康检查间隔（毫秒）
        /// </summary>
        private const int HealthCheckIntervalMs = 30000; // 30秒

        /// <summary>
        /// 断路器管理器
        /// </summary>
        private readonly PlcCircuitBreakerManager _circuitBreakerManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="plcConfigRepository">PLC配置数据仓储</param>
        /// <param name="logger">日志记录器</param>
        public PlcCommunicationService(IPLC_ConfigRepository plcConfigRepository, ILogger<PlcCommunicationService> logger)
        {
            _plcConfigRepository = plcConfigRepository;
            _logger = logger;
            _circuitBreakerManager = new PlcCircuitBreakerManager(new CircuitBreakerOptions
            {
                FailureThreshold = 5,
                OpenDurationMs = 30000,
                HalfOpenSampleCount = 3,
                HalfOpenSuccessThreshold = 2
            }, logger);
        }

        /// <summary>
        /// 初始化所有PLC连接
        /// 从数据库加载PLC配置并创建客户端实例
        /// </summary>
        public void InitializeConnections()
        {
            lock (_initLock)
            {
                if (_initialized) return;

                try
                {
                    var configs = _plcConfigRepository.Query().ToList();
                    foreach (var config in configs)
                    {
                        var client = new HslPlcClient(config);
                        _plcClients.TryAdd(config.PlcCode, client);
                    }
                    _initialized = true;

                    // 启动健康检查定时器
                    StartHealthCheckTimer();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "初始化PLC连接失败");
                }
            }
        }

        /// <summary>
        /// 启动健康检查定时器
        /// </summary>
        private void StartHealthCheckTimer()
        {
            if (_healthCheckTimer == null)
            {
                _healthCheckTimer = new Timer(
                    async (state) => await PerformHealthCheckAsync(),
                    null,
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(HealthCheckIntervalMs));
            }
        }

        /// <summary>
        /// 执行健康检查
        /// 检查所有PLC连接状态，断开的连接自动重连
        /// </summary>
        private async Task PerformHealthCheckAsync()
        {
            foreach (var kvp in _plcClients)
            {
                string plcCode = kvp.Key;
                var client = kvp.Value;

                try
                {
                    if (!client.IsConnected)
                    {
                        _logger?.LogWarning($"PLC连接断开，正在尝试重连: {plcCode}");
                        await TryReconnectAsync(plcCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"PLC健康检查失败: {plcCode}");
                }
            }
        }

        /// <summary>
        /// 尝试重新连接PLC
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否重连成功</returns>
        private async Task<bool> TryReconnectAsync(string plcCode)
        {
            for (int retry = 0; retry < MaxRetryCount; retry++)
            {
                try
                {
                    if (_plcClients.TryGetValue(plcCode, out var client))
                    {
                        // 先断开旧连接
                        try { client.Disconnect(); } catch { }

                        // 创建新客户端并重连
                        var config = _plcConfigRepository.QueryListByClause(c => c.PlcCode == plcCode).FirstOrDefault();
                        if (config != null)
                        {
                            var newClient = new HslPlcClient(config);
                            var result = newClient.Connect();
                            if (result.IsSuccess)
                            {
                                _plcClients.TryUpdate(plcCode, newClient, client);
                                _logger?.LogInformation($"PLC重连成功: {plcCode}");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"PLC重连失败(重试 {retry + 1}/{MaxRetryCount}): {plcCode}");
                }

                // 指数退避等待
                await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
            }

            _logger?.LogError($"PLC重连失败，已达到最大重试次数: {plcCode}");
            return false;
        }

        /// <summary>
        /// 带重试的读取布尔值
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果</returns>
        private OperateResult<bool> ReadBoolWithRetry(string plcCode, string address)
        {
            return ExecuteWithRetry(() =>
            {
                var client = GetClient(plcCode);
                return client?.ReadBool(address) ?? new OperateResult<bool>("PLC客户端不存在");
            }, plcCode, $"读取布尔值: {address}");
        }

        /// <summary>
        /// 带重试的执行操作
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="operation">操作委托</param>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="operationName">操作名称</param>
        /// <returns>操作结果</returns>
        private OperateResult<T> ExecuteWithRetry<T>(Func<OperateResult<T>> operation, string plcCode, string operationName)
        {
            // 检查断路器状态，如果熔断中则直接返回失败
            if (!_circuitBreakerManager.IsRequestAllowed(plcCode))
            {
                var stateDesc = _circuitBreakerManager.GetStateDescription(plcCode);
                _logger?.LogWarning($"{operationName}被断路器拦截，PLC[{plcCode}]状态: {stateDesc}");
                return new OperateResult<T>($"断路器已熔断，PLC[{plcCode}]暂时不可用");
            }

            for (int retry = 0; retry < MaxRetryCount; retry++)
            {
                try
                {
                    var result = operation();
                    if (result.IsSuccess)
                    {
                        // 操作成功，记录成功状态
                        _circuitBreakerManager.RecordSuccess(plcCode);
                        return result;
                    }

                    // 如果连接断开，尝试重连
                    if (!IsConnected(plcCode))
                    {
                        _logger?.LogWarning($"PLC连接断开，尝试重连后重试: {plcCode}");
                        TryReconnectAsync(plcCode).Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"{operationName}失败(重试 {retry + 1}/{MaxRetryCount}): {plcCode}");
                }

                if (retry < MaxRetryCount - 1)
                {
                    Thread.Sleep(RetryDelayMs * (int)Math.Pow(2, retry));
                }
            }

            // 所有重试都失败，记录失败状态，可能触发熔断
            _circuitBreakerManager.RecordFailure(plcCode);
            _logger?.LogError($"{operationName}失败，已达到最大重试次数: {plcCode}");
            return new OperateResult<T>("操作失败，已达到最大重试次数");
        }

        /// <summary>
        /// 获取PLC客户端实例
        /// 如果客户端不存在，则从数据库加载配置并创建新实例
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>PLC客户端实例，未找到返回null</returns>
        private HslPlcClient GetClient(string plcCode)
        {
            if (!_initialized)
            {
                InitializeConnections();
            }

            if (_plcClients.TryGetValue(plcCode, out var client))
            {
                return client;
            }

            // 如果缓存中不存在，从数据库查询配置
            var config = _plcConfigRepository.QueryListByClause(c => c.PlcCode == plcCode).FirstOrDefault();
            if (config != null)
            {
                client = new HslPlcClient(config);
                _plcClients.TryAdd(plcCode, client);
                return client;
            }

            return null;
        }

        /// <summary>
        /// 建立PLC连接
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否连接成功</returns>
        public bool Connect(string plcCode)
        {
            var client = GetClient(plcCode);
            if (client == null) return false;

            try
            {
                var result = client.Connect();
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否断开成功</returns>
        public bool Disconnect(string plcCode)
        {
            if (_plcClients.TryGetValue(plcCode, out var client))
            {
                try
                {
                    client.Disconnect();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断PLC是否已连接
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否已连接</returns>
        public bool IsConnected(string plcCode)
        {
            var client = GetClient(plcCode);
            return client?.IsConnected ?? false;
        }

        /// <summary>
        /// 读取单个布尔值
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<bool> ReadBool(string plcCode, string address)
        {
            var client = GetClient(plcCode);
            return client?.ReadBool(address) ?? new OperateResult<bool>("PLC客户端不存在");
        }

        /// <summary>
        /// 读取布尔数组
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">起始PLC地址</param>
        /// <param name="length">读取长度</param>
        /// <returns>读取结果</returns>
        public OperateResult<bool[]> ReadBool(string plcCode, string address, ushort length)
        {
            var client = GetClient(plcCode);
            return client?.ReadBool(address, length) ?? new OperateResult<bool[]>("PLC客户端不存在");
        }

        /// <summary>
        /// 读取32位整数
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<int> ReadInt32(string plcCode, string address)
        {
            var client = GetClient(plcCode);
            return client?.ReadInt32(address) ?? new OperateResult<int>("PLC客户端不存在");
        }

        /// <summary>
        /// 读取单精度浮点数
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<float> ReadFloat(string plcCode, string address)
        {
            var client = GetClient(plcCode);
            return client?.ReadFloat(address) ?? new OperateResult<float>("PLC客户端不存在");
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="length">字符串长度</param>
        /// <returns>读取结果</returns>
        public OperateResult<string> ReadString(string plcCode, string address, ushort length)
        {
            var client = GetClient(plcCode);
            return client?.ReadString(address, length) ?? new OperateResult<string>("PLC客户端不存在");
        }

        /// <summary>
        /// 地址范围信息
        /// </summary>
        private class AddressRange
        {
            public string StartAddress { get; set; }
            public ushort Length { get; set; }
            public List<int> OriginalIndices { get; set; } // 原始索引列表
        }

        /// <summary>
        /// 获取地址类型（如 M、X、Y、I、Q等）
        /// </summary>
        private string GetAddressType(string address)
        {
            if (string.IsNullOrEmpty(address)) return string.Empty;
            // 提取地址类型前缀（如 M100 -> M, X0.0 -> X）
            string type = new string(address.TakeWhile(c => !char.IsDigit(c)).ToArray()).ToUpper();
            return type;
        }

        /// <summary>
        /// 获取地址中的数字部分
        /// </summary>
        private int GetAddressNumber(string address)
        {
            if (string.IsNullOrEmpty(address)) return 0;
            string numberPart = new string(address.SkipWhile(c => !char.IsDigit(c)).TakeWhile(c => char.IsDigit(c)).ToArray());
            return int.TryParse(numberPart, out int num) ? num : 0;
        }

        /// <summary>
        /// 批量读取多个布尔地址（按地址类型分组批量读取，索引安全）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="addresses">地址列表</param>
        /// <returns>布尔值数组，顺序与地址列表对应</returns>
        public async Task<bool[]> ReadBatchAsync(string plcCode, List<string> addresses)
        {
            var result = new bool[addresses.Count];

            // 检查断路器状态，如果熔断中则直接返回失败
            if (!_circuitBreakerManager.IsRequestAllowed(plcCode))
            {
                var stateDesc = _circuitBreakerManager.GetStateDescription(plcCode);
                _logger?.LogWarning($"批量读取被断路器拦截，PLC[{plcCode}]状态: {stateDesc}");
                return result;
            }

            // 执行带重试的批量读取
            for (int retry = 0; retry < MaxRetryCount; retry++)
            {
                try
                {
                    var client = GetClient(plcCode);

                    if (client == null)
                    {
                        _logger?.LogError($"PLC客户端不存在: {plcCode}");
                        return result;
                    }

                    // 检查连接状态，必要时重连
                    if (!client.IsConnected)
                    {
                        _logger?.LogWarning($"PLC连接断开，尝试重连: {plcCode}");
                        if (!await TryReconnectAsync(plcCode))
                        {
                            if (retry < MaxRetryCount - 1)
                            {
                                await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
                                continue;
                            }
                            return result;
                        }
                    }

                    // 重新获取客户端（可能已更新）
                    client = GetClient(plcCode);
                    if (client == null) return result;

                    // 按传入顺序直接读取，保持索引一一对应
                    // 调用方应该已经按地址类型分组好，连续地址应该已经合并
                    var tasks = addresses.Select((address, index) =>
                        Task.Run(() =>
                        {
                            try
                            {
                                var readResult = client.ReadBool(address);
                                return (Index: index, Value: readResult.IsSuccess ? readResult.Content : false);
                            }
                            catch
                            {
                                return (Index: index, Value: false);
                            }
                        })
                    );

                    var results = await Task.WhenAll(tasks);

                    // 按原始索引写入结果
                    foreach (var item in results)
                    {
                        result[item.Index] = item.Value;
                    }

                    // 操作成功，记录成功状态
                    _circuitBreakerManager.RecordSuccess(plcCode);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"批量读取失败(重试 {retry + 1}/{MaxRetryCount}): {plcCode}");

                    if (retry < MaxRetryCount - 1)
                    {
                        await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
                    }
                }
            }

            // 所有重试都失败，记录失败状态，可能触发熔断
            _circuitBreakerManager.RecordFailure(plcCode);
            _logger?.LogError($"批量读取失败，已达到最大重试次数: {plcCode}");
            return result;
        }

        /// <summary>
        /// 查找连续地址范围（优化版）
        /// </summary>
        /// <param name="sortedItems">已按地址编号排序的地址项列表</param>
        /// <returns>连续地址范围列表</returns>
        private List<AddressRangeOptimized> FindContinuousRangesOptimized<T>(List<T> sortedItems) where T : class
        {
            var ranges = new List<AddressRangeOptimized>();
            
            if (sortedItems == null || sortedItems.Count == 0)
                return ranges;

            AddressRangeOptimized currentRange = new AddressRangeOptimized
            {
                StartIndex = 0,
                Length = 1
            };

            for (int i = 1; i < sortedItems.Count; i++)
            {
                string prevAddress = GetAddressFromItem(sortedItems[i - 1]);
                string currAddress = GetAddressFromItem(sortedItems[i]);
                
                int prevNum = GetAddressNumber(prevAddress);
                int currNum = GetAddressNumber(currAddress);

                if (currNum == prevNum + 1)
                {
                    // 连续地址，扩展范围
                    currentRange.Length++;
                }
                else
                {
                    // 不连续，保存当前范围并开始新范围
                    ranges.Add(currentRange);
                    currentRange = new AddressRangeOptimized
                    {
                        StartIndex = i,
                        Length = 1
                    };
                }
            }

            ranges.Add(currentRange);
            return ranges;
        }

        /// <summary>
        /// 从匿名类型中获取地址
        /// </summary>
        private string GetAddressFromItem(object item)
        {
            var prop = item.GetType().GetProperty("Address");
            return prop?.GetValue(item)?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 地址范围信息（优化版）
        /// </summary>
        private class AddressRangeOptimized
        {
            public int StartIndex { get; set; }
            public int Length { get; set; }
        }

        /// <summary>
        /// 写入布尔值
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的布尔值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string plcCode, string address, bool value)
        {
            var client = GetClient(plcCode);
            return client?.Write(address, value) ?? new OperateResult("PLC客户端不存在");
        }

        /// <summary>
        /// 写入32位整数
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的整数值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string plcCode, string address, int value)
        {
            var client = GetClient(plcCode);
            return client?.Write(address, value) ?? new OperateResult("PLC客户端不存在");
        }

        /// <summary>
        /// 写入单精度浮点数
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的浮点数值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string plcCode, string address, float value)
        {
            var client = GetClient(plcCode);
            return client?.Write(address, value) ?? new OperateResult("PLC客户端不存在");
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的字符串</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string plcCode, string address, string value)
        {
            var client = GetClient(plcCode);
            return client?.Write(address, value) ?? new OperateResult("PLC客户端不存在");
        }

        /// <summary>
        /// 写入双精度浮点数
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的双精度浮点数值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string plcCode, string address, double value)
        {
            var client = GetClient(plcCode);
            return client?.Write(address, value) ?? new OperateResult("PLC客户端不存在");
        }

        /// <summary>
        /// 批量写入布尔数组（连续地址）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="values">要写入的布尔数组</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string plcCode, string startAddress, bool[] values)
        {
            try
            {
                // 参数验证
                if (string.IsNullOrWhiteSpace(plcCode))
                {
                    return new OperateResult("PLC编码不能为空");
                }

                if (string.IsNullOrWhiteSpace(startAddress))
                {
                    return new OperateResult("起始地址不能为空");
                }

                if (values == null || values.Length == 0)
                {
                    return new OperateResult("写入值数组不能为空");
                }

                // 获取PLC客户端
                var client = GetClient(plcCode);
                if (client == null)
                {
                    _logger?.LogError($"批量写入布尔数组失败：PLC客户端不存在，PLC编码={plcCode}");
                    return new OperateResult("PLC客户端不存在");
                }

                // 检查连接状态
                if (!client.IsConnected)
                {
                    _logger?.LogWarning($"批量写入布尔数组失败：PLC未连接，PLC编码={plcCode}");
                    return new OperateResult("PLC未连接");
                }

                // 执行写入操作
                var result = client.Write(startAddress, values);
                
                if (!result.IsSuccess)
                {
                    _logger?.LogError($"批量写入布尔数组失败：{result.Message}，PLC编码={plcCode}，起始地址={startAddress}，数量={values.Length}");
                }

                return result;
            }
            catch (ArgumentNullException ex)
            {
                _logger?.LogError(ex, $"批量写入布尔数组失败：参数为空，PLC编码={plcCode}");
                return new OperateResult($"参数错误：{ex.Message}");
            }
            catch (FormatException ex)
            {
                _logger?.LogError(ex, $"批量写入布尔数组失败：地址格式错误，PLC编码={plcCode}，地址={startAddress}");
                return new OperateResult($"地址格式错误：{ex.Message}");
            }
            catch (TimeoutException ex)
            {
                _logger?.LogError(ex, $"批量写入布尔数组失败：操作超时，PLC编码={plcCode}");
                return new OperateResult("操作超时");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"批量写入布尔数组发生未知错误，PLC编码={plcCode}，起始地址={startAddress}");
                return new OperateResult($"写入失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量写入多个参数（按地址类型分组批量写入）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="writeRequests">写入请求列表</param>
        /// <returns>写入结果数组，顺序与请求顺序对应</returns>
        public async Task<bool[]> WriteBatchAsync(string plcCode, List<BatchWriteRequest> writeRequests)
        {
            var result = new bool[writeRequests.Count];

            // 检查断路器状态，如果熔断中则直接返回失败
            if (!_circuitBreakerManager.IsRequestAllowed(plcCode))
            {
                var stateDesc = _circuitBreakerManager.GetStateDescription(plcCode);
                _logger?.LogWarning($"批量写入被断路器拦截，PLC[{plcCode}]状态: {stateDesc}");
                return result;
            }

            for (int retry = 0; retry < MaxRetryCount; retry++)
            {
                try
                {
                    var client = GetClient(plcCode);
                    if (client == null)
                    {
                        _logger?.LogError($"PLC客户端不存在: {plcCode}");
                        return result;
                    }

                    if (!client.IsConnected)
                    {
                        _logger?.LogWarning($"PLC连接断开，尝试重连: {plcCode}");
                        if (!await TryReconnectAsync(plcCode))
                        {
                            if (retry < MaxRetryCount - 1)
                            {
                                await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
                                continue;
                            }
                            return result;
                        }
                    }

                    // 按地址类型分组处理
                    var groupedByType = writeRequests.GroupBy(r => r.DataType?.ToLower() ?? "int").ToList();

                    foreach (var typeGroup in groupedByType)
                    {
                        var dataType = typeGroup.Key;
                        var requests = typeGroup.ToList();

                        if (dataType == "bool")
                        {
                            // 布尔值按连续地址分组批量写入
                            var boolRequests = requests.Select((r, i) => new { Request = r, OriginalIndex = i })
                                                      .OrderBy(x => GetAddressBitOffset(x.Request.Address))
                                                      .ToList();

                            // 找出连续地址段
                            var continuousGroups = new List<List<(int OriginalIndex, string Address, bool Value)>>();
                            List<(int, string, bool)> currentGroup = null;

                            foreach (var item in boolRequests)
                            {
                                var address = item.Request.Address;
                                var bitOffset = GetAddressBitOffset(address);

                                if (currentGroup == null)
                                {
                                    currentGroup = new List<(int, string, bool)>();
                                    currentGroup.Add((item.OriginalIndex, address, bool.Parse(item.Request.Value)));
                                }
                                else
                                {
                                    // 检查是否连续
                                    var lastOffset = GetAddressBitOffset(currentGroup.Last().Item2);
                                    if (bitOffset == lastOffset + 1)
                                    {
                                        currentGroup.Add((item.OriginalIndex, address, bool.Parse(item.Request.Value)));
                                    }
                                    else
                                    {
                                        continuousGroups.Add(currentGroup);
                                        currentGroup = new List<(int, string, bool)>();
                                        currentGroup.Add((item.OriginalIndex, address, bool.Parse(item.Request.Value)));
                                    }
                                }
                            }

                            if (currentGroup != null && currentGroup.Any())
                            {
                                continuousGroups.Add(currentGroup);
                            }

                            // 批量写入每个连续地址段
                            foreach (var group in continuousGroups)
                            {
                                var startAddress = group.First().Item2;
                                var values = group.Select(g => g.Item3).ToArray();

                                if (values.Length == 1)
                                {
                                    // 单个值，使用单写
                                    var writeResult = client.Write(startAddress, values[0]);
                                    if (writeResult.IsSuccess)
                                    {
                                        result[group.First().Item1] = true;
                                    }
                                }
                                else
                                {
                                    // 多个连续值，使用批量写入
                                    var writeResult = client.Write(startAddress, values);
                                    if (writeResult.IsSuccess)
                                    {
                                        foreach (var item in group)
                                        {
                                            result[item.Item1] = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 非布尔类型，逐个写入（这些类型通常不连续）
                            foreach (var item in requests)
                            {
                                var writeResult = ExecuteWriteByType(client, item.Address, item.Value, dataType);
                                if (writeResult.IsSuccess)
                                {
                                    result[item.Index] = true;
                                }
                            }
                        }
                    }

                    // 操作成功，记录成功状态
                    _circuitBreakerManager.RecordSuccess(plcCode);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"批量写入失败(重试 {retry + 1}/{MaxRetryCount}): {plcCode}");

                    if (retry < MaxRetryCount - 1)
                    {
                        await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
                    }
                }
            }

            // 所有重试都失败，记录失败状态
            _circuitBreakerManager.RecordFailure(plcCode);
            _logger?.LogError($"批量写入失败，已达到最大重试次数: {plcCode}");
            return result;
        }

        /// <summary>
        /// 根据数据类型执行写入操作
        /// </summary>
        private OperateResult ExecuteWriteByType(HslPlcClient client, string address, string value, string dataType)
        {
            switch (dataType?.ToLower())
            {
                case "bool":
                    return client.Write(address, bool.Parse(value));
                case "int":
                case "int32":
                    return client.Write(address, int.Parse(value));
                case "short":
                case "int16":
                    return client.Write(address, (short)int.Parse(value));
                case "float":
                case "single":
                    return client.Write(address, float.Parse(value));
                case "double":
                    return client.Write(address, double.Parse(value));
                case "string":
                    return client.Write(address, value);
                default:
                    return client.Write(address, int.Parse(value));
            }
        }

        /// <summary>
        /// 批量读取多个布尔地址（支持连续地址批量读取）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="requests">批量读取请求列表（起始地址+长度+映射索引）</param>
        /// <param name="totalCount">总结果数量（与 mappings 数量一致）</param>
        /// <returns>布尔值数组，顺序与原始请求顺序对应</returns>
        public async Task<bool[]> ReadBatchRequestsAsync(string plcCode, List<BatchReadRequest> requests, int totalCount)
        {
            var result = new bool[totalCount];

            for (int retry = 0; retry < MaxRetryCount; retry++)
            {
                try
                {
                    var client = GetClient(plcCode);
                    if (client == null)
                    {
                        _logger?.LogError($"PLC 客户端不存在：{plcCode}");
                        return result;
                    }

                    if (!client.IsConnected)
                    {
                        _logger?.LogWarning($"PLC 连接断开，尝试重连：{plcCode}");
                        if (!await TryReconnectAsync(plcCode))
                        {
                            if (retry < MaxRetryCount - 1)
                            {
                                await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
                                continue;
                            }
                            return result;
                        }
                    }

                    client = GetClient(plcCode);
                    if (client == null) return result;

                    // 并行执行所有批量读取请求
                    var tasks = requests.Select(request =>
                        Task.Run(() =>
                        {
                            try
                            {
                                if (request.Length == 1)
                                {
                                    // 单地址读取
                                    var readResult = client.ReadBool(request.StartAddress);
                                    if (readResult.IsSuccess && request.MappingIndices.Count > 0)
                                    {
                                        result[request.MappingIndices[0]] = readResult.Content;
                                    }
                                }
                                else
                                {
                                    // 批量读取连续地址
                                    var batchResult = client.ReadBool(request.StartAddress, request.Length);
                                    if (batchResult.IsSuccess && batchResult.Content != null)
                                    {
                                        for (int i = 0; i < batchResult.Content.Length && i < request.MappingIndices.Count; i++)
                                        {
                                            result[request.MappingIndices[i]] = batchResult.Content[i];
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // 读取失败，保留默认值 false
                            }
                        })
                    );

                    await Task.WhenAll(tasks);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"批量读取失败 (重试 {retry + 1}/{MaxRetryCount}): {plcCode}");
                    if (retry < MaxRetryCount - 1)
                    {
                        await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retry));
                    }
                }
            }

            _logger?.LogError($"批量读取失败，已达到最大重试次数：{plcCode}");
            return result;
        }

        /// <summary>
        /// 清理所有PLC客户端连接
        /// </summary>
        public void Cleanup()
        {
            // 停止健康检查定时器
            _healthCheckTimer?.Dispose();
            _healthCheckTimer = null;

            // 断开所有PLC连接
            foreach (var client in _plcClients.Values)
            {
                client?.Dispose();
            }
            _plcClients.Clear();
            _initialized = false;
        }
        /// <summary>
        /// 获取地址的位偏移量（用于判断西门子PLC地址连续性）
        /// 西门子PLC地址格式：DB10.DBX0.0 -> 位偏移量 = 0 * 8 + 0 = 0
        /// 普通地址格式：M10.0 -> 位偏移量 = 10 * 8 + 0 = 80
        /// </summary>
        public int GetAddressBitOffset(string address)
        {
            if (string.IsNullOrEmpty(address)) return 0;

            // 处理西门子DB地址格式：DB10.DBX{byte}.{bit}
            var dbMatch = Regex.Match(address, @"DB\d+\.DBX(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            if (dbMatch.Success)
            {
                if (int.TryParse(dbMatch.Groups[1].Value, out int byteOffset) &&
                    int.TryParse(dbMatch.Groups[2].Value, out int bitOffset))
                {
                    return byteOffset * 8 + bitOffset;
                }
            }

            // 处理普通地址格式：M10.0, X0.0, I1.2 等
            var normalMatch = Regex.Match(address, @"^[A-Za-z]+(\d+)\.(\d+)$");
            if (normalMatch.Success)
            {
                if (int.TryParse(normalMatch.Groups[1].Value, out int byteOffset) &&
                    int.TryParse(normalMatch.Groups[2].Value, out int bitOffset))
                {
                    return byteOffset * 8 + bitOffset;
                }
            }

            // 处理不带小数点的格式：M100, X10 等（默认为第0位）
            var simpleMatch = Regex.Match(address, @"^[A-Za-z]+(\d+)$");
            if (simpleMatch.Success)
            {
                if (int.TryParse(simpleMatch.Groups[1].Value, out int byteOffset))
                {
                    return byteOffset * 8;
                }
            }

            // 默认返回地址数字部分
            return GetAddressNumber(address) * 8;
        }

    }
}
