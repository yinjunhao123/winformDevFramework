using Microsoft.Extensions.Logging;
using PLCBasic;
using PLCBasic.IRepository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.IServices.PLCBasic;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC实时参数采集服务
    /// 定时采集TreatmentType为real类型的参数，每5分钟采集一次，保存到PLC_RealTimeParamCollect表
    /// </summary>
    public class PlcRealTimeParamCollectService : IPlcRealTimeParamCollectService, IDisposable
    {
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_TriggerParamRepository _triggerParamRepository;
        private readonly IPLC_RealTimeParamCollectRepository _realTimeParamCollectRepository;
        private readonly IPLC_ParameterLimitRepository _parameterLimitRepository;
        private readonly IPLC_AddressRepository _addressRepository;
        private readonly ILogger<PlcRealTimeParamCollectService> _logger;

        private volatile bool _isRunning;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _collectTask;

        /// <summary>
        /// 采集间隔（毫秒），默认5分钟
        /// </summary>
        private readonly int _collectIntervalMs = 5 * 60 * 1000;

        /// <summary>
        /// 缓存的采集参数配置（按PLC+工站分组）
        /// Key: PlcCode_StationCode, Value: 参数列表
        /// </summary>
        private readonly ConcurrentDictionary<string, List<PLC_TriggerParam>> _cachedRealTimeParams = new ConcurrentDictionary<string, List<PLC_TriggerParam>>();

        /// <summary>
        /// 设备状态地址缓存
        /// Key: PlcCode_StationCode, Value: 设备状态地址配置
        /// </summary>
        private readonly ConcurrentDictionary<string, PLC_Address> _cachedDeviceStatusAddresses = new ConcurrentDictionary<string, PLC_Address>();

        /// <summary>
        /// 允许采集的设备状态值
        /// </summary>
        private static readonly int[] AllowedDeviceStatusValues = { 90, 83, 100, 70 };

        /// <summary>
        /// PLC在线状态缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _plcOnlineStatus = new ConcurrentDictionary<string, bool>();

        public bool IsRunning => _isRunning;

        public PlcRealTimeParamCollectService(
            IPlcCommunicationService plcCommunicationService,
            IPLC_TriggerParamRepository triggerParamRepository,
            IPLC_RealTimeParamCollectRepository realTimeParamCollectRepository,
            IPLC_ParameterLimitRepository parameterLimitRepository,
            IPLC_AddressRepository addressRepository,
            ILogger<PlcRealTimeParamCollectService> logger)
        {
            _plcCommunicationService = plcCommunicationService;
            _triggerParamRepository = triggerParamRepository;
            _realTimeParamCollectRepository = realTimeParamCollectRepository;
            _parameterLimitRepository = parameterLimitRepository;
            _addressRepository = addressRepository;
            _logger = logger;

            // 订阅PLC状态变更事件
            DataPushBus.PlcStatusChanged += DataPushBus_PlcStatusChanged;
        }
        //实时数据采集
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            LoadCache();
            _collectTask = CollectDataLoopAsync();
            _logger?.LogInformation("PLC实时参数采集服务已启动，采集间隔: 5分钟");
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();

            try
            {
                _collectTask?.Wait(5000);
            }
            catch
            {
            }

            _logger?.LogInformation("PLC实时参数采集服务已停止");
        }

        public void ReloadConfig()
        {
            LoadCache();
            _logger?.LogInformation("PLC实时参数采集服务配置已重新加载");
        }

        /// <summary>
        /// 加载缓存数据
        /// </summary>
        private void LoadCache()
        {
            try
            {
                // 加载TreatmentType为real的参数配置
                var realTimeParams = _triggerParamRepository
                    .QueryListByClause(p => p.TreatmentType == "real")
                    ?.ToList() ?? new List<PLC_TriggerParam>();

                // 按PLC+工站分组
                var grouped = realTimeParams
                    .GroupBy(p => $"{p.PlcCode}_{p.StationCode}")
                    .ToDictionary(g => g.Key, g => g.ToList());

                _cachedRealTimeParams.Clear();
                foreach (var kvp in grouped)
                {
                    _cachedRealTimeParams.TryAdd(kvp.Key, kvp.Value);
                }

                // 加载设备状态地址配置（Category为DeviceStatus）
                var deviceStatusAddresses = _addressRepository
                    .QueryListByClause(a => a.Category == "DeviceStatus")
                    ?.ToList() ?? new List<PLC_Address>();

                _cachedDeviceStatusAddresses.Clear();
                foreach (var addr in deviceStatusAddresses)
                {
                    var key = $"{addr.PlcCode}_{addr.StationCode}";
                    _cachedDeviceStatusAddresses.TryAdd(key, addr);
                }

                _logger?.LogInformation($"已加载 {realTimeParams.Count} 条实时参数配置，{deviceStatusAddresses.Count} 条设备状态地址配置");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "加载实时参数配置缓存失败");
            }
        }

        /// <summary>
        /// 采集数据循环
        /// </summary>
        private async Task CollectDataLoopAsync()
        {
            while (_isRunning && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await CollectAllDataAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "PLC实时参数采集异常");
                }

                // 等待下一个采集周期
                try
                {
                    await Task.Delay(_collectIntervalMs, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 采集所有数据
        /// </summary>
        private async Task CollectAllDataAsync()
        {
            if (_cachedRealTimeParams.IsEmpty)
            {
                _logger?.LogDebug("没有需要采集的实时参数配置");
                return;
            }

            var collectTime = DateTime.Now;
            var tasks = _cachedRealTimeParams.Select(kvp => CollectFromGroupAsync(kvp.Key, kvp.Value, collectTime));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 从一组PLC+工站采集数据
        /// </summary>
        private async Task CollectFromGroupAsync(string groupKey, List<PLC_TriggerParam> parameters, DateTime collectTime)
        {
            try
            {
                if (parameters == null || parameters.Count == 0) return;

                // 获取PLC编码（从第一个参数获取）
                var plcCode = parameters.First().PlcCode;
                var stationCode = parameters.First().StationCode;

                // 检查PLC是否在线
                bool isOnline = _plcOnlineStatus.TryGetValue(plcCode, out var online) && online;
                if (!isOnline)
                {
                    _logger?.LogDebug($"PLC {plcCode} 离线，跳过实时参数采集");
                    return;
                }

                // 检查设备状态是否允许采集
                var deviceStatusKey = $"{plcCode}_{stationCode}";
                if (!_cachedDeviceStatusAddresses.TryGetValue(deviceStatusKey, out var statusAddress))
                {
                    _logger?.LogDebug($"PLC {plcCode} 工站 {stationCode} 未配置设备状态地址，跳过采集");
                    return;
                }

                var deviceStatus = await ReadPlcValueAsync(plcCode, statusAddress.AddressCode, statusAddress.DataType);
                if (!deviceStatus.HasValue || !AllowedDeviceStatusValues.Contains((int)deviceStatus.Value))
                {
                    _logger?.LogDebug($"PLC {plcCode} 工站 {stationCode} 设备状态 {deviceStatus} 不在允许采集范围内(90/83/100/70)，跳过采集");
                    return;
                }

                // 按参数基础名称分组（如：温度、温度上限、温度下限 → 温度）
                // 按ParamID排序后，基础参数名依次对应SubResult1~6
                var paramGroups = GroupParametersByName(parameters);

                // 创建采集记录
                var record = new PLC_RealTimeParamCollect
                {
                    PlcCode = plcCode,
                    StationCode = stationCode,
                    CollectTime = collectTime
                };

                // 采集每个参数组（最多6组）
                int groupIndex = 1;
                foreach (var group in paramGroups)
                {
                    if (groupIndex > 6) break;

                    var paramName = group.Key;
                    var paramList = group.Value;

                    // 从PLC采集值、上限、下限、结果
                    double? value = null;
                    double? usl = null;
                    double? lsl = null;
                    int? result = null;

                    foreach (var param in paramList)
                    {
                        var plcValue = await ReadPlcValueAsync(plcCode, param.AddressCode, param.DataType);

                        if (param.ParamName == paramName)
                        {
                            value = plcValue;
                        }
                        else if (param.ParamName.EndsWith("上限") || param.ParamName.EndsWith("USL"))
                        {
                            usl = plcValue;
                        }
                        else if (param.ParamName.EndsWith("下限") || param.ParamName.EndsWith("LSL"))
                        {
                            lsl = plcValue;
                        }
                        else if (param.ParamName.EndsWith("结果") || param.ParamName.EndsWith("Result"))
                        {
                            result = plcValue.HasValue ? (int)plcValue.Value : (int?)null;
                        }
                    }

                    // 设置对应索引的结果
                    SetSubResult(record, groupIndex, paramName, value, usl, lsl, result);
                    groupIndex++;
                }

                // 保存到数据库
                await _realTimeParamCollectRepository.InsertAsync(record);
                _logger?.LogDebug($"PLC {plcCode} 工站 {stationCode} 实时参数采集完成，共 {paramGroups.Count} 组参数");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"采集实时参数失败: GroupKey={groupKey}");
            }
        }

        /// <summary>
        /// 按参数基础名称分组
        /// 例如：温度、温度上限、温度下限 → 分组为"温度"
        /// 按ParamID排序，确保基础参数名按ID顺序依次对应SubResult1~6
        /// </summary>
        private Dictionary<string, List<PLC_TriggerParam>> GroupParametersByName(List<PLC_TriggerParam> parameters)
        {
            var groups = new Dictionary<string, List<PLC_TriggerParam>>();

            // 先按ParamID排序，确保分组顺序一致
            foreach (var param in parameters.OrderBy(p => p.ParamID))
            {
                var baseName = GetBaseParamName(param.ParamName);

                if (!groups.ContainsKey(baseName))
                {
                    groups[baseName] = new List<PLC_TriggerParam>();
                }

                groups[baseName].Add(param);
            }

            return groups;
        }

        /// <summary>
        /// 获取参数基础名称（去除"上限"、"下限"、"结果"、"USL"、"LSL"、"Result"后缀）
        /// </summary>
        private string GetBaseParamName(string paramName)
        {
            if (string.IsNullOrEmpty(paramName)) return paramName;

            // 去除常见后缀
            string[] suffixes = { "上限", "下限", "结果", "USL", "LSL", "Result" };
            foreach (var suffix in suffixes)
            {
                if (paramName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return paramName.Substring(0, paramName.Length - suffix.Length).Trim();
                }
            }

            return paramName;
        }

        /// <summary>
        /// 读取PLC值
        /// </summary>
        private async Task<double?> ReadPlcValueAsync(string plcCode, string addressCode, string dataType)
        {
            try
            {
                if (string.IsNullOrEmpty(addressCode)) return null;

                switch (dataType?.ToLower())
                {
                    case "int":
                    case "int32":
                        var intResult = await Task.Run(() => _plcCommunicationService.ReadInt32(plcCode, addressCode));
                        return intResult.IsSuccess ? (double?)intResult.Content : null;

                    case "int16":
                    case "short":
                        var shortResult = await Task.Run(() => _plcCommunicationService.ReadInt32(plcCode, addressCode));
                        return shortResult.IsSuccess ? (double?)(short)shortResult.Content : null;

                    case "float":
                    case "double":
                        var floatResult = await Task.Run(() => _plcCommunicationService.ReadFloat(plcCode, addressCode));
                        return floatResult.IsSuccess ? (double?)floatResult.Content : null;

                    case "bool":
                        var boolResult = await Task.Run(() => _plcCommunicationService.ReadBool(plcCode, addressCode));
                        return boolResult.IsSuccess ? (double?)(boolResult.Content ? 1 : 0) : null;

                    default:
                        // 默认尝试读取Int32
                        var defaultResult = await Task.Run(() => _plcCommunicationService.ReadInt32(plcCode, addressCode));
                        return defaultResult.IsSuccess ? (double?)defaultResult.Content : null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"读取PLC值失败: PlcCode={plcCode}, Address={addressCode}, DataType={dataType}");
                return null;
            }
        }

        /// <summary>
        /// 设置分项结果
        /// </summary>
        private void SetSubResult(PLC_RealTimeParamCollect record, int index, string paramName, double? value, double? usl, double? lsl, int? result)
        {
            double resultValue = value ?? 0;
            double uslValue = usl ?? 0;
            double lslValue = lsl ?? 0;
            int resultInt = result ?? 0;

            switch (index)
            {
                case 1:
                    record.ParamName1 = paramName;
                    record.SubResult1 = resultInt;
                    record.SubResultValue1 = resultValue;
                    record.SubUSL1 = uslValue;
                    record.SubLSL1 = lslValue;
                    break;
                case 2:
                    record.ParamName2 = paramName;
                    record.SubResult2 = resultInt;
                    record.SubResultValue2 = resultValue;
                    record.SubUSL2 = uslValue;
                    record.SubLSL2 = lslValue;
                    break;
                case 3:
                    record.ParamName3 = paramName;
                    record.SubResult3 = resultInt;
                    record.SubResultValue3 = resultValue;
                    record.SubUSL3 = uslValue;
                    record.SubLSL3 = lslValue;
                    break;
                case 4:
                    record.ParamName4 = paramName;
                    record.SubResult4 = resultInt;
                    record.SubResultValue4 = resultValue;
                    record.SubUSL4 = uslValue;
                    record.SubLSL4 = lslValue;
                    break;
                case 5:
                    record.ParamName5 = paramName;
                    record.SubResult5 = resultInt;
                    record.SubResultValue5 = resultValue;
                    record.SubUSL5 = uslValue;
                    record.SubLSL5 = lslValue;
                    break;
                case 6:
                    record.ParamName6 = paramName;
                    record.SubResult6 = resultInt;
                    record.SubResultValue6 = resultValue;
                    record.SubUSL6 = uslValue;
                    record.SubLSL6 = lslValue;
                    break;
            }
        }

        /// <summary>
        /// PLC状态变更事件处理
        /// </summary>
        private void DataPushBus_PlcStatusChanged(object sender, PlcStatusChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrEmpty(e.PlcCode))
            {
                _plcOnlineStatus[e.PlcCode] = e.IsOnline;
            }
        }

        public void Dispose()
        {
            DataPushBus.PlcStatusChanged -= DataPushBus_PlcStatusChanged;
            Stop();
            _cts.Dispose();
        }
    }
}
