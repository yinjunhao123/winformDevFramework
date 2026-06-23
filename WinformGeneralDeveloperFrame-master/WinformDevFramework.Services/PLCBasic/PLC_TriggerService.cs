using Microsoft.Extensions.Logging;
using PLCBasic;
using PLCBasic.IRepository;
using PLCBasic.Repository;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.IO;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Basic.Repository;
using WinformDevFramework.IRepository;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Models.PLCBasic;
using System.Text;

namespace WinformDevFramework.Services.PLCBasic
{
    public class PLC_TriggerService : IPLC_TriggerService, IDisposable
    {
        #region 字段

        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_ConfigRepository _plcConfigRepository;
        private readonly IPLC_Event_Trigger_MasterRepository _eventMasterRepository;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        private readonly IMD_RoutingRepository _routingRepository;
        private readonly IMD_RoutingListRepository _routingListRepository;
        private readonly IMD_BarCodeRuleRepository _barCodeRuleRepository;
        private readonly IMD_BarCodeRuleListRepository _barCodeRuleListRepository;
        private readonly IWipBarCodeRepository _wipBarcodeRepository;
        private readonly IWipMaterialInfoRepository _wipMaterialInfoRepository;
        private readonly IWipBatchRepository _wipBatchRepository;
        private readonly IWipBarCodeProcessRepository _wipBarCodeProcessRepository;
        private readonly IMD_ProductModelRepository _md_ProductModelRepository;
        private readonly IPLC_StationRecipeCurrentRepository _stationRecipeCurrentRepository;
        private readonly IPLC_ParameterDistributionRepository _parameterDistributionRepository;
        private readonly IPLC_ParameterDistributionDetailRepository _parameterDistributionDetailRepository;
        private readonly IPLC_ParameterDistributionHistoryRepository _parameterDistributionHistoryRepository;
        private readonly IPLC_ParameterDistributionDetailHistoryRepository _parameterDistributionDetailHistoryRepository;
        private readonly IPLC_ParameterDistributionRecordRepository _parameterDistributionRecordRepository;
        private readonly IPLC_ParameterDistributionRecordDetailRepository _parameterDistributionRecordDetailRepository;
        private readonly IWipProcessingIrreversibleRepository _wipProcessingIrreversibleRepository;
        private readonly IWipBarcodeHistoryRepository _wipBarcodeHistoryRepository;
        private readonly IPLC_TriggerParamRepository _plcTriggerParamRepository;
        private readonly IPLC_CollectParametersRepository _plcCollectParametersRepository;
        private readonly IPLC_CollectParametersDetailRepository _plcCollectParametersDetailRepository;
        private readonly IProcessResultDataRepository _processResultDataRepository;


        // ============ 心跳服务集成 ============

        /// <summary>
        /// PLC心跳服务（用于复用连接状态检查）
        /// </summary>
        private readonly IPlcHeartbeatService _plcHeartbeatService;

        /// <summary>
        /// PLC连接状态缓存（从心跳服务获取）
        /// </summary>
        private readonly Dictionary<string, bool> _plcOnlineStatus = new Dictionary<string, bool>();

        private Thread _pollingThread;
        private volatile bool _isRunning;
        private CancellationTokenSource _pollingCancellationToken;
        private int _pollingIntervalMs = 100;

        private readonly Dictionary<string, EventTriggerConfig> _eventConfigCache = new Dictionary<string, EventTriggerConfig>();
        private readonly Dictionary<string, bool> _lastTriggerStatus = new Dictionary<string, bool>();
        private readonly Dictionary<string, PLC_Address> _deviceStatusCache = new Dictionary<string, PLC_Address>();
        private readonly Dictionary<int, List<PLC_Event_Data_Detail>> _eventDetailCache = new Dictionary<int, List<PLC_Event_Data_Detail>>();

        // ============ 工站工艺信息缓存（从PLC_StationRecipeCurrent表加载） ============

        /// <summary>
        /// 工站工艺信息缓存：Key = PlcCode_StationCode，Value = 工站工艺信息
        /// </summary>
        private readonly Dictionary<string, StationProcessInfo> _stationProcessCache = new Dictionary<string, StationProcessInfo>();

        // ============ MES模式切换 ============

        /// <summary>
        /// MES模式缓存：Key = PlcCode_StationCode，Value = true=MES在线，false=MES离线
        /// 使用 ConcurrentDictionary 保证线程安全，避免锁竞争
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _mesModeCache = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// MES模式地址缓存：Key = PlcCode_StationCode，Value = PLC地址配置
        /// </summary>
        private readonly Dictionary<string, PLC_Address> _mesModeAddressCache = new Dictionary<string, PLC_Address>();

        /// <summary>
        /// 上次MES模式状态：Key = PlcCode_StationCode，Value = 上次模式状态
        /// </summary>
        private readonly Dictionary<string, bool> _lastMesModeStatus = new Dictionary<string, bool>();

        private readonly ILogger<PLC_TriggerService> _logger;

        // Socket通讯服务（用于与外部设备通信，接收图片数据）
        private readonly ISocketCommunicationService _socketCommunicationService;

        // 限流信号量，最多同时处理12个事件
        private readonly SemaphoreSlim _eventSemaphore = new SemaphoreSlim(12, 20);

        // ============ 轮询间隔动态调整 ============

        /// <summary>
        /// 滑动窗口：记录最近N次的处理时间（毫秒）
        /// </summary>
        private readonly Queue<long> _recentProcessingTimes = new Queue<long>();

        /// <summary>
        /// 滑动窗口大小（保留最近多少次轮询的处理时间）
        /// </summary>
        private const int SlidingWindowSize = 20;

        /// <summary>
        /// 最小轮询间隔（毫秒），防止间隔过小导致CPU占用过高
        /// </summary>
        private const int MinPollingIntervalMs = 50;

        /// <summary>
        /// 最大轮询间隔（毫秒），防止间隔过大导致响应延迟
        /// </summary>
        private const int MaxPollingIntervalMs = 500;

        /// <summary>
        /// 调整阈值：处理时间超过当前间隔的此比例时才触发调整
        /// </summary>
        private const double AdjustmentThreshold = 0.7;

        /// <summary>
        /// 安全余量：目标间隔 = 平均处理时间 × 安全余量
        /// </summary>
        private const double SafetyMargin = 1.5;

        /// <summary>
        /// 调整步长：每次调整的最大变化量（毫秒），防止间隔剧烈波动
        /// </summary>
        private const int AdjustmentStepMs = 20;

        /// <summary>
        /// 上次调整间隔的时间戳，用于防止频繁调整
        /// </summary>
        private DateTime _lastAdjustmentTime = DateTime.MinValue;

        /// <summary>
        /// 调整冷却时间（秒），两次调整之间至少间隔此时间
        /// </summary>
        private const int AdjustmentCooldownSeconds = 10;

        // ============ 离线模式心跳下发 ============

        /// <summary>
        /// 心跳交替值缓存：Key = PlcCode_StationCode, Value = 0或1
        /// </summary>
        private readonly ConcurrentDictionary<string, int> _heartbeatToggleValues = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 离线心跳线程
        /// </summary>
        private Thread _heartbeatThread;

        /// <summary>
        /// 离线心跳间隔（毫秒）
        /// </summary>
        private const int HeartbeatIntervalMs = 5000;

        // ============ 熔断机制（Circuit Breaker） ============

        /// <summary>
        /// 断路器字典：Key = 事件类型，Value = 对应的断路器实例
        /// </summary>
        private readonly Dictionary<string, CircuitBreaker> _circuitBreakers = new Dictionary<string, CircuitBreaker>();

        /// <summary>
        /// 断路器配置（可根据需要调整）
        /// </summary>
        private readonly CircuitBreakerOptions _circuitBreakerOptions = new CircuitBreakerOptions
        {
            FailureThreshold = 5,          // 连续失败5次触发熔断
            OpenDurationMs = 30000,       // 熔断持续30秒
            HalfOpenSampleCount = 3,       // 半开状态下允许3个请求
            HalfOpenSuccessThreshold = 2   // 成功2个则恢复
        };

        #endregion

        #region 属性

        public bool IsRunning => _isRunning;

        public int PollingIntervalMs
        {
            get => _pollingIntervalMs;
            set => _pollingIntervalMs = Math.Max(50, value);
        }

        #endregion

        #region 事件

        public event EventHandler<PlcEventTriggeredEventArgs> EventTriggered;

        #endregion

        #region 构造函数

        public PLC_TriggerService(IPlcCommunicationService plcCommunicationService,
            IPLC_ConfigRepository plcConfigRepository,
            IPLC_Event_Trigger_MasterRepository eventMasterRepository,
            IPLC_Event_Data_DetailRepository eventDetailRepository,
            IPLC_AddressRepository plcAddressRepository,
            IMD_RoutingRepository routingRepository,
            IMD_RoutingListRepository routingListRepository,
            IMD_BarCodeRuleRepository barCodeRuleRepository,
            IMD_BarCodeRuleListRepository barCodeRuleListRepository,
            IWipBarCodeRepository wipBarcodeRepository,
            IWipMaterialInfoRepository wipMaterialInfoRepository,
            IWipBatchRepository wipBatchRepository,
            IWipBarCodeProcessRepository wipBarCodeProcessRepository,
            IMD_ProductModelRepository md_ProductModelRepository,
            IPLC_StationRecipeCurrentRepository stationRecipeCurrentRepository,
            IPLC_ParameterDistributionRepository parameterDistributionRepository,
            IPLC_ParameterDistributionDetailRepository parameterDistributionDetailRepository,
            IPLC_ParameterDistributionHistoryRepository parameterDistributionHistoryRepository,
            IPLC_ParameterDistributionDetailHistoryRepository parameterDistributionDetailHistoryRepository,
            IPLC_ParameterDistributionRecordRepository parameterDistributionRecordRepository,
            IPLC_ParameterDistributionRecordDetailRepository parameterDistributionRecordDetailRepository,
            IWipProcessingIrreversibleRepository wipProcessingIrreversibleRepository,
            IWipBarcodeHistoryRepository wipBarcodeHistoryRepository,
            IPlcHeartbeatService plcHeartbeatService,
            ISocketCommunicationService socketCommunicationService,
            IPLC_TriggerParamRepository  pLC_TriggerParamRepository,
            IPLC_CollectParametersDetailRepository plcCollectParametersDetailRepository, IPLC_CollectParametersRepository plcCollectParametersRepository,
            IProcessResultDataRepository processResultDataRepository,
            ILogger<PLC_TriggerService> logger)
        {
            _plcCommunicationService = plcCommunicationService;
            _plcConfigRepository = plcConfigRepository;
            _eventMasterRepository = eventMasterRepository;
            _eventDetailRepository = eventDetailRepository;
            _plcAddressRepository = plcAddressRepository;
            _routingRepository = routingRepository;
            _routingListRepository = routingListRepository;
            _barCodeRuleRepository = barCodeRuleRepository;
            _barCodeRuleListRepository = barCodeRuleListRepository;
            _wipBarcodeRepository = wipBarcodeRepository;
            _wipMaterialInfoRepository = wipMaterialInfoRepository;
            _wipBatchRepository = wipBatchRepository;
            _wipBarCodeProcessRepository = wipBarCodeProcessRepository;
            _md_ProductModelRepository = md_ProductModelRepository;
            _stationRecipeCurrentRepository = stationRecipeCurrentRepository;
            _parameterDistributionRepository = parameterDistributionRepository;
            _parameterDistributionDetailRepository = parameterDistributionDetailRepository;
            _parameterDistributionHistoryRepository = parameterDistributionHistoryRepository;
            _parameterDistributionDetailHistoryRepository = parameterDistributionDetailHistoryRepository;
            _parameterDistributionRecordRepository = parameterDistributionRecordRepository;
            _parameterDistributionRecordDetailRepository = parameterDistributionRecordDetailRepository;
            _wipProcessingIrreversibleRepository = wipProcessingIrreversibleRepository;
            _wipBarcodeHistoryRepository = wipBarcodeHistoryRepository;
            _plcHeartbeatService = plcHeartbeatService;
            _socketCommunicationService = socketCommunicationService;
            _plcTriggerParamRepository = pLC_TriggerParamRepository;
            _plcCollectParametersDetailRepository = plcCollectParametersDetailRepository;
            _plcCollectParametersRepository = plcCollectParametersRepository;
            _processResultDataRepository = processResultDataRepository;
            _logger = logger;

            // 初始化性能日志记录器
            PerformanceLogger.SetLogger(logger);

            // 订阅 DataPushBus 的 PLC 状态变化事件（推荐方式）
            DataPushBus.PlcStatusChanged += DataPushBus_PlcStatusChanged;

            // 初始化断路器（为每种事件类型创建一个断路器实例）
            InitializeCircuitBreakers();
        }

        /// <summary>
        /// DataPushBus PLC状态变化事件处理
        /// </summary>
        private void DataPushBus_PlcStatusChanged(object sender, PlcStatusChangedEventArgs e)
        {
            // 安全检查：确保 PlcCode 不为空
            if (string.IsNullOrWhiteSpace(e.PlcCode))
            {
                _logger.LogWarning($"收到PLC状态变化事件，但PlcCode为空，跳过");
                return;
            }

            lock (_plcOnlineStatus)
            {
                bool previousStatus;
                bool hadKey = _plcOnlineStatus.TryGetValue(e.PlcCode, out previousStatus);

                // 更新缓存
                _plcOnlineStatus[e.PlcCode] = e.IsOnline;

                // 只有状态变化时才记录日志
                if (!hadKey || previousStatus != e.IsOnline)
                {
                    string previousStr = hadKey ? (previousStatus ? "在线" : "离线") : "未知";
                    _logger.LogInformation($"PLC状态变化: {e.PlcCode} -> {(e.IsOnline ? "在线" : "离线")} (之前: {previousStr})");
                }
            }
        }

        /// <summary>
        /// 检查PLC连接状态（复用心跳服务状态）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>true=已连接，false=未连接</returns>
        private bool CheckPlcConnectionStatus(string plcCode)
        {
            // 优先使用心跳服务的状态缓存
            lock (_plcOnlineStatus)
            {
                if (_plcOnlineStatus.TryGetValue(plcCode, out bool isOnline))
                {
                    return isOnline;
                }
            }

            // 心跳服务尚未提供状态时，默认信任PLC在线
            // PLC通信服务本身会处理连接异常，不需要在这里阻塞
            return true;
        }

        /// <summary>
        /// 初始化断路器
        /// 为每种事件类型创建一个独立的断路器实例
        /// </summary>
        private void InitializeCircuitBreakers()
        {
            var eventTypes = new[]
            {
                "BatchCheckIn",      // 批次校验上升沿触发
                "BatchCheckOut",     // 批次校验下降沿触发
                "MainCheckIn",       // 主条码进站校验上升沿触发
                "MainCheckDownIn",   // 主条码进站校验下降沿触发
                "SubCheckIn",        // 子条码进站校验上升沿触发
                "SubCheckDownIn",    // 子条码进站校验下降沿触发
                "MainCheckOut",      // 采集加工数据上升沿触发
                "MainCheckDownOut",  // 采集加工数据下降沿触发
                "Processing",        // 不可逆工艺上升沿
                "ProcessDowning",    // 不可逆工艺下降沿
                "Repair",            // 返修上线上升沿
                "Calibra_Data",     // 标定参数
                "Bounce"//跳动

            };

            foreach (var eventType in eventTypes)
            {
                _circuitBreakers[eventType] = new CircuitBreaker(
                    name: $"PLC_Trigger_{eventType}",
                    options: _circuitBreakerOptions,
                    logger: _logger);
            }
        }

        #region 熔断机制辅助方法

        /// <summary>
        /// 检查事件是否允许通过断路器
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <returns>true=允许处理，false=熔断中拒绝处理</returns>
        private bool IsEventAllowed(string eventType)
        {
            if (_circuitBreakers.TryGetValue(eventType, out var circuitBreaker))
            {
                return circuitBreaker.IsAllowed();
            }
            
            // 如果没有对应的断路器，默认允许处理
            return true;
        }

        /// <summary>
        /// 获取断路器剩余熔断时间
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <returns>剩余熔断时间（毫秒）</returns>
        private int GetCircuitBreakerRemainingTime(string eventType)
        {
            if (_circuitBreakers.TryGetValue(eventType, out var circuitBreaker))
            {
                return circuitBreaker.GetRemainingOpenTimeMs();
            }
            
            return 0;
        }

        /// <summary>
        /// 记录事件处理成功
        /// </summary>
        /// <param name="eventType">事件类型</param>
        private void RecordEventSuccess(string eventType)
        {
            if (_circuitBreakers.TryGetValue(eventType, out var circuitBreaker))
            {
                circuitBreaker.RecordSuccess();
            }
        }

        /// <summary>
        /// 记录事件处理失败
        /// </summary>
        /// <param name="eventType">事件类型</param>
        private void RecordEventFailure(string eventType)
        {
            if (_circuitBreakers.TryGetValue(eventType, out var circuitBreaker))
            {
                circuitBreaker.RecordFailure();
            }
        }

        /// <summary>
        /// 重置指定事件类型的断路器
        /// </summary>
        /// <param name="eventType">事件类型</param>
        public void ResetCircuitBreaker(string eventType)
        {
            if (_circuitBreakers.TryGetValue(eventType, out var circuitBreaker))
            {
                circuitBreaker.Reset();
                _logger.LogInformation($"断路器[{circuitBreaker.State}]已重置: {eventType}");
            }
        }

        /// <summary>
        /// 获取指定事件类型的断路器状态
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <returns>断路器状态</returns>
        public CircuitBreakerState GetCircuitBreakerState(string eventType)
        {
            if (_circuitBreakers.TryGetValue(eventType, out var circuitBreaker))
            {
                return circuitBreaker.State;
            }
            
            return CircuitBreakerState.Closed;
        }

        #endregion

        #endregion

        #region 公共方法

        /// <summary>
        /// 配置加载完成信号
        /// </summary>
        private readonly TaskCompletionSource<bool> _configLoadedSignal = new TaskCompletionSource<bool>();

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _pollingCancellationToken = new CancellationTokenSource();

            // 在后台线程异步加载配置，避免阻塞主线程
            Task.Run(() =>
            {
                try
                {
                    ReloadConfig();
                    _logger.LogInformation("PLC_TriggerService 配置加载完成");
                    _configLoadedSignal.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PLC_TriggerService 配置加载失败");
                    _configLoadedSignal.TrySetResult(false);
                }
            });

            // 启动离线心跳线程（每5秒执行一次）
            _heartbeatThread = new Thread(async () => await HeartbeatLoop());
            _heartbeatThread.IsBackground = true;
            _heartbeatThread.Name = "PLC_Trigger_Heartbeat";
            _heartbeatThread.Start();

            _pollingThread = new Thread(async () => await PollingLoop());
            _pollingThread.IsBackground = true;
            _pollingThread.Name = "PLC_Trigger_Polling";
            _pollingThread.Start();
            
        }

        public void Stop()
        {
            _isRunning = false;
            _pollingCancellationToken?.Cancel();
            _pollingThread?.Join(5000);
            _pollingCancellationToken?.Dispose();
            _pollingCancellationToken = null;

            // 停止离线心跳线程
            _heartbeatThread?.Join(5000);
            _logger.LogInformation("离线心跳线程已停止");
        }

        public void ReloadConfig()
        {
            try
            {
                lock (_eventConfigCache)
                {
                    _eventConfigCache.Clear();
                    _lastTriggerStatus.Clear();
                    _deviceStatusCache.Clear();
                    _eventDetailCache.Clear();

                    // ============ 工站工艺信息缓存清理 ============
                    _stationProcessCache.Clear();  // 工站工艺信息缓存
                    
                    // ============ MES模式缓存清理 ============
                    _mesModeCache.Clear();
                    _mesModeAddressCache.Clear();
                    _lastMesModeStatus.Clear();

                    var eventConfigs = _eventMasterRepository.Query().ToList();
                    _logger.LogInformation($"开始加载事件触发配置，共 {eventConfigs.Count} 条");

                    foreach (var config in eventConfigs)
                    {
                        string eventCacheKey = BuildEventConfigCacheKey(config.PlcCode, config.StationCode, config.TriggerAddress, config.EventId);
                        _eventConfigCache[eventCacheKey] = new EventTriggerConfig
                        {
                            EventId = config.EventId,
                            PlcCode = config.PlcCode,
                            StationCode = config.StationCode,
                            TriggerAddress = config.TriggerAddress,
                            TriggerAddressType = config.TriggerAddressType,
                            EventType = config.EventType,
                            TriggerEdgeType = config.TriggerEdgeType,
                            Description = config.Description
                        };

                        string statusCacheKey = BuildTriggerStatusCacheKey(config.PlcCode, config.StationCode, config.TriggerAddress);
                        _lastTriggerStatus[statusCacheKey] = false;

                        var eventDetails = _eventDetailRepository.Query()
                            .Where(d => d.EventId == config.EventId)
                            .OrderBy(d => d.SortOrder)
                            .ToList();
                        _eventDetailCache[config.EventId] = eventDetails;
                        _logger.LogDebug($"事件ID {config.EventId} 加载了 {eventDetails.Count} 个数据点配置");

                        LoadDeviceStatusConfig(config.PlcCode, config.StationCode);
                        
                        // 加载MES模式地址配置（全局配置，每个PLC只有一个）
                        LoadMesModeConfig(config.PlcCode);
                    }

                    LoadStationRecipeCurrent();

                    _logger.LogInformation($"事件触发配置加载完成，共 {_eventConfigCache.Count} 个触发点，{_deviceStatusCache.Count} 个设备状态配置，{_mesModeCache.Count} 个MES模式配置");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新加载配置时发生错误");
            }
        }

        /// <summary>
        /// 加载工站工艺信息（从PLC_StationRecipeCurrent表）
        /// </summary>
        private void LoadStationRecipeCurrent()
        {
            try
            {
                var stationRecipes = _stationRecipeCurrentRepository.Query().ToList();

                foreach (var recipe in stationRecipes)
                {
                    string key = $"{recipe.PlcCode}_{recipe.StationCode}";
                    _stationProcessCache[key] = BuildStationProcessInfo(recipe.PlcCode, recipe.StationCode,
                        recipe.ProductModelCode, recipe.RecipeCode, recipe.RoutingCode);
                }

                _logger.LogInformation($"加载工站工艺信息完成，共 {stationRecipes.Count} 条记录");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载工站工艺信息时发生错误");
            }
        }

        /// <summary>
        /// 获取工站工艺信息
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="stationCode">工站编码</param>
        /// <returns>工站工艺信息，若无返回null</returns>
        private StationProcessInfo GetStationRecipeCurrent(string plcCode, string stationCode)
        {
            string key = $"{plcCode}_{stationCode}";
            _stationProcessCache.TryGetValue(key, out var processInfo);
            return processInfo;
        }

        /// <summary>
        /// 更新工站工艺信息（参数下发成功后调用）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="stationCode">工站编码</param>
        /// <param name="recipeCode">Recipe编码</param>
        /// <param name="productModelCode">产品型号编码</param>
        /// <param name="routingCode">工艺路线编码</param>
        /// <param name="createUser">操作人</param>
        private async Task UpdateStationRecipeCurrent(string plcCode, string stationCode, string recipeCode,
            string productModelCode, string routingCode, string createUser)
        {
            try
            {
                string key = $"{plcCode}_{stationCode}";

                // 先尝试更新现有记录
                var existing = await _stationRecipeCurrentRepository.QueryByClauseAsync(sr => sr.PlcCode == plcCode && sr.StationCode == stationCode);

                if (existing != null)
                {
                    // 更新现有记录
                    existing.RecipeCode = recipeCode;
                    existing.ProductModelCode = productModelCode;
                    existing.RoutingCode = routingCode;
                    await _stationRecipeCurrentRepository.UpdateAsync(existing);
                    _logger.LogInformation($"更新工站工艺信息: {plcCode}_{stationCode}, Recipe={recipeCode}");
                }
                else
                {
                    // 创建新记录
                    var newRecord = new PLC_StationRecipeCurrent
                    {
                        PlcCode = plcCode,
                        StationCode = stationCode,
                        RecipeCode = recipeCode,
                        ProductModelCode = productModelCode,
                        RoutingCode = routingCode,
                        CreateTime = DateTime.Now,
                        CreateUser = createUser
                    };
                    await _stationRecipeCurrentRepository.InsertAsync(newRecord);
                    _logger.LogInformation($"创建工站工艺信息: {plcCode}_{stationCode}, Recipe={recipeCode}");
                }

                // 更新缓存（构建完整的工艺信息）
                _stationProcessCache[key] = BuildStationProcessInfo(plcCode, stationCode, productModelCode, recipeCode, routingCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新工站工艺信息失败: {plcCode}_{stationCode}");
            }
        }

        /// <summary>
        /// 构建工站工艺信息
        /// </summary>
        private StationProcessInfo BuildStationProcessInfo(string plcCode, string stationCode, string productModel,
            string recipe, string routingCode)
        {
            var processInfo = new StationProcessInfo
            {
                PlcCode = plcCode,
                StationCode = stationCode,
                ProductModel = productModel,
                Recipe = recipe,
                RoutingCode = routingCode,
                RoutingList = new List<MD_RoutingList>(),
                UpdateTime = DateTime.Now
            };

            try
            {
                // 获取工艺路线列表
                var routing = _routingRepository.Query()
                    .FirstOrDefault(r => r.RoutingCode == routingCode);

                if (routing != null)
                {
                    processInfo.RoutingList = _routingListRepository.Query()
                        .Where(rl => rl.RoutingId == routing.RoutingID)
                        .OrderBy(rl => rl.SortOrder)
                        .ToList();

                    // 获取当前工站配置
                    var stationConfig = processInfo.RoutingList.FirstOrDefault(r => r.StationCode == stationCode);

                    if (stationConfig != null)
                    {
                        // 加载主零件条码规则
                        processInfo.MainBarCodeRule = LoadBarCodeRule(stationConfig.MainPartsRule);
                        if (processInfo.MainBarCodeRule != null)
                        {
                            processInfo.MainBarCodeRuleList = _barCodeRuleListRepository.Query()
                                .Where(rl => rl.BarCodeRuleId == processInfo.MainBarCodeRule.BarCodeRuleId)
                                .OrderBy(rl => rl.SortOrder)
                                .ToList();
                        }

                        // 加载子零件1条码规则
                        processInfo.FirstBarCodeRule = LoadBarCodeRule(stationConfig.FirstPartsRule);
                        if (processInfo.FirstBarCodeRule != null)
                        {
                            processInfo.FirstBarCodeRuleList = _barCodeRuleListRepository.Query()
                                .Where(rl => rl.BarCodeRuleId == processInfo.FirstBarCodeRule.BarCodeRuleId)
                                .OrderBy(rl => rl.SortOrder)
                                .ToList();
                        }

                        // 加载子零件2条码规则
                        processInfo.SecondBarCodeRule = LoadBarCodeRule(stationConfig.SecondPartsRule);
                        if (processInfo.SecondBarCodeRule != null)
                        {
                            processInfo.SecondBarCodeRuleList = _barCodeRuleListRepository.Query()
                                .Where(rl => rl.BarCodeRuleId == processInfo.SecondBarCodeRule.BarCodeRuleId)
                                .OrderBy(rl => rl.SortOrder)
                                .ToList();
                        }

                        // 加载子零件3条码规则
                        processInfo.ThirdBarCodeRule = LoadBarCodeRule(stationConfig.ThirdPartsRule);
                        if (processInfo.ThirdBarCodeRule != null)
                        {
                            processInfo.ThirdBarCodeRuleList = _barCodeRuleListRepository.Query()
                                .Where(rl => rl.BarCodeRuleId == processInfo.ThirdBarCodeRule.BarCodeRuleId)
                                .OrderBy(rl => rl.SortOrder)
                                .ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"构建工站工艺信息失败: {plcCode}_{stationCode}");
            }

            return processInfo;
        }

        /// <summary>
        /// 加载条码规则
        /// </summary>
        private MD_BarCodeRule LoadBarCodeRule(string barCodeName)
        {
            if (string.IsNullOrEmpty(barCodeName))
                return null;

            return _barCodeRuleRepository.Query()
                .FirstOrDefault(r => r.BarCodeName == barCodeName);
        }

        private void LoadDeviceStatusConfig(string plcCode, string stationCode)
        {
            try
            {
                string cacheKey = $"{plcCode}_{stationCode}";

                if (_deviceStatusCache.ContainsKey(cacheKey))
                {
                    return;
                }

                var deviceStatusAddress = _plcAddressRepository.Query()
                    .FirstOrDefault(a => a.PlcCode == plcCode && a.StationCode == stationCode && a.Category == "DeviceStatus");

                if (deviceStatusAddress != null)
                {
                    _deviceStatusCache[cacheKey] = new PLC_Address
                    {
                        PlcCode = plcCode,
                        StationCode = stationCode,
                        AddressCode = deviceStatusAddress.AddressCode,
                        AddressType = deviceStatusAddress.AddressType,
                        DataType = deviceStatusAddress.DataType
                    };
                    _logger.LogDebug($"加载设备状态配置: PlcCode={plcCode}, StationCode={stationCode}, Address={deviceStatusAddress.AddressCode}");
                }
                else
                {
                    _logger.LogWarning($"未找到设备状态地址配置: PlcCode={plcCode}, StationCode={stationCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载设备状态配置时发生错误: PlcCode={plcCode}, StationCode={stationCode}");
            }
        }

        /// <summary>
        /// 加载MES模式地址配置（Category = 'OperationMethod'）
        /// MES模式地址是全局的，每个PLC只有一个，与工站无关
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        private void LoadMesModeConfig(string plcCode)
        {
            try
            {
                // MES模式地址是全局的，缓存Key只使用PlcCode
                if (_mesModeAddressCache.ContainsKey(plcCode))
                {
                    return;
                }

                var mesModeAddress = _plcAddressRepository.Query()
                    .FirstOrDefault(a => a.PlcCode == plcCode && a.Category == "OperationMethod");

                if (mesModeAddress != null)
                {
                    _mesModeAddressCache[plcCode] = new PLC_Address
                    {
                        PlcCode = plcCode,
                        AddressCode = mesModeAddress.AddressCode,
                        AddressType = mesModeAddress.AddressType,
                        DataType = mesModeAddress.DataType
                    };
                    _mesModeCache[plcCode] = false; // 默认MES离线
                    _lastMesModeStatus[plcCode] = false;
                    _logger.LogDebug($"加载MES模式地址配置: PlcCode={plcCode}, Address={mesModeAddress.AddressCode}");
                }
                else
                {
                    _logger.LogWarning($"未找到MES模式地址配置: PlcCode={plcCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载MES模式地址配置时发生错误: PlcCode={plcCode}");
            }
        }

        /// <summary>
        /// 读取MES模式状态
        /// MES模式地址是全局的，每个PLC只有一个，与工站无关
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>true=MES在线，false=MES离线</returns>
        private bool ReadMesModeStatus(string plcCode)
        {
            try
            {
                // MES模式地址是全局的，缓存Key只使用PlcCode
                if (!_mesModeAddressCache.TryGetValue(plcCode, out var addressConfig))
                {
                    _logger.LogWarning($"未找到MES模式地址配置: PlcCode={plcCode}");
                    return false; // 默认MES离线
                }

                // 读取PLC地址值（根据数据类型调用对应的读取方法）
                object value = ReadPlcValueByDataType(plcCode, addressConfig);
                
                if (value == null)
                {
                    _logger.LogWarning($"读取MES模式地址失败: PlcCode={plcCode}, Address={addressConfig.AddressCode}");
                    return false;
                }

                // 转换为布尔值：true=MES在线，false=MES离线
                bool isMesOnline = Convert.ToBoolean(value);
                
                // 检查模式是否发生变化
                if (_lastMesModeStatus.TryGetValue(plcCode, out var lastStatus) && lastStatus != isMesOnline)
                {
                    _logger.LogInformation($"MES模式切换: PlcCode={plcCode} -> {(isMesOnline ? "MES在线" : "MES离线")}");
                }
                
                _lastMesModeStatus[plcCode] = isMesOnline;
                _mesModeCache[plcCode] = isMesOnline;
                
                return isMesOnline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取MES模式状态失败: PlcCode={plcCode}");
                return false; // 异常时默认MES离线
            }
        }

        /// <summary>
        /// 离线心跳循环（参考 PollingLoop 架构模式）
        /// 每5秒向PLC心跳地址写入0/1交替值
        /// </summary>
        private async Task HeartbeatLoop()
        {
            _logger.LogInformation("离线心跳线程已启动，间隔: {HeartbeatIntervalMs}ms，等待配置加载...");

            // 等待配置加载完成（最多等待30秒）
            try
            {
                using var cts = new CancellationTokenSource(30000);
                bool configLoaded = await _configLoadedSignal.Task.WaitAsync(cts.Token);
                if (!configLoaded)
                {
                    _logger.LogWarning("配置加载失败，离线心跳将以空配置运行");
                }
                else
                {
                    _logger.LogInformation("配置加载完成，开始离线心跳");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("配置加载超时（30秒），离线心跳将以当前配置运行");
            }

            while (_isRunning)
            {
                try
                {
                    await SendOfflineHeartbeatAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "离线心跳循环异常");
                }

                await Task.Delay(HeartbeatIntervalMs);
            }

            _logger.LogInformation("离线心跳线程已停止");
        }

        /// <summary>
        /// 离线模式心跳下发：向PLC心跳地址写入0/1交替值
        /// 仅对MES离线模式的工站执行
        /// </summary>
        private async Task SendOfflineHeartbeatAsync()
        {
            try
            {
                // 1. 获取所有MES模式配置
                List<string> mesModeKeys;
                lock (_mesModeAddressCache)
                {
                    mesModeKeys = _mesModeAddressCache.Keys.ToList();
                }

                if (!mesModeKeys.Any())
                    return;

                foreach (var key in mesModeKeys)
                {
                    var parts = key.Split('_');
                    if (parts.Length != 2) continue;

                    string plcCode = parts[0];
                    string stationCode = parts[1];

                    // 2. 检查是否为MES离线模式（在线模式不需要心跳）
                    // 使用 ConcurrentDictionary 缓存读取，无锁线程安全
                    _mesModeCache.TryGetValue(key, out bool isMesOnline);
                    if (isMesOnline)
                    {
                        _logger.LogDebug($"工站 {key} 为MES在线模式，跳过心跳下发");
                        continue;
                    }

                    // 3. 检查PLC连接状态
                    if (!CheckPlcConnectionStatus(plcCode))
                    {
                        _logger.LogDebug($"PLC {plcCode} 未连接，跳过心跳下发");
                        continue;
                    }

                    // 4. 获取心跳地址（Category = 'Heartbeat'）
                    var heartbeatAddress = _plcAddressRepository.Query()
                        .FirstOrDefault(a => a.PlcCode == plcCode 
                            && a.StationCode == stationCode 
                            && a.Category == "Heartbeat");

                    if (heartbeatAddress == null)
                    {
                        _logger.LogWarning($"未找到心跳地址配置: PlcCode={plcCode}, StationCode={stationCode}");
                        continue;
                    }

                    // 5. 0/1交替写入
                    int currentValue = _heartbeatToggleValues.GetOrAdd(key, 0);
                    int nextValue = currentValue == 0 ? 1 : 0;

                    try
                    {
                        var writeResult = _plcCommunicationService.Write(plcCode, heartbeatAddress.AddressCode, nextValue == 1);
                        if (writeResult.IsSuccess)
                        {
                            _heartbeatToggleValues[key] = nextValue;
                            _logger.LogDebug($"离线心跳下发成功: {key}, Address={heartbeatAddress.AddressCode}, Value={nextValue}");
                        }
                        else
                        {
                            _logger.LogWarning($"离线心跳下发失败: {key}, Address={heartbeatAddress.AddressCode}, 错误: {writeResult.Message}");
                        }
                    }
                    catch (Exception writeEx)
                    {
                        _logger.LogError(writeEx, $"离线心跳下发异常: {key}, Address={heartbeatAddress.AddressCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "离线心跳下发任务执行失败");
            }
        }

        /// <summary>
        /// 根据数据类型读取PLC值
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="addressConfig">地址配置</param>
        /// <returns>读取到的值，失败返回null</returns>
        private object ReadPlcValueByDataType(string plcCode, PLC_Address addressConfig)
        {
            try
            {
                string dataType = addressConfig.DataType?.ToLower();
                
                switch (dataType)
                {
                    case "bool":
                    case "bit":
                        var boolResult = _plcCommunicationService.ReadBool(plcCode, addressConfig.AddressCode);
                        if (boolResult.IsSuccess)
                        {
                            return boolResult.Content;
                        }
                        _logger.LogWarning($"读取Bool类型失败: {addressConfig.AddressCode}, 错误: {boolResult.Message}");
                        return null;

                    case "int":
                    case "int16":
                    case "int32":
                    case "dint":    
                        var intResult = _plcCommunicationService.ReadInt32(plcCode, addressConfig.AddressCode);
                        if (intResult.IsSuccess)
                        {
                            return intResult.Content;
                        }
                        _logger.LogWarning($"读取Int类型失败: {addressConfig.AddressCode}, 错误: {intResult.Message}");
                        return null;

                    case "float":
                    case "real":    
                        var floatResult = _plcCommunicationService.ReadFloat(plcCode, addressConfig.AddressCode);
                        if (floatResult.IsSuccess)
                        {
                            return floatResult.Content;
                        }
                        _logger.LogWarning($"读取Float类型失败: {addressConfig.AddressCode}, 错误: {floatResult.Message}");
                        return null;

                    case "string":
                    case "varchar":     
                        var stringResult = _plcCommunicationService.ReadString(plcCode, addressConfig.AddressCode, 100);
                        if (stringResult.IsSuccess)
                        {
                            return stringResult.Content;
                        }
                        _logger.LogWarning($"读取String类型失败: {addressConfig.AddressCode}, 错误: {stringResult.Message}");
                        return null;

                    default:
                        _logger.LogWarning($"不支持的数据类型: {dataType}, 地址: {addressConfig.AddressCode}");
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取PLC值异常: PlcCode={plcCode}, Address={addressConfig.AddressCode}, DataType={addressConfig.DataType}");
                return null;
            }
        }

        public List<PLC_Event_Trigger_Master> GetAllEventConfigs()
        {
            return _eventMasterRepository.Query().ToList();
        }

        #endregion

        #region 私有方法

        private async Task PollingLoop()
        {
            int consecutiveErrorCount = 0;
            long totalPollCount = 0;
            long totalProcessingTime = 0;

            _logger.LogInformation($"PLC轮询线程已启动，初始间隔: {_pollingIntervalMs}ms，等待配置加载...");
            PerformanceLogger.Log("TriggerService.PollingLoop", "Status", "Started");
            PerformanceLogger.Log("TriggerService.PollingLoop", "PollingIntervalMs", _pollingIntervalMs);

            // 等待配置加载完成（最多等待30秒）
            try
            {
                using var cts = new CancellationTokenSource(30000);
                bool configLoaded = await _configLoadedSignal.Task.WaitAsync(cts.Token);
                if (!configLoaded)
                {
                    _logger.LogWarning("配置加载失败，轮询线程将以空配置运行");
                }
                else
                {
                    _logger.LogInformation("配置加载完成，开始轮询");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("配置加载超时（30秒），轮询线程将以当前配置运行");
            }

            while (_isRunning)
            {
                var pollStopwatch = global::System.Diagnostics.Stopwatch.StartNew();
                totalPollCount++;

                try
                {
                   // _logger.LogDebug($"[轮询] 第 {totalPollCount} 次轮询开始，当前间隔: {_pollingIntervalMs}ms");

                    // 检查MES模式并执行相应逻辑（使用await替代.Wait）
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_pollingCancellationToken.Token))
                    {
                        cts.CancelAfter(_pollingIntervalMs);
                        await CheckAndExecuteByMesModeAsync().WaitAsync(cts.Token);
                    }
                    
                    consecutiveErrorCount = 0; // 重置错误计数

                    pollStopwatch.Stop();
                    long elapsedMs = pollStopwatch.ElapsedMilliseconds;
                    totalProcessingTime += elapsedMs;

                    // 记录轮询性能指标
                    PerformanceLogger.Log("TriggerService.PollingLoop", "PollCount", totalPollCount);
                    PerformanceLogger.Log("TriggerService.PollingLoop", "LastPollTimeMs", elapsedMs);
                    PerformanceLogger.Log("TriggerService.PollingLoop", "AveragePollTimeMs", totalProcessingTime / totalPollCount);

                    // 动态调整轮询间隔
                    AdjustPollingInterval(elapsedMs);

                  //  _logger.LogInformation($"[轮询] 第 {totalPollCount} 次轮询完成，耗时: {elapsedMs}ms，平均耗时: {totalProcessingTime / totalPollCount}ms");
                }
                catch (TaskCanceledException)
                {
                    // 超时或取消，继续下一轮
                    pollStopwatch.Stop();
                    if (_pollingCancellationToken?.Token.IsCancellationRequested == true)
                    {
                        _logger.LogInformation("轮询任务被取消");
                        break;
                    }
                    _logger.LogDebug($"轮询超时，继续下一轮");
                }
                catch (OperationCanceledException)
                {
                    // 超时或取消，继续下一轮
                    pollStopwatch.Stop();
                    if (_pollingCancellationToken?.Token.IsCancellationRequested == true)
                    {
                        _logger.LogInformation("轮询任务被取消");
                        break;
                    }
                    _logger.LogDebug($"轮询超时，继续下一轮");
                }
                catch (Exception ex)
                {
                    consecutiveErrorCount++;
                    pollStopwatch.Stop();

                    // 记录错误性能指标
                    PerformanceLogger.Log("TriggerService.PollingLoop", "ConsecutiveErrorCount", consecutiveErrorCount);
                    PerformanceLogger.Log("TriggerService.PollingLoop", "LastError", ex.Message);

                    _logger.LogError(ex, $"轮询异常，连续错误次数: {consecutiveErrorCount}");
                    
                    // 如果连续错误过多，增加间隔时间
                    if (consecutiveErrorCount > 10)
                    {
                        _logger.LogWarning($"连续错误超过10次，增加轮询间隔");
                        PerformanceLogger.Log("TriggerService.PollingLoop", "IncreasedInterval", true);
                        await Task.Delay(_pollingIntervalMs * 5);
                        continue;
                    }
                }
                
                await Task.Delay(_pollingIntervalMs);
            }

            // 记录轮询结束统计
            PerformanceLogger.Log("TriggerService.PollingLoop", "Status", "Stopped");
            PerformanceLogger.Log("TriggerService.PollingLoop", "TotalPollCount", totalPollCount);
            PerformanceLogger.Log("TriggerService.PollingLoop", "TotalProcessingTimeMs", totalProcessingTime);

            _logger.LogInformation($"PLC轮询线程已停止，总轮询次数: {totalPollCount}");
        }

        /// <summary>
        /// 动态调整轮询间隔
        /// 根据最近N次的处理时间，自适应调整轮询间隔，避免间隔过小导致CPU占用过高，或间隔过大导致响应延迟
        /// </summary>
        /// <param name="lastProcessingTimeMs">上次轮询的处理时间（毫秒）</param>
        private void AdjustPollingInterval(long lastProcessingTimeMs)
        {
            // 1. 将处理时间加入滑动窗口
            lock (_recentProcessingTimes)
            {
                _recentProcessingTimes.Enqueue(lastProcessingTimeMs);
                // 保持窗口大小
                while (_recentProcessingTimes.Count > SlidingWindowSize)
                {
                    _recentProcessingTimes.Dequeue();
                }

                // 窗口未满时不调整（需要足够的样本）
                if (_recentProcessingTimes.Count < SlidingWindowSize / 2)
                {
                    return;
                }

                // 2. 计算平均处理时间
                double avgProcessingTime = _recentProcessingTimes.Average();

                // 3. 计算目标间隔（平均处理时间 × 安全余量）
                int targetInterval = (int)Math.Round(avgProcessingTime * SafetyMargin);

                // 4. 限制在最小/最大范围内
                targetInterval = Math.Max(MinPollingIntervalMs, Math.Min(MaxPollingIntervalMs, targetInterval));

                // 5. 计算与当前间隔的差值
                int currentInterval = _pollingIntervalMs;
                int diff = targetInterval - currentInterval;

                // 6. 如果差值很小，不调整（避免频繁微调）
                if (Math.Abs(diff) < AdjustmentStepMs / 2)
                {
                    return;
                }

                // 7. 检查冷却时间，防止频繁调整
                if ((DateTime.Now - _lastAdjustmentTime).TotalSeconds < AdjustmentCooldownSeconds)
                {
                    return;
                }

                // 8. 限制单次调整步长，防止剧烈波动
                if (diff > AdjustmentStepMs)
                {
                    diff = AdjustmentStepMs;
                }
                else if (diff < -AdjustmentStepMs)
                {
                    diff = -AdjustmentStepMs;
                }

                // 9. 执行调整
                int newInterval = currentInterval + diff;
                newInterval = Math.Max(MinPollingIntervalMs, Math.Min(MaxPollingIntervalMs, newInterval));

                if (newInterval != currentInterval)
                {
                    _pollingIntervalMs = newInterval;
                    _lastAdjustmentTime = DateTime.Now;

                    string direction = diff > 0 ? "增大" : "减小";
                    _logger.LogInformation($"轮询间隔动态调整: {currentInterval}ms → {newInterval}ms ({direction}), " +
                        $"平均处理时间={avgProcessingTime:F1}ms, 样本数={_recentProcessingTimes.Count}");
                    PerformanceLogger.Log("TriggerService.PollingLoop", "AdjustedIntervalMs", newInterval);
                    PerformanceLogger.Log("TriggerService.PollingLoop", "AvgProcessingTimeMs", avgProcessingTime);
                }
            }
        }

        /// <summary>
        /// 根据MES模式执行不同的处理逻辑
        /// </summary>
        private async Task CheckAndExecuteByMesModeAsync()
        {
            try
            {
                // 获取所有MES模式配置
                List<string> mesModeKeys;
                lock (_mesModeAddressCache)
                {
                    mesModeKeys = _mesModeAddressCache.Keys.ToList();
                }

                if (!mesModeKeys.Any())
                {
                    // 如果没有MES模式配置，默认走MES离线模式（定时任务轮询）
                    _logger.LogInformation("[MES模式检查] 未配置MES模式地址，默认执行MES离线模式（定时任务轮询）");
                    await CheckAllTriggersAsync(false);
                    return;
                }

                // 检查是否有任何PLC处于MES离线模式
                bool hasOfflineStation = false;
                int offlineCount = 0;
                int onlineCount = 0;
                
                foreach (var plcCode in mesModeKeys)
                {
                    // 读取当前MES模式状态（全局配置，与工站无关）
                    bool isMesOnline = ReadMesModeStatus(plcCode);

                    if (!isMesOnline)
                    {
                        // 发现MES离线PLC，需要执行定时轮询
                        hasOfflineStation = true;
                        offlineCount++;
                    }
                    else
                    {
                        onlineCount++;
                    }
                }

                //_logger.LogInformation($"[MES模式检查] 完成，在线: {onlineCount}, 离线: {offlineCount}, 执行轮询: {hasOfflineStation}");

                // 只要有任何工站处于MES离线模式，就执行定时轮询
                if (hasOfflineStation)
                {
                    _logger.LogDebug("[MES模式检查] 存在MES离线PLC，执行定时任务轮询");
                    await CheckAllTriggersAsync(false);
                }
                else
                {
                    // 所有工站都是MES在线模式，不需要主动轮询，等待事件触发
                    _logger.LogDebug("[MES模式检查] 所有PLC均为MES在线模式，跳过轮询，等待事件触发");
                    await CheckAllTriggersAsync(true);  
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MES模式检查] 发生错误");
            }
        }

        private async Task CheckAllTriggersAsync(bool flag)
        {
            List<EventTriggerConfig> configsToCheck;

            lock (_eventConfigCache)
            {
                configsToCheck = _eventConfigCache.Values.ToList();
            }

            if (!configsToCheck.Any())
            {
                _logger.LogDebug("[触发检查] 无触发点配置，跳过");
                return;
            }

            // 记录性能指标：触发点总数
            PerformanceLogger.Log("TriggerService.CheckAllTriggers", "TriggerPointCount", configsToCheck.Count);

            // 按PLC分组
            var plcGroups = configsToCheck.GroupBy(c => c.PlcCode).ToList();

            // 记录PLC分组数量
            PerformanceLogger.Log("TriggerService.CheckAllTriggers", "PlcGroupCount", plcGroups.Count);

            _logger.LogDebug($"[触发检查] 开始，模式={(flag ? "MES在线" : "MES离线")}，PLC分组数: {plcGroups.Count}，触发点总数: {configsToCheck.Count}");

            var stopwatch = global::System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!flag)
                {
                    // 并行处理所有PLC（核心优化）
                    var tasks = plcGroups.Select(g => ProcessPlcGroupAsync(g.Key, g.ToList(),false)).ToList();
                    await Task.WhenAll(tasks);
                }
                else
                {
                    // mes在线并行处理所有PLC（核心优化）
                    var tasks = plcGroups.Select(g => ProcessPlcGroupAsync(g.Key, g.ToList(),true)).ToList();
                    await Task.WhenAll(tasks);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[触发检查] 并行处理PLC触发点时发生错误");
            }

            stopwatch.Stop();

            // 记录性能日志
            PerformanceLogger.Log("TriggerService.CheckAllTriggers", "TotalProcessingTimeMs", stopwatch.ElapsedMilliseconds);

            _logger.LogDebug($"[触发检查] 完成，耗时: {stopwatch.ElapsedMilliseconds}ms");

            // 如果处理时间超过轮询间隔的50%，记录警告
            if (stopwatch.ElapsedMilliseconds > _pollingIntervalMs * 0.5)
            {
                _logger.LogWarning($"[触发检查] 耗时 {stopwatch.ElapsedMilliseconds}ms，超过轮询间隔 {_pollingIntervalMs}ms 的50%，建议优化");
            }
        }

        /// <summary>
        /// 处理单个PLC的所有触发点（批量读取优化）
        /// </summary>
        private async Task ProcessPlcGroupAsync(string plcCode, List<EventTriggerConfig> configs,bool flag)
        {
            var stopwatch = global::System.Diagnostics.Stopwatch.StartNew();
            int triggerCount = 0;
            int errorCount = 0;

            try
            {
                _logger.LogDebug($"[PLC处理] 开始处理 PLC={plcCode}, 触发点数={configs.Count}, 模式={(flag ? "MES在线" : "MES离线")}");

                // 检查连接状态（复用心跳服务状态）
                if (!CheckPlcConnectionStatus(plcCode))
                {
                    // 心跳服务说离线，不要自己重连，等待心跳服务的状态更新
                    // 心跳服务会负责PLC的连接和重连
                    _logger.LogDebug($"[PLC处理] PLC {plcCode} 未连接，等待心跳服务处理");
                    stopwatch.Stop();
                    PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "Status", "Offline");
                    PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ProcessingTimeMs", stopwatch.ElapsedMilliseconds);
                    return;
                }

                // 心跳服务说在线，继续处理批量读取
                var boolConfigs = configs.Where(c => 
                    string.Equals(c.TriggerAddressType, "bool", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.TriggerAddressType, "bit", StringComparison.OrdinalIgnoreCase)).ToList();
                
                var otherConfigs = configs.Where(c => 
                    !string.Equals(c.TriggerAddressType, "bool", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(c.TriggerAddressType, "bit", StringComparison.OrdinalIgnoreCase)).ToList();

                // 记录PLC处理的触发点统计
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BoolTriggerCount", boolConfigs.Count);
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "OtherTriggerCount", otherConfigs.Count);

                // 批量读取布尔类型的触发点
                if (boolConfigs.Any())
                {
                    // 按地址分组，同一地址只读取一次
                    var groupedByAddress = boolConfigs.GroupBy(c => c.TriggerAddress).ToList();
                    var addressResults = new Dictionary<string, bool>();

                    // 批量读取所有唯一地址
                    var uniqueAddresses = groupedByAddress.Select(g => g.Key).ToList();
                    try
                    {
                        var batchStopwatch = global::System.Diagnostics.Stopwatch.StartNew();
                        _logger.LogDebug($"[PLC处理] PLC {plcCode} 开始批量读取 {uniqueAddresses.Count} 个唯一地址");
                        bool[] results = await _plcCommunicationService.ReadBatchAsync(plcCode, uniqueAddresses);
                        batchStopwatch.Stop();

                        PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BatchReadTimeMs", batchStopwatch.ElapsedMilliseconds);
                        PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BatchReadCount", uniqueAddresses.Count);

                        _logger.LogDebug($"[PLC处理] PLC {plcCode} 批量读取完成，耗时={batchStopwatch.ElapsedMilliseconds}ms");

                        // 保存读取结果
                        for (int i = 0; i < uniqueAddresses.Count && i < results.Length; i++)
                        {
                            addressResults[uniqueAddresses[i]] = results[i];
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, $"[PLC处理] PLC {plcCode} 批量读取失败，回退到逐个读取");
                        PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BatchReadError", 1);

                        // 回退到逐个读取
                        foreach (var address in uniqueAddresses)
                        {
                            try
                            {
                                bool status = ReadTriggerBit(plcCode, address, "bool");
                                addressResults[address] = status;
                            }
                            catch (Exception readEx)
                            {
                                errorCount++;
                                _logger.LogError(readEx, $"[PLC处理] PLC {plcCode} 读取地址 {address} 失败");
                            }
                        }
                    }

                    // 对每个地址，处理所有关联的事件配置
                    foreach (var group in groupedByAddress)
                    {
                        if (addressResults.TryGetValue(group.Key, out bool currentStatus))
                        {
                            // 获取该地址的上一次状态（所有事件共享同一个lastStatus）
                            string statusCacheKey = BuildTriggerStatusCacheKey(plcCode, group.First().StationCode, group.Key);
                            bool lastStatus;
                            lock (_lastTriggerStatus)
                            {
                                if (!_lastTriggerStatus.TryGetValue(statusCacheKey, out lastStatus))
                                {
                                    lastStatus = false;
                                }
                            }

                            // 处理该地址的所有事件配置（使用相同的 lastStatus）
                            foreach (var config in group)
                            {
                                ProcessTriggerStatusWithSharedLastStatus(config, currentStatus, lastStatus, flag);
                                triggerCount++;
                            }

                            // 所有事件处理完后，统一更新缓存
                            lock (_lastTriggerStatus)
                            {
                                _lastTriggerStatus[statusCacheKey] = currentStatus;
                            }
                        }
                    }
                }

                // 单独处理非布尔类型的触发点
                foreach (var config in otherConfigs)
                {
                    try
                    {
                        bool status = ReadTriggerBit(plcCode, config.TriggerAddress, config.TriggerAddressType);
                        ProcessTriggerStatus(config, status, flag);
                        triggerCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, $"[PLC处理] PLC {plcCode} 读取地址 {config.TriggerAddress} 失败");
                    }
                }

                stopwatch.Stop();

                // 记录详细的性能指标
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ProcessingTimeMs", stopwatch.ElapsedMilliseconds);
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "TriggerProcessedCount", triggerCount);
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ErrorCount", errorCount);
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "Status", "Success");

                // 如果处理时间超过50ms，记录详细信息
                if (stopwatch.ElapsedMilliseconds > 50)
                {
                    _logger.LogInformation($"[PLC处理] PLC {plcCode} 处理完成: 触发点={triggerCount}, 错误={errorCount}, 耗时={stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    _logger.LogDebug($"[PLC处理] PLC {plcCode} 处理完成: 触发点={triggerCount}, 错误={errorCount}, 耗时={stopwatch.ElapsedMilliseconds}ms");
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogError(ex, $"[PLC处理] 处理PLC {plcCode} 触发点失败");

                stopwatch.Stop();
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ProcessingTimeMs", stopwatch.ElapsedMilliseconds);
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ErrorCount", errorCount);
                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "Status", "Failed");
            }
        }

        /// <summary>
        /// 处理触发状态变化（使用外部传入的共享lastStatus，不更新缓存）
        /// </summary>
        private void ProcessTriggerStatusWithSharedLastStatus(EventTriggerConfig config, bool currentStatus, bool lastStatus, bool flag)
        {
            bool shouldTrigger = false;
            if (config.IsFallingEdgeTrigger)
            {
                shouldTrigger = lastStatus && !currentStatus;
            }
            else
            {
                shouldTrigger = !lastStatus && currentStatus;
            }

            if (shouldTrigger)
            {
                bool isRisingEdge = !lastStatus && currentStatus;
                var triggerEvent = new PlcEventTriggeredEventArgs()
                {
                    EventId = config.EventId,
                    PlcCode = config.PlcCode,
                    StationCode = config.StationCode,
                    EventType = config.EventType,
                    TriggerAddress = config.TriggerAddress,
                    TriggerTime = DateTime.Now,
                    IsRisingEdge = isRisingEdge
                };

                string edgeType = isRisingEdge ? "Rising" : "Falling";
                _logger.LogInformation($"检测到事件触发: EventId={config.EventId}, EventType={config.EventType}, PlcCode={config.PlcCode}, StationCode={config.StationCode}, EdgeType={edgeType}");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleEventAsync(triggerEvent, flag);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"事件处理异常: EventId={triggerEvent.EventId}, EventType={triggerEvent.EventType}");
                    }
                });
            }
        }

        /// <summary>
        /// 处理触发状态变化（旧方法，保留用于非布尔类型触发点）
        /// </summary>
        private void ProcessTriggerStatus(EventTriggerConfig config, bool currentStatus, bool flag)
        {
            string statusCacheKey = BuildTriggerStatusCacheKey(config.PlcCode, config.StationCode, config.TriggerAddress);

            bool lastStatus;
            lock (_lastTriggerStatus)
            {
                if (!_lastTriggerStatus.TryGetValue(statusCacheKey, out lastStatus))
                {
                    lastStatus = false;
                }
            }

            bool shouldTrigger = false;
            if (config.IsFallingEdgeTrigger)
            {
                shouldTrigger = lastStatus && !currentStatus;
            }
            else
            {
                shouldTrigger = !lastStatus && currentStatus;
            }

            if (shouldTrigger)
            {
                bool isRisingEdge = !lastStatus && currentStatus;
                var triggerEvent = new PlcEventTriggeredEventArgs()
                {
                    EventId = config.EventId,
                    PlcCode = config.PlcCode,
                    StationCode = config.StationCode,
                    EventType = config.EventType,
                    TriggerAddress = config.TriggerAddress,
                    TriggerTime = DateTime.Now,
                    IsRisingEdge = isRisingEdge
                };

                string edgeType = isRisingEdge ? "Rising" : "Falling";
                _logger.LogInformation($"检测到事件触发: EventId={config.EventId}, EventType={config.EventType}, PlcCode={config.PlcCode}, StationCode={config.StationCode}, EdgeType={edgeType}");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleEventAsync(triggerEvent, flag);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"事件处理异常: EventId={triggerEvent.EventId}, EventType={triggerEvent.EventType}");
                    }
                });
            }

            lock (_lastTriggerStatus)
            {
                _lastTriggerStatus[statusCacheKey] = currentStatus;
            }
        }

        /// <summary>
        /// 异步处理事件
        /// </summary>
        private async Task HandleEventAsync(PlcEventTriggeredEventArgs e,bool flag)
        {
            // 尝试获取信号量，最多等待100ms
            if (!await _eventSemaphore.WaitAsync(TimeSpan.FromMilliseconds(100)))
            {
                _logger.LogWarning($"事件处理队列已满，丢弃事件: EventId={e.EventId}, EventType={e.EventType}");
                return;
            }

            try
            {
                // 检查断路器状态
                if (!IsEventAllowed(e.EventType))
                {
                    var remainingTime = GetCircuitBreakerRemainingTime(e.EventType);
                    _logger.LogWarning($"事件[{e.EventType}]已熔断，拒绝处理，剩余熔断时间: {remainingTime}ms");
                    return;
                }

                bool eventHandledSuccessfully = false;

                if (!flag)   //mes离线处理事件
                {
                    switch (e.EventType)
                    {
                        case "BatchCheckIn"://批次校验上升沿触发
                        case "BatchCheckOut"://批次校验下降沿触发
                            await HandleBatchCheckAsync(e, e.IsRisingEdge);
                            eventHandledSuccessfully = true;
                            break;
                        case "MainCheckIn"://主条码进站校验上升沿触发
                        case "MainCheckDownIn"://主条码进站校验下降沿触发
                            await HandleMainCheckInAsync(e, e.IsRisingEdge);
                            eventHandledSuccessfully = true;
                            break;
                        case "SubCheckIn"://子条码进站校验上升沿触发
                        case "SubCheckDownIn"://子条码进站校验下降沿触发
                            await HandleSubCheckAsync(e, e.IsRisingEdge);
                            eventHandledSuccessfully = true;
                            break;
                        case "MainCheckOut"://采集加工数据上升沿触发
                        case "MainCheckDownOut"://采集加工数据下降沿触发
                            await HandleMainCheckOutAsync(e, e.IsRisingEdge);
                            eventHandledSuccessfully = true;
                            break;
                        case "Processing"://表明工件已经入不可逆工艺 上升沿
                        case "ProcessDowning"://表明工件已经入不可逆工艺 下降沿
                            await HandleProcessingAsync(e, e.IsRisingEdge);
                            eventHandledSuccessfully = true;
                            break;
                        //返修上线相关
                        case "Repair":     //上升沿触发
                            await HandleRepairUpAsync(e);
                            eventHandledSuccessfully = true;
                            break;
                        //标定参数
                        case "Calibra_Data":
                            await HandleCalibraUpDataAsync(e);
                            eventHandledSuccessfully = true;
                            break;
                        //打印上升沿处理事件
                        case "PrintExchange":
                            await HandlePrintExchangeAsync(e);
                            eventHandledSuccessfully = true;
                            break;
                        case "Bounce"://跳动
                            await HandleBounceExchangeAsync(e);
                            eventHandledSuccessfully = true;
                            break;
                        default:
                            _logger.LogWarning($"未知的事件类型: {e.EventType}");
                            break;
                    }
                }
                else
                {
                    switch (e.EventType)
                    {
                        //参数下发下降沿
                        case "ParameterDistributionDown":
                                await HandleParameterDistributionDownAsync(e);
                            break;
                        case "Bounce"://跳动
                            break;
                        case "BitwiseAND"://位移压力
                            break;
                        case "":
                            break;  

                    }
              }
                

                // 事件处理成功，记录成功
                if (eventHandledSuccessfully)
                {
                    RecordEventSuccess(e.EventType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"异步处理事件失败: EventId={e.EventId}, EventType={e.EventType}");
                
                // 事件处理失败，记录失败（触发熔断计数）
                RecordEventFailure(e.EventType);
            }
            finally
            {
                _eventSemaphore.Release();
            }
        }

        /// <summary>
        /// 根据数据类型读取PLC数据
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">PLC地址</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>读取的值（字符串形式）</returns>
        private string ReadPlcData(string plcCode, string address, string dataType)
        {
            try
            {
                if (string.IsNullOrEmpty(dataType))
                    return string.Empty;

                switch (dataType.ToLower())
                {
                    case "bool":
                        var boolResult = _plcCommunicationService.ReadBool(plcCode, address);
                        return boolResult.IsSuccess ? boolResult.Content.ToString() : string.Empty;
                    case "int32":
                        var intResult = _plcCommunicationService.ReadInt32(plcCode, address);
                        return intResult.IsSuccess ? intResult.Content.ToString() : string.Empty;
                    case "float":
                        var floatResult = _plcCommunicationService.ReadFloat(plcCode, address);
                        return floatResult.IsSuccess ? floatResult.Content.ToString(CultureInfo.InvariantCulture) : string.Empty;
                    case "string":
                        var stringResult = _plcCommunicationService.ReadString(plcCode, address, 256);
                        return stringResult.IsSuccess ? stringResult.Content : string.Empty;
                    default:
                        // 默认尝试读取int32
                        var defaultResult = _plcCommunicationService.ReadInt32(plcCode, address);
                        return defaultResult.IsSuccess ? defaultResult.Content.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"读取PLC数据失败: PlcCode={plcCode}, Address={address}, DataType={dataType}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 异步读取PLC参数值（根据数据类型）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="address">地址编码</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>参数值字符串</returns>
        private async Task<string> ReadPlcParamAsync(string plcCode, string address, string dataType)
        {
            return await Task.Run(() => ReadPlcData(plcCode, address, dataType));
        }

        #region  批次校验事件处理逻辑
        /// <summary>
        /// 批次校验事件
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleBatchCheckAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleBatchCheckInAsync(e);
            }
            else
            {
                await HandleBatchCheckOutAsync(e);
            }
        }

        private async Task HandleBatchCheckInAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理批次码验证事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }
                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);
                // 第四步：批次码校验逻辑
                string batchCode = CleanPlcString(readDataPointsDict.ContainsKey("BatchCheckID") ? readDataPointsDict["BatchCheckID"] : string.Empty);
                _logger.LogInformation($"读取当前批次码为{batchCode}");
                // 第五步：写入PLC（批次码验证）
                await BatchCheckInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, batchCode);

                _logger.LogInformation($"批次码验证事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理子条码验证事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task BatchCheckInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, string batchCode)
        {
            try
            {
                var flag = true;
                string msg = string.Empty;
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
                if (string.IsNullOrWhiteSpace(batchCode))
                {
                    msg = "批次码为空，无法继续处理";
                    _logger.LogError(msg);
                }
                // 第一步：验证批次码是否已存在
                if (string.IsNullOrEmpty(msg))
                {
                    var existingBatch = await _wipBatchRepository.QueryByClauseAsync(w => w.BatchCode == batchCode);

                    if (existingBatch != null)
                    {
                        msg = $"批次码 {batchCode} 已存在于 WipBatch 表中，当前状态: {existingBatch.Status}";
                        _logger.LogError(msg);
                    }
                }
                // 第二步：结束当前使用的批次，将状态改为4
                if (string.IsNullOrEmpty(msg))
                {
                    var currentActiveBatch = await _wipBatchRepository.QueryByClauseAsync(w => w.Status == 2);

                    if (currentActiveBatch != null)
                    {
                        currentActiveBatch.Status = 4;
                        await _wipBatchRepository.UpdateAsync(currentActiveBatch);
                        _logger.LogInformation($"结束当前批次: BatchCode={currentActiveBatch.BatchCode}, Status更新为4");
                    }
                }

                // 根据验证结果设置PLC写入数据
                if (string.IsNullOrEmpty(msg))
                {
                    // 验证成功
                    dataToWrite.Add("BatchCheckDone", true);
                    dataToWrite.Add("BatchCheckOK", true);
                    _logger.LogInformation($"批次码验证成功: BatchCode={batchCode}");
                }
                else
                {
                    // 验证失败
                    dataToWrite.Add("BatchCheckDone", 1);
                    dataToWrite.Add("BatchCheckNG", 1);
                    dataToWrite.Add("BatchCheckErrorMsg", msg);
                    dataToWrite.Add("BatchCheckErrorCode", 102);
                    _logger.LogWarning($"批次码验证失败: {msg}");
                }

                // 收集并写入PLC数据点
                bool writeSuccess = await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);

                if (!writeSuccess)
                {
                    _logger.LogWarning($"批次进站PLC写入部分失败: StationCode={stationCode}, BatchCode={batchCode}");
                }

                // 验证成功：添加新批次记录，status=1
                if (string.IsNullOrEmpty(msg))
                {
                    var newBatch = new WipBatch
                    {
                        BatchCode = batchCode,
                        Status = 2,
                        CreateTime = DateTime.Now,
                        CreateUser = "System"
                    };
                    await _wipBatchRepository.InsertAsync(newBatch);
                    _logger.LogInformation($"新增批次记录: BatchCode={batchCode}, Status=1");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次码验证写入PLC时发生错误");
            }
        }

        /// <summary>
        /// 批次验证完成（下降沿触发示例）
        /// 此方法演示如何处理下降沿触发的事件
        /// 配置：在 PLC_Event_Trigger_Master 表中设置 TriggerEdgeType = 'Falling'
        /// </summary>
        /// <param name="e">事件参数</param>
        /// <returns></returns>
        private async Task HandleBatchCheckOutAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理批次码验证完成事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");


                // 第一步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }
                // 第三步：写入PLC（批次码验证完成事件）
                await BatchCheckOutWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints);

                _logger.LogInformation($"批次码验证完成事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理子条码验证事件时发生错误: EventId={e.EventId}");
            }
        }
        /// <summary>
        /// 批次出站写入PLC
        /// 1. 将当前状态为1的批次号更新为状态2
        /// 2. 向PLC写入 BatchCheckDone=0, BatchCheckOK=0
        /// </summary>
        private async Task BatchCheckOutWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints)
        {
            try
            {
                string msg = string.Empty;
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();


                // 设置PLC写入数据（无论是否找到批次，都写入Done和OK为0）
                dataToWrite.Add("BatchCheckDone", false);
                dataToWrite.Add("BatchCheckOK", false);
                dataToWrite.Add("BatchCheckNG", false);
                dataToWrite.Add("BatchCheckErrorCode", 0);



                // 收集并写入PLC数据点
                bool writeSuccess = await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);

                if (!writeSuccess)
                {
                    _logger.LogWarning($"批次出站PLC写入部分失败: StationCode={stationCode}");
                }

                _logger.LogInformation($"批次出站处理完成: StationCode={stationCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批次出站处理异常: StationCode={stationCode}");
            }
        }

        #endregion

        #region 主条码校验 - 处理主条码进站校验上升沿/下降沿触发事件

        private async Task HandleMainCheckInAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleMainCheckInUpAsync(e);
            }
            else
            {
                await HandleMainCheckInDownAsync(e);
            }
        }

        /// <summary>
        /// 主条码进站校验上升沿处理
        /// </summary>
        private async Task HandleMainCheckInUpAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理主条码验证事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：写入PLC
                await MainCheckInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"主条码验证事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理主条码验证事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 主条码进站校验下降沿处理
        /// </summary>
        private async Task HandleMainCheckInDownAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理主条码进站校验下降沿触发事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：写入PLC
                await MainCheckDownInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"主条码验证事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理主条码验证事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 异步写入PLC
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="stationCode">工站编码</param>
        /// <param name="writeDataPoints">写入数据点列表</param>
        /// <param name="readDataPointsDict">读取数据点字典</param>

        private async Task MainCheckInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                string msg = string.Empty;
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
                //型号
                string partType = CleanPlcString(readDataPointsDict.ContainsKey("PartType") ? readDataPointsDict["PartType"] : string.Empty);
                //RFID条码信息
                string partId = CleanPlcString(readDataPointsDict.ContainsKey("PartID") ? readDataPointsDict["PartID"] : string.Empty);
                //批次号码
                string batchCheckId = CleanPlcString(readDataPointsDict.ContainsKey("BatchCheckID") ? readDataPointsDict["BatchCheckID"] : string.Empty);
                //程序号
                string RecipeVer = CleanPlcString(readDataPointsDict.ContainsKey("RecipeVer") ? readDataPointsDict["RecipeVer"] : string.Empty);
                
                //返修标志
                bool atRepair = readDataPointsDict.ContainsKey("AtRepair") ? bool.Parse(readDataPointsDict["AtRepair"]) : false;
                WipBarCode wipBarcode = new WipBarCode();
                //执行返修操作
                if (atRepair)
                {
                   string suPartId = CleanPlcString(readDataPointsDict.ContainsKey("PartID1") ? readDataPointsDict["PartID1"] : string.Empty);
                    if(string.IsNullOrWhiteSpace(suPartId))
                    {
                        msg = $"返修标识触发，{stationCode}工站未传递子条码信息";
                        await ErrorMsg(plcCode, stationCode, suPartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    //根据子条码获取主条码信息
                    var material = await _wipMaterialInfoRepository.QueryByClauseAsync(o => o.MaterialCode == suPartId);
                    if (material==null)
                    {
                        msg = $"未找到该零件信息: 子条码={suPartId}";
                        await ErrorMsg(plcCode, stationCode, suPartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == material.RfidCode);
                    if(wipBarcode == null)
                    {
                        msg = $"未找到主条码信息: 子条码={suPartId}";
                        await ErrorMsg(plcCode, stationCode, suPartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    //创建新RFID条码
                    partId = await GenerateBarcode(stationCode,RecipeVer);
                    partType = wipBarcode.ModelCode;
                    RecipeVer = wipBarcode.RecipeCode;
                }
                int PartStatus=0;
                bool isRecipeMatch = false;
                StationProcessInfo stationProcessInfo = null;
                PLC_StationRecipeCurrent stationRecipeCurrent = new PLC_StationRecipeCurrent();
                if (!atRepair)
                {
                    if (stationCode == "OP05-1A")
                    {
                        var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(pd => pd.RecipeCode == RecipeVer);
                        if (parameterDistribution == null)
                        {
                            msg = $"OP05-1A工站程序号{RecipeVer}未获取到对应参数分配信息";
                            _logger.LogInformation($"发布主零件进站校验失败: StationCode={stationCode}, Barcode={partId},失败原因{msg}");
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        partType = parameterDistribution.ProductModelCode;
                        //重新生成RFID条码
                        partId = await GenerateBarcode(stationCode, RecipeVer);

                    }
                    else
                    {
                        //OP05-1A工站单独处理  型号、条码信息都为空
                        if (string.IsNullOrWhiteSpace(RecipeVer))
                        {
                            msg = $"{stationCode}工站未传递程序版本号";
                            _logger.LogInformation($"发布主零件进站校验失败: StationCode={stationCode}, Barcode={partId},失败原因{msg}");
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(partType))
                        {
                            msg = $"{stationCode}工站未传递型号信息";
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        if (string.IsNullOrEmpty(partId))
                        {
                            msg = $"{stationCode}工站未传递RFID条码信息";
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == partId);
                        if (wipBarcode == null)
                        {
                            msg = $"{stationCode}工站{partId}条码没有在制信息";
                            await ErrorMsg(plcCode, stationCode, partId, msg, 5, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        if (wipBarcode.NextStationCode != stationCode)
                        {
                            msg = $"当前站点{wipBarcode.NextStationCode}与目标站点{stationCode}不一致";
                            await ErrorMsg(plcCode, stationCode, partId, msg, 6, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                    }
                    //不可逆信息
                    var process = await _wipBarCodeProcessRepository.QueryByClauseAsync(o => o.RfidCode == partId && o.StationCode == stationCode);
                    if (process != null)
                    {
                        msg = $"该条码{partId}在工站{stationCode}有不可逆信息";
                        await ErrorMsg(plcCode, stationCode, partId, msg, 6, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                }              
                stationRecipeCurrent = await _stationRecipeCurrentRepository.QueryByClauseAsync(sr => sr.PlcCode == plcCode && sr.StationCode == stationCode);
                if (stationRecipeCurrent == null)
                {
                    isRecipeMatch = false;
                }
                else
                {
                    //OP05-1A和当前值进行对比,其他情况 跟历史得信息进行对比
                    if (stationCode == "OP05-1A")
                    {
                        isRecipeMatch = string.Equals(RecipeVer, stationRecipeCurrent.RecipeCode, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        isRecipeMatch = string.Equals(RecipeVer, wipBarcode.RecipeCode, StringComparison.OrdinalIgnoreCase);

                    }
                }
                if (isRecipeMatch)
                {
                    _logger.LogInformation($"参数版本一致: RecipeVer={RecipeVer}, 当前Recipe={stationRecipeCurrent.RecipeCode}");
                    dataToWrite.Add("PartOK", true);
                    dataToWrite.Add("RecipeDone", false);
                }
                else
                {
                    //进行参数下发
                    // 参数版本不一致，需要下发新参数
                   string mbRecipeVer= stationCode == "OP05-1A" ? RecipeVer : wipBarcode.RecipeCode;
                    _logger.LogInformation($"参数版本不一致，开始下发新参数: 当前RecipeVer={RecipeVer}, 目标Recipe={mbRecipeVer}");
                    // 3.1 拿RecipeVer值匹配PLC_ParameterDistribution的RecipeCode
                    var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(pd => pd.RecipeCode == mbRecipeVer);
                    if(parameterDistribution==null)
                    {
                        msg = $"未找到参数下发配置: RecipeCode={mbRecipeVer}";
                        await ErrorMsg(plcCode, stationCode, partId, msg,5, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    var distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(o=>o.DistributionID== parameterDistribution.ID&&o.StationCode==stationCode);
                    if (distributionDetails == null)
                    {
                        msg = $"未找到程序号{mbRecipeVer}该工站{stationCode}的下发详情配置信息";
                        await ErrorMsg(plcCode, stationCode, partId, msg, 5,writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    // 3.3 记录PLC当前参数值（下发前的快照）
                    Dictionary<string, string> originalValues = new Dictionary<string, string>();
                    foreach (var detail in distributionDetails)
                    {
                        if (!string.IsNullOrEmpty(detail.AddressCode))
                        {
                            try
                            {
                                string originalValue = ReadPlcData(plcCode, detail.AddressCode, detail.DataType);
                                originalValues[detail.ParamName] = originalValue;
                                _logger.LogDebug($"读取PLC当前值: {detail.ParamName} = {originalValues[detail.ParamName]}");

                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"读取PLC当前值失败: {detail.ParamName}");
                                originalValues[detail.ParamName] = string.Empty;
                            }
                        }
                    }

                    // 3.4 创建历史记录（下发前的快照）
                    var history = new PLC_ParameterDistributionHistory
                    {
                        ProductModelCode = partType,
                        RecipeCode = RecipeVer,
                         StationCode=stationCode,
                        SnapshotTime = DateTime.Now,
                        CreateUser = "System",
                        CreateTime = DateTime.Now
                    };
                    await _parameterDistributionHistoryRepository.InsertAsync(history);
                    
                    // 查询获取实际生成的HistoryID
                    var insertedHistory = await _parameterDistributionHistoryRepository.QueryByClauseAsync(
                        h => h.ProductModelCode == partType && h.RecipeCode == RecipeVer && h.SnapshotTime == history.SnapshotTime);
                    long actualHistoryID = insertedHistory?.ID ?? 0;

                    // 3.5 创建历史明细记录
                    List<PLC_ParameterDistributionDetailHistory> historyDetails = new List<PLC_ParameterDistributionDetailHistory>();
                    foreach (var detail in distributionDetails)
                    {
                        var historyDetail = new PLC_ParameterDistributionDetailHistory
                        {
                            HistoryID = actualHistoryID,
                            PlcCode = detail.PlcCode,
                            EquipmentCode = detail.EquipmentCode,
                            StationCode = detail.StationCode,
                            AddressCode = detail.AddressCode,
                            DataType = detail.DataType,
                            ParamName = detail.ParamName,
                            OriginalValue = originalValues.ContainsKey(detail.ParamName) ? originalValues[detail.ParamName] : string.Empty,
                            NewValue = detail.ParamValue,
                            DistributionType = detail.DistributionType
                        };
                        historyDetails.Add(historyDetail);
                    }
                    await _parameterDistributionDetailHistoryRepository.InsertAsync(historyDetails);
                    _logger.LogInformation($"创建历史记录成功: HistoryID={actualHistoryID}, DetailCount={historyDetails.Count}");

                    _logger.LogInformation("参数下发数据准备完成");

                    // 3.9 写入PLC（先写入参数下发配置中的参数）
                    DateTime startTime = DateTime.Now;
                    int successCount = 0;
                    int failedCount = 0;
                    List<PLC_ParameterDistributionRecordDetail> recordDetails = new List<PLC_ParameterDistributionRecordDetail>();

                    // 3.9.1 写入参数下发配置中的参数（使用 distributionDetails 中的地址）
                    foreach (var detail in distributionDetails)
                    {
                        if (!string.IsNullOrEmpty(detail.ParamName) && !string.IsNullOrEmpty(detail.AddressCode))
                        {
                            try
                            {
                                WriteDataPoint(plcCode, detail.AddressCode, detail.DataType, detail.ParamValue);
                                _logger.LogInformation($"写入PLC参数成功: ParamName={detail.ParamName}, Address={detail.AddressCode}, Value={detail.ParamValue}");
                                successCount++;
                                var recordDetail = new PLC_ParameterDistributionRecordDetail
                                {
                                    RecordID = 0,
                                    DetailID = detail.ID,
                                    PlcCode = plcCode,
                                    EquipmentCode = detail.EquipmentCode ?? string.Empty,
                                    StationCode = stationCode,
                                    AddressCode = detail.AddressCode,
                                    DataType = detail.DataType ?? string.Empty,
                                    ParamName = detail.ParamName,
                                    ParamValue = detail.ParamValue?.ToString() ?? string.Empty,
                                    Status = "Success",
                                    ErrorMessage = string.Empty,
                                    RetryCount = 0,
                                    DurationMs = 0,
                                    DistributionTime = DateTime.Now
                                };
                                recordDetails.Add(recordDetail);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"写入PLC参数失败: ParamName={detail.ParamName}, Address={detail.AddressCode}, Value={detail.ParamValue}");
                                failedCount++;

                                var recordDetail = new PLC_ParameterDistributionRecordDetail
                                {
                                    RecordID = 0,
                                    DetailID = detail.ID,
                                    PlcCode = plcCode,
                                    EquipmentCode = detail.EquipmentCode ?? string.Empty,
                                    StationCode = stationCode,
                                    AddressCode = detail.AddressCode,
                                    DataType = detail.DataType ?? string.Empty,
                                    ParamName = detail.ParamName,
                                    ParamValue = detail.ParamValue?.ToString() ?? string.Empty,
                                    Status = "Failed",
                                    ErrorMessage = ex.Message,
                                    RetryCount = 0,
                                    DurationMs = 0,
                                    DistributionTime = DateTime.Now
                                };
                                recordDetails.Add(recordDetail);
                            }
                        }
                    }
                    DateTime endTime = DateTime.Now;
                    long durationMs = (long)(endTime - startTime).TotalMilliseconds;

                    // 3.10 创建下发记录
                    string recordCode = $"DR{DateTime.Now:yyyyMMddHHmmssfff}";
                    string status = failedCount == 0 ? "Success" : (successCount > 0 ? "PartialSuccess" : "Failed");

                    var record = new PLC_ParameterDistributionRecord
                    {
                        DistributionID = parameterDistribution.ID,
                        RecordCode = recordCode,
                        ProductModelCode = parameterDistribution.ProductModelCode,
                        RecipeCode = parameterDistribution.RecipeCode,
                        DistributionType = "Dispatching",
                        PlcCode = plcCode,
                        EquipmentCode = distributionDetails.FirstOrDefault()?.EquipmentCode ?? string.Empty,
                        StationCode = stationCode,
                        Status = status,
                        TotalCount = distributionDetails.Count,
                        SuccessCount = successCount,
                        FailedCount = failedCount,
                        Message = status == "Success" ? "参数下发成功" : $"参数下发部分成功，成功{successCount}条，失败{failedCount}条",
                        StartTime = startTime,
                        EndTime = endTime,
                        DurationMs = durationMs,
                        DistributionUser = "System",
                        CreateTime = DateTime.Now,
                        CreateUser = "System"
                    };
                    await _parameterDistributionRecordRepository.InsertAsync(record);

                    // 查询获取实际生成的RecordID
                    var insertedRecord = await _parameterDistributionRecordRepository.QueryByClauseAsync(
                        r => r.RecordCode == recordCode);
                    long actualRecordID = insertedRecord?.ID ?? 0;

                    // 3.11 设置下发记录明细的RecordID并插入数据库
                    foreach (var recordDetail in recordDetails)
                    {
                        recordDetail.RecordID = actualRecordID;
                        await _parameterDistributionRecordDetailRepository.InsertAsync(recordDetail);
                    }

                    _logger.LogInformation($"创建下发记录成功: RecordID={actualRecordID}, Status={status}, Success={successCount}, Failed={failedCount}");

                    // 3.12 更新PLC_StationRecipeCurrent表
                    await UpdateStationRecipeCurrent(plcCode, stationCode, parameterDistribution.RecipeCode,
                        parameterDistribution.ProductModelCode, parameterDistribution.RoutingCode, "System");
                    msg="参数下发完成";
                    // 发布PLC参数下发事件（下发完成）
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                }
                //成功才写入的数据

                if (stationCode == "OP05-1A")
                {
                    dataToWrite.Add("PartType", partType);
                    dataToWrite.Add("RecipeVer", RecipeVer);
                }
                else
                {
                    dataToWrite.Add("PartType", wipBarcode.ModelCode);
                    dataToWrite.Add("RecipeVer", wipBarcode.RecipeCode);
                }

                dataToWrite.Add("PartID", partId);
                dataToWrite.Add("PartStatus", PartStatus);
                DateTime today = DateTime.Today;
                bool hasTodayData = await _wipBarcodeRepository.QueryByClauseAsync(o => o.StationCode == stationCode && o.ModelCode == partType && o.CreateTime >= today) != null;
                dataToWrite.Add("IsFir", hasTodayData ? false : true);
                dataToWrite.Add("MasterPartStatus", false);
                //转台路由指示工位专用状态字
                dataToWrite.Add("PartRouting", "");

                // 收集并写入PLC数据点
                bool writeSuccess = await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);

                if (!writeSuccess)
                {
                    _logger.LogWarning($"主条码校验PLC写入部分失败: StationCode={stationCode}, Barcode={partId}");
                }
                stationProcessInfo = _stationProcessCache[$"{plcCode}_{stationCode}"];
                // PLC 操作完成后，异步更新或新增 WipBarCode 表数据
                if (!atRepair)
                {
                    await UpdateWipBarcodeAsync(partId, partType, stationCode, batchCheckId, stationProcessInfo,RecipeVer);
                }
                else
                {
                    // 返修场景：更新主条码、子零件关联关系，并保存历史记录
                    try
                    {
                        // 1. 保存原主条码的历史记录
                        await _wipBarcodeHistoryRepository.SaveHistoryAsync(wipBarcode, "Repair");
                        _logger.LogInformation($"返修-保存原主条码历史记录: OldBarCode={wipBarcode.RFIDCode}, NewBarCode={partId}");

                        // 2. 更新 WipMaterialInfo 表中所有 BarCode 等于原主条码的记录
                        string oldBarCode = wipBarcode.RFIDCode;
                        var materialList = await _wipMaterialInfoRepository.QueryListByClauseAsync(m => m.RfidCode == oldBarCode);
                        if (materialList != null && materialList.Count > 0)
                        {
                            foreach (var mat in materialList)
                            {
                                mat.RfidCode = partId;
                            }
                            await _wipMaterialInfoRepository.UpdateAsync(materialList);
                            _logger.LogInformation($"返修-批量更新子零件关联主条码: Count={materialList.Count}, OldBarCode={oldBarCode}, NewBarCode={partId}");
                        }

                        // 3. 更新主条码表的 Barcode 字段为新条码 partId
                        wipBarcode.RFIDCode = partId;
                        wipBarcode.InputTime = DateTime.Now;
                        wipBarcode.OutputTime = null;
                        wipBarcode.StationCode = stationCode;
                        wipBarcode.PrStationCode = wipBarcode.StationCode;
                        wipBarcode.RepairCount = (wipBarcode.RepairCount ?? 0) + 1;
                        wipBarcode.UpdateUser = "system";
                        wipBarcode.UpdateTime = DateTime.Now;
                        await _wipBarcodeRepository.UpdateAsync (wipBarcode);
                        _logger.LogInformation($"返修-更新主条码: BarCode={partId}, RepairCount={wipBarcode.RepairCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"返修-更新主条码和子零件关联失败: OldBarCode={wipBarcode.BarCode}, NewBarCode={partId}");
                    }
                }

                // 发布主零件进站校验事件到前端监控
                  DataPushBus.PublishMainPartStationCheck(stationCode, partId, true, "主条码进站校验成功");
                 _logger.LogInformation($"发布主零件进站成功事件: StationCode={stationCode}, Barcode={partId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入 PLC 时发生错误");
            }
        }

        public async Task ErrorMsg(string plcCode, string stationCode, string partId, string msg,int MainCheckInErrorCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, Object> dataToWrite)
        {
            //直接返回错误
            dataToWrite.Add("PartNG", true);
            dataToWrite.Add("PartIDReqDone", true);
            // 主条码进站失败错误代码：3，错误信息：msg
            dataToWrite.Add("MainCheckInErrorCode", MainCheckInErrorCode);
            //dataToWrite.Add("MainCheckInErrorMsg", msg);
            _logger.LogInformation($"发布主零件进站校验失败: StationCode={stationCode}, Barcode={partId},失败原因{msg}");
            bool isSuccess = await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);
            if (!isSuccess)
            {
                _logger.LogWarning($"主条码校验PLC写入部分失败: StationCode={stationCode}, Barcode={partId}");
            }
        }

        /// <summary>
        /// 主条码验证下降沿触发事件写入PLC逻辑
        /// </summary>
        /// <param name="plcCode"></param>
        /// <param name="stationCode"></param>
        /// <param name="writeDataPoints"></param>
        /// <param name="readDataPointsDict"></param>
        /// <returns></returns>
        private async Task MainCheckDownInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                // 复位相关标志位
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "PartIDReqDone", false },
                    { "PartOK", false },
                    { "PartNG", false },
                    { "RecipeDone", false },
                    { "MainCheckInErrorCode",0}
                };

                // 写入PLC（使用重试机制）
                int successCount = 0;
                int failedCount = 0;
                List<string> failedParamNames = new List<string>();

                foreach (var dataPoint in writeDataPoints)
                {
                    string paramName = dataPoint.ParamName;
                    if (dataToWrite.TryGetValue(paramName, out var value))
                    {
                        try
                        {
                            // 使用指数退避重试机制写入PLC（包含写入验证）
                            bool writeSuccess = await WriteDataPointWithRetryAsync(plcCode, dataPoint, value);

                            if (writeSuccess)
                            {
                                successCount++;
                                _logger.LogInformation($"写入PLC复位状态成功: ParamName={paramName}, Value={value}");
                            }
                            else
                            {
                                failedCount++;
                                failedParamNames.Add(paramName);
                                _logger.LogError($"写入PLC复位状态失败，已达到最大重试次数: ParamName={paramName}, Value={value}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            failedParamNames.Add(paramName);
                            _logger.LogError(ex, $"写入PLC复位状态异常: ParamName={paramName}, Value={value}");
                        }
                    }
                }

                // 记录汇总信息
                _logger.LogInformation($"主条码下降沿写入数据: 成功={successCount}, 失败={failedCount}, 总计={writeDataPoints.Count}");

                if (failedCount > 0)
                {
                    _logger.LogError($"主条码下降沿写入失败的数据点: {string.Join(", ", failedParamNames)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"主条码下降沿写入PLC失败: PlcCode={plcCode}, StationCode={stationCode}");
            }
        }

        #endregion

        #region 子条码校验 
        /// <summary>
        /// 子条码绑定事件
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleSubCheckAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleSubCheckInAsync(e);
            }
            else
            {
                await HandleSubCheckDownInAsync(e);
            }
        }

        private async Task HandleSubCheckInAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理子条码验证事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }
                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);
                // 第四步：写入PLC（子条码验证使用子条码）
                await SubCheckInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                // OnEventTriggered(e);
                _logger.LogInformation($"子条码验证事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理子条码验证事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 异步写入 PLC
        /// </summary>
        /// <param name="plcCode">PLC 编码</param>
        /// <param name="stationCode">工站编码</param>
        /// <param name="writeDataPoints">写入数据点列表</param>
        /// <param name="readDataPointsDict">读取数据点字典</param>

        private async Task SubCheckInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            string subpartId = CleanPlcString(readDataPointsDict.ContainsKey("SubPartID") ? readDataPointsDict["SubPartID"] : string.Empty);
            //rfid条码
            string partId = CleanPlcString(readDataPointsDict.ContainsKey("PartID") ? readDataPointsDict["PartID"] : string.Empty);
            string partType = CleanPlcString(readDataPointsDict.ContainsKey("PartType") ? readDataPointsDict["PartType"] : string.Empty);
            Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
            string msg = string.Empty;
            try
            {
                var flag = true;
                StationProcessInfo stationProcessInfo = GetStationRecipeCurrent(plcCode, stationCode);
                if (stationProcessInfo==null)
                {
                    msg = $"请先扫描主零件";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                }
                dataToWrite.Add("SubPartDone", true);

                // 获取当前工艺路线该工站需要的零件数量
                int requiredPartCount = 0;
                MD_RoutingList currentStationConfig = stationProcessInfo.RoutingList.FirstOrDefault(o=>o.StationCode==stationCode);

                if (string.IsNullOrEmpty(partId) || string.IsNullOrEmpty(subpartId) || string.IsNullOrEmpty(partType))
                {
                    msg = $"子条码、主条码或型号为空，无法继续处理";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId,msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;

                }

                // 验证采集的型号与当前缓存型号是否一致
                if (!string.Equals(partType, stationProcessInfo.ProductModel, StringComparison.OrdinalIgnoreCase))
                {
                    msg = $"采集型号 [{partType}] 与当前生产型号 [{stationProcessInfo.ProductModel}] 不一致";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                }
                var wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == partId && o.StationCode == stationCode);
                if (wipBarcode == null)
                {
                    msg = $"未找到RFID条码 {partId} 对应的在制品信息";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                }
                // 从型号获取工艺路线
                var productModelInfo = stationProcessInfo.RoutingList.FirstOrDefault(o => o.StationCode == stationCode);
                 if (productModelInfo != null)
                 {

                     // 统计需要扫描的子零件数量
                     if (productModelInfo.IsScanFirst.HasValue && productModelInfo.IsScanFirst.Value)
                         requiredPartCount++;
                     if (productModelInfo.IsScanSecond.HasValue && productModelInfo.IsScanSecond.Value)
                         requiredPartCount++;
                     if (productModelInfo.IsScanThird.HasValue && productModelInfo.IsScanThird.Value)
                         requiredPartCount++;
                    if (productModelInfo.IsScanBatch.HasValue && productModelInfo.IsScanBatch.Value)
                    {
                        requiredPartCount++;
                    }
                    _logger.LogInformation($"当前工站 {stationCode} 需要扫描 {requiredPartCount} 个子零件");
                 }
                 else
                 {
                     msg = $"未找到型号{partType}对应工站{stationCode}的工艺路线配置";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                 }              
                // 验证子零件是否已存在（避免重复扫描）
                if (string.IsNullOrEmpty(msg))
                {
                    var existingCount = await _wipMaterialInfoRepository.QueryByClauseAsync(w => w.RfidCode == partId && w.StationCode == stationCode && w.MaterialCode == subpartId);

                    if (existingCount !=null)
                    {
                        msg = $"WipMaterialInfo 已存在该条码 {partId} 和工站 {stationCode} 对应的子零件 {subpartId}";
                        await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                }
                // 查询已扫描的子零件数量
                int scannedCount = 0;
                if (string.IsNullOrEmpty(msg))
                {
                    scannedCount = await _wipMaterialInfoRepository.GetCountAsync(w => w.RfidCode == partId && w.StationCode == stationCode && w.MaterialType == "SubPart");

                    _logger.LogInformation($"主条码 {partId} 在工站 {stationCode} 已扫描 {scannedCount} 个子零件");
                }

                // 计算当前是第几个零件
                int currentPartSequence = scannedCount + 1;
                var validationResult = await ValidateBarcodeFormat(subpartId, currentPartSequence, stationProcessInfo);
                if (!validationResult.isValid)
                {
                    msg = validationResult.errorMsg;
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                }                
                // 计算当前子零件序号
                int num = scannedCount + 1;

                // 添加数据到字典
                dataToWrite.Add("SubassemblyPoint", num);

                // 判断是否全部完成
                if (num >= requiredPartCount)
                {
                    dataToWrite.Add("SubKittingDone", num);
                    _logger.LogInformation($"子零件扫描完成，共 {num} 个");
                }
                dataToWrite.Add("SubPartOK", true);             
                // 收集并写入PLC数据点
                bool writeSuccess = await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);

                if (!writeSuccess)
                {
                    _logger.LogWarning($"子零件绑定PLC写入部分失败: StationCode={stationCode}, SubPartBarcode={subpartId}");
                    msg = $"子零件绑定PLC写入部分失败: StationCode={stationCode}, SubPartBarcode={subpartId}";
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                }
                // PLC 操作完成后，添加 WipMaterialInfo 记录
                await AddWipMaterialInfoAsync(partId, stationProcessInfo, stationCode, subpartId, num, wipBarcode);                
                // 发布子零件绑定事件到前端监控
                string partName = $"第{num}个子零件";
                if (string.IsNullOrEmpty(msg))
                {
                    DataPushBus.PublishPartBinding(stationCode, partName, subpartId, true, "子零件绑定成功");
                    _logger.LogInformation($"发布子零件绑定成功事件: StationCode={stationCode}, PartName={partName}, Barcode={subpartId}");
                }
                else
                {
                    DataPushBus.PublishPartBinding(stationCode, partName, subpartId, false, msg);
                    _logger.LogWarning($"发布子零件绑定失败事件: StationCode={stationCode}, PartName={partName}, Barcode={subpartId}, Error={msg}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入PLC时发生错误");
                await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
            }
        }
    
        /// <summary>
        /// 子条码校验失败时的处理逻辑
        /// </summary>
        /// <param name="plcCode"></param>
        /// <param name="stationCode"></param>
        /// <param name="partId"></param>
        /// <param name="subpartId"></param>
        /// <param name="msg"></param>
        /// <param name="SubCheckInErrorCode"></param>
        /// <param name="writeDataPoints"></param>
        /// <param name="dataToWrite"></param>
        /// <returns></returns>
        public async Task SuCheckErrorMsg(string plcCode, string stationCode, string partId, string subpartId, string msg, int SubCheckInErrorCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, Object> dataToWrite)
        {
            //直接返回错误
            dataToWrite.Add("SubPartNG", false);           
            dataToWrite.Add("SubCheckInErrorCode", SubCheckInErrorCode);

            _logger.LogInformation($"发布子零件进站校验失败: StationCode={stationCode}, Barcode={partId}, SubpartId={subpartId}, 失败原因={msg}");
            bool isSuccess = await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);
            if (!isSuccess)
            {
                _logger.LogWarning($"子条码校验PLC写入部分失败: StationCode={stationCode}, Barcode={partId}, SubpartId={subpartId}");
            }
        }


        /// <summary>
        /// 子条码进站校验下降沿触发事件处理逻辑
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleSubCheckDownInAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理子条码进站校验下降沿触发事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");
                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：写入PLC
                await SubCheckDownInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"主条码验证事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理主条码验证事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 子条码验证下降沿触发事件写入PLC逻辑
        /// </summary>
        /// <param name="plcCode"></param>
        /// <param name="stationCode"></param>
        /// <param name="writeDataPoints"></param>
        /// <param name="readDataPointsDict"></param>
        /// <returns></returns>
        private async Task SubCheckDownInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                // 复位相关标志位
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "SubPartDone", false },
                    { "SubPartOK", false },
                    { "SubPartNG",false }
                };

                // 写入PLC（使用重试机制）
                int successCount = 0;
                int failedCount = 0;
                List<string> failedParamNames = new List<string>();

                foreach (var dataPoint in writeDataPoints)
                {
                    string paramName = dataPoint.ParamName;
                    if (dataToWrite.TryGetValue(paramName, out var value))
                    {
                        try
                        {
                            // 使用指数退避重试机制写入PLC（包含写入验证）
                            bool writeSuccess = await WriteDataPointWithRetryAsync(plcCode, dataPoint, value);

                            if (writeSuccess)
                            {
                                successCount++;
                                _logger.LogInformation($"写入PLC复位状态成功: ParamName={paramName}, Value={value}");
                            }
                            else
                            {
                                failedCount++;
                                failedParamNames.Add(paramName);
                                _logger.LogError($"写入PLC复位状态失败，已达到最大重试次数: ParamName={paramName}, Value={value}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            failedParamNames.Add(paramName);
                            _logger.LogError(ex, $"写入PLC复位状态异常: ParamName={paramName}, Value={value}");
                        }
                    }
                }

                // 记录汇总信息
                _logger.LogInformation($"子条码下降沿写入数据: 成功={successCount}, 失败={failedCount}, 总计={writeDataPoints.Count}");

                if (failedCount > 0)
                {
                    _logger.LogError($"子条码下降沿写入失败的数据点: {string.Join(", ", failedParamNames)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"子条码下降沿写入PLC失败: PlcCode={plcCode}, StationCode={stationCode}");
            }
        }

        #endregion

        #region   工件不可逆加工事件处理逻辑
        /// <summary>
        /// MES记录工件进入不可逆工站
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleProcessingAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleProcessingUpAsync(e);
            }
            else
            {
                await HandleProcessingDownAsync(e);
            }
        }

        /// <summary>
        /// 加工事件上升沿处理
        /// </summary>
        private async Task HandleProcessingUpAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理加工事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：处理不可逆加工标记
                await HandleProcessingEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"加工事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理加工事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 加工事件下降沿处理（复位标志）
        /// </summary>
        private async Task HandleProcessingDownAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理加工事件(下降沿): EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：复位ProcessingDone标志
                await HandleProcessingDownEvent(e.PlcCode, e.StationCode, writeDataPoints);

                _logger.LogInformation($"加工事件(下降沿)处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理加工事件(下降沿)时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 处理不可逆加工事件
        /// </summary>
        private async Task HandleProcessingEvent(string plcCode, string stationCode,
            List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                string msg = string.Empty;
                // 读取不可逆条码（清理PLC字符串中的特殊字符）
                string processingPartId = CleanPlcString(readDataPointsDict.TryGetValue("ProcessingPartID", out string pid) ? pid : string.Empty);
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                if (string.IsNullOrWhiteSpace(processingPartId))
                {
                    msg = "不可逆条码为空，跳过处理";
                    _logger.LogWarning(msg);
                    dataToWrite.Add("ProcessingDone", false);
                    dataToWrite.Add("ProcessingErrorMsg", msg);
                    dataToWrite.Add("ProcessingErrorCode", 101);                    
                    // 向PLC下发错误状态（带重试机制）
                    await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, ProcessingErrorCode=101");
                    DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, false, msg);
                    return;
                }

                var wipBarcode= await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == processingPartId && o.StationCode == stationCode);
                if (wipBarcode == null)
                {
                    msg = $"条码 {processingPartId} 在工站 {stationCode} 没有在制信息";
                    _logger.LogWarning(msg);
                    // 向PLC下发错误状态（带重试机制）
                    dataToWrite.Add("ProcessingDone", false);
                    dataToWrite.Add("ProcessingErrorMsg", msg);
                    dataToWrite.Add("ProcessingErrorCode", 102);
                    await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, false, msg);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, ProcessingErrorCode=102");
                    return;
                }

                var process= await _wipProcessingIrreversibleRepository.QueryByClauseAsync(o=>o.RFIDCode==processingPartId&&o.StationCode==stationCode);
                if (process != null)
                {
                    msg = $"条码 {processingPartId} 在工站 {stationCode} 已存在不可逆加工记录，跳过处理";
                    _logger.LogWarning(msg);

                    // 向PLC下发错误状态（带重试机制）
                    dataToWrite.Add("ProcessingDone", false);
                    dataToWrite.Add("ProcessingErrorMsg", msg);
                    dataToWrite.Add("ProcessingErrorCode", 102);
                    await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, false, msg);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, ProcessingErrorCode=102");
                    return;
                }
                // 通过工站获取型号和RecipeCode
                var stationProcessInfo = GetStationRecipeCurrent(plcCode, stationCode);
                string productModel = stationProcessInfo?.ProductModel;
                string recipeCode = stationProcessInfo?.Recipe;

                _logger.LogInformation($"不可逆加工标记: BarCode={processingPartId}, ProductModel={productModel}, StationCode={stationCode}, RecipeCode={recipeCode}");

                // 将信息新增到不可逆加工标记表
                var irreversibleRecord = new WipProcessingIrreversible
                {
                    BarCode=wipBarcode.BarCode,
                    RFIDCode = processingPartId,
                    ProductModel = productModel,
                    StationCode = stationCode,
                    RecipeCode = recipeCode,
                    CreateTime = DateTime.Now,
                    CreateUser = "System"
                };
                // 数据获取完成后，下发ProcessingDone=true到PLC
                dataToWrite.Add("ProcessingDone", true);               
                await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, true, "可逆工站已标注成功");
                _logger.LogInformation($"已向PLC下发ProcessingDone=true");
                await _wipProcessingIrreversibleRepository.InsertAsync(irreversibleRecord);
                _logger.LogInformation($"不可逆加工标记记录已保存: BarCode={processingPartId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理不可逆加工事件时发生错误");

                // 向PLC下发错误状态
                try
                {
                    Dictionary<string, object> errorData = new Dictionary<string, object>
                    {
                        { "ProcessingDone", false },
                        { "SubCheckInErrorMsg", ex.Message }
                    };

                    await WriteDataPointsToPlc(plcCode, writeDataPoints, errorData);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, SubCheckInErrorMsg={ex.Message}");
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "向PLC下发错误状态失败");
                }
            }
        }

        /// <summary>
        /// 处理不可逆加工下降沿事件（复位标志）
        /// </summary>
        private async Task HandleProcessingDownEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints)
        {
            try
            {
                // 复位ProcessingDone标志
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "ProcessingDone", false }
                };

                await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                _logger.LogInformation($"已向PLC下发ProcessingDone=false");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理不可逆加工下降沿事件时发生错误");
            }
        }

        #endregion

        #region  PLC主条码出站事件

        /// <summary>
        /// PLC主条码出站事件（合并上升沿/下降沿）
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isRisingEdge">true=上升沿，false=下降沿</param>
        /// <returns></returns>
        private async Task HandleMainCheckOutAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleMainCheckOutUpAsync(e);
            }
            else
            {
                await HandleMainCheckOutDownAsync(e);
            }
        }

        /// <summary>
        /// 主条码出站事件（上升沿）- 数据采集与验证
        /// </summary>
        private async Task HandleMainCheckOutUpAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理主条码出站事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetMainOutEventDataPoints(e.EventId, out var readQualityDataPoints, out var writeDataPoints, out var readResultDataPoints))
                {
                    return;
                }
                // 第三步：质量数据
                Dictionary<string, string> readQualityDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readQualityDataPoints);
                Dictionary<string, string> readResultDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readResultDataPoints);


                // 第四步：主条码过站数据采集与验证
                await ProcessMainBarCodeCheckOut(e.PlcCode, e.StationCode, writeDataPoints, readQualityDataPointsDict, readResultDataPointsDict);

                _logger.LogInformation($"主条码出站事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理主条码出站事件异常: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 主条码出站事件（下降沿）- 复位信号
        /// </summary>
        private async Task HandleMainCheckOutDownAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理主条码出站事件(下降沿): EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：准备回写PLC的数据（下降沿重置信号）
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
                dataToWrite["DataSaveDone"] = false;
                dataToWrite["DataSaveOK"] = false;
                dataToWrite["DataSaveNG"] = false;
                dataToWrite["DataSaveErrCode"] = false;

                _logger.LogInformation($"下降沿回写信号: DataSaveDone=0, DataSaveOK=0, DataSaveErrCode=0");

                // 第四步：写入PLC响应
                await WriteDataPointsToPlc(e.PlcCode, writeDataPoints, dataToWrite);

                _logger.LogInformation($"主条码出站事件(下降沿)处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理主条码出站事件(下降沿)时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 条码过站数据采集与验证
        /// </summary>
        /// <param name="plcCode"></param>
        /// <param name="stationCode">站点</param>
        /// <param name="writeDataPoints">写入地址</param>
        /// <param name="readQualityDataPointsDict">读取质量数据点字典</param>
        /// <param name="readResultDataPointsDict">读取结果数据点字典</param>
        /// <returns></returns>
        private async Task ProcessMainBarCodeCheckOut(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readQualityDataPointsDict, Dictionary<string, string> readResultDataPointsDict)
        {
            try
            {
                // 质量采集数据（清理PLC字符串中的特殊字符）
                string productModel = CleanPlcString(readQualityDataPointsDict.TryGetValue("PartType", out string pt) ? pt : string.Empty);
                string RFIDCode = CleanPlcString(readQualityDataPointsDict.TryGetValue("PartID", out string pid) ? pid : string.Empty);
                string subPartID1 = CleanPlcString(readQualityDataPointsDict.TryGetValue("SubPartID1", out string spid1) ? spid1 : string.Empty);
                string subPartID2 = CleanPlcString(readQualityDataPointsDict.TryGetValue("SubPartID2", out string spid2) ? spid2 : string.Empty);
                string subPartID3 = CleanPlcString(readQualityDataPointsDict.TryGetValue("SubPartID3", out string spid3) ? spid3 : string.Empty);
                int PartResult = readQualityDataPointsDict.TryGetValue("PartResult", out string prStr) && int.TryParse(prStr, out int pr) ? pr : 0;
                string RecipeVer = CleanPlcString(readQualityDataPointsDict.TryGetValue("RecipeVer", out string rv) ? rv : string.Empty);
                int testCode = readQualityDataPointsDict.TryGetValue("TestCode", out string tcStr) && int.TryParse(tcStr, out int tc) ? tc : 0;

                string LineID = CleanPlcString(readResultDataPointsDict.TryGetValue("LineID", out string li) ? li : string.Empty);
                string StationId = CleanPlcString(readResultDataPointsDict.TryGetValue("StationID", out string si) ? si : string.Empty);
                int PalletNo = readResultDataPointsDict.TryGetValue("PalletNo", out string po) && int.TryParse(po, out int por) ? por : 0;
                int PalletType = readResultDataPointsDict.TryGetValue("PalletType", out string strpty)&&int.TryParse(strpty,out int pty) ? pty : 0;
                int PalletStatus = readResultDataPointsDict.TryGetValue("PalletStatus", out string strps) && int.TryParse(strps, out int ps) ? ps : 0;

                // 读取子零件结果数据 (P-Data[1] ~ P-Data[8])
                // P-Data[1]
                int SubResult1 = readResultDataPointsDict.TryGetValue("SubResult1", out string sd1) && int.TryParse(sd1, out int sdr1) ? sdr1 : 0;
                double SubUSL1 = readResultDataPointsDict.TryGetValue("SubUSL1", out string su1) && double.TryParse(su1, out double sur1) ? sur1 : 0;
                double SubResultValue1 = readResultDataPointsDict.TryGetValue("SubResultValue1", out string srv1) && double.TryParse(srv1, out double srvr1) ? srvr1 : 0;
                double SubLSL1 = readResultDataPointsDict.TryGetValue("SubLSL1", out string sl1) && double.TryParse(sl1, out double slr1) ? slr1 : 0;

                // P-Data[2]
                int SubResult2 = readResultDataPointsDict.TryGetValue("SubResult2", out string sd2) && int.TryParse(sd2, out int sdr2) ? sdr2 : 0;
                double SubUSL2 = readResultDataPointsDict.TryGetValue("SubUSL2", out string su2) && double.TryParse(su2, out double sur2) ? sur2 : 0;
                double SubResultValue2 = readResultDataPointsDict.TryGetValue("SubResultValue2", out string srv2) && double.TryParse(srv2, out double srvr2) ? srvr2 : 0;
                double SubLSL2 = readResultDataPointsDict.TryGetValue("SubLSL2", out string sl2) && double.TryParse(sl2, out double slr2) ? slr2 : 0;

                // P-Data[3]
                int SubResult3 = readResultDataPointsDict.TryGetValue("SubResult3", out string sd3) && int.TryParse(sd3, out int sdr3) ? sdr3 : 0;
                double SubUSL3 = readResultDataPointsDict.TryGetValue("SubUSL3", out string su3) && double.TryParse(su3, out double sur3) ? sur3 : 0;
                double SubResultValue3 = readResultDataPointsDict.TryGetValue("SubResultValue3", out string srv3) && double.TryParse(srv3, out double srvr3) ? srvr3 : 0;
                double SubLSL3 = readResultDataPointsDict.TryGetValue("SubLSL3", out string sl3) && double.TryParse(sl3, out double slr3) ? slr3 : 0;

                // P-Data[4]
                int SubResult4 = readResultDataPointsDict.TryGetValue("SubResult4", out string sd4) && int.TryParse(sd4, out int sdr4) ? sdr4 : 0;
                double SubUSL4 = readResultDataPointsDict.TryGetValue("SubUSL4", out string su4) && double.TryParse(su4, out double sur4) ? sur4 : 0;
                double SubResultValue4 = readResultDataPointsDict.TryGetValue("SubResultValue4", out string srv4) && double.TryParse(srv4, out double srvr4) ? srvr4 : 0;
                double SubLSL4 = readResultDataPointsDict.TryGetValue("SubLSL4", out string sl4) && double.TryParse(sl4, out double slr4) ? slr4 : 0;

                // P-Data[5]
                int SubResult5 = readResultDataPointsDict.TryGetValue("SubResult5", out string sd5) && int.TryParse(sd5, out int sdr5) ? sdr5 : 0;
                double SubUSL5 = readResultDataPointsDict.TryGetValue("SubUSL5", out string su5) && double.TryParse(su5, out double sur5) ? sur5 : 0;
                double SubResultValue5 = readResultDataPointsDict.TryGetValue("SubResultValue5", out string srv5) && double.TryParse(srv5, out double srvr5) ? srvr5 : 0;
                double SubLSL5 = readResultDataPointsDict.TryGetValue("SubLSL5", out string sl5) && double.TryParse(sl5, out double slr5) ? slr5 : 0;

                // P-Data[6]
                int SubResult6 = readResultDataPointsDict.TryGetValue("SubResult6", out string sd6) && int.TryParse(sd6, out int sdr6) ? sdr6 : 0;
                double SubUSL6 = readResultDataPointsDict.TryGetValue("SubUSL6", out string su6) && double.TryParse(su6, out double sur6) ? sur6 : 0;
                double SubResultValue6 = readResultDataPointsDict.TryGetValue("SubResultValue6", out string srv6) && double.TryParse(srv6, out double srvr6) ? srvr6 : 0;
                double SubLSL6 = readResultDataPointsDict.TryGetValue("SubLSL6", out string sl6) && double.TryParse(sl6, out double slr6) ? slr6 : 0;

                // P-Data[7]
                int SubResult7 = readResultDataPointsDict.TryGetValue("SubResult7", out string sd7) && int.TryParse(sd7, out int sdr7) ? sdr7 : 0;
                double SubUSL7 = readResultDataPointsDict.TryGetValue("SubUSL7", out string su7) && double.TryParse(su7, out double sur7) ? sur7 : 0;
                double SubResultValue7 = readResultDataPointsDict.TryGetValue("SubResultValue7", out string srv7) && double.TryParse(srv7, out double srvr7) ? srvr7 : 0;
                double SubLSL7 = readResultDataPointsDict.TryGetValue("SubLSL7", out string sl7) && double.TryParse(sl7, out double slr7) ? slr7 : 0;

                // P-Data[8]
                int SubResult8 = readResultDataPointsDict.TryGetValue("SubResult8", out string sd8) && int.TryParse(sd8, out int sdr8) ? sdr8 : 0;
                double SubUSL8 = readResultDataPointsDict.TryGetValue("SubUSL8", out string su8) && double.TryParse(su8, out double sur8) ? sur8 : 0;
                double SubResultValue8 = readResultDataPointsDict.TryGetValue("SubResultValue8", out string srv8) && double.TryParse(srv8, out double srvr8) ? srvr8 : 0;
                double SubLSL8 = readResultDataPointsDict.TryGetValue("SubLSL8", out string sl8) && double.TryParse(sl8, out double slr8) ? slr8 : 0;

                // 读取工步节拍时间 (P-CT_Sub[1] ~ P-CT_Sub[7])
                double CT_Sub1 = readResultDataPointsDict.TryGetValue("CT_Sub1", out string ct1) && double.TryParse(ct1, out double ctr1) ? ctr1 : 0;
                double CT_Sub2 = readResultDataPointsDict.TryGetValue("CT_Sub2", out string ct2) && double.TryParse(ct2, out double ctr2) ? ctr2 : 0;
                double CT_Sub3 = readResultDataPointsDict.TryGetValue("CT_Sub3", out string ct3) && double.TryParse(ct3, out double ctr3) ? ctr3 : 0;
                double CT_Sub4 = readResultDataPointsDict.TryGetValue("CT_Sub4", out string ct4) && double.TryParse(ct4, out double ctr4) ? ctr4 : 0;
                double CT_Sub5 = readResultDataPointsDict.TryGetValue("CT_Sub5", out string ct5) && double.TryParse(ct5, out double ctr5) ? ctr5 : 0;
                double CT_Sub6 = readResultDataPointsDict.TryGetValue("CT_Sub6", out string ct6) && double.TryParse(ct6, out double ctr6) ? ctr6 : 0;
                double CT_Sub7 = readResultDataPointsDict.TryGetValue("CT_Sub7", out string ct7) && double.TryParse(ct7, out double ctr7) ? ctr7 : 0;

                // 总循环时间
                double CT_Total = readResultDataPointsDict.TryGetValue("CT_Total", out string ctt) && double.TryParse(ctt, out double cttr) ? cttr : 0;






                StationProcessInfo stationProcessInfo = GetStationRecipeCurrent(plcCode, stationCode);
                WipProcessingIrreversible wipProcessingIrreversible = null;
                _logger.LogInformation($"采集主条码质量过站数据: ProductMode={productModel}, RFIDCode={RFIDCode}, SubPartID1={subPartID1},SubPartID2={subPartID2},SubPartID3={subPartID3}, PartResult={PartResult}, RecipeVer={RecipeVer}, TestCode={testCode}");
                // 准备写入PLC的数据
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
                string errorMsg = string.Empty;
                // 验证主条码是否为空
                if (string.IsNullOrWhiteSpace(RFIDCode))
                {
                    errorMsg = "RFID主条码为空";
                    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 2, errorMsg);
                    DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                    return;
                }
                //条码不可逆加工验证  待确定是否需要放在这里验证，还是只要在加工事件中记录不可逆加工状态即可

                //wipProcessingIrreversible = await _wipProcessingIrreversibleRepository.QueryByClauseAsync(o => o.RFIDCode == RFIDCode && o.StationCode == stationCode);
                //if (wipProcessingIrreversible != null)
                //{
                //    errorMsg = $"条码 [{RFIDCode}] 已处理，不能重复处理";
                //    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 2, errorMsg);
                //    DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);

                //   _logger.LogError(errorMsg);
                //    return;
                //}               
                //验证是否有在制信息
                 var wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == RFIDCode && o.StationCode == stationCode&&o.BarCodeType=="2");
                if(wipBarcode == null)
                {
                    errorMsg = $"未找到RFID条码 {RFIDCode} 对应的在制品信息";
                    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 101, errorMsg);
                    DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                    _logger.LogError(errorMsg);
                    return;
                }
                // 验证采集的型号与工站当前生产型号是否一致
                if (!string.IsNullOrWhiteSpace(productModel))
                { 
                    errorMsg = $"未上传型号信息";
                    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 101, errorMsg);
                    DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                    _logger.LogError(errorMsg);
                    return;
                }               
                if (stationProcessInfo != null && !string.IsNullOrEmpty(stationProcessInfo.ProductModel))
                {
                    if (!string.Equals(productModel, stationProcessInfo.ProductModel, StringComparison.OrdinalIgnoreCase))
                    {
                        errorMsg = $"采集型号 [{productModel}] 与工站当前生产型号 [{stationProcessInfo.ProductModel}] 不一致";
                        await WriteErrorStatusToPlc(plcCode, writeDataPoints, 101, errorMsg);
                        DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                        _logger.LogError(errorMsg);
                        return;
                    }
                }               
                // 获取该工站需要的零件数量
                int requiredPartCount = 0;
                requiredPartCount = GetRequiredPartCount(stationCode, stationProcessInfo);
                _logger.LogInformation($"工站 {stationCode} 需要 {requiredPartCount} 个子零件");

                // 解析子零件数组
                List<string> collectedSubParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(subPartID1)&&subPartID1!="0")
                {
                    collectedSubParts.Add(subPartID1);
                }
                if (!string.IsNullOrWhiteSpace(subPartID2) && subPartID2 != "0")
                {
                    collectedSubParts.Add(subPartID2);
                }
                if (!string.IsNullOrWhiteSpace(subPartID3) && subPartID3 != "0")
                {
                    collectedSubParts.Add(subPartID3);
                }
                // 验证零件数量是否齐全
                if (collectedSubParts.Count < requiredPartCount)
                {
                    errorMsg = $"零件数量不足，期望 {requiredPartCount} 个，实际 {collectedSubParts.Count} 个";
                    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 2, errorMsg);
                    DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                    _logger.LogWarning(errorMsg);
                    return;
                }
                // 验证零件是否匹配WipMaterialInfo
                var existingSubParts = await _wipMaterialInfoRepository.QueryListByClauseAsync(
                    w => w.RfidCode == RFIDCode && w.StationCode == stationCode && w.MaterialType == "SubPart");
                var subPartCodes = existingSubParts?.Select(w => w.MaterialCode).ToList() ?? new List<string>();

                // 检查采集的零件是否都在WipMaterialInfo中存在
                if (subPartCodes != null&&subPartCodes.Count>0)
                {
                    foreach (var subPart in collectedSubParts)
                    {
                        if (!subPartCodes.Contains(subPart))
                        {
                            errorMsg = $"采集的零件 {subPart} 与系统记录不匹配";
                            await WriteErrorStatusToPlc(plcCode, writeDataPoints, 2, errorMsg);
                            DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                            return;
                        }
                    }
                }
               
                //采集收集参数
                List<PLC_TriggerParam> plcTriggerParams = await _plcTriggerParamRepository.QueryListByClauseAsync(o => o.StationCode == stationCode);
                
                // 从PLC读取参数值并保存到字典
                var collectedParams = new Dictionary<string, string>();
                foreach (var param in plcTriggerParams)
                {
                    try
                    {
                        var paramValue = await ReadPlcParamAsync(plcCode, param.AddressCode, param.DataType);
                        collectedParams[param.ParamName] = paramValue;
                        _logger.LogDebug($"采集参数成功: ParamName={param.ParamName}, AddressCode={param.AddressCode}, Value={paramValue}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"采集参数失败: ParamName={param.ParamName}, AddressCode={param.AddressCode}");
                        collectedParams[param.ParamName] = $"Error: {ex.Message}";
                    }
                }
                // 保存到PLC_CollectParameters主表
                var collectRecord = new PLC_CollectParameters
                {
                    BarCode = wipBarcode.BarCode,
                    RFIDCode = RFIDCode,
                    PlcCode = plcCode,
                    StationCode = stationCode,
                    CollectTime = DateTime.Now,
                    Result = "Success",
                    Message = $"采集{collectedParams.Count}个参数"
                };
                var insertId = await _plcCollectParametersRepository.InsertAsync(collectRecord);
                collectRecord.ParamID = insertId;

                // 保存到PLC_CollectParametersDetail详情表
                var detailList = collectedParams.Select(kv => new PLC_CollectParametersDetail
                {
                    ParamID = collectRecord.ParamID,
                    ParamName = kv.Key,
                    ParamValue = kv.Value
                }).ToList();

                foreach (var detail in detailList)
                {
                    await _plcCollectParametersDetailRepository.InsertAsync(detail);
                }

                _logger.LogInformation($"参数采集保存成功: RFIDCode={RFIDCode}, StationCode={stationCode}, 参数数量={collectedParams.Count}");

                // 通过DataPushBus发布参数采集结果到前端
                DataPushBus.PublishParamUpdated(stationCode, plcCode, collectedParams);
                // 回写PLC：数据采集完成
                dataToWrite["DataSaveDone"] = true;
                dataToWrite["DataSaveOK"] = true;
                dataToWrite["PartResult"] = 1;
                 _logger.LogInformation($"主条码过站验证通过: RFIDCode={RFIDCode}");
                // 验证通过
                // 保存过站记录到WipBarCodeProcess表 结果数据
                var processRecord = new WipBarCodeProcess
                {
                    RfidCode=RFIDCode,
                    BarCode = wipBarcode.BarCode,
                    ProductMode = productModel,
                    RecipeCode = RecipeVer,
                    StationCode = stationCode,
                    FirstSubBarCode = collectedSubParts.Count > 0 ? collectedSubParts[0] : string.Empty,
                    SecondSubBarCode = collectedSubParts.Count > 1 ? collectedSubParts[1] : string.Empty,
                    ThirdSubBarCode = collectedSubParts.Count > 2 ? collectedSubParts[2] : string.Empty,
                    PartResult = PartResult,
                    TestCode = testCode,
                    CreateTime = DateTime.Now,
                    CreateUser = "System"
                };
                await _wipBarCodeProcessRepository.InsertAsync(processRecord);

                var processResultData = new ProcessResultData
                {
                    BarCode = wipBarcode.BarCode,
                    RfidBarcode = RFIDCode,
                    CT_Sub1 = CT_Sub1,
                    CT_Sub2 = CT_Sub2,
                    CT_Sub3 = CT_Sub3,
                    CT_Sub4 = CT_Sub4,
                    CT_Sub5 = CT_Sub5,
                    CT_Sub6 = CT_Sub6,
                    CT_Sub7 = CT_Sub7,
                    CT_Total = CT_Total,
                    LineID = LineID,
                    PalletNo = PalletNo,
                    PalletStatus = PalletStatus,
                    PalletType = PalletType,
                    StationID = stationCode,
                    SubLSL1 = SubLSL1,
                    SubLSL2 = SubLSL2,
                    SubLSL3 = SubLSL3,
                    SubLSL4 = SubLSL4,
                    SubLSL5 = SubLSL5,
                    SubLSL6 = SubLSL6,
                    SubLSL7 = SubLSL7,
                    SubLSL8 = SubLSL8,
                    SubResultValue1 = SubResultValue1,
                    SubResultValue2 = SubResultValue2,
                    SubResultValue3 = SubResultValue3,
                    SubResultValue4 = SubResultValue4,
                    SubResultValue5 = SubResultValue5,
                    SubResultValue6 = SubResultValue6,
                    SubResultValue7 = SubResultValue7,
                    SubResultValue8 = SubResultValue8,
                    ColletTime=DateTime.Now
                };
                await _processResultDataRepository.InsertAsync(processResultData);
                // 更新WipBarCode表
                await UpdateWipBarCodeStatus(RFIDCode, stationCode);
                await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);

                // 写入PLC响应
                DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, true, "主条码过站验证通过");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "主条码过站数据采集处理异常");

                // 向PLC下发错误状态
                try
                {
                    Dictionary<string, object> errorData = new Dictionary<string, object>
                    {
                        { "DataSaveDone", 1 },
                        { "DataSaveNG", 1 },
                        { "DataSaveErrCode", 101 },
                    };

                    await WriteDataPointsToPlc(plcCode, writeDataPoints, errorData);
                    _logger.LogInformation($"向PLC下发错误状态: DataSaveDone=1, DataSaveNG=1, DataSaveErrCode=101, MainCheckInErrorMsg={ex.Message}");
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "向PLC下发错误状态失败");
                }
            }
        }



        #endregion

        #region  返修相关事件

        private async Task HandleRepairUpAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理加工返修上线事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：复位ProcessingDone标志
                await HandleRepairUpEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"返修上线事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理返修上线事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 处理返工上线上升沿事件
        /// </summary>  
        private async Task HandleRepairUpEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                // 复位ProcessingDone标志
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "ProcessingDone", false }
                };

                await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                _logger.LogInformation($"已向PLC下发ProcessingDone=false");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理不可逆加工下降沿事件时发生错误");
            }
        }

        #endregion
      
        #region  标定相关事件
        private async Task HandleCalibraUpDataAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理标定数据事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");
                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }
                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }
                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);
                // 第四步：采集标定数据并回写PLC
                await HandleCalibraUpEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"标定数据事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理返修上线事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 处理校准事件（上升沿）
        /// TODO: 后期需要实现以下功能
        /// 1. 采集校准数据（具体参数待确定）
        /// 2. 保存校准数据到数据库（表结构待确定）
        /// 3. 验证校准数据有效性
        /// 4. 向PLC下发校验结果
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="stationCode">工站编码</param>
        /// <param name="writeDataPoints">写入数据点列表</param>
        /// <param name="readDataPointsDict">读取数据点字典</param>
        private async Task HandleCalibraUpEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                _logger.LogInformation($"开始处理校准事件(上升沿): PlcCode={plcCode}, StationCode={stationCode}");

                // TODO: 后期实现校准数据采集逻辑
                // 需要确定：
                // - 从PLC读取哪些校准参数（如校准值、校准时间、校准人员等）
                // - 数据验证规则
                // - 数据库表结构（如WipCalibration表）
                // - 数据保存逻辑

                // 临时实现：仅复位ProcessingDone标志
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "ProcessingDone", false }
                };

                await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
                _logger.LogInformation($"已向PLC下发ProcessingDone=false");
                _logger.LogWarning($"校准事件处理为临时实现，后期需要实现数据采集和保存功能");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理校准事件时发生错误");
            }
        }
        #endregion

        #region 打印相关事件
        private async Task HandlePrintExchangeAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理打印相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");
                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第四步：采集标定数据并回写PLC
                await HandleCalibraUpEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"打印相关事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"打印相关事件时发生错误: EventId={e.EventId}");
            }
        }
        #endregion

        #region 跳动数据
        private async Task HandleBounceExchangeAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理跳动数据相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");
                // 第一步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode); 
                if (deviceStatus == -1)
                {
                    return;
                }

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第五步：通过Socket通信触发外部设备执行并接收数据
                await TriggerSocketAndReceiveDataAsync(e);
                //读到的数据要与工站进行绑定，然后生成对应的跳动主表


                _logger.LogInformation($"跳动数据事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理跳动数据事件时发生错误: EventId={e.EventId}");
            }
        }

        /// <summary>
        /// 通过Socket通信触发外部设备执行并接收数据
        /// 流程：1. 连接Socket 2. 发送开始执行命令 3. 接收设备发送的所有数据 4. 处理并保存数据
        /// </summary>
        private async Task TriggerSocketAndReceiveDataAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                // 检查Socket服务是否可用
                if (_socketCommunicationService == null)
                {
                    _logger.LogWarning("Socket通讯服务未初始化");
                    return;
                }

                // 如果未连接，尝试连接
                if (!_socketCommunicationService.IsConnected)
                {
                    _logger.LogInformation("Socket未连接，尝试连接...");
                    // 从配置读取IP和端口（这里使用默认值，实际应从配置获取）
                    string socketIp = "127.0.0.1";
                    int socketPort = 8000;
                    
                    bool connected = await _socketCommunicationService.ConnectAsync(socketIp, socketPort);
                    if (!connected)
                    {
                        _logger.LogWarning($"Socket连接失败: {socketIp}:{socketPort}，跳过设备执行");
                        return;
                    }
                    _logger.LogInformation($"Socket连接成功: {socketIp}:{socketPort}");
                }

                // 构建并发送"开始执行"命令
                byte[] startCommand = BuildStartCommand(e);
                if (startCommand == null || startCommand.Length == 0)
                {
                    _logger.LogWarning("无法构建开始执行命令");
                    return;
                }

                _logger.LogInformation($"发送开始执行命令: {Encoding.UTF8.GetString(startCommand)}");
                bool sendSuccess = await _socketCommunicationService.SendDataAsync(startCommand);
                if (!sendSuccess)
                {
                    _logger.LogWarning("发送开始执行命令失败");
                    return;
                }

                // 接收设备执行过程中发送的所有数据
                _logger.LogInformation("等待接收设备执行数据...");
                await ReceiveAndProcessDeviceData(e);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Socket通讯触发设备执行失败");
            }
        }

        /// <summary>
        /// 接收并处理设备发送的数据
        /// 设备执行过程中会发送多条数据，需要持续接收直到设备发送完成信号
        /// </summary>
        private async Task ReceiveAndProcessDeviceData(PlcEventTriggeredEventArgs e)
        {
            int dataCount = 0;

            try
            {
                while (true)
                {
                    // 接收数据（带超时，防止无限等待）
                    byte[] data = await _socketCommunicationService.ReceiveDataAsync();
                    
                    if (data == null || data.Length == 0)
                    {
                        _logger.LogInformation("设备数据接收完成或超时");
                        break;
                    }

                    dataCount++;
                    _logger.LogInformation($"接收到第 {dataCount} 批数据，长度: {data.Length} 字节");

                    // 检查是否为结束信号
                    string dataStr = Encoding.UTF8.GetString(data);
                    if (dataStr.StartsWith("END") || dataStr.StartsWith("COMPLETE"))
                    {
                        _logger.LogInformation("收到设备执行完成信号");
                        break;
                    }

                    // TODO: 数据处理逻辑待完善
                    // 当前传入的数据格式未知，暂时只记录日志，后续根据实际数据格式补充处理逻辑
                    _logger.LogInformation($"收到设备数据，待后续处理: 长度={data.Length}字节");

                    // 添加小延迟，避免过快接收
                    await Task.Delay(100);
                }

                if (dataCount == 0)
                {
                    _logger.LogWarning("未接收到任何设备数据");
                }
                else
                {
                    _logger.LogInformation($"共接收 {dataCount} 批设备数据");
                }
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning($"设备数据接收超时: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理设备数据时发生错误，已接收 {dataCount} 批数据");
            }
        }

        /// <summary>
        /// 构建开始执行命令
        /// </summary>
        private byte[] BuildStartCommand(PlcEventTriggeredEventArgs e)
        {
            try
            {
                // 构建命令：START|PLC编码|工站编码|事件ID|时间戳
                string commandStr = $"START|{e.PlcCode}|{e.StationCode}|{e.EventId}|{DateTime.Now:yyyyMMddHHmmss}";
                return Encoding.UTF8.GetBytes(commandStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "构建开始执行命令失败");
                return null;
            }
        }

        #endregion

        #region  MES在线

        #region 参数下发事件
        private async Task HandleParameterDistributionDownAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理跳动数据相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                // 第二步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                // 第三步：异步读取数据点
                Dictionary<string, string> readDataPointsDict = await ReadEventDataPointsAsync(e.PlcCode, readDataPoints);

                // 第五步：通过Socket通信触发外部设备执行并接收数据
                await MesOnlineParameterDistributionPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);
                //读到的数据要与工站进行绑定，然后生成对应的跳动主表


                _logger.LogInformation($"MES在线参数下发事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MES在线参数下发事件时发生错误: EventId={e.EventId}");
            }
        }
        /// <summary>
        /// MesOnline参数信息保存
        /// </summary>
        /// <param name="plcCode"></param>
        /// <param name="stationCode"></param>
        /// <param name="writeDataPoints"></param>
        /// <param name="readDataPointsDict"></param>
        /// <returns></returns>
        private async Task MesOnlineParameterDistributionPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            //型号
            string partType = CleanPlcString(readDataPointsDict.ContainsKey("PartType") ? readDataPointsDict["PartType"] : string.Empty);
            //程序号
            string recipeVer = CleanPlcString(readDataPointsDict.ContainsKey("RecipeVer") ? readDataPointsDict["RecipeVer"] : string.Empty);

            try
            {
                _logger.LogInformation($"MES在线模式参数下发开始: PlcCode={plcCode}, StationCode={stationCode}, PartType={partType}, RecipeVer={recipeVer}");

                // 1. 根据产品型号编码、工艺参数版本号查询PLC_ParameterDistribution信息
                var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(p => p.ProductModelCode == partType && p.RecipeCode == recipeVer);

                PLC_ParameterDistribution distributionToUse;
                List<PLC_ParameterDistributionDetail> distributionDetails;

                if (parameterDistribution == null)
                {
                    // 2. 如果没有信息，获取PLC_ParameterDistribution表的第一条信息和对应的详细信息
                    _logger.LogInformation($"未找到匹配的参数配置，使用第一条配置: PartType={partType}, RecipeVer={recipeVer}");
                    
                    var allDistributions = await _parameterDistributionRepository.QueryAsync();
                    var firstDistribution = allDistributions?.FirstOrDefault();
                    if (firstDistribution == null)
                    {
                        _logger.LogError($"PLC_ParameterDistribution表为空，无法进行参数下发");
                        return;
                    }

                    // 实例化PLC_ParameterDistribution类，把第一条信息赋值给实例化的对象
                    distributionToUse = new PLC_ParameterDistribution
                    {
                        // ID不赋值（新记录）
                        RecipeCode = firstDistribution.RecipeCode,
                        ProductModelCode = partType,  // ProductModelCode=partType
                        RoutingCode = recipeVer,      // RoutingCode=recipeVer
                        IsEnabled = firstDistribution.IsEnabled,
                        Description = firstDistribution.Description,
                        CreateUser = "System",
                        CreateDate = DateTime.Now
                    };

                    // 根据工站和ID查出来的PLC_ParameterDistributionDetail信息
                    distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(
                        d => d.DistributionID == firstDistribution.ID && d.StationCode == stationCode);

                    if (distributionDetails == null || !distributionDetails.Any())
                    {
                        _logger.LogWarning($"未找到工站{stationCode}的参数下发明细配置");
                        return;
                    }

                    // 遍历PLC_ParameterDistributionDetailList，新建PLC_ParameterDistributionDetail对象A1
                    // 参数ParamValue，通过AddressCode，DataType进行读取
                    List< PLC_ParameterDistributionDetail >list=new List<PLC_ParameterDistributionDetail>();
                    foreach (var detail in distributionDetails)
                    {
                        try
                        {
                            // 通过AddressCode，DataType读取PLC实际参数值
                            string actualValue = ReadPlcData(plcCode, detail.AddressCode, detail.DataType);
                            
                            // 创建新的明细对象
                            var newDetail = new PLC_ParameterDistributionDetail
                            {
                                // ID不赋值（新记录）
                                DistributionID = 0,  // 暂时为0，后续会关联到新创建的Distribution记录
                                StationCode = detail.StationCode,
                                ModelCode = detail.ModelCode,
                                PlcCode = detail.PlcCode,
                                EquipmentCode = detail.EquipmentCode,
                                AddressCode = detail.AddressCode,
                                DataType = detail.DataType,
                                ParamName = detail.ParamName,
                                ParamValue = actualValue,  // 使用从PLC读取的实际值
                                DistributionType = detail.DistributionType,
                                SortOrder = detail.SortOrder,
                                CreateUser = "System",
                                CreateTime = DateTime.Now
                            };
                            list.Add(newDetail);
                            _logger.LogDebug($"读取PLC参数: ParamName={detail.ParamName}, Address={detail.AddressCode}, Value={actualValue}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"读取PLC参数失败: ParamName={detail.ParamName}, Address={detail.AddressCode}");
                        }
                    }

                    // 保存新的参数配置到PLC_ParameterDistribution表
                    await _parameterDistributionRepository.InsertAsync(distributionToUse);
                    _logger.LogInformation($"创建新的参数配置记录: DistributionID={distributionToUse.ID}");

                    // 更新明细的DistributionID并保存
                    foreach (var detail in list)
                    {
                        detail.DistributionID = distributionToUse.ID;
                    }
                    await _parameterDistributionDetailRepository.InsertAsync(list);
                    _logger.LogInformation($"保存参数下发明细: Count={list.Count}");
                }
                else
                {
                    // 3. 如果有信息，对比PLC_ParameterDistributionDetail对应的ParamValue与实际plc的参数值是否一样
                    _logger.LogInformation($"找到匹配的参数配置: DistributionID={parameterDistribution.ID}");
                    
                    distributionToUse = parameterDistribution;
                    distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(
                        d => d.DistributionID == parameterDistribution.ID && d.StationCode == stationCode);

                    if (distributionDetails == null || !distributionDetails.Any())
                    {
                        _logger.LogWarning($"未找到工站{stationCode}的参数下发明细配置");
                        return;
                    }

                    // 检查是否有参数值发生变化
                    bool hasChanges = false;
                    List<PLC_ParameterDistributionDetail> changedDetails = new List<PLC_ParameterDistributionDetail>();

                    foreach (var detail in distributionDetails)
                    {
                        try
                        {
                            // 读取PLC实际参数值
                            string actualValue = ReadPlcData(plcCode, detail.AddressCode, detail.DataType);
                            
                            // 对比ParamValue与实际plc的参数值
                            if (detail.ParamValue != actualValue)
                            {
                                hasChanges = true;
                                _logger.LogInformation($"参数值发生变化: ParamName={detail.ParamName}, 原值={detail.ParamValue}, 新值={actualValue}");
                                
                                // 记录变更前的值
                                changedDetails.Add(new PLC_ParameterDistributionDetail
                                {
                                    ID = detail.ID,
                                    DistributionID = detail.DistributionID,
                                    StationCode = detail.StationCode,
                                    ModelCode = detail.ModelCode,
                                    PlcCode = detail.PlcCode,
                                    EquipmentCode = detail.EquipmentCode,
                                    AddressCode = detail.AddressCode,
                                    DataType = detail.DataType,
                                    ParamName = detail.ParamName,
                                    ParamValue = detail.ParamValue,  // 保存原值
                                    DistributionType = detail.DistributionType,
                                    SortOrder = detail.SortOrder,
                                    UpdateUser = "System",
                                    UpdateTime = DateTime.Now
                                });

                                // 更新ParamValue为实际值
                                detail.ParamValue = actualValue;
                                detail.UpdateUser = "System";
                                detail.UpdateTime = DateTime.Now;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"读取PLC参数失败: ParamName={detail.ParamName}, Address={detail.AddressCode}");
                        }
                    }

                    // 如果有变化，进行更新并记录历史
                    if (hasChanges)
                    {
                        // 创建历史记录（下发前的快照）
                        var history = new PLC_ParameterDistributionHistory
                        {
                            ProductModelCode = partType,
                            RecipeCode = recipeVer,
                            SnapshotTime = DateTime.Now,
                            CreateUser = "System",
                            CreateTime = DateTime.Now
                        };
                        await _parameterDistributionHistoryRepository.InsertAsync(history);
                        
                        // 查询获取实际生成的HistoryID
                        var insertedHistory = await _parameterDistributionHistoryRepository.QueryByClauseAsync(
                            h => h.ProductModelCode == partType && h.RecipeCode == recipeVer && h.SnapshotTime == history.SnapshotTime);
                        long actualHistoryID = insertedHistory?.ID ?? 0;

                        // 创建历史明细记录
                        List<PLC_ParameterDistributionDetailHistory> historyDetails = new List<PLC_ParameterDistributionDetailHistory>();
                        foreach (var changedDetail in changedDetails)
                        {
                            // 读取当前PLC值作为新值
                            string newValue = changedDetail.ParamValue;
                            // 读取实际PLC值
                            string currentValue = ReadPlcData(plcCode, changedDetail.AddressCode, changedDetail.DataType);

                            var historyDetail = new PLC_ParameterDistributionDetailHistory
                            {
                                HistoryID = actualHistoryID,
                                PlcCode = changedDetail.PlcCode,
                                EquipmentCode = changedDetail.EquipmentCode,
                                StationCode = changedDetail.StationCode,
                                AddressCode = changedDetail.AddressCode,
                                DataType = changedDetail.DataType,
                                ParamName = changedDetail.ParamName,
                                OriginalValue = newValue,      // 原值（数据库中的值）
                                NewValue = currentValue,       // 新值（PLC实际值）
                                DistributionType = changedDetail.DistributionType
                            };
                            historyDetails.Add(historyDetail);
                        }
                        await _parameterDistributionDetailHistoryRepository.InsertAsync(historyDetails);
                        _logger.LogInformation($"创建历史记录成功: HistoryID={actualHistoryID}, DetailCount={historyDetails.Count}");

                        // 更新参数下发明细表
                        await _parameterDistributionDetailRepository.UpdateAsync(distributionDetails);
                        _logger.LogInformation($"更新参数下发明细: Count={distributionDetails.Count}");
                    }
                    else
                    {
                        _logger.LogInformation($"参数值未发生变化，无需更新");
                    }
                }

                // 4. 生成对应的下发参数记录PLC_ParameterDistributionRecord和对应的PLC_ParameterDistributionRecordDetail表
                await CreateParameterDistributionRecordAsync(plcCode, stationCode, distributionToUse, distributionDetails, partType, recipeVer);

                _logger.LogInformation($"MES离线模式参数下发完成: PlcCode={plcCode}, StationCode={stationCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MES离线模式参数下发失败: PlcCode={plcCode}, StationCode={stationCode}");
            }
        }

        /// <summary>
        /// 创建参数下发记录
        /// </summary>
        private async Task CreateParameterDistributionRecordAsync(string plcCode, string stationCode, 
            PLC_ParameterDistribution distribution, List<PLC_ParameterDistributionDetail> details, 
            string partType, string recipeVer)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                int successCount = 0;
                int failedCount = 0;
                List<PLC_ParameterDistributionRecordDetail> recordDetails = new List<PLC_ParameterDistributionRecordDetail>();

                //只保存现在的记录，不进行实际的PLC写入操作
                foreach (var detail in details)
                {
                    if (!string.IsNullOrEmpty(detail.ParamName) && !string.IsNullOrEmpty(detail.AddressCode))
                    {
                        try
                        {
                            successCount++;                      
                            var recordDetail = new PLC_ParameterDistributionRecordDetail
                            {
                                RecordID = 0,  // 暂时为0，后续会更新
                                DetailID = detail.ID,
                                PlcCode = plcCode,
                                EquipmentCode = detail.EquipmentCode ?? string.Empty,
                                StationCode = stationCode,
                                AddressCode = detail.AddressCode,
                                DataType = detail.DataType ?? string.Empty,
                                ParamName = detail.ParamName,
                                ParamValue = detail.ParamValue?.ToString() ?? string.Empty,
                                Status = "Success",
                                ErrorMessage = string.Empty,
                                RetryCount = 0,
                                DurationMs = 0,
                                DistributionTime = DateTime.Now
                            };
                            recordDetails.Add(recordDetail);
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            var recordDetail = new PLC_ParameterDistributionRecordDetail
                            {
                                RecordID = 0,
                                DetailID = detail.ID,
                                PlcCode = plcCode,
                                EquipmentCode = detail.EquipmentCode ?? string.Empty,
                                StationCode = stationCode,
                                AddressCode = detail.AddressCode,
                                DataType = detail.DataType ?? string.Empty,
                                ParamName = detail.ParamName,
                                ParamValue = detail.ParamValue?.ToString() ?? string.Empty,
                                Status = "Failed",
                                ErrorMessage = ex.Message,
                                RetryCount = 0,
                                DurationMs = 0,
                                DistributionTime = DateTime.Now
                            };
                            recordDetails.Add(recordDetail);
                        }
                    }
                }

                DateTime endTime = DateTime.Now;
                long durationMs = (long)(endTime - startTime).TotalMilliseconds;

                // 创建下发记录
                string recordCode = $"DR{DateTime.Now:yyyyMMddHHmmssfff}";
                string status = failedCount == 0 ? "Success" : (successCount > 0 ? "PartialSuccess" : "Failed");

                var record = new PLC_ParameterDistributionRecord
                {
                    DistributionID = distribution.ID,
                    RecordCode = recordCode,
                    ProductModelCode = partType,
                    RecipeCode = recipeVer,
                    DistributionType = "Dispatching",
                    PlcCode = plcCode,
                    EquipmentCode = details.FirstOrDefault()?.EquipmentCode ?? string.Empty,
                    StationCode = stationCode,
                    Status = status,
                    TotalCount = details.Count,
                    SuccessCount = successCount,
                    FailedCount = failedCount,
                    Message = status == "Success" ? "参数下发成功" : $"参数下发部分成功，成功{successCount}条，失败{failedCount}条",
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationMs = durationMs,
                    DistributionUser = "System",
                    CreateTime = DateTime.Now,
                    CreateUser = "System"
                };
                await _parameterDistributionRecordRepository.InsertAsync(record);

                // 更新下发记录明细的RecordID
                foreach (var recordDetail in recordDetails)
                {
                    recordDetail.RecordID = record.ID;
                }
                await _parameterDistributionRecordDetailRepository.InsertAsync(recordDetails);

                _logger.LogInformation($"创建下发记录成功: RecordID={record.ID}, Status={status}, Success={successCount}, Failed={failedCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建参数下发记录失败: PlcCode={plcCode}, StationCode={stationCode}");
            }
        }



        #endregion

        #endregion


        /// <summary>
        /// 使用指数退避重试机制写入数据点
        /// </summary>
        private async Task<bool> WriteDataPointWithRetryAsync(string plcCode, PLC_Event_Data_Detail dataPoint, object value, int maxRetries = 3)
        {
            string paramName = dataPoint.ParamName?.Trim();
            string address = dataPoint.DataAddress?.Trim();
            int delayMs = 50;  // 初始延迟50ms

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    // 写入PLC
                    WriteDataPoint(plcCode, dataPoint, value);
                    _logger.LogInformation($"写入PLC: ParamName={paramName}, Address={address}, Value={value}");

                    // 写入后重新读取验证
                    object readBackValue = ReadDataPoint(plcCode, dataPoint);
                    bool writeSuccess = CompareValues(value, readBackValue);

                    if (writeSuccess)
                    {
                        return true;
                    }

                    _logger.LogWarning($"写入验证失败: ParamName={paramName}, Address={address}, 写入值={value}, 读取值={readBackValue}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"第 {retry + 1} 次写入失败: ParamName={paramName}, Address={address}");
                }

                // 如果不是最后一次重试，等待后重试
                if (retry < maxRetries - 1)
                {
                    _logger.LogInformation($"等待 {delayMs}ms 后进行第 {retry + 2} 次重试");
                    await Task.Delay(delayMs);
                    delayMs *= 2;  // 指数退避
                }
            }

            return false;
        }

        /// <summary>
        /// 比较写入值和读取值是否相等
        /// </summary>
        private bool CompareValues(object expected, object actual)
        {
            if (actual == null) return false;

            if (expected is bool boolValue)
            {
                return actual is bool && (bool)actual == boolValue;
            }
            else if (expected is int intValue)
            {
                return actual is int && (int)actual == intValue;
            }
            else if (expected is float floatValue)
            {
                return actual is float && Math.Abs((float)actual - floatValue) < 0.001f;
            }
            else if (expected is string stringValue)
            {
                // 清理读取值中的控制字符和空格后再比较
                string cleanedActual = CleanPlcString(actual as string);
                string cleanedExpected = CleanPlcString(stringValue);
                return string.Equals(cleanedActual, cleanedExpected, StringComparison.Ordinal);
            }
            else
            {
                return actual.Equals(expected);
            }
        }

        /// <summary>
        /// 清理PLC字符串中的控制字符和空格
        /// </summary>
        private string CleanPlcString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            
            // 找到第一个控制字符（ASCII < 32）的位置
            int firstControlIndex = -1;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] < 32)
                {
                    firstControlIndex = i;
                    break;
                }
            }
            
            // 如果找到控制字符，去掉控制字符及其之前的所有内容
            if (firstControlIndex >= 0)
            {
                int startIndex = firstControlIndex + 1;
                if (startIndex < value.Length)
                {
                    // 去掉前面的内容后，再移除剩余的控制字符
                    return new string(value.Substring(startIndex).Where(c => c >= 32).ToArray()).Trim();
                }
                return string.Empty;
            }
            
            // 没有控制字符，直接移除控制字符后trim
            return new string(value.Where(c => c >= 32).ToArray()).Trim();
        }

        /// <summary>
        /// 异步更新或新增 WipBarcode 表数据
        /// </summary>
        /// <param name="barCode">条码</param>
        /// <param name="modelCode">型号</param>
        /// <param name="stationCode">当前工站</param>
        private async Task UpdateWipBarcodeAsync(string barCode, string modelCode, string stationCode, string batchCode, StationProcessInfo stationProcessInfo,string RecipeCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barCode))
                {
                    _logger.LogWarning("条码为空，跳过WipBarcode更新");
                    return;
                }

                var existingBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == barCode);
                DateTime now = DateTime.Now;

                if (existingBarcode == null)
                {
                    // 新增数据
                    var newBarcode = new WipBarCode
                    {
                        RFIDCode = barCode,
                        ModelCode = modelCode, 
                        BarCodeType = "2",
                        Status = 0,
                        RepairCount = 0,
                        PrStationCode = "OP05-1A",
                        StationCode = "OP05-1A",
                        NextStationCode = "OP05-1B",
                        InputTime = now,
                        CreateUser = "system",
                        CreateTime = now,
                        BatchCode = batchCode,
                        RecipeCode = RecipeCode
                    };

                    await _wipBarcodeRepository.InsertAsync(newBarcode);
                    _logger.LogInformation($"新增WipBarcode记录: BarCode={barCode}, ModelCode={modelCode}");
                }
                else
                {
                    // 更新数据
                    string nextStationCode = GetNextStationCode(stationCode, stationProcessInfo);
                    existingBarcode.BarCodeType = "2";
                    existingBarcode.PrStationCode = existingBarcode.StationCode;
                    existingBarcode.StationCode = stationCode;
                    existingBarcode.NextStationCode = nextStationCode;
                    existingBarcode.InputTime = now;
                    existingBarcode.OutputTime = null;
                    existingBarcode.UpdateUser = "system";
                    existingBarcode.UpdateTime = now;

                   await _wipBarcodeRepository.UpdateAsync(existingBarcode);
                    _logger.LogInformation($"更新WipBarcode记录: BarCode={barCode}, StationCode={stationCode}, NextStationCode={nextStationCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新WipBarcode失败: BarCode={barCode}");
            }
        }

        /// <summary>
        /// 获取工艺路线对应的下一个工站
        /// </summary>
        /// <param name="currentStationCode">当前工站</param>
        /// <param name="stationProcessInfo">工站工艺信息</param>
        /// <returns>下一个工站编码，如果未找到则返回空字符串</returns>
        private string GetNextStationCode(string currentStationCode, StationProcessInfo stationProcessInfo)
        {
            try
            {
                if (currentStationCode == "OP45")
                {
                    return "END";
                }
                if (stationProcessInfo?.RoutingList == null || stationProcessInfo.RoutingList.Count == 0)
                {
                    _logger.LogWarning("获取下一个工站失败: RoutingList为空");
                    return string.Empty;
                }

                // 找到当前工站在路由列表中的位置
                var currentRouting = stationProcessInfo.RoutingList
                    .FirstOrDefault(r => r.StationCode == currentStationCode);

                if (currentRouting == null)
                {
                    _logger.LogWarning($"未找到当前工站在工艺路线中: CurrentStation={currentStationCode}");
                    return string.Empty;
                }

                // 找到SortOrder大于当前工站的第一个工站
                var nextRouting = stationProcessInfo.RoutingList
                    .Where(r => r.SortOrder > currentRouting.SortOrder)
                    .OrderBy(r => r.SortOrder)
                    .FirstOrDefault();

                if (nextRouting != null)
                {
                    _logger.LogDebug($"找到下一个工站: 当前工站={currentStationCode}, 下一个工站={nextRouting.StationCode}");
                    return nextRouting.StationCode;
                }
                else
                {
                    _logger.LogInformation($"未找到下一个工站: 当前工站={currentStationCode}，已是最后一个工站");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取下一个工站失败: CurrentStation={currentStationCode}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 转换常量值为对应的数据类型
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="constantValue">常量值</param>
        /// <returns></returns>
        private object ConvertConstantValue(string dataType, string constantValue)
        {
            try
            {
                switch (dataType?.ToLower())
                {
                    case "bool":
                    case "bit":
                        return bool.Parse(constantValue);
                    case "int":
                    case "int16":
                    case "int32":
                        return int.Parse(constantValue);
                    case "float":
                        return float.Parse(constantValue);
                    case "string":
                        return constantValue;
                    default:
                        return constantValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"转换常量值失败: DataType={dataType}, Value={constantValue}");
                return null;
            }
        }

        private void WriteDataPoint(string plcCode, PLC_Event_Data_Detail dataPoint, object value)
        {
            WriteDataPoint(plcCode, dataPoint.DataAddress, dataPoint.DataType, value);
        }

        private void WriteDataPoint(string plcCode, string dataAddress, string dataType, object value)
        {
            try
            {
                switch (dataType?.ToLower())
                {
                    case "bool":
                    case "bit":
                        if (value is bool boolValue)
                        {
                            _plcCommunicationService.Write(plcCode, dataAddress, boolValue);
                        }
                        break;
                    case "int":
                    case "int16":
                    case "int32":
                        if (value is int intValue)
                        {
                            _plcCommunicationService.Write(plcCode, dataAddress, intValue);
                        }
                        break;
                    case "string":
                        if (value is string stringValue)
                        {
                            _plcCommunicationService.Write(plcCode, dataAddress, stringValue);
                        }
                        break;
                    default:
                        _logger.LogWarning($"不支持的数据类型: {dataType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"写入数据点失败: Address={dataAddress}, DataType={dataType}");
            }
        }

        /// <summary>
        /// 异步并行读取多个数据点
        /// </summary>
        private async Task<Dictionary<string, string>> ReadDataPointsAsync(string plcCode, List<PLC_Event_Data_Detail> dataPoints)
        {
            var results = new Dictionary<string, string>();

            // 创建并行读取任务
            var tasks = dataPoints.Select(async dp =>
            {
                try
                {
                    object value = ReadDataPoint(plcCode, dp);
                    return new KeyValuePair<string, string>(dp.ParamName, value?.ToString() ?? string.Empty);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"读取数据点失败: ParamName={dp.ParamName}, Address={dp.DataAddress}");
                    return new KeyValuePair<string, string>(dp.ParamName, string.Empty);
                }
            });

            // 并行等待所有任务完成
            var values = await Task.WhenAll(tasks);

            foreach (var kvp in values)
            {
                results[kvp.Key] = kvp.Value;
            }

            return results;
        }


        /// <summary>
        /// 获取工站需要的零件数量
        /// </summary>
        private int GetRequiredPartCount(string stationCode, StationProcessInfo stationProcessInfo)
        {
            int count = 0;
            var routingInfo = stationProcessInfo.RoutingList.FirstOrDefault(o => o.StationCode == stationCode);
            if (routingInfo != null)
            {
                if (routingInfo.IsScanFirst.HasValue && routingInfo.IsScanFirst.Value) count++;
                if (routingInfo.IsScanSecond.HasValue && routingInfo.IsScanSecond.Value) count++;
                if (routingInfo.IsScanThird.HasValue && routingInfo.IsScanThird.Value) count++;
            }
            return count;
        }

        /// <summary>
        /// 更新WipBarCode表状态
        /// </summary>
        private async Task UpdateWipBarCodeStatus(string RFIDCode, string stationCode)
        {
            try
            {
                var wipBarCode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == RFIDCode && o.StationCode == stationCode);
                if (wipBarCode != null)
                {
                    // 先保存历史记录
                    await _wipBarcodeHistoryRepository.SaveHistoryAsync(wipBarCode, "UpdateStatus");
                    _logger.LogInformation($"保存WipBarCode历史记录: RFIDCode={RFIDCode}, StationCode={stationCode}");

                    wipBarCode.BarCodeType = "0";
                    wipBarCode.OutputTime = DateTime.Now;
                    await _wipBarcodeRepository.UpdateAsync (wipBarCode);
                    _logger.LogInformation($"更新WipBarCode状态: RFIDCode={RFIDCode}, StationCode={stationCode}, BarCodeType=0, OutputTime={wipBarCode.OutputTime}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新WipBarCode状态失败: RFIDCode={RFIDCode}");
            }
        }

        /// <summary>
        /// 通用写入PLC方法
        /// </summary>
        private async Task WriteDataPointsToPlc(string plcCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, object> dataToWrite)
        {
            List<PLC_Event_Data_Detail> collectedDataPoints = new List<PLC_Event_Data_Detail>();

            foreach (var dataPoint in writeDataPoints)
            {
                try
                {
                    string paramName = dataPoint.ParamName?.Trim();
                    string address = dataPoint.DataAddress?.Trim();

                    if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(address))
                    {
                        continue;
                    }

                    if (dataToWrite.ContainsKey(paramName))
                    {
                        object dictValue = dataToWrite[paramName];

                        PLC_Event_Data_Detail clonedDataPoint = new PLC_Event_Data_Detail
                        {
                            EventId = dataPoint.EventId,
                            ParamName = dataPoint.ParamName,
                            DataAddress = dataPoint.DataAddress,
                            DataType = dataPoint.DataType,
                            IOOperation = dataPoint.IOOperation,
                            ConstantValue = dictValue?.ToString(),
                            SortOrder = dataPoint.SortOrder,
                            Length = dataPoint.Length
                        };

                        collectedDataPoints.Add(clonedDataPoint);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"收集数据点失败: ParamName={dataPoint.ParamName}");
                }
            }

            // 按SortOrder排序后写入
            collectedDataPoints = collectedDataPoints.OrderBy(d => d.SortOrder).ToList();

            foreach (var dataPoint in collectedDataPoints)
            {
                try
                {
                    object value = ConvertConstantValue(dataPoint.DataType, dataPoint.ConstantValue);
                    if (value != null)
                    {
                        await WriteDataPointWithRetryAsync(plcCode, dataPoint, value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"写入PLC失败: ParamName={dataPoint.ParamName}");
                }
            }
        }

        private int ReadDeviceStatus(string plcCode, PLC_Address config)
        {
            try
            {
                switch (config.DataType?.ToLower())
                {
                    case "int":
                    case "int16":
                    case "int32":
                        var intResult = _plcCommunicationService.ReadInt32(plcCode, config.AddressCode);
                        return intResult.IsSuccess ? intResult.Content : -1;
                    case "byte":
                        var byteResult = _plcCommunicationService.ReadByte(plcCode, config.AddressCode);
                        return byteResult.IsSuccess ? byteResult.Content : -1;
                    default:
                        var defaultResult = _plcCommunicationService.ReadInt32(plcCode, config.AddressCode);
                        return defaultResult.IsSuccess ? defaultResult.Content : -1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取设备状态失败: PlcCode={plcCode}, Address={config.AddressCode}");
                return -1;
            }
        }

        private object ReadDataPoint(string plcCode, PLC_Event_Data_Detail dataPoint)
        {
            try
            {
                switch (dataPoint.DataType?.ToLower())
                {
                    case "bool":
                    case "bit":
                        var boolResult = _plcCommunicationService.ReadBool(plcCode, dataPoint.DataAddress);
                        return boolResult.IsSuccess ? boolResult.Content : false;
                    case "int":
                    case "int16":
                    case "int32":
                        var intResult = _plcCommunicationService.ReadInt32(plcCode, dataPoint.DataAddress);
                        return intResult.IsSuccess ? intResult.Content : 0;
                    case "float":
                        var floatResult = _plcCommunicationService.ReadFloat(plcCode, dataPoint.DataAddress);
                        return floatResult.IsSuccess ? floatResult.Content : 0.0f;
                    case "string":
                        var stringResult = _plcCommunicationService.ReadString(plcCode, dataPoint.DataAddress, (ushort)(dataPoint.Length ?? 100));
                        return stringResult.IsSuccess ? stringResult.Content : string.Empty;
                    case "byte":
                        var byteResult = _plcCommunicationService.ReadByte(plcCode, dataPoint.DataAddress);
                        return byteResult.IsSuccess? byteResult.Content : 0;
                    default:
                        var defaultResult = _plcCommunicationService.ReadString(plcCode, dataPoint.DataAddress, (ushort)(dataPoint.Length ?? 100));
                        return defaultResult.IsSuccess ? defaultResult.Content : string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取数据点失败: ParamName={dataPoint.ParamName}, Address={dataPoint.DataAddress}");
                return null;
            }
        }


        private bool ReadTriggerBit(string plcCode, string address, string addressType)
        {
            try
            {
                switch (addressType?.ToLower())
                {
                    case "bool":
                    case "bit":
                        var boolResult = _plcCommunicationService.ReadBool(plcCode, address);
                        return boolResult.IsSuccess && boolResult.Content;
                    case "int":
                    case "int16":
                        var intResult = _plcCommunicationService.ReadInt32(plcCode, address);
                        return intResult.IsSuccess && intResult.Content != 0;
                    case "int32":
                        var int32Result = _plcCommunicationService.ReadInt32(plcCode, address);
                        return int32Result.IsSuccess && int32Result.Content != 0;
                    default:
                        var defaultResult = _plcCommunicationService.ReadBool(plcCode, address);
                        return defaultResult.IsSuccess && defaultResult.Content;
                }
            }
            catch
            {
                return false;
            }
        }

        private string BuildEventConfigCacheKey(string plcCode, string stationCode, string triggerAddress, int eventId)
        {
            return $"{plcCode}_{stationCode}_{triggerAddress}_{eventId}";
        }

        private string BuildTriggerStatusCacheKey(string plcCode, string stationCode, string triggerAddress)
        {
            return $"{plcCode}_{stationCode}_{triggerAddress}";
        }

        protected virtual void OnEventTriggered(PlcEventTriggeredEventArgs e)
        {
            EventTriggered?.Invoke(this, e);
        }


        /// <summary>
        /// 检查设备状态是否满足条件（90或83）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="stationCode">工站编码</param>
        /// <returns>设备状态值，如果检查失败返回-1</returns>
        private int CheckDeviceStatus(string plcCode, string stationCode)
        {
            string deviceStatusKey = $"{plcCode}_{stationCode}";
            if (!_deviceStatusCache.TryGetValue(deviceStatusKey, out var deviceStatusConfig))
            {
                _logger.LogError($"未找到设备状态配置: PlcCode={plcCode}, StationCode={stationCode}");
                return -1;
            }

            int deviceStatus = ReadDeviceStatus(plcCode, deviceStatusConfig);
            _logger.LogInformation($"设备状态检查: PlcCode={plcCode}, StationCode={stationCode}, Status={deviceStatus}");

            if (deviceStatus != 90 && deviceStatus != 83)
            {
                _logger.LogWarning($"设备状态不满足条件，跳过后续操作: PlcCode={plcCode}, StationCode={stationCode}, Status={deviceStatus}");
                return -1;
            }

            return deviceStatus;
        }

        /// <summary>
        /// 获取事件数据点配置
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="readDataPoints">输出读取数据点列表</param>
        /// <param name="writeDataPoints">输出写入数据点列表</param>
        /// <returns>是否获取成功</returns>
        private bool GetEventDataPoints(int eventId, out List<PLC_Event_Data_Detail> readDataPoints, out List<PLC_Event_Data_Detail> writeDataPoints)
        {
            readDataPoints = null;
            writeDataPoints = null;

            if (!_eventDetailCache.TryGetValue(eventId, out var eventDetails))
            {
                _logger.LogError($"未找到事件数据点配置: EventId={eventId}");
                return false;
            }

            readDataPoints = eventDetails.Where(d => d.IOOperation?.ToLower() == "read").ToList();
            writeDataPoints = eventDetails.Where(d => d.IOOperation?.ToLower() == "write").ToList();
            _logger.LogInformation($"事件ID {eventId} 共有 {readDataPoints.Count} 个读取数据点, {writeDataPoints.Count} 个写入数据点");

            return true;
        }
        private bool GetMainOutEventDataPoints(int eventId, out List<PLC_Event_Data_Detail> readQualityDataPoints, out List<PLC_Event_Data_Detail> writeDataPoints, out List<PLC_Event_Data_Detail> readReslutDataPoints)
        {
            List<PLC_Event_Data_Detail> readDataPoints = null;
            writeDataPoints = null;
            readReslutDataPoints = null;
            readQualityDataPoints = null;

            if (!_eventDetailCache.TryGetValue(eventId, out var eventDetails))
            {
                _logger.LogError($"未找到事件数据点配置: EventId={eventId}");
                return false;
            }

            readDataPoints = eventDetails.Where(d => d.IOOperation?.ToLower() == "read").ToList();
            writeDataPoints = eventDetails.Where(d => d.IOOperation?.ToLower() == "write").ToList();
            readReslutDataPoints = readDataPoints.Where(d => d.DataCategory?.ToLower() == "result").ToList();
            readQualityDataPoints = readDataPoints.Where(d => d.DataCategory?.ToLower() == "quality").ToList();
            _logger.LogInformation($"事件ID {eventId} 共有 {readDataPoints.Count} 个读取数据点, {writeDataPoints.Count} 个写入数据点");

            return true;
        }


        /// <summary>
        /// 异步读取事件数据点
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="readDataPoints">读取数据点列表</param>
        /// <returns>数据点名称和值的字典</returns>
        private async Task<Dictionary<string, string>> ReadEventDataPointsAsync(string plcCode, List<PLC_Event_Data_Detail> readDataPoints)
        {
            if (readDataPoints == null || readDataPoints.Count == 0)
            {
                _logger.LogInformation("没有需要读取的数据点");
                return new Dictionary<string, string>();
            }

            // 异步并行读取数据点
            Dictionary<string, string> readDataPointsDict = await ReadDataPointsAsync(plcCode, readDataPoints);

            // 记录读取结果日志
            foreach (var kvp in readDataPointsDict)
            {
                _logger.LogDebug($"读取数据点完成: ParamName={kvp.Key}, Value={kvp.Value}");
            }

            return readDataPointsDict;
        }

        private async Task<string> GenerateBarcode(string stationCode,string RecipeCode)
        {
            try
            {
                string routingCode =string.Empty;
                // 从工站当前生产状态获取工艺路线
                var stationRecipe = await _stationRecipeCurrentRepository.QueryByClauseAsync(sr => sr.StationCode == stationCode);

                if (stationRecipe == null || string.IsNullOrEmpty(stationRecipe.RoutingCode))
                {
                    _logger.LogError("未找到工站当前生产状态或工艺路线编码");
                    var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(pd => pd.RecipeCode == RecipeCode);
                    routingCode = parameterDistribution.RoutingCode;
                    if (string.IsNullOrEmpty(routingCode)){
                        _logger.LogError($"未找到默认工艺路线编码: RecipeCode={RecipeCode}");   
                        return string.Empty;
                    }
                }else{
                    routingCode=stationRecipe.RoutingCode;
                }

                // 从数据库获取工艺路线详情
                var routingList = await _routingRepository.QueryListByClauseAsync(r => r.RoutingCode == routingCode);
                var routing = routingList?.FirstOrDefault();

                if (routing == null)
                {
                    _logger.LogError($"未找到工艺路线: RoutingCode={routingCode}");
                    return string.Empty;
                }

                var stationList = await _routingListRepository.QueryListByClauseAsync(s => s.RoutingId == routing.RoutingID && s.StationCode == stationCode);
                var station = stationList?.FirstOrDefault();

                if (station == null)
                {
                    _logger.LogError($"未找到工站: StationCode={stationCode}");
                    return string.Empty;
                }

                string barCodeName = station.MainPartsRule;
                if (string.IsNullOrEmpty(barCodeName))
                {
                    _logger.LogError($"工站未配置条码规则: StationCode={stationCode}");
                    return string.Empty;
                }

                // 从数据库获取条码规则
                var barCodeRuleList = await _barCodeRuleRepository.QueryListByClauseAsync(r => r.BarCodeName == barCodeName);
                var barCodeRule = barCodeRuleList?.FirstOrDefault();

                if (barCodeRule == null)
                {
                    _logger.LogError($"未找到条码规则: BarCodeName={barCodeName}");
                    return string.Empty;
                }

                var barCodeRuleDetailList = await _barCodeRuleListRepository.QueryListByClauseAsync(rl => rl.BarCodeRuleId == barCodeRule.BarCodeRuleId);
                var ruleDetails = barCodeRuleDetailList
                    .Where(rl => rl.BarCodeRuleId == barCodeRule.BarCodeRuleId)
                    .OrderBy(rl => rl.SortOrder)
                    .ToList();

                if (ruleDetails == null || ruleDetails.Count == 0)
                {
                    _logger.LogError($"未找到条码规则明细: BarCodeRuleId={barCodeRule.BarCodeRuleId}");
                    return string.Empty;
                }

                string barcode = string.Empty;
                foreach (var ruleItem in ruleDetails)
                {
                    string segment = await GenerateBarcodeSegmentAsync(ruleItem);
                    barcode += segment;
                }

                return barcode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成条码时发生错误: StationCode={stationCode}");
                return string.Empty;
            }
        }

        private async Task<string> GenerateBarcodeSegmentAsync(MD_BarCodeRuleList ruleItem)
        {
            try
            {
                string segmentType = ruleItem.SegmentType?.Trim().ToUpper();

                if (string.IsNullOrEmpty(segmentType))
                {
                    _logger.LogWarning($"条码规则明细未配置 SegmentType: BarCodeRuleListId={ruleItem.BarCodeRuleListId}");
                    return string.Empty;
                }

                switch (segmentType)
                {
                    case "FIXED":
                        // 固定字符，直接使用 Content 字段
                        return ruleItem.Content?.Trim() ?? string.Empty;

                    case "DATE":
                        // 日期格式，根据 Content 字段的格式生成日期
                        string dateFormat = ruleItem.Content?.Trim() ?? "yyyyMMdd";
                        return DateTime.Now.ToString(dateFormat);

                    case "SEQUENCE":
                        // 流水号，查询当日WipBarCode表中RFIDCode最大流水号+1
                        int length = ruleItem.FixedLength ?? 6;
                        return await GenerateSerialNumberAsync(length);

                    default:
                        _logger.LogWarning($"未知的 SegmentType: {segmentType}");
                        return ruleItem.Content?.Trim() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成条码片段时发生错误: SegmentType={ruleItem.SegmentType}, Content={ruleItem.Content}");
                return string.Empty;
            }
        }

        private async Task<string> GenerateSerialNumberAsync(int length)
        {
            try
            {
                // 查询当日所有RFIDCode
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);
                var todayBarcodes = await _wipBarcodeRepository.QueryListByClauseAsync(
                    o => o.CreateTime >= today && o.CreateTime < tomorrow && !string.IsNullOrEmpty(o.RFIDCode));

                if (todayBarcodes == null || todayBarcodes.Count == 0)
                {
                    // 当日无数据，从1开始
                    return "1".PadLeft(length, '0');
                }

                // 提取所有RFIDCode中的流水号部分（最后length位数字），找出最大值
                long maxSequence = 0;
                foreach (var barcode in todayBarcodes)
                {
                    // 从RFIDCode中提取所有数字字符
                    var digits = new string(barcode.RFIDCode.Where(char.IsDigit).ToArray());
                    if (string.IsNullOrEmpty(digits) || digits.Length < length)
                        continue;

                    // 取最后length位作为流水号
                    string sequenceStr = digits.Substring(digits.Length - length);
                    if (long.TryParse(sequenceStr, out long seq) && seq > maxSequence)
                    {
                        maxSequence = seq;
                    }
                }

                // 最大值+1
                long nextSequence = maxSequence + 1;
                return nextSequence.ToString().PadLeft(length, '0');
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成流水号时发生错误: Length={length}");
                return "1".PadLeft(length, '0');
            }
        }
        #endregion

        #region 资源释放

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region 内部类

        private class EventTriggerConfig
        {
            public int EventId { get; set; }
            public string PlcCode { get; set; }
            public string StationCode { get; set; }
            public string TriggerAddress { get; set; }
            public string TriggerAddressType { get; set; }
            public string EventType { get; set; }
            public string TriggerEdgeType { get; set; }
            public string Description { get; set; }

            public bool IsFallingEdgeTrigger => !string.IsNullOrEmpty(TriggerEdgeType) && TriggerEdgeType.Equals("Falling", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 工站工艺信息类
        /// </summary>
        private class StationProcessInfo
        {
            /// <summary>
            /// PLC编码
            /// </summary>
            public string PlcCode { get; set; }

            /// <summary>
            /// 工站编码
            /// </summary>
            public string StationCode { get; set; }

            /// <summary>
            /// 产品型号
            /// </summary>
            public string ProductModel { get; set; }

            /// <summary>
            /// Recipe编码
            /// </summary>
            public string Recipe { get; set; }

            /// <summary>
            /// 工艺路线编码
            /// </summary>
            public string RoutingCode { get; set; }

            /// <summary>
            /// 工艺路线列表
            /// </summary>
            public List<MD_RoutingList> RoutingList { get; set; }

            /// <summary>
            /// 工站对应的条码规则（主零件）
            /// </summary>
            public MD_BarCodeRule MainBarCodeRule { get; set; }

            /// <summary>
            /// 工站对应的条码规则列表（主零件）
            /// </summary>
            public List<MD_BarCodeRuleList> MainBarCodeRuleList { get; set; }

            /// <summary>
            /// 工站对应的条码规则（子零件1）
            /// </summary>
            public MD_BarCodeRule FirstBarCodeRule { get; set; }

            /// <summary>
            /// 工站对应的条码规则列表（子零件1）
            /// </summary>
            public List<MD_BarCodeRuleList> FirstBarCodeRuleList { get; set; }

            /// <summary>
            /// 工站对应的条码规则（子零件2）
            /// </summary>
            public MD_BarCodeRule SecondBarCodeRule { get; set; }

            /// <summary>
            /// 工站对应的条码规则列表（子零件2）
            /// </summary>
            public List<MD_BarCodeRuleList> SecondBarCodeRuleList { get; set; }

            /// <summary>
            /// 工站对应的条码规则（子零件3）
            /// </summary>
            public MD_BarCodeRule ThirdBarCodeRule { get; set; }

            /// <summary>
            /// 工站对应的条码规则列表（子零件3）
            /// </summary>
            public List<MD_BarCodeRuleList> ThirdBarCodeRuleList { get; set; }

            /// <summary>
            /// 缓存更新时间
            /// </summary>
            public DateTime UpdateTime { get; set; }
        }

        #endregion

        #region 条码验证方法

        /// <summary>
        /// 验证条码格式是否符合工艺路线配置的规则
        /// </summary>
        private async Task<(bool isValid, string errorMsg)> ValidateBarcodeFormat(string barcode, int partSequence, StationProcessInfo stationProcessInfo)
        {
            try
            {

                // 获取工站配置
                var stationConfig = stationProcessInfo.RoutingList.FirstOrDefault(rl => rl.StationCode == stationProcessInfo.StationCode);

                if (stationConfig == null)
                {
                    return (false, $"未找到工站 {stationProcessInfo.StationCode} 的配置");
                }

                // 根据零件序号获取对应的条码规则名称
                string ruleName = partSequence switch
                {
                    1 => stationConfig.FirstPartsRule,
                    2 => stationConfig.SecondPartsRule,
                    3 => stationConfig.ThirdPartsRule,
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(ruleName))
                {
                    return (true, string.Empty);
                }

                // 从 MD_BarCodeRule 表获取条码规则
                var barCodeRuleList = await _barCodeRuleRepository.QueryListByClauseAsync(r => r.BarCodeName == ruleName);
                var barCodeRule = barCodeRuleList?.FirstOrDefault();
                // 检查条码规则是否存在，不存在则不验证
                if (barCodeRule == null)
                {
                    return (true, string.Empty);
                }

                // 获取条码规则明细
                var ruleDetailsList = await _barCodeRuleListRepository.QueryListByClauseAsync(r => r.BarCodeRuleId == barCodeRule.BarCodeRuleId);
                var ruleDetails = ruleDetailsList
                    .Where(r => r.BarCodeRuleId == barCodeRule.BarCodeRuleId)
                    .OrderBy(r => r.SortOrder)
                    .ToList();

                if (ruleDetails.Count == 0)
                {
                    return (false, $"条码规则 {ruleName} 未配置明细");
                }

                // 验证条码格式
                int currentPosition = 0;
                foreach (var detail in ruleDetails)
                {
                    if (string.IsNullOrEmpty(detail.SegmentType))
                        continue;

                    int segmentLength = detail.FixedLength ?? (detail.Content?.Length ?? 0);
                    if (segmentLength <= 0)
                        continue;

                    // 检查剩余长度是否足够
                    if (currentPosition + segmentLength > barcode.Length)
                    {
                        return (false, $"条码长度不足，期望长度：{barCodeRule.BarCodeLength}，实际长度：{barcode.Length}");
                    }

                    string segment = barcode.Substring(currentPosition, segmentLength);

                    // 根据 SegmentType 验证
                    switch (detail.SegmentType.ToUpper())
                    {
                        case "FIXED":
                            // 固定内容验证
                            if (!string.IsNullOrEmpty(detail.Content) && segment != detail.Content)
                            {
                                return (false, $"条码第 {currentPosition + 1} 位固定内容不匹配，期望：{detail.Content}，实际：{segment}");
                            }
                            break;

                        case "DATE":
                            // 日期格式验证，使用配置的日期格式
                            string expectedFormat = detail.Content?.Trim() ?? "yyyyMMdd";
                            if (!DateTime.TryParseExact(segment, expectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                return (false, $"条码第 {currentPosition + 1} 位日期格式不正确，期望格式：{expectedFormat}，实际：{segment}");
                            }
                            break;

                        case "SEQUENCE":
                            // 流水号验证（指定位数，必须为数字）
                            if (!long.TryParse(segment, out _))
                            {
                                return (false, $"条码第 {currentPosition + 1} 位流水号格式不正确：{segment}");
                            }
                            break;
                    }

                    currentPosition += segmentLength;
                }

                // 验证总长度
                if (barcode.Length != barCodeRule.BarCodeLength)
                {
                    return (false, $"条码长度不正确，期望：{barCodeRule.BarCodeLength}，实际：{barcode.Length}");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                string errorMsg = $"条码验证异常：{ex.Message}";
                _logger.LogError(ex, errorMsg);
                return (false, errorMsg);
            }
        }

        /// <summary>
        /// 添加 WipMaterialInfo 记录
        /// </summary>
        private async Task AddWipMaterialInfoAsync(string RFIDcode, StationProcessInfo stationProcessInfo, string stationCode, string subpartBarcode, int scanOrder, WipBarCode wipBarcode)
        {
            try
            {
                // 获取当前工站的条码规则名称
                string PartsName = string.Empty;

                var stationConfig = stationProcessInfo.RoutingList.FirstOrDefault(rl => rl.StationCode == stationCode);
                if (stationConfig != null)
                {
                    PartsName = scanOrder switch
                    {
                        1 => stationConfig.FirstPartsName,
                        2 => stationConfig.SecondPartsName,
                        3 => stationConfig.ThirdPartsName,
                        _ => string.Empty
                    };
                }
                var wipMaterialInfo = new WipMaterialInfo
                {
                    RfidCode = RFIDcode,
                    MaterialCode = subpartBarcode,
                    MaterialType = "SubPart",
                    StationCode = stationCode,
                    MaterialName = PartsName,
                    CreateUser = "system",
                    CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await _wipMaterialInfoRepository.InsertAsync(wipMaterialInfo);

                if(stationCode== "OP05-1B"&& scanOrder==1)
                {
                    //主条码是盖子，对应第二个子零件条码是盖盖
                    wipBarcode.BarCode = subpartBarcode;
                    await _wipBarcodeRepository.UpdateAsync(wipBarcode);
                    
                    //更新OP05-A站点的WipBarCodeProcess，把BarCode赋值成为subpartBarcode
                    var wipBarCodeProcessList = await _wipBarCodeProcessRepository.QueryListByClauseAsync(o => o.RfidCode == RFIDcode && o.StationCode == "OP05-1A");
                    if (wipBarCodeProcessList != null && wipBarCodeProcessList.Any())
                    {
                        foreach (var process in wipBarCodeProcessList)
                        {
                            process.BarCode = subpartBarcode;
                            await _wipBarCodeProcessRepository.UpdateAsync(process);
                        }
                        _logger.LogInformation($"更新WipBarCodeProcess成功: RFIDCode={RFIDcode}, StationCode={stationCode}, BarCode={subpartBarcode}");
                    }
                    
                    //更新PLC_CollectParameters，把BarCode赋值成为subpartBarcode
                    var plcCollectParamsList = await _plcCollectParametersRepository.QueryListByClauseAsync(o => o.RFIDCode == RFIDcode && o.StationCode == "OP05-1A");
                    if (plcCollectParamsList != null && plcCollectParamsList.Any())
                    {
                        foreach (var param in plcCollectParamsList)
                        {
                            param.BarCode = subpartBarcode;
                            await _plcCollectParametersRepository.UpdateAsync(param);
                        }
                        _logger.LogInformation($"更新PLC_CollectParameters成功: RFIDCode={RFIDcode}, StationCode={stationCode}, BarCode={subpartBarcode}");
                    }
                    
                    //更新WipBarcodeHistory，把BarCode赋值成为subpartBarcode
                    var wipBarcodeHistoryList = await _wipBarcodeHistoryRepository.QueryListByClauseAsync(o => o.RFIDCode == RFIDcode && o.StationCode == "OP05-1A");
                    if (wipBarcodeHistoryList != null && wipBarcodeHistoryList.Any())
                    {
                        foreach (var history in wipBarcodeHistoryList)
                        {
                            history.BarCode = subpartBarcode;
                            await _wipBarcodeHistoryRepository.UpdateAsync(history);
                        }
                        _logger.LogInformation($"更新WipBarcodeHistory成功: RFIDCode={RFIDcode}, StationCode={stationCode}, BarCode={subpartBarcode}");
                    }
                }


                _logger.LogInformation($"添加 WipMaterialInfo 记录成功:RFIDCode={RFIDcode}, MaterialCode={subpartBarcode}, StationCode={stationCode}, ScanOrder={scanOrder}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"添加 WipMaterialInfo 记录失败：RFIDCode={RFIDcode}, MaterialCode={subpartBarcode}");
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 收集并写入PLC数据点（公共方法）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="writeDataPoints">写入数据点列表</param>
        /// <param name="dataToWrite">待写入数据字典</param>
        /// <returns>是否全部写入成功</returns>
        public async Task<bool> CollectAndWriteDataPointsAsync(string plcCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, object> dataToWrite)
        {
            // 新建集合收集需要写入PLC的数据点
            List<PLC_Event_Data_Detail> collectedDataPoints = new List<PLC_Event_Data_Detail>();

            foreach (var dataPoint in writeDataPoints)
            {
                try
                {
                    string paramName = dataPoint.ParamName?.Trim();
                    string address = dataPoint.DataAddress?.Trim();

                    if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(address))
                    {
                        continue;
                    }

                    // 如果dataToWrite字典中包含该ParamName，则将对应的ConstantValue进行赋值
                    if (dataToWrite.ContainsKey(paramName))
                    {
                        object dictValue = dataToWrite[paramName];

                        // 克隆数据点并设置ConstantValue
                        PLC_Event_Data_Detail clonedDataPoint = new PLC_Event_Data_Detail
                        {
                            EventId = dataPoint.EventId,
                            ParamName = dataPoint.ParamName,
                            DataAddress = dataPoint.DataAddress,
                            DataType = dataPoint.DataType,
                            IOOperation = dataPoint.IOOperation,
                            ConstantValue = dictValue?.ToString(),
                            SortOrder = dataPoint.SortOrder,
                            Length = dataPoint.Length
                        };

                        collectedDataPoints.Add(clonedDataPoint);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"收集数据点失败: ParamName={dataPoint.ParamName}, Address={dataPoint.DataAddress}");
                }
            }

            // 收集完成后统一写入PLC
            int successCount = 0;
            int failedCount = 0;
            List<string> failedParamNames = new List<string>();

            foreach (var dataPoint in collectedDataPoints)
            {
                string paramName = dataPoint.ParamName?.Trim();
                string address = dataPoint.DataAddress?.Trim();

                try
                {
                    object value = ConvertConstantValue(dataPoint.DataType, dataPoint.ConstantValue);
                    if (value != null)
                    {
                        // 使用指数退避重试机制写入PLC（包含写入验证）
                        bool writeSuccess = await WriteDataPointWithRetryAsync(plcCode, dataPoint, value);

                        if (writeSuccess)
                        {
                            successCount++;
                            _logger.LogInformation($"写入成功: ParamName={paramName}, Address={address}, Value={value}");
                        }
                        else
                        {
                            failedCount++;
                            failedParamNames.Add(paramName);
                            _logger.LogError($"写入失败，已达到最大重试次数: ParamName={paramName}, Address={address}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    failedParamNames.Add(paramName);
                    _logger.LogError(ex, $"写入数据点失败: ParamName={paramName}, Address={address}");
                }
            }

            // 记录汇总信息
            _logger.LogInformation($"PLC写入完成: 成功={successCount}, 失败={failedCount}, 总计={collectedDataPoints.Count}");

            if (failedCount > 0)
            {
                _logger.LogError($"失败的数据点: {string.Join(", ", failedParamNames)}");
            }

            // 如果没有失败的数据点，返回true
            return failedCount == 0;
        }

        /// <summary>
        /// 向PLC下发错误状态（公共方法）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="writeDataPoints">写入数据点列表</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorMsg">错误信息</param>
        public async Task WriteErrorStatusToPlc(string plcCode, List<PLC_Event_Data_Detail> writeDataPoints, int errorCode, string errorMsg)
        {
            Dictionary<string, object> dataToWrite = new Dictionary<string, object>
            {
                { "DataSaveDone", true },
                { "DataSaveNG", true },
                { "DataSaveErrCode", errorCode },
                { "PartResult", 2 },

                //{ "DataSaveErrMsg", errorMsg }
            };

            await WriteDataPointsToPlc(plcCode, writeDataPoints, dataToWrite);
            _logger.LogWarning($"向PLC下发错误状态: DataSaveDone=true, DataSaveNG=true, DataSaveErrCode={errorCode}, DataSaveErrMsg={errorMsg}");
        }
    }
}