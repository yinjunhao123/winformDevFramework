using Microsoft.Extensions.Logging;
using PLCBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WinformDevFramework.IRepository;
using WinformDevFramework.IServices.PLCBasic;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC心跳服务
    /// 定期检查所有PLC连接状态，实现自动重连和状态监控
    /// </summary>
    public class PlcHeartbeatService : IPlcHeartbeatService, IDisposable
    {
        /// <summary>
        /// PLC通信服务
        /// </summary>
        private readonly IPlcCommunicationService _plcCommunicationService;

        /// <summary>
        /// PLC连接管理器
        /// </summary>
        private readonly PlcConnectionManager _connectionManager;

        /// <summary>
        /// PLC配置数据仓储
        /// </summary>
        private readonly IBaseRepository<PLC_Config> _plcConfigRepository;

        /// <summary>
        /// 心跳检测线程
        /// </summary>
        private Thread _heartbeatThread;

        /// <summary>
        /// 服务运行状态标志
        /// </summary>
        private volatile bool _isRunning;

        /// <summary>
        /// 心跳检测间隔（毫秒）
        /// </summary>
        private int _intervalMs = 5000;

        /// <summary>
        /// 当前服务运行状态
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 心跳检测间隔（毫秒），最小值为1000ms
        /// </summary>
        public int IntervalMs
        {
            get => _intervalMs;
            set => _intervalMs = Math.Max(1000, value);
        }

        ILogger<PlcHeartbeatService> _logger;

        /// <summary>
        /// PLC状态变更事件（保留用于向后兼容）
        /// 当PLC连接状态发生变化时触发
        /// </summary>
        [Obsolete("请使用 DataPushBus.PlcStatusChanged 事件")]
        public event EventHandler<PlcStatusChangedEventArgs> PlcStatusChanged;
        
        /// <summary>
        /// 检查指定PLC是否已连接
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>true表示已连接，false表示未连接</returns>
        public bool IsPlcConnected(string plcCode)
        {
            if (string.IsNullOrEmpty(plcCode))
                return false;
            
            // 使用心跳检查来判断PLC是否在线
            return CheckHeartbeat(plcCode);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="plcCommunicationService">PLC通信服务</param>
        /// <param name="connectionManager">PLC连接管理器</param>
        /// <param name="plcConfigRepository">PLC配置数据仓储</param>
        public PlcHeartbeatService(IPlcCommunicationService plcCommunicationService,
            PlcConnectionManager connectionManager,
            IBaseRepository<PLC_Config> plcConfigRepository,ILogger<PlcHeartbeatService> logger)
        {
            _plcCommunicationService = plcCommunicationService;
            _connectionManager = connectionManager;
            _plcConfigRepository = plcConfigRepository;
            _logger = logger;
        }

        /// <summary>
        /// 启动心跳服务
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            
            // 启动前先从数据库加载PLC配置到连接管理器
            LoadPlcConfigsFromDatabase();

            // 启动时主动发布所有PLC的初始状态
            // 这样UI页面打开时能立即收到所有PLC的状态信息
            PublishInitialPlcStatus();

            _heartbeatThread = new Thread(HeartbeatLoop);
            _heartbeatThread.IsBackground = true;
            _heartbeatThread.Name = "PLC_Heartbeat";
            _heartbeatThread.Start();
        }

        /// <summary>
        /// 发布所有PLC的初始状态
        /// 在服务启动时调用，确保UI能获取到当前所有PLC的连接状态
        /// </summary>
        private void PublishInitialPlcStatus()
        {
            PublishCurrentPlcStatus();
        }

        /// <summary>
        /// 发布当前所有PLC的状态（公开方法）
        /// UI页面可以调用此方法获取当前所有PLC的连接状态
        /// 通过事件订阅方式返回，不直接返回结果
        /// </summary>
        public void PublishCurrentPlcStatus()
        {
            // 在后台线程执行，避免阻塞调用方
            Task.Run(() => PublishCurrentPlcStatusAsync());
        }

        /// <summary>
        /// 异步发布当前所有PLC的状态
        /// </summary>
        private async Task PublishCurrentPlcStatusAsync()
        {
            try
            {
                var plcConfigs = _connectionManager.GetAllPlcConfigs().ToList();

                foreach (var config in plcConfigs)
                {
                    try
                    {
                        // 使用 Task.Run 异步检查心跳，避免阻塞
                        bool isOnline = await Task.Run(() => CheckHeartbeat(config.PlcCode));
                        
                        OnPlcStatusChanged(new PlcStatusChangedEventArgs
                        {
                            PlcCode = config.PlcCode,
                            PlcName = config.PlcName,
                            IsOnline = isOnline,
                            ChangeTime = DateTime.Now
                        });

                        _logger?.LogInformation($"发布PLC状态: {config.PlcCode} = {isOnline}");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"发布PLC状态失败: {config.PlcCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发布PLC状态时发生异常");
            }
        }

        /// <summary>
        /// 从数据库加载PLC配置到连接管理器
        /// </summary>
        private void LoadPlcConfigsFromDatabase()
        {
            try
            {
                var configs = _plcConfigRepository.Query().ToList();
                foreach (var config in configs)
                {
                    _connectionManager.AddOrUpdateConnection(config);
                }
            }
            catch (Exception ex)
            {
                // 记录数据库查询失败日志
                try
                {
                    string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    string logPath = Path.Combine(logDir, $"PlcHeartbeat_Error_{DateTime.Now:yyyyMMdd}.log");
                    string errorContent = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - LoadPlcConfigsFromDatabase Error: {ex.Message}\r\nStack Trace: {ex.StackTrace}\r\n\r\n";
                    File.AppendAllText(logPath, errorContent, Encoding.UTF8);
                }
                catch { }
            }
        }

        /// <summary>
        /// 停止心跳服务
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _heartbeatThread?.Join(5000);
        }

        /// <summary>
        /// 心跳检测主循环
        /// </summary>
        private void HeartbeatLoop()
        {
            long loopCount = 0;
            _logger.LogInformation("[心跳服务] 心跳检测线程已启动，间隔: {_intervalMs}ms");

            while (_isRunning)
            {
                loopCount++;
                try
                {
                   // _logger.LogDebug($"[心跳服务] 第 {loopCount} 次心跳检测开始");
                    CheckAllConnections();
                    //_logger.LogDebug($"[心跳服务] 第 {loopCount} 次心跳检测完成");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[心跳服务] 第 {loopCount} 次心跳检测异常");
                }

                Thread.Sleep(_intervalMs);
            }

            _logger.LogInformation("[心跳服务] 心跳检测线程已停止");
        }

        /// <summary>
        /// 检查所有PLC连接状态
        /// </summary>
        private void CheckAllConnections()
        {
            try
            {
                var plcConfigs = _connectionManager.GetAllPlcConfigs().ToList();

                foreach (var config in plcConfigs)
                {
                    CheckSingleConnection(config);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[心跳服务] 检查所有PLC连接状态异常");
            }
        }

        /// <summary>
        /// 已发布初始状态的PLC集合（用于确保每个PLC首次检测时都发布状态）
        /// </summary>
        private readonly HashSet<string> _publishedInitialStatus = new HashSet<string>();

        /// <summary>
        /// 检查单个PLC连接状态
        /// </summary>
        /// <param name="config">PLC配置信息</param>
        private void CheckSingleConnection(PLC_Config config)
        {
            try
            {
                // 获取之前的连接状态
                bool wasConnected = _connectionManager.IsConnected(config.PlcCode);

                // 获取当前连接状态（通过读取心跳地址DB8000.0）
                bool isConnectedNow = CheckHeartbeat(config.PlcCode);

                // 如果之前连接但现在断开，尝试重连
                if (wasConnected && !isConnectedNow)
                {
                    _logger?.LogWarning($"[心跳检查] PLC心跳检测失败，尝试重连: PlcCode={config.PlcCode}");
                    
                    // 尝试重连
                    bool reconnectSuccess = TryReconnectPlc(config.PlcCode);
                    
                    if (reconnectSuccess)
                    {
                        // 重连成功后再次检查心跳
                        isConnectedNow = CheckHeartbeat(config.PlcCode);
                        _logger?.LogInformation($"[心跳检查] PLC重连后心跳检查: PlcCode={config.PlcCode}, IsOnline={isConnectedNow}");
                    }
                    else
                    {
                        _logger?.LogWarning($"[心跳检查] PLC重连失败: PlcCode={config.PlcCode}");
                    }
                }

                // 更新连接管理器状态
                _connectionManager.UpdateConnectionStatus(config.PlcID, isConnectedNow);

                // 判断是否需要发布状态：
                // 1. 首次检测（从未发布过初始状态）- 必须发布，让其他服务知道初始状态
                // 2. 状态发生变化 - 发布变化
                bool isFirstCheck = false;
                lock (_publishedInitialStatus)
                {
                    if (!_publishedInitialStatus.Contains(config.PlcCode))
                    {
                        _publishedInitialStatus.Add(config.PlcCode);
                        isFirstCheck = true;
                    }
                }

                if (isFirstCheck || wasConnected != isConnectedNow)
                {
                    OnPlcStatusChanged(new PlcStatusChangedEventArgs
                    {
                        PlcCode = config.PlcCode,
                        PlcName = config.PlcName,
                        IsOnline = isConnectedNow,
                        ChangeTime = DateTime.Now
                    });

                    if (isFirstCheck)
                    {
                        _logger?.LogInformation($"[心跳检查] 首次发布PLC状态: PlcCode={config.PlcCode}, IsOnline={isConnectedNow}");
                    }
                    else
                    {
                        _logger?.LogInformation($"[心跳检查] PLC状态变化: PlcCode={config.PlcCode}, {wasConnected} -> {isConnectedNow}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[心跳检查] 检查PLC连接状态异常: PlcCode={config.PlcCode}");
            }
        }

        /// <summary>
        /// 尝试重新连接PLC
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否重连成功</returns>
        private bool TryReconnectPlc(string plcCode)
        {
            try
            {
                // 调用通信服务的重连方法
                var reconnectTask = _plcCommunicationService.ReconnectAsync(plcCode);
                reconnectTask.Wait(10000); // 等待最多10秒
                return reconnectTask.Result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"PLC重连异常: PlcCode={plcCode}");
                return false;
            }
        }

        /// <summary>
        /// 检查PLC心跳（读取DB8000.0地址，返回true表示心跳成功）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>心跳是否成功（DB8000.0=1表示成功）</returns>
        private bool CheckHeartbeat(string plcCode)
        {
            try
            {
                // 读取心跳地址 DB8000.0
                var result = _plcCommunicationService.ReadBool(plcCode, "DB8000.0");
                
                if (result.IsSuccess)
                {
                    return result.IsSuccess;
                }
                else
                {
                    _logger.LogDebug($"[心跳检查] PLC {plcCode} 读取失败: {result.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[心跳检查] PLC {plcCode} 心跳检查异常");
                return false;
            }
        }

        /// <summary>
        /// 触发PLC状态变更事件
        /// 同时发布到 DataPushBus 总线，实现统一事件管理
        /// </summary>
        /// <param name="e">状态变更事件参数</param>
        protected virtual void OnPlcStatusChanged(PlcStatusChangedEventArgs e)
        {
            // 发布到 DataPushBus 总线（推荐方式）
            DataPushBus.PublishPlcStatusChanged(e.PlcCode, e.PlcName, e.IsOnline);
            
            // 触发自身事件（保留用于向后兼容）
            PlcStatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
