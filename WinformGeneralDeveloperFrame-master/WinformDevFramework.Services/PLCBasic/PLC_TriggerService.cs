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
using System.Threading.Channels;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Basic.Repository;
using WinformDevFramework.IRepository;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Models.PLCBasic;
using System.Text;
using Newtonsoft.Json;
using System.Drawing.Text;
using WinformDevFramework.Core.Configuration;

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
        private readonly IWipProcessDataRepository _wipProcessDataRepository;
        private readonly IPLC_CalibraCollectRepository _calibraCollectRepository;


        // ============ 心跳服务集成 ============

        /// <summary>
        /// PLC心跳服务（用于复用连接状态检查）
        /// </summary>
        private readonly IPlcHeartbeatService _plcHeartbeatService;

        /// <summary>
        /// PLC连接状态缓存（从心跳服务获取）
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _plcOnlineStatus = new ConcurrentDictionary<string, bool>();

        private readonly ConcurrentDictionary<string, bool> _lastPlcOfflineLogged = new ConcurrentDictionary<string, bool>();

        private readonly IPLC_BatchCheckHandler _batchCheckHandler;
        private readonly IPLC_SubCheckHandler _subCheckHandler;
        private readonly IPLC_ProcessingHandler _processingHandler;
        private readonly IPLC_CalibraDataHandler _calibraDataHandler;
        private readonly IPLC_BounceHandler _bounceHandler;
        private readonly IPLC_ParameterDistributionHandler _parameterDistributionHandler;
        private readonly IPLC_PrintHandler _printHandler;
        private readonly IPLC_ScrewHandler _screwHandler;

        private Thread _pollingThread;
        private volatile bool _isRunning;
        private CancellationTokenSource _pollingCancellationToken;
        private int _pollingIntervalMs = 100;

        private readonly ConcurrentDictionary<string, EventTriggerConfig> _eventConfigCache = new ConcurrentDictionary<string, EventTriggerConfig>();
        private readonly ConcurrentDictionary<string, bool> _lastTriggerStatus = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentDictionary<string, PLC_Address> _deviceStatusCache = new ConcurrentDictionary<string, PLC_Address>();
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
        private readonly ConcurrentDictionary<string, PLC_Address> _mesModeAddressCache = new ConcurrentDictionary<string, PLC_Address>();

        /// <summary>
        /// 上次MES模式状态：Key = PlcCode_StationCode，Value = 上次模式状态
        /// </summary>
        private readonly Dictionary<string, bool> _lastMesModeStatus = new Dictionary<string, bool>();

        private readonly ILogger<PLC_TriggerService> _logger;

        // Socket通讯服务（用于与外部设备通信，接收图片数据）
        private readonly ISocketCommunicationService _socketCommunicationService;

        // ============ Channel事件队列（替代信号量，避免事件丢失） ============

        /// <summary>
        /// 事件队列：有界Channel，最多缓存200个待处理事件
        /// Item1=事件参数, Item2=flag(MES在线/离线)
        /// </summary>
        private readonly Channel<(PlcEventTriggeredEventArgs Event, bool Flag)> _eventChannel =
            Channel.CreateBounded<(PlcEventTriggeredEventArgs, bool)>(new BoundedChannelOptions(200)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        

        /// <summary>
        /// Worker消费者任务列表
        /// </summary>
        private readonly List<Task> _eventWorkers = new List<Task>();

        /// <summary>
        /// Worker消费者数量
        /// </summary>
        private const int EventWorkerCount = 14;

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
            IPLC_CalibraCollectRepository calibraCollectRepository, IWipProcessDataRepository wipProcessDataRepository,
            ILogger<PLC_TriggerService> logger,
            IPLC_BatchCheckHandler batchCheckHandler,
            IPLC_SubCheckHandler subCheckHandler,
            IPLC_ProcessingHandler processingHandler,
            IPLC_CalibraDataHandler calibraDataHandler,
            IPLC_BounceHandler bounceHandler,
            IPLC_ParameterDistributionHandler parameterDistributionHandler,
            IPLC_PrintHandler printHandler,
            IPLC_ScrewHandler screwHandler)
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
            _calibraCollectRepository = calibraCollectRepository;
            _wipProcessDataRepository = wipProcessDataRepository;
            _logger = logger;

            _batchCheckHandler = batchCheckHandler;
            _subCheckHandler = subCheckHandler;
            _processingHandler = processingHandler;
            _calibraDataHandler = calibraDataHandler;
            _bounceHandler = bounceHandler;
            _parameterDistributionHandler = parameterDistributionHandler;
            _printHandler = printHandler;
            _screwHandler = screwHandler;

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

        /// <summary>
        /// 检查PLC连接状态（复用心跳服务状态）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>true=已连接，false=未连接</returns>
        private bool CheckPlcConnectionStatus(string plcCode)
        {
            // 优先使用心跳服务的状态缓存
            if (_plcOnlineStatus.TryGetValue(plcCode, out bool isOnline))
            {
                return isOnline;
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
                "CalibraDown_Data", // 标定参数下降沿
                "PrintExchange",    // 打印交换
                "ParameterDistributionDown", // MES条码进站更新参数
                "Bounce",           // 跳动
                "BounceDown",        // 跳动下降沿
                "BitwiseAND",        //压机上升沿
                "BitwiseANDDown",    //压机下降沿
                "ScrewDownRising",   //拧紧上升沿
                "ScrewDownLower",    //拧紧下降沿

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

            // 启动事件处理Worker消费者
            StartEventWorkers();

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

            // 关闭事件队列，等待Worker处理完剩余事件
            _eventChannel.Writer.TryComplete();
            Task.WaitAll(_eventWorkers.ToArray(), 10000);
            _eventWorkers.Clear();
            _logger.LogInformation("事件处理Worker已停止");

            // 停止离线心跳线程
            _heartbeatThread?.Join(5000);
            _logger.LogInformation("离线心跳线程已停止");
        }

        /// <summary>
        /// 启动事件处理Worker消费者
        /// 从Channel队列中读取事件并处理，替代原来的信号量模式
        /// </summary>
        private void StartEventWorkers()
        {
            for (int i = 0; i < EventWorkerCount; i++)
            {
                var workerTask = Task.Run(async () =>
                {
                    await foreach (var (eventData, flag) in _eventChannel.Reader.ReadAllAsync())
                    {
                        try
                        {
                            await HandleEventInternalAsync(eventData, flag);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Worker处理事件异常: EventId={eventData.EventId}, EventType={eventData.EventType}");
                        }
                    }
                });
                _eventWorkers.Add(workerTask);
            }
            _logger.LogInformation($"已启动 {EventWorkerCount} 个事件处理Worker");
        }

        public void ReloadConfig()
        {
            try
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

                    LoadDeviceStatusConfig(config.PlcCode, config.StationCode);
                    
                    // 加载MES模式地址配置（全局配置，每个PLC只有一个）
                    LoadMesModeConfig(config.PlcCode);
                }

                LoadStationRecipeCurrent();

                _logger.LogInformation($"事件触发配置加载完成，共 {_eventConfigCache.Count} 个触发点，{_deviceStatusCache.Count} 个设备状态配置，{_mesModeCache.Count} 个MES模式配置");
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
                    _mesModeCache[plcCode] = false;
                    _lastMesModeStatus[plcCode] = false;
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
                if (!CheckPlcConnectionStatus(plcCode))
                {
                    return false;
                }

                if (!_mesModeAddressCache.TryGetValue(plcCode, out var addressConfig))
                {
                    _logger.LogWarning($"未找到MES模式地址配置: PlcCode={plcCode}");
                    return false;
                }

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
            _logger.LogInformation($"离线心跳线程已启动，间隔: {HeartbeatIntervalMs}ms，等待配置加载...");

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
                List<string> mesModeKeys = _mesModeAddressCache.Keys.ToList();

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
                        continue;
                    }

                    // 3. 检查PLC连接状态
                    if (!CheckPlcConnectionStatus(plcCode))
                    {
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
           // PerformanceLogger.Log("TriggerService.PollingLoop", "Status", "Started");
            //PerformanceLogger.Log("TriggerService.PollingLoop", "PollingIntervalMs", _pollingIntervalMs);

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
                    _logger.LogDebug($"发生异常，轮询超时，继续下一轮");
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
                    _logger.LogDebug($"发生异常，轮询超时，继续下一轮");
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
                        await Task.Delay(_pollingIntervalMs * 2);
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
                List<string> mesModeKeys = _mesModeAddressCache.Keys.ToList();

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
                    //_logger.LogDebug("[MES模式检查] 存在MES离线PLC，执行定时任务轮询");
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
            List<EventTriggerConfig> configsToCheck = _eventConfigCache.Values.ToList();

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

           // _logger.LogDebug($"[触发检查] 开始，模式={(flag ? "MES在线" : "MES离线")}，PLC分组数: {plcGroups.Count}，触发点总数: {configsToCheck.Count}");

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

         //   _logger.LogDebug($"[触发检查] 完成，耗时: {stopwatch.ElapsedMilliseconds}ms");

            // 如果处理时间超过轮询间隔的50%，记录警告
            //if (stopwatch.ElapsedMilliseconds > _pollingIntervalMs * 0.5)
            //{
            //    _logger.LogWarning($"[触发检查] 耗时 {stopwatch.ElapsedMilliseconds}ms，超过轮询间隔 {_pollingIntervalMs}ms 的50%，建议优化");
            //}
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
               // _logger.LogDebug($"[PLC处理] 开始处理 PLC={plcCode}, 触发点数={configs.Count}, 模式={(flag ? "MES在线" : "MES离线")}");

                // 检查连接状态（复用心跳服务状态）
                if (!CheckPlcConnectionStatus(plcCode))
                {
                    _lastPlcOfflineLogged.TryGetValue(plcCode, out bool wasOffline);
                    if (!wasOffline)
                    {
                        _logger.LogWarning($"[PLC处理] PLC {plcCode} 未连接，等待心跳服务处理");
                        _lastPlcOfflineLogged[plcCode] = true;
                    }
                    stopwatch.Stop();
                    PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "Status", "Offline");
                    PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ProcessingTimeMs", stopwatch.ElapsedMilliseconds);
                    return;
                }

                _lastPlcOfflineLogged.TryUpdate(plcCode, false, true);

                // 心跳服务说在线，继续处理批量读取
                var boolConfigs = configs.Where(c => 
                    string.Equals(c.TriggerAddressType, "bool", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.TriggerAddressType, "bit", StringComparison.OrdinalIgnoreCase)).ToList();

                PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BoolTriggerCount", boolConfigs.Count);

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
                        //_logger.LogDebug($"[PLC处理] PLC {plcCode} 开始批量读取 {uniqueAddresses.Count} 个唯一地址");
                        bool[] results = await _plcCommunicationService.ReadBatchAsync(plcCode, uniqueAddresses);
                        batchStopwatch.Stop();

                        PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BatchReadTimeMs", batchStopwatch.ElapsedMilliseconds);
                        PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "BatchReadCount", uniqueAddresses.Count);

                        //_logger.LogDebug($"[PLC处理] PLC {plcCode} 批量读取完成，耗时={batchStopwatch.ElapsedMilliseconds}ms");

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
                            _lastTriggerStatus.TryGetValue(statusCacheKey, out bool lastStatus);

                            // 处理该地址的所有事件配置（使用相同的 lastStatus）
                            foreach (var config in group)
                            {
                                ProcessTriggerStatusWithSharedLastStatus(config, currentStatus, lastStatus, flag);
                                triggerCount++;
                            }

                            // 所有事件处理完后，统一更新缓存
                            _lastTriggerStatus[statusCacheKey] = currentStatus;
                        }
                    }
                }

                stopwatch.Stop();

                // 记录详细的性能指标
                //PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ProcessingTimeMs", stopwatch.ElapsedMilliseconds);
                //PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "TriggerProcessedCount", triggerCount);
                //PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "ErrorCount", errorCount);
                //PerformanceLogger.Log($"TriggerService.ProcessPlcGroup.{plcCode}", "Status", "Success");

                // 如果处理时间超过50ms，记录详细信息
                if (stopwatch.ElapsedMilliseconds > 50)
                {
                    _logger.LogInformation($"[PLC处理] PLC {plcCode} 处理完成: 触发点={triggerCount}, 错误={errorCount}, 耗时={stopwatch.ElapsedMilliseconds}ms");
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

                // 写入Channel队列，由Worker消费者处理（替代Task.Run+信号量）
                _eventChannel.Writer.TryWrite((triggerEvent, flag));
            }
        }

        /// <summary>
        /// 内部事件处理方法（Channel队列模式，无信号量限流）
        /// </summary>
        private async Task HandleEventInternalAsync(PlcEventTriggeredEventArgs e, bool flag)
        {
            if (!IsEventAllowed(e.EventType))
            {
                var remainingTime = GetCircuitBreakerRemainingTime(e.EventType);
                _logger.LogWarning($"事件[{e.EventType}]已熔断，拒绝处理，剩余熔断时间: {remainingTime}ms");
                return;
            }

            bool eventHandledSuccessfully = false;

            if (!flag)
            {
                switch (e.EventType)
                {
                    case "BatchCheckIn":
                    case "BatchCheckOut":
                        await _batchCheckHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "MainCheckIn":
                    case "MainCheckDownIn":
                        await HandleMainCheckInAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "SubCheckIn":
                    case "SubCheckDownIn":
                        await _subCheckHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "MainCheckOut":
                    case "MainCheckDownOut":
                        await HandleMainCheckOutAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "Processing":
                    case "ProcessDowning":
                        await _processingHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "Calibra_Data":
                    case "CalibraDown_Data":
                        await _calibraDataHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "PrintExchange":
                        await _printHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "Bounce":
                    case "BounceDown":
                        await _bounceHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "ScrewDownRising":
                    case "ScrewDownLower":
                        await _screwHandler.HandleAsync(e, e.IsRisingEdge);
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
                    case "ParameterDistributionDown":
                        await _parameterDistributionHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "Bounce"://跳动
                    case "BounceDown":
                        await _bounceHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                    case "BitwiseAND"://位压
                    case "BitwiseANDDown"://位压
                        eventHandledSuccessfully = true;
                        break;
                    case "ScrewDownRising":
                    case "ScrewDownLower":
                        await _screwHandler.HandleAsync(e, e.IsRisingEdge);
                        eventHandledSuccessfully = true;
                        break;
                }
            }

            if (eventHandledSuccessfully)
            {
                RecordEventSuccess(e.EventType);
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
                    case "byte":
                        var  byteResult= _plcCommunicationService.ReadByte(plcCode, address);
                        return byteResult.IsSuccess ? byteResult.Content.ToString() : string.Empty;
                    case "double":
                        var doubleResult = _plcCommunicationService.ReadDouble(plcCode,address);
                        return doubleResult.IsSuccess ? doubleResult.Content.ToString() : string.Empty;
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

                // 第一步：获取事件数据点配置
                if (!GetEventDataPoints(e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    string msg = $"未找到该事件{e.EventId}的数据点配置";
                    _logger.LogError(msg);
                    return;
                }
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
                // 第二步：检查设备状态
                int deviceStatus = CheckDeviceStatus(e.PlcCode, e.StationCode);
                
                if (deviceStatus == -1)
                {
                    string msg = "当前设备状态不为83，90，不满足生产条件";
                    await ErrorMsg(e.PlcCode, e.StationCode, "", "", 103, writeDataPoints, dataToWrite);
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
                //提前下发参数
                bool preReleaseReq= readDataPointsDict.ContainsKey("preReleaseReq") ? bool.Parse(readDataPointsDict["preReleaseReq"]) : false;

                if (preReleaseReq)
                {
                    if (string.IsNullOrWhiteSpace(RecipeVer) || RecipeVer=="0")
                    {
                        msg = $"提前下发参数触发，{stationCode}工站未传递程序号信息";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, partId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    //根据程序号和工站号获取参数下发配置
                    var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(pd => pd.RecipeCode == RecipeVer);
                    if (parameterDistribution == null)
                    {
                        msg = $"提前下发参数：未找到程序号{RecipeVer}的参数下发配置";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, partId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    var distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(o => o.DistributionID == parameterDistribution.ID && o.StationCode == stationCode);
                    if (distributionDetails == null || !distributionDetails.Any())
                    {
                        msg = $"提前下发参数：未找到程序号{RecipeVer}工站{stationCode}的下发详情配置";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, partId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    var startTime = DateTime.Now;
                    
                    // 1. 创建下发批次记录
                    var record = new PLC_ParameterDistributionRecord
                    {
                        DistributionID = parameterDistribution.ID,
                        RecordCode = $"DR{DateTime.Now:yyyyMMddHHmmss}",
                        ProductModelCode = parameterDistribution.ProductModelCode,
                        RecipeCode = RecipeVer,
                        DistributionType = "Dispatching",
                        PlcCode = plcCode,
                        StationCode = stationCode,
                        Status = "Pending",
                        StartTime = startTime,
                        DistributionUser = "System",
                        CreateTime = startTime,
                        CreateUser = "System"
                    };
                    await _parameterDistributionRecordRepository.InsertAsync(record);
                    
                    // 2. 保存下发前参数快照到历史表
                    var history = new PLC_ParameterDistributionHistory
                    {
                        RecordID = record.ID,
                        ProductModelCode = parameterDistribution.ProductModelCode,
                        RecipeCode = RecipeVer,
                        StationCode = stationCode,
                        SnapshotTime = startTime,
                        CreateTime = startTime,
                        CreateUser = "System"
                    };
                    await _parameterDistributionHistoryRepository.InsertAsync(history);
                    
                    // 批量读取PLC当前值作为快照
                    var historyDetails = new List<PLC_ParameterDistributionDetailHistory>();
                    foreach (var detail in distributionDetails)
                    {
                        string originalValue = "读取失败";
                        try
                        {
                            string dataType = detail.DataType?.ToLower();
                            switch (dataType)
                            {
                                case "bool":
                                    var boolResult = _plcCommunicationService.ReadBool(plcCode, detail.AddressCode);
                                    originalValue = boolResult.IsSuccess ? boolResult.Content.ToString() : "读取失败";
                                    break;
                                case "int":
                                case "int16":
                                case "int32":
                                    var intResult = _plcCommunicationService.ReadInt32(plcCode, detail.AddressCode);
                                    originalValue = intResult.IsSuccess ? intResult.Content.ToString() : "读取失败";
                                    break;
                                case "float":
                                case "real":
                                    var floatResult = _plcCommunicationService.ReadFloat(plcCode, detail.AddressCode);
                                    originalValue = floatResult.IsSuccess ? floatResult.Content.ToString("F2") : "读取失败";
                                    break;
                                case "string":
                                    var stringResult = _plcCommunicationService.ReadString(plcCode, detail.AddressCode, 256);
                                    originalValue = stringResult.IsSuccess ? stringResult.Content : "读取失败";
                                    break;
                                default:
                                    var defaultResult = _plcCommunicationService.ReadInt32(plcCode, detail.AddressCode);
                                    originalValue = defaultResult.IsSuccess ? defaultResult.Content.ToString() : "读取失败";
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            msg=$"读取PLC参数失败: PlcCode={plcCode}, Address={detail.AddressCode}, DataType={detail.DataType}, Error={ex.Message}";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        }
                        historyDetails.Add(new PLC_ParameterDistributionDetailHistory
                        {
                            HistoryID = history.ID,
                            PlcCode = plcCode,
                            StationCode = detail.StationCode,
                            AddressCode = detail.AddressCode,
                            DataType = detail.DataType,
                            ParamName = detail.ParamName,
                            OriginalValue = originalValue,
                            NewValue = detail.ParamValue,
                            DistributionType = detail.DistributionType
                        });
                    }
                    if (historyDetails.Any())
                    {
                        await _parameterDistributionDetailHistoryRepository.InsertAsync(historyDetails);
                    }
                    
                    // 3. 下发参数到PLC并记录每个点位的下发结果
                    var recordDetails = new List<PLC_ParameterDistributionRecordDetail>();
                    int successCount = 0;
                    int failedCount = 0;
                    
                    foreach (var detail in distributionDetails)
                    {
                        if (!string.IsNullOrEmpty(detail.AddressCode) && !string.IsNullOrEmpty(detail.ParamValue))
                        {
                            var recordDetail = new PLC_ParameterDistributionRecordDetail
                            {
                                RecordID = record.ID,
                                DetailID = detail.ID,
                                PlcCode = plcCode,
                                StationCode = detail.StationCode,
                                AddressCode = detail.AddressCode,
                                DataType = detail.DataType,
                                ParamName = detail.ParamName,
                                ParamValue = detail.ParamValue,
                                DistributionTime = DateTime.Now
                            };
                            
                            try
                            {
                                WriteDataPoint(plcCode, detail.AddressCode, detail.DataType, detail.ParamValue);
                                recordDetail.Status = "Success";
                                successCount++;
                                _logger.LogInformation($"提前下发参数: {detail.ParamName} = {detail.ParamValue} -> {detail.AddressCode}");
                            }
                            catch (Exception ex)
                            {
                                recordDetail.Status = "Failed";
                                recordDetail.ErrorMessage = ex.Message;
                                failedCount++;
                                _logger.LogWarning(ex, $"提前下发参数失败: {detail.ParamName} -> {detail.AddressCode}");
                            }
                            recordDetails.Add(recordDetail);
                        }
                    }
                    
                    // 4. 保存点位下发结果
                    if (recordDetails.Any())
                    {
                        await _parameterDistributionRecordDetailRepository.InsertAsync(recordDetails);
                    }
                    
                    // 5. 更新批次记录统计信息
                    var endTime = DateTime.Now;
                    record.SuccessCount = successCount;
                    record.FailedCount = failedCount;
                    record.EndTime = endTime;
                    record.DurationMs = (endTime - startTime).Milliseconds;
                    record.Status = failedCount == 0 ? "Success" : (successCount > 0 ? "PartialSuccess" : "Failed");
                    record.Message = failedCount == 0 
                        ? $"提前下发完成: 成功{successCount}条" 
                        : $"提前下发完成: 成功{successCount}条, 失败{failedCount}条";
                    await _parameterDistributionRecordRepository.UpdateAsync(record);
                    
                    // 写入PLC响应
                    dataToWrite.Add("PreReleaseReq", true);
                    dataToWrite.Add("PartType", parameterDistribution.ProductModelCode);
                    dataToWrite.Add("RecipeVer", RecipeVer);
                    await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);
                    _logger.LogInformation($"提前下发参数完成: StationCode={stationCode}, RecipeVer={RecipeVer}, PartType={parameterDistribution.ProductModelCode}, RecordID={record.ID}");
                    return;
                }
                WipBarCode wipBarcode = new WipBarCode();
                //执行返修操作
                if (atRepair)
                {
                   string suPartId = CleanPlcString(readDataPointsDict.ContainsKey("PartID1") ? readDataPointsDict["PartID1"] : string.Empty);
                    if(string.IsNullOrWhiteSpace(suPartId))
                    {
                        msg = $"返修标识触发，{stationCode}工站未传递子条码信息";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, suPartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    //子条码实际是主产品条码，在OP05-1B工站更新得，流程一般是按照RFIDCode进行处理，
                    wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.BarCode == suPartId);
                    if(wipBarcode == null)
                    {
                        msg = $"未找到主条码信息: 子条码={suPartId}";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, suPartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    //创建新RFID条码
                    partId = await GenerateBarcode(stationCode, wipBarcode.RecipeCode);
                    if (string.IsNullOrEmpty(partId))
                    {
                        msg = $"返修功能触发，生成得条码为空，请{wipBarcode.RecipeCode}程序号对应的工艺路线";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, suPartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
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
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            await ErrorMsg(plcCode, stationCode, partId, msg, 103, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        partType = parameterDistribution.ProductModelCode;
                        //重新生成RFID条码
                        if (string.IsNullOrWhiteSpace(partId)|| partId=="0")
                        {
                            partId = await GenerateBarcode(stationCode, RecipeVer);
                            if (string.IsNullOrEmpty(partId))
                            {
                                msg = $"生成的条码为空，请查阅{RecipeVer}程序号对应的工艺路线";
                                await ErrorMsg(plcCode, stationCode, partId, msg, 101, writeDataPoints, dataToWrite);
                                DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                                return;
                            }
                        }
                        else
                        {
                            // 传入RFID时，检查是否是复投情况
                            wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == partId);
                            if (wipBarcode != null && wipBarcode.StationCode == stationCode)
                            {
                                msg = "当前工件已加工，请勿重复加工";
                                LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                                await ErrorMsg(plcCode, stationCode, partId, msg, 102, writeDataPoints, dataToWrite);
                                DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                                return;
                            }
                            // 非复投情况，检查WIP是否存在及下一工站是否匹配
                            if (wipBarcode == null)
                            {
                                msg = $"{stationCode}工站{partId}条码没有在制信息";
                                LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                                await ErrorMsg(plcCode, stationCode, partId, msg, 102, writeDataPoints, dataToWrite);
                                DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                                return;
                            }
                            if (wipBarcode.NextStationCode != stationCode)
                            {
                                msg = $"当前站点{wipBarcode.NextStationCode}与目标站点{stationCode}不一致";
                                LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                                await ErrorMsg(plcCode, stationCode, partId, msg, 101, writeDataPoints, dataToWrite);
                                DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                                return;
                            }
                        }
                    }
                    else
                    {
                        //OP05-1A工站单独处理  型号、条码信息都为空
                        if (string.IsNullOrWhiteSpace(RecipeVer))
                        {
                            msg = $"{stationCode}工站未传递程序版本号";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            _logger.LogInformation($"发布主零件进站校验失败: StationCode={stationCode}, Barcode={partId},失败原因{msg}");
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(partType))
                        {
                            msg = $"{stationCode}工站未传递型号信息";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        if (string.IsNullOrEmpty(partId))
                        {
                            msg = $"{stationCode}工站未传递RFID条码信息";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            await ErrorMsg(plcCode, stationCode, partId, msg, 4, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == partId);
                        if (wipBarcode == null)
                        {
                            msg = $"{stationCode}工站{partId}条码没有在制信息";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            await ErrorMsg(plcCode, stationCode, partId, msg, 5, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        //重新进站的情况，直接返回OK的信号
                        if(wipBarcode.StationCode==stationCode &&wipBarcode.BarCodeType=="2"){
                            
                            dataToWrite.Add("PartType", partType);
                            dataToWrite.Add("RecipeVer", RecipeVer);
                            dataToWrite.Add("PartID", partId);
                            dataToWrite.Add("PartOK", true);
                            dataToWrite.Add("PartIDReqDone", true);
                            msg = $"允许{partId}重新进站{stationCode}";                           
                            await CollectAndWriteDataPointsAsync(plcCode, writeDataPoints, dataToWrite);
                            _logger.LogInformation(msg);
                            return;
                        }
                        // 验证前一站是否已过站（BarCodeType=0表示已过站，1表示未过站）
                        if (wipBarcode.BarCodeType != "0")
                        {
                            msg = $"前一站未过站（BarCodeType={wipBarcode.BarCodeType}），不允许进入{stationCode}工站";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            await ErrorMsg(plcCode, stationCode, partId, msg, 6, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                        if (wipBarcode.NextStationCode != stationCode)
                        {
                            msg = $"当前站点{wipBarcode.NextStationCode}与目标站点{stationCode}不一致";
                            LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                            await ErrorMsg(plcCode, stationCode, partId, msg, 6, writeDataPoints, dataToWrite);
                            DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                            return;
                        }
                    }
                }
                //不可逆信息（正常进站和返修都需要检查）
                var process = await _wipBarCodeProcessRepository.QueryByClauseAsync(o => o.RfidCode == partId && o.StationCode == stationCode);
                if (process != null)
                {
                    msg = $"该条码{partId}在工站{stationCode}有不可逆信息";
                    LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                    await ErrorMsg(plcCode, stationCode, partId, msg, 202, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
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
                    dataToWrite.Add("RecipeDone", false);
                }
                else
                {
                    //进行参数下发
                    // 返修场景：程序号统一来源于WIP表
                    string mbRecipeVer = atRepair ? wipBarcode.RecipeCode : (stationCode == "OP05-1A" ? RecipeVer : wipBarcode.RecipeCode);
                    _logger.LogInformation($"参数版本不一致，开始下发新参数: 当前RecipeVer={RecipeVer}, 目标Recipe={mbRecipeVer}");
                    // 3.1 拿RecipeVer值匹配PLC_ParameterDistribution的RecipeCode
                    var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(pd => pd.RecipeCode == mbRecipeVer);
                    if(parameterDistribution==null)
                    {
                        msg = $"未找到参数下发配置: RecipeCode={mbRecipeVer}";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, partId, msg,103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        return;
                    }
                    var distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(o=>o.DistributionID== parameterDistribution.ID&&o.StationCode==stationCode);
                    if (distributionDetails == null)
                    {
                        msg = $"未找到程序号{mbRecipeVer}该工站{stationCode}的下发详情配置信息";
                        LogReadValuesOnError(plcCode, stationCode, partId, msg, readDataPointsDict);
                        await ErrorMsg(plcCode, stationCode, partId, msg, 103,writeDataPoints, dataToWrite);
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
                    dataToWrite.Add("RecipeDone", true);
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
                dataToWrite.Add("PartOK", true);
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
                        // 3. 更新主条码表的 RFIDCode 字段为新条码 partId
                        wipBarcode.RFIDCode = partId;
                        wipBarcode.InputTime = DateTime.Now;
                        wipBarcode.OutputTime = null;
                        wipBarcode.StationCode = stationCode;
                        wipBarcode.NextStationCode = GetNextStationCode(stationCode,stationProcessInfo);
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
            dataToWrite.Add("PartIDReqDone", false);
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
                    { "MainCheckInErrorCode",0},
                    { "PreReleaseReq",false}
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
                List<string> listscrewdown = new List<string>() { "OP05-2","OP30-1","OP30-2" };
                // 质量采集数据（清理PLC字符串中的特殊字符）
                string productModel = CleanPlcString(readQualityDataPointsDict.TryGetValue("PartType", out string pt) ? pt : string.Empty);
                string RFIDCode = CleanPlcString(readQualityDataPointsDict.TryGetValue("PartID", out string pid) ? pid : string.Empty);
                string subPartID1 = CleanPlcString(readQualityDataPointsDict.TryGetValue("SubPartID1", out string spid1) ? spid1 : string.Empty);
                string subPartID2 = CleanPlcString(readQualityDataPointsDict.TryGetValue("SubPartID2", out string spid2) ? spid2 : string.Empty);
                string subPartID3 = CleanPlcString(readQualityDataPointsDict.TryGetValue("SubPartID3", out string spid3) ? spid3 : string.Empty);
                int PartResult = readQualityDataPointsDict.TryGetValue("PartResult", out string prStr) && int.TryParse(prStr, out int pr) ? pr : 0;
                string RecipeVer = CleanPlcString(readQualityDataPointsDict.TryGetValue("RecipeVer", out string rv) ? rv : string.Empty);
                int testCode = readQualityDataPointsDict.TryGetValue("TestCode", out string tcStr) && int.TryParse(tcStr, out int tc) ? tc : 0;

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
                    LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict);
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
                    LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
                    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 101, errorMsg);
                    DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                    _logger.LogError(errorMsg);
                    return;
                }
                // 验证采集的型号与工站当前生产型号是否一致
                if (string.IsNullOrWhiteSpace(productModel))
                { 
                    errorMsg = $"未上传型号信息";
                    LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
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
                        LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
                        await WriteErrorStatusToPlc(plcCode, writeDataPoints, 101, errorMsg);
                        DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                        _logger.LogError(errorMsg);
                        return;
                    }
                }
                if (listscrewdown.Contains(stationCode))
                {
                    //WipProcessData主表
                    var processData = await _wipProcessDataRepository.QueryByClauseAsync(o => o.BarCode == RFIDCode && o.StationCode == stationCode&&o.CurveTypes=="拧紧");
                    if (processData == null)
                    {
                        errorMsg = $"托盘条码{RFIDCode}没有拧紧数据";
                        LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
                        await WriteErrorStatusToPlc(plcCode, writeDataPoints, 101, errorMsg);
                        DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                        _logger.LogError(errorMsg);
                        return;
                    }
                }
                if ("OP10" == stationCode)
                {
                    //WipProcessData主表
                    var processData = await _wipProcessDataRepository.QueryByClauseAsync(o => o.BarCode == RFIDCode && o.StationCode == stationCode && o.CurveTypes == "跳动");
                    if (processData == null)
                    {
                        errorMsg = $"托盘条码{RFIDCode}在{stationCode}工站没有跳动数据";
                        LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
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
                    LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
                    await WriteErrorStatusToPlc(plcCode, writeDataPoints, 102, errorMsg);
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
                            LogReadValuesOnError(plcCode, stationCode, RFIDCode, errorMsg, readQualityDataPointsDict, readResultDataPointsDict);
                            await WriteErrorStatusToPlc(plcCode, writeDataPoints, 2, errorMsg);
                            DataPushBus.PublishMainPartStationCheck(stationCode, RFIDCode, false, errorMsg);
                            return;
                        }
                    }
                }               
                //采集收集参数
                List<PLC_TriggerParam> plcTriggerParams = await _plcTriggerParamRepository.QueryListByClauseAsync(o => o.StationCode == stationCode &&(o.TreatmentType =="MSA" ||o.TreatmentType== "collect"));

                // 从PLC读取参数值并保存到字典
                if (plcTriggerParams!=null)
                {
                    var collectedParams = new Dictionary<string, string>();
                    foreach (var param in plcTriggerParams)
                    {
                        try
                        {
                            var paramValue = await ReadPlcParamAsync(plcCode, param.AddressCode, param.DataType);
                            collectedParams[param.ParamName] = paramValue;
                        }
                        catch (Exception ex)
                        {
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

                    // 保存到PLC_CollectParametersDetail详情表（批量插入）
                    var detailList = collectedParams.Select(kv => new PLC_CollectParametersDetail
                    {
                        ParamID = collectRecord.ParamID,
                        ParamName = kv.Key,
                        ParamValue = kv.Value
                    }).ToList();

                    if (detailList.Count > 0)
                    {
                        await _plcCollectParametersDetailRepository.InsertAsync(detailList);
                    }
                    // 通过DataPushBus发布参数采集结果到前端
                    DataPushBus.PublishParamUpdated(stationCode, plcCode, collectedParams);
                }
                // 回写PLC：数据采集完成
                dataToWrite["DataSaveDone"] = true;
                dataToWrite["DataSaveOK"] = true;
                dataToWrite["PartResult"] = PartResult;
                if (PartResult == 2)
                {
                    dataToWrite["DataSaveErrCode"] = 1;
                }
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
                // 更新WipBarCode表（PartResult=0未作时BarCodeType=1，否则=0已过站）
                await UpdateWipBarCodeStatus(RFIDCode, stationCode, PartResult);
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
        /// 错误时记录读取的PLC值，方便后续追踪问题
        /// </summary>
        private void LogReadValuesOnError(string plcCode, string stationCode, string partId, string errorMsg, Dictionary<string, string> readDataPointsDict)
        {
            if (readDataPointsDict == null || readDataPointsDict.Count == 0)
                return;

            var sb = new StringBuilder();
            sb.AppendLine($"[错误追踪] PlcCode={plcCode}, StationCode={stationCode}, PartId={partId}");
            sb.AppendLine($"[错误追踪] 错误信息: {errorMsg}");
            sb.AppendLine($"[错误追踪] PLC读取值:");
            foreach (var kvp in readDataPointsDict)
            {
                sb.AppendLine($"  {kvp.Key} = {kvp.Value}");
            }
            _logger.LogError(sb.ToString());
        }

        /// <summary>
        /// 错误时记录两组PLC读取值，方便后续追踪问题
        /// </summary>
        private void LogReadValuesOnError(string plcCode, string stationCode, string partId, string errorMsg,
            Dictionary<string, string> qualityDict, Dictionary<string, string> resultDict)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[错误追踪] PlcCode={plcCode}, StationCode={stationCode}, PartId={partId}");
            sb.AppendLine($"[错误追踪] 错误信息: {errorMsg}");

            if (qualityDict != null && qualityDict.Count > 0)
            {
                sb.AppendLine($"[错误追踪] 质量采集数据:");
                foreach (var kvp in qualityDict)
                {
                    sb.AppendLine($"  {kvp.Key} = {kvp.Value}");
                }
            }

            if (resultDict != null && resultDict.Count > 0)
            {
                sb.AppendLine($"[错误追踪] 结果采集数据:");
                foreach (var kvp in resultDict)
                {
                    sb.AppendLine($"  {kvp.Key} = {kvp.Value}");
                }
            }

            _logger.LogError(sb.ToString());
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
                    existingBarcode.BatchCode = batchCode;

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
        private async Task UpdateWipBarCodeStatus(string RFIDCode, string stationCode, int partResult = 1)
        {
            try
            {
                var wipBarCode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == RFIDCode && o.StationCode == stationCode);
                if (wipBarCode != null)
                {
                    // 先保存历史记录
                    await _wipBarcodeHistoryRepository.SaveHistoryAsync(wipBarCode, "UpdateStatus");
                    _logger.LogInformation($"保存WipBarCode历史记录: RFIDCode={RFIDCode}, StationCode={stationCode}");

                    // PartResult=1合格时BarCodeType=0已过站，0未作和2不合格时BarCodeType=1未过站
                    wipBarCode.BarCodeType = partResult == 1 ? "0" : "1";
                    wipBarCode.OutputTime = DateTime.Now;
                    await _wipBarcodeRepository.UpdateAsync (wipBarCode);
                    _logger.LogInformation($"更新WipBarCode状态: RFIDCode={RFIDCode}, StationCode={stationCode}, BarCodeType={wipBarCode.BarCodeType}, PartResult={partResult}, OutputTime={wipBarCode.OutputTime}");
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
            _logger.LogWarning($"向PLC下发错误状态: DataSaveDone=true, DataSaveNG=true, DataSaveErrCode={errorCode}");
        }
    }
}