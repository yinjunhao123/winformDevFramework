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

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC数据采集服务
    /// 负责定时采集PLC设备状态和报警信息
    /// </summary>
    public class PlcDataCollectService : IPlcDataCollectService, IDisposable
    {
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly PlcConnectionManager _connectionManager;
        private readonly IPLC_RealTimeStatusRepository _statusRepository;
        private readonly IPLC_RealTimeAlarmRepository _alarmRepository;
        private readonly IPLC_ConfigRepository _plcConfigRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;
        private readonly IPLC_StatusDefRepository _plcStatusDefRepository;
        private readonly IPLC_AlarmInfoRepository _plcAlarmInfoRepository;
        private volatile bool _isRunning;
        private int _collectIntervalMs = 500;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5, 10);
        private readonly ConcurrentBag<Task> _collectTasks = new ConcurrentBag<Task>();
        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<PlcDataCollectService> _logger;
        /// <summary>
        /// 已触发报警的缓存（用于去重）
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _triggeredAlarms = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 缓存的地址配置（按PLC分组）
        /// </summary>
        private readonly ConcurrentDictionary<string, List<PLC_Address>> _cachedAddresses = new ConcurrentDictionary<string, List<PLC_Address>>();

        /// <summary>
        /// 缓存的状态定义（按PLC分组）
        /// </summary>
        private readonly ConcurrentDictionary<string, List<PLC_StatusDef>> _cachedStatusDefs = new ConcurrentDictionary<string, List<PLC_StatusDef>>();

        /// <summary>
        /// 缓存的报警信息定义（按PLC分组）
        /// </summary>
        private readonly ConcurrentDictionary<string, List<PLC_AlarmInfo>> _cachedAlarmInfos = new ConcurrentDictionary<string, List<PLC_AlarmInfo>>();

        /// <summary>
        /// PLC在线状态缓存（从心跳服务获取）
        /// Key: PlcCode, Value: 是否在线
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _plcOnlineStatus = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 设备状态地址缓存（按PLC分组，预计算批量读取请求）
        /// </summary>
        private readonly ConcurrentDictionary<string, StatusAddressCache> _cachedStatusAddresses = new ConcurrentDictionary<string, StatusAddressCache>();

        /// <summary>
        /// 设备报警地址缓存（按PLC分组，预计算批量读取请求）
        /// </summary>
        private readonly ConcurrentDictionary<string, AlarmAddressCache> _cachedAlarmAddresses = new ConcurrentDictionary<string, AlarmAddressCache>();

        /// <summary>
        /// 上次采集的状态值缓存（用于变化检测）
        /// Key: PlcCode_AddressCode, Value: 上次的状态值
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _lastStatusValues = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 设备状态地址缓存类
        /// </summary>
        private class StatusAddressCache
        {
            /// <summary>
            /// 批量读取请求列表（按地址类型分组，连续地址合并）
            /// </summary>
            public List<BatchReadRequest> BatchRequests { get; set; } = new List<BatchReadRequest>();

            /// <summary>
            /// 状态映射列表（索引与读取结果一一对应）
            /// </summary>
            public List<StatusMapping> Mappings { get; set; } = new List<StatusMapping>();

            /// <summary>
            /// 总地址数量
            /// </summary>
            public int TotalCount => Mappings.Count;
        }

        /// <summary>
        /// 状态映射（地址信息）
        /// </summary>
        private class StatusMapping
        {
            /// <summary>
            /// PLC编码
            /// </summary>
            public string PlcCode { get; set; }

            /// <summary>
            /// 地址编码（如 DB100.01, DB100.02, DB300.02）
            /// </summary>
            public string AddressCode { get; set; }

            /// <summary>
            /// 设备编码
            /// </summary>
            public string EquipmentCode { get; set; }

            /// <summary>
            /// 工站编码
            /// </summary>
            public string StationCode { get; set; }

            /// <summary>
            /// 数据类型
            /// </summary>
            public string DataType { get; set; }
        }

        /// <summary>
        /// 设备报警地址缓存类
        /// </summary>
        private class AlarmAddressCache
        {
            /// <summary>
            /// 批量读取请求列表（按地址类型分组，连续地址合并）
            /// </summary>
            public List<BatchReadRequest> BatchRequests { get; set; } = new List<BatchReadRequest>();

            /// <summary>
            /// 报警映射列表（索引与读取结果一一对应）
            /// </summary>
            public List<AlarmMapping> Mappings { get; set; } = new List<AlarmMapping>();

            /// <summary>
            /// 总地址数量
            /// </summary>
            public int TotalCount => Mappings.Count;
        }

        /// <summary>
        /// 报警映射（地址信息）
        /// </summary>
        private class AlarmMapping
        {
            public string PlcCode { get; set; }
            public string AddressCode { get; set; }
            public string StationCode { get; set; }
            public string DataType { get; set; }
            
            public string EquipmentCode { get; set; }
        }

        /// <summary>
        /// 批量读取请求
        /// </summary>
        private class BatchReadRequest
        {
            public string StartAddress { get; set; }
            public ushort Length { get; set; }
            public List<int> MappingIndices { get; set; } = new List<int>();
        }

        public bool IsRunning => _isRunning;
        public int CollectIntervalMs
        {
            get => _collectIntervalMs;
            set => _collectIntervalMs = Math.Max(1000, value);
        }


        public PlcDataCollectService(IPlcCommunicationService plcCommunicationService,
            PlcConnectionManager connectionManager,
            IPLC_RealTimeStatusRepository statusRepository,
            IPLC_RealTimeAlarmRepository alarmRepository,
            IPLC_ConfigRepository plcConfigRepository,
            IPLC_AddressRepository plcAddressRepository,
            IPLC_StatusDefRepository plcStatusDefRepository,
            IPLC_AlarmInfoRepository plcAlarmInfoRepository,
             ILogger<PlcDataCollectService> logger)
        {
            _plcCommunicationService = plcCommunicationService;
            _connectionManager = connectionManager;
            _statusRepository = statusRepository;
            _alarmRepository = alarmRepository;
            _plcConfigRepository = plcConfigRepository;
            _plcAddressRepository = plcAddressRepository;
            _plcStatusDefRepository = plcStatusDefRepository;
            _plcAlarmInfoRepository = plcAlarmInfoRepository;
            _logger = logger;

            // 订阅PLC状态变更事件（从心跳服务获取在线状态）
            DataPushBus.PlcStatusChanged += DataPushBus_PlcStatusChanged;
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _plcCommunicationService.InitializeConnections();

            LoadAddressCache();
            LoadStatusDefCache();
            LoadAlarmInfoCache();
            LoadStatusAddressCache();
            LoadAlarmAddressCache();

            _ = CollectDataLoopAsync();
        }

        /// <summary>
        /// 加载设备状态地址缓存（预计算批量读取请求）
        /// </summary>
        private void LoadStatusAddressCache()
        {
            try
            {
                // 遍历所有PLC的状态地址配置
                foreach (var kvp in _cachedAddresses)
                {
                    var plcCode = kvp.Key;
                    var allAddresses = kvp.Value;

                    // 筛选 DeviceStatus 类别的地址
                    var deviceStatusAddresses = allAddresses
                        .Where(a => a.Category == "DeviceStatus")
                        .OrderBy(a => GetAddressType(a.AddressCode))
                        .ThenBy(a => _plcCommunicationService.GetAddressBitOffset(a.AddressCode))
                        .ToList();

                    if (deviceStatusAddresses.Count == 0)
                        continue;

                    // 收集所有地址映射（只保留地址信息，不关联状态定义）
                    var allMappings = new List<(string Address, StatusMapping Mapping)>();

                    foreach (var address in deviceStatusAddresses)
                    {
                        allMappings.Add((address.AddressCode, new StatusMapping
                        {
                            PlcCode = plcCode,
                            AddressCode = address.AddressCode,
                            EquipmentCode = address.EquipmentCode,
                            StationCode = address.StationCode,
                            DataType = address.DataType
                        }));
                    }

                    // 按地址类型分组（如 X、M、Y、I、Q 等），每组内按地址数字排序
                    var groupedMappings = allMappings
                        .GroupBy(item => GetAddressType(item.Address))
                        .SelectMany(group => group.OrderBy(item => _plcCommunicationService.GetAddressBitOffset(item.Address)))
                        .ToList();

                    // 构建批量读取请求列表
                    var batchRequests = new List<BatchReadRequest>();
                    int currentIndex = 0;

                    while (currentIndex < groupedMappings.Count)
                    {
                        var request = new BatchReadRequest
                        {
                            StartAddress = groupedMappings[currentIndex].Address,
                            Length = 1,
                            MappingIndices = new List<int> { currentIndex }
                        };

                        // 查找连续的地址（使用位偏移量判断）
                        int nextIndex = currentIndex + 1;
                        while (nextIndex < groupedMappings.Count)
                        {
                            int currentOffset = _plcCommunicationService.GetAddressBitOffset(groupedMappings[nextIndex - 1].Address);
                            int nextOffset = _plcCommunicationService.GetAddressBitOffset(groupedMappings[nextIndex].Address);

                            // 西门子PLC地址连续是指位偏移量相差1
                            if (nextOffset == currentOffset + 1)
                            {
                                // 连续地址，扩展范围
                                request.Length++;
                                request.MappingIndices.Add(nextIndex);
                                nextIndex++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        batchRequests.Add(request);
                        currentIndex = nextIndex;
                    }

                    var cache = new StatusAddressCache
                    {
                        Mappings = groupedMappings.Select(item => item.Mapping).ToList(),
                        BatchRequests = batchRequests
                    };

                    _cachedStatusAddresses.AddOrUpdate(plcCode, cache, (key, old) => cache);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 加载设备报警地址缓存（预计算批量读取请求）
        /// </summary>
        private void LoadAlarmAddressCache()
        {
            try
            {
                // 获取所有PLC配置
                var plcConfigs = _plcConfigRepository.Query().ToList();

                foreach (var plc in plcConfigs)
                {
                    string plcCode = plc.PlcCode;

                    // 获取该PLC的所有地址配置
                    var allAddresses = _plcAddressRepository
                        .QueryListByClause(a => a.PlcCode == plcCode)
                        .ToList();

                    // 筛选 DeviceAlarm 类别的地址
                    var alarmAddresses = allAddresses
                        .Where(a => a.Category == "DeviceAlarm")
                        .OrderBy(a => GetAddressType(a.AddressCode))
                        .ThenBy(a => _plcCommunicationService.GetAddressBitOffset(a.AddressCode))
                        .ToList();

                    if (alarmAddresses.Count == 0)
                        continue;

                    // 收集所有地址映射
                    var allMappings = new List<(string Address, AlarmMapping Mapping)>();

                    foreach (var address in alarmAddresses)
                    {
                        allMappings.Add((address.AddressCode, new AlarmMapping
                        {
                            PlcCode = plcCode,
                            AddressCode = address.AddressCode,
                            StationCode = address.StationCode,
                            DataType = address.DataType,
                            EquipmentCode = address.EquipmentCode
                        }));
                    }

                    // 按地址类型分组，每组内按位偏移量排序
                    var groupedMappings = allMappings
                        .GroupBy(item => GetAddressType(item.Address))
                        .SelectMany(group => group.OrderBy(item => _plcCommunicationService.GetAddressBitOffset(item.Address)))
                        .ToList();

                    // 构建批量读取请求列表
                    var batchRequests = new List<BatchReadRequest>();
                    int currentIndex = 0;

                    while (currentIndex < groupedMappings.Count)
                    {
                        var request = new BatchReadRequest
                        {
                            StartAddress = groupedMappings[currentIndex].Address,
                            Length = 1,
                            MappingIndices = new List<int> { currentIndex }
                        };

                        // 查找连续的地址（使用位偏移量判断）
                        int nextIndex = currentIndex + 1;
                        while (nextIndex < groupedMappings.Count)
                        {
                            int currentOffset = _plcCommunicationService.GetAddressBitOffset(groupedMappings[nextIndex - 1].Address);
                            int nextOffset = _plcCommunicationService.GetAddressBitOffset(groupedMappings[nextIndex].Address);

                            // 西门子PLC地址连续是指位偏移量相差1
                            if (nextOffset == currentOffset + 1)
                            {
                                request.Length++;
                                request.MappingIndices.Add(nextIndex);
                                nextIndex++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        batchRequests.Add(request);
                        currentIndex = nextIndex;
                    }

                    var cache = new AlarmAddressCache
                    {
                        Mappings = groupedMappings.Select(item => item.Mapping).ToList(),
                        BatchRequests = batchRequests
                    };

                    _cachedAlarmAddresses.AddOrUpdate(plcCode, cache, (key, old) => cache);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 获取地址类型（如 M、X、Y、I、Q、DB 等）
        /// </summary>
        private string GetAddressType(string address)
        {
            if (string.IsNullOrEmpty(address)) return string.Empty;
            
            // 处理西门子DB地址格式：DB10.DBX0.0 -> 返回 DB10
            if (address.StartsWith("DB", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(address, @"^DB(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return "DB" + match.Groups[1].Value;
                }
            }
            
            string type = new string(address.TakeWhile(c => !char.IsDigit(c)).ToArray()).ToUpper();
            return type;
        }


        /// <summary>
        /// 加载地址配置缓存
        /// </summary>
        private void LoadAddressCache()
        {
            try
            {
                var allAddresses = _plcAddressRepository.Query().ToList();
                var grouped = allAddresses.GroupBy(a => a.PlcCode).ToList();

                foreach (var group in grouped)
                {
                    _cachedAddresses.AddOrUpdate(group.Key, group.ToList(), (key, old) => group.ToList());
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 加载状态定义缓存
        /// </summary>
        private void LoadStatusDefCache()
        {
            try
            {
                var allStatusDefs = _plcStatusDefRepository.Query().ToList();
                var grouped = allStatusDefs.GroupBy(s => s.PlcCode).ToList();

                foreach (var group in grouped)
                {
                    _cachedStatusDefs.AddOrUpdate(group.Key, group.ToList(), (key, old) => group.ToList());
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 加载报警信息定义缓存
        /// </summary>
        private void LoadAlarmInfoCache()
        {
            try
            {
                var allAlarmInfos = _plcAlarmInfoRepository.QueryListByClause(a => a.IsActive == true).ToList();
                var grouped = allAlarmInfos.GroupBy(a => a.PlcCode).ToList();

                foreach (var group in grouped)
                {
                    _cachedAlarmInfos.AddOrUpdate(group.Key, group.ToList(), (key, old) => group.ToList());
                }
            }
            catch
            {
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();

            try
            {
                Task.WhenAll(_collectTasks).Wait(5000);
            }
            catch
            {
            }

            _collectTasks.Clear();
        }

        private async Task CollectDataLoopAsync()
        {
            while (_isRunning && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var plcConfigs = _plcConfigRepository.Query().ToList();

                    var tasks = plcConfigs.Select(config =>
                        CollectFromSinglePlcAsync(config));

                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }

                await Task.Delay(_collectIntervalMs, _cts.Token);
            }
        }

        private async Task CollectFromSinglePlcAsync(PLC_Config config)
        {
            await _semaphore.WaitAsync(_cts.Token);
            try
            {
                // 使用心跳服务的状态缓存判断PLC是否在线
                bool isOnline = _plcOnlineStatus.TryGetValue(config.PlcCode, out var online) && online;
                
                if (!isOnline)
                {
                    // PLC离线，不执行采集，等待心跳服务重连
                    return;
                }

                await Task.WhenAll(
                    CollectStatusAsync(config),
                    CollectAlarmsOptimizedAsync(config)
                );
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"PLC数据采集异常: PlcCode={config.PlcCode}");
                // 采集异常时更新状态为离线，心跳服务会检测到并尝试重连
                _connectionManager.UpdateConnectionStatus(config.PlcID, false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task CollectStatusAsync(PLC_Config config)
        {
            try
            {
                // 直接从预计算的缓存获取状态地址配置
                if (!_cachedStatusAddresses.TryGetValue(config.PlcCode, out var statusCache))
                    return;

                if (statusCache.TotalCount == 0)
                    return;

                // 批量读取所有状态地址（只负责与PLC通信）
                var results = await ReadStatusBatchAsync(config, statusCache);

                // 初步变化检测：只发送有变化的数据到消息队列
                var changedMappings = new List<StatusMappingItem>();
                var changedValues = new List<string>();

                for (int i = 0; i < results.Length && i < statusCache.Mappings.Count; i++)
                {
                    var mapping = statusCache.Mappings[i];
                    var currentValue = results[i];
                    var cacheKey = $"{config.PlcCode}_{mapping.AddressCode}";

                    // 检查值是否有变化
                    if (_lastStatusValues.TryGetValue(cacheKey, out var lastValue) && lastValue == currentValue)
                    {
                        // 值没有变化，跳过
                        continue;
                    }

                    // 更新缓存的值
                    _lastStatusValues.AddOrUpdate(cacheKey, currentValue, (key, old) => currentValue);

                    // 添加变化的数据
                    changedMappings.Add(new StatusMappingItem
                    {
                        PlcCode = mapping.PlcCode,
                        AddressCode = mapping.AddressCode,
                        EquipmentCode = mapping.EquipmentCode,
                        StationCode = mapping.StationCode,
                        DataType = mapping.DataType
                    });
                    changedValues.Add(currentValue);
                }

                // 只有变化的数据才发送到消息队列
                if (changedMappings.Any())
                {
                    // 将变化的状态数据保存到数据库
                    var statusList = new List<PLC_RealTimeStatus>();
                    
                    for (int i = 0; i < changedMappings.Count && i < changedValues.Count; i++)
                    {
                        var mapping = changedMappings[i];
                        var status = new PLC_RealTimeStatus
                        {
                            PlcCode = mapping.PlcCode,
                            AddressCode = mapping.AddressCode,
                            EquipmentCode = mapping.EquipmentCode,
                            StationCode = mapping.StationCode,
                            StatusValue = changedValues[i],                         
                            CollectTime = DateTime.Now,
                            IsOnline = _plcOnlineStatus.TryGetValue(mapping.PlcCode, out var isOnline) ? isOnline : false,
                             StatusName= _cachedStatusDefs.TryGetValue(mapping.PlcCode, out var defs) 
                                ? defs.FirstOrDefault(d => d.StatusCode.ToString() == changedValues[i])?.StatusName ?? "未知状态"
                                : "未知状态"

                        };
                        statusList.Add(status);
                    }

                    // 批量插入到数据库
                    await _statusRepository.InsertAsync(statusList);

                    // 发布状态更新事件
                    foreach (var status in statusList)
                    {
                        DataPushBus.PublishStatus(status);
                    }

                    _logger?.LogInformation($"PLC {config.PlcCode} 状态变化: {statusList.Count} 条记录已保存并发布");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"处理PLC状态读取异常: PLC={config.PlcCode}");
            }
        }

        /// <summary>
        /// 批量读取状态值
        /// </summary>
        private async Task<string[]> ReadStatusBatchAsync(PLC_Config config, StatusAddressCache cache)
        {
            var result = new string[cache.TotalCount];

            try
            {
                // 并行执行所有批量读取请求
                var tasks = cache.BatchRequests.Select(request =>
                    Task.Run(() =>
                    {
                        try
                        {
                            if (request.Length == 1)
                            {
                                // 单地址读取
                                var value = ReadSingleValue(config, request.StartAddress, cache.Mappings[request.MappingIndices[0]].DataType);
                                result[request.MappingIndices[0]] = value;
                            }
                            else
                            {
                                // 批量读取连续地址
                                var values = ReadBatchValues(config, request.StartAddress, request.Length, cache.Mappings[request.MappingIndices[0]].DataType);
                                for (int i = 0; i < values.Length && i < request.MappingIndices.Count; i++)
                                {
                                    result[request.MappingIndices[i]] = values[i];
                                }
                            }
                        }
                        catch
                        {
                            // 读取失败，保留默认值 null
                        }
                    })
                );

                await Task.WhenAll(tasks);
            }
            catch
            {
            }

            return result;
        }

        /// <summary>
        /// 批量读取报警值（布尔类型）
        /// </summary>
        private async Task<string[]> ReadAlarmBatchAsync(PLC_Config config, AlarmAddressCache cache)
        {
            var result = new string[cache.TotalCount];

            try
            {
                // 并行执行所有批量读取请求
                var tasks = cache.BatchRequests.Select(request =>
                    Task.Run(() =>
                    {
                        try
                        {
                            // 报警地址都是布尔类型，直接批量读取
                            var batchResult = _plcCommunicationService.ReadBool(config.PlcCode, request.StartAddress, request.Length);
                            
                            if (batchResult.IsSuccess && batchResult.Content != null)
                            {
                                for (int i = 0; i < batchResult.Content.Length && i < request.MappingIndices.Count; i++)
                                {
                                    result[request.MappingIndices[i]] = batchResult.Content[i].ToString();
                                }
                            }
                        }
                        catch
                        {
                            // 读取失败，保留默认值 null
                        }
                    })
                );

                await Task.WhenAll(tasks);
            }
            catch
            {
            }

            return result;
        }

        /// <summary>
        /// 读取单个状态值
        /// </summary>
        private string ReadSingleValue(PLC_Config config, string address, string dataType)
        {
            try
            {
                switch (dataType?.ToLower())
                {
                    case "int":
                    case "int32":
                        var intResult = _plcCommunicationService.ReadInt32(config.PlcCode, address);
                        return intResult.IsSuccess ? intResult.Content.ToString() : null;
                    case "int16":
                    case "short":
                        var shortResult = _plcCommunicationService.ReadInt32(config.PlcCode, address);
                        return shortResult.IsSuccess ? ((short)shortResult.Content).ToString() : null;
                    case "float":
                        var floatResult = _plcCommunicationService.ReadFloat(config.PlcCode, address);
                        return floatResult.IsSuccess ? floatResult.Content.ToString() : null;
                    case "byte":
                        var byteResult = _plcCommunicationService.ReadByte(config.PlcCode, address);
                        return byteResult.IsSuccess ? byteResult.Content.ToString() : null;
                    default:
                        var defaultResult = _plcCommunicationService.ReadInt32(config.PlcCode, address);
                        return defaultResult.IsSuccess ? defaultResult.Content.ToString() : null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 批量读取连续地址的状态值
        /// </summary>
        private string[] ReadBatchValues(PLC_Config config, string startAddress, ushort length, string dataType)
        {
            var result = new string[length];

            try
            {
                // 使用并行读取每个地址
                var tasks = new Task<string>[length];
                for (int i = 0; i < length; i++)
                {
                    int index = i;
                    tasks[i] = Task.Run(() =>
                    {
                        string address = IncrementAddress(startAddress, index);
                        return ReadSingleValue(config, address, dataType);
                    });
                }

                Task.WaitAll(tasks);

                for (int i = 0; i < length; i++)
                {
                    result[i] = tasks[i].Result;
                }
            }
            catch
            {
            }

            return result;
        }

        private async Task CollectAlarmsOptimizedAsync(PLC_Config config)
        {
            try
            {
                // 直接从预计算的缓存获取报警地址配置
                if (!_cachedAlarmAddresses.TryGetValue(config.PlcCode, out var alarmCache))
                    return;

                if (alarmCache.TotalCount == 0)
                    return;

                // 批量读取所有报警地址（只负责与PLC通信）
                var results = await ReadAlarmBatchAsync(config, alarmCache);

                // 只处理值为 true 的报警
                var triggeredAlarms = new List<AlarmMappingItem>();
                for (int i = 0; i < results.Length && i < alarmCache.Mappings.Count; i++)
                {
                    if (bool.TryParse(results[i], out bool isTriggered) && isTriggered)
                    {
                        var mapping = alarmCache.Mappings[i];
                        triggeredAlarms.Add(new AlarmMappingItem
                        {
                            PlcCode = mapping.PlcCode,
                            EquipmentCode=mapping.EquipmentCode,
                            AddressCode = mapping.AddressCode,
                            StationCode = mapping.StationCode,
                            DataType = mapping.DataType
                        });
                    }
                }

                // 如果有触发的报警，保存到数据库并发布事件
                if (triggeredAlarms.Any())
                {
                    // 将报警数据保存到数据库
                    var alarmList = new List<PLC_RealTimeAlarm>();
                    
                    foreach (var mapping in triggeredAlarms)
                    {
                        var alarm = new PLC_RealTimeAlarm
                        {
                            PlcCode = mapping.PlcCode,
                            AddressCode = mapping.AddressCode,
                            EquipmentCode = mapping.EquipmentCode,
                            StationCode = mapping.StationCode,
                            TriggerTime = DateTime.Now,
                            IsHandled = false,
                           AlarmCode = _cachedAlarmInfos.TryGetValue(mapping.PlcCode, out var infos) 
                              ? infos.FirstOrDefault(a => a.AddressCode == mapping.AddressCode)?.AlarmCode 
                              : null,
                            AlarmName = _cachedAlarmInfos.TryGetValue(mapping.PlcCode, out var infos1)
                                ? infos1.FirstOrDefault(a => a.AddressCode == mapping.AddressCode)?.AlarmName
                                : null,
                             AlarmDesc = _cachedAlarmInfos.TryGetValue(mapping.PlcCode, out var infos2)
                                ? infos2.FirstOrDefault(a => a.AddressCode == mapping.AddressCode)?.AlarmDesc
                                : null,
                              AlarmLevel= _cachedAlarmInfos.TryGetValue(mapping.PlcCode, out var infos3)
                                ? infos3.FirstOrDefault(a => a.AddressCode == mapping.AddressCode)?.AlarmLevel
                                : null,
                               Suggestion = _cachedAlarmInfos.TryGetValue(mapping.PlcCode, out var infos4)
                                ? infos4.FirstOrDefault(a => a.AddressCode == mapping.AddressCode)?.Suggestion
                                : null                                
                        };
                        alarmList.Add(alarm);
                    }

                    // 批量插入到数据库
                    await _alarmRepository.InsertAsync(alarmList);

                    // 发布报警事件
                    foreach (var alarm in alarmList)
                    {
                        DataPushBus.PublishAlarm(alarm);
                    }

                    _logger?.LogInformation($"PLC {config.PlcCode} 报警触发: {alarmList.Count} 条记录已保存并发布");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"处理PLC报警读取异常: PLC={config.PlcCode}");
            }
        }

        /// <summary>
        /// 递增地址偏移量
        /// </summary>
        /// <param name="address">原始地址</param>
        /// <param name="offset">偏移量</param>
        /// <returns>递增后的地址</returns>
        private string IncrementAddress(string address, int offset)
        {
            try
            {
                // 提取地址类型前缀（如 M、X、Y、I、Q等）
                string type = new string(address.TakeWhile(c => !char.IsDigit(c)).ToArray());
                // 提取地址中的数字部分
                string numberPart = new string(address.SkipWhile(c => !char.IsDigit(c)).ToArray());
                
                if (int.TryParse(numberPart, out int num))
                {
                    return type + (num + offset).ToString();
                }
            }
            catch
            {
            }
            return address;
        }

        /// <summary>
        /// PLC状态变更事件处理
        /// 从心跳服务获取PLC在线状态
        /// </summary>
        private void DataPushBus_PlcStatusChanged(object sender, PlcStatusChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrEmpty(e.PlcCode))
            {
                _plcOnlineStatus[e.PlcCode] = e.IsOnline;
                _logger?.LogDebug($"PLC状态变更: {e.PlcCode} - {(e.IsOnline ? "在线" : "离线")}");
            }
        }

        public void Dispose()
        {
            // 取消订阅事件
            DataPushBus.PlcStatusChanged -= DataPushBus_PlcStatusChanged;

            Stop();
            _cts.Dispose();
            _semaphore.Dispose();
        }
    }
}
