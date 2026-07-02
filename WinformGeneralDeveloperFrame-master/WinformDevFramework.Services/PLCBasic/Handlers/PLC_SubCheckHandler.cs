using Microsoft.Extensions.Logging;
using PLCBasic;
using PLCBasic.IRepository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Models.PLCBasic;

namespace WinformDevFramework.Services.PLCBasic.Handlers
{
    public class PLC_SubCheckHandler : IPLC_SubCheckHandler
    {
        private readonly ILogger<PLC_SubCheckHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IWipBarCodeRepository _wipBarcodeRepository;
        private readonly IWipMaterialInfoRepository _wipMaterialInfoRepository;
        private readonly IWipProcessDataRepository _wipProcessDataRepository;
        private readonly IPLC_StationRecipeCurrentRepository _stationRecipeCurrentRepository;
        private readonly IMD_BarCodeRuleRepository _barCodeRuleRepository;
        private readonly IMD_BarCodeRuleListRepository _barCodeRuleListRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_SubCheckHandler(ILogger<PLC_SubCheckHandler> logger,
                                    IPlcCommunicationService plcCommunicationService,
                                    IPLC_Event_Data_DetailRepository eventDetailRepository,
                                    IWipBarCodeRepository wipBarcodeRepository,
                                    IWipMaterialInfoRepository wipMaterialInfoRepository,
                                    IWipProcessDataRepository wipProcessDataRepository,
                                    IPLC_StationRecipeCurrentRepository stationRecipeCurrentRepository, 
                                    IMD_BarCodeRuleRepository barCodeRuleRepository,
                                    IMD_BarCodeRuleListRepository barCodeRuleListRepository,
                                    IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _wipBarcodeRepository = wipBarcodeRepository;
            _wipMaterialInfoRepository = wipMaterialInfoRepository;
            _wipProcessDataRepository = wipProcessDataRepository;
            _stationRecipeCurrentRepository = stationRecipeCurrentRepository;
            _barCodeRuleRepository = barCodeRuleRepository;
            _barCodeRuleListRepository = barCodeRuleListRepository;
            _plcAddressRepository = plcAddressRepository;
        }

        /// <summary>
        /// 子条码验证事件处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isRisingEdge"></param>
        /// <returns></returns>
        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
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

            _logger.LogInformation($"开始处理子条码验证事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

            if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
            {
                return;
            }

            Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
            try
            {
                int deviceStatus = await PlcHandlerHelper.CheckDeviceStatusAsync(_plcCommunicationService, _logger, _plcAddressRepository, e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    string msg = $"设备状态异常，无法继续处理子条码验证事件";
                    await SuCheckErrorMsg(e.PlcCode, e.StationCode, "", "", msg, 101, writeDataPoints, dataToWrite);
                    return;
                }

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                await SubCheckInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"子条码验证事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                string msg = $"处理子条码验证事件时发生错误: {ex.Message}";
                await SuCheckErrorMsg(e.PlcCode, e.StationCode, "", "", msg, 101, writeDataPoints, dataToWrite);
                _logger.LogError(ex, msg);
            }
        }

        private async Task SubCheckInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            string subpartId = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("SubPartID") ? readDataPointsDict["SubPartID"] : string.Empty);
            string partId = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("PartID") ? readDataPointsDict["PartID"] : string.Empty);
            string partType = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("PartType") ? readDataPointsDict["PartType"] : string.Empty);
            Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
            string msg = string.Empty;

            try
            {
                StationProcessInfo stationProcessInfo = await GetStationRecipeCurrentAsync(plcCode, stationCode);
                if (stationProcessInfo == null)
                {
                    msg = $"请先扫描主零件";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    return;
                }
                dataToWrite.Add("SubPartDone", true);

                int requiredPartCount = 0;
                MD_RoutingList currentStationConfig = stationProcessInfo.RoutingList.FirstOrDefault(o => o.StationCode == stationCode);

                if (string.IsNullOrEmpty(partId) || string.IsNullOrEmpty(subpartId) || string.IsNullOrEmpty(partType))
                {
                    msg = $"子条码、主条码或型号为空，无法继续处理";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 104, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    _logger.LogError(msg);
                    return;
                }

                if (!string.Equals(partType, stationProcessInfo.ProductModel, StringComparison.OrdinalIgnoreCase))
                {
                    msg = $"采集型号 [{partType}] 与当前生产型号 [{stationProcessInfo.ProductModel}] 不一致";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 104, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    _logger.LogError(msg);
                    return;
                }

                var wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == partId && o.StationCode == stationCode);
                if (wipBarcode == null)
                {
                    msg = $"未找到RFID条码 {partId} 对应的在制品信息";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 104, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    _logger.LogError(msg);
                    return;
                }

                var productModelInfo = stationProcessInfo.RoutingList.FirstOrDefault(o => o.StationCode == stationCode);
                if (productModelInfo != null)
                {
                    if (productModelInfo.IsScanFirst.HasValue && productModelInfo.IsScanFirst.Value)
                        requiredPartCount++;
                    if (productModelInfo.IsScanSecond.HasValue && productModelInfo.IsScanSecond.Value)
                        requiredPartCount++;
                    if (productModelInfo.IsScanThird.HasValue && productModelInfo.IsScanThird.Value)
                        requiredPartCount++;
                    _logger.LogInformation($"当前工站 {stationCode} 需要扫描 {requiredPartCount} 个子零件");
                }
                else
                {
                    msg = $"未找到型号{partType}对应工站{stationCode}的工艺路线配置";
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 104, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    _logger.LogError(msg);
                    return;
                }

                if (string.IsNullOrEmpty(msg))
                {
                    var existingCount = await _wipMaterialInfoRepository.QueryByClauseAsync(w => w.RfidCode == partId && w.StationCode == stationCode && w.MaterialCode == subpartId);
                    if (existingCount != null)
                    {
                        msg = $"WipMaterialInfo 已存在该条码 {partId} 和工站 {stationCode} 对应的子零件 {subpartId}";
                        await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 103, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        _logger.LogError(msg);
                        return;
                    }
                }

                if (stationCode == "OP30")
                {
                    var wipProcess = await _wipProcessDataRepository.QueryByClauseAsync(O => O.BarCode == subpartId && O.CurveTypes == "位压");
                    if (wipProcess == null)
                    {
                        msg = $"wipProcess 不存在该条码 {subpartId} 的位压曲线";
                        await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 102, writeDataPoints, dataToWrite);
                        DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                        _logger.LogError(msg);
                        return;
                    }
                }

                int scannedCount = 0;
                if (string.IsNullOrEmpty(msg))
                {
                    scannedCount = await _wipMaterialInfoRepository.GetCountAsync(w => w.RfidCode == partId && w.StationCode == stationCode && w.MaterialType == "SubPart");
                    _logger.LogInformation($"主条码 {partId} 在工站 {stationCode} 已扫描 {scannedCount} 个子零件");
                }

                int currentPartSequence = scannedCount + 1;
                var validationResult = await ValidateBarcodeFormat(subpartId, currentPartSequence, stationProcessInfo);
                if (!validationResult.isValid)
                {
                    msg = validationResult.errorMsg;
                    await SuCheckErrorMsg(plcCode, stationCode, partId, subpartId, msg, 101, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, partId, false, msg);
                    _logger.LogError(msg);
                    return;
                }

                int num = scannedCount + 1;
                dataToWrite.Add("SubassemblyPoint", num);

                if (string.IsNullOrEmpty(msg))
                {
                    var materialInfo = new WipMaterialInfo
                    {
                        RfidCode = partId,
                        StationCode = stationCode,
                        MaterialCode = subpartId,
                        MaterialType = "SubPart",
                        CreateTime = DateTime.Now,
                        CreateUser = "System"
                    };
                    await _wipMaterialInfoRepository.InsertAsync(materialInfo);
                    _logger.LogInformation($"新增子零件绑定记录: RFID={partId}, Station={stationCode}, MaterialCode={subpartId}");
                }

                if (scannedCount + 1 >= requiredPartCount)
                {
                    dataToWrite.Add("AllSubPartsScanned", true);
                    _logger.LogInformation($"主条码 {partId} 在工站 {stationCode} 已完成所有子零件扫描");
                }
                else
                {
                    dataToWrite.Add("AllSubPartsScanned", false);
                }

                bool writeSuccess = await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                if (!writeSuccess)
                {
                    _logger.LogWarning($"子条码校验PLC写入部分失败: StationCode={stationCode}, PartID={partId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"子条码校验写入PLC时发生错误");
            }
        }

        private async Task HandleSubCheckDownInAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理子条码验证下降沿事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();
                dataToWrite.Add("SubPartDone", false);
                dataToWrite.Add("SubassemblyPoint", 0);
                dataToWrite.Add("AllSubPartsScanned", false);

                bool writeSuccess = await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, writeDataPoints, dataToWrite);
                if (!writeSuccess)
                {
                    _logger.LogWarning($"子条码校验下降沿PLC写入部分失败: StationCode={e.StationCode}");
                }

                _logger.LogInformation($"子条码验证下降沿事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理子条码验证下降沿事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task<StationProcessInfo> GetStationRecipeCurrentAsync(string plcCode, string stationCode)
        {
            var stationRecipe = await _stationRecipeCurrentRepository.QueryByClauseAsync(sr => sr.PlcCode == plcCode && sr.StationCode == stationCode);
            if (stationRecipe == null)
            {
                return null;
            }

            return new StationProcessInfo
            {
                PlcCode = stationRecipe.PlcCode,
                StationCode = stationRecipe.StationCode,
                ProductModel = stationRecipe.ProductModelCode,
                Recipe = stationRecipe.RecipeCode,
                RoutingCode = stationRecipe.RoutingCode,
                UpdateTime = DateTime.Now
            };
        }

        private async Task SuCheckErrorMsg(string plcCode, string stationCode, string partId, string subpartId, string msg, int errorCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, object> dataToWrite)
        {
            _logger.LogError($"PLC子条码校验错误: PlcCode={plcCode}, StationCode={stationCode}, PartID={partId}, SubPartID={subpartId}, ErrorCode={errorCode}, Message={msg}");

            dataToWrite["SubPartDone"] = false;
            dataToWrite["ErrorCode"] = errorCode;
            dataToWrite["ErrorMsg"] = msg;

            await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
        }

        private async Task<(bool isValid, string errorMsg)> ValidateBarcodeFormat(string barcode, int partSequence, StationProcessInfo stationProcessInfo)
        {
            try
            {
                var stationConfig = stationProcessInfo.RoutingList.FirstOrDefault(rl => rl.StationCode == stationProcessInfo.StationCode);

                if (stationConfig == null)
                {
                    return (false, $"未找到工站 {stationProcessInfo.StationCode} 的配置");
                }

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

                var barCodeRuleList = await _barCodeRuleRepository.QueryListByClauseAsync(r => r.BarCodeName == ruleName);
                var barCodeRule = barCodeRuleList?.FirstOrDefault();
                if (barCodeRule == null)
                {
                    return (true, string.Empty);
                }

                var ruleDetailsList = await _barCodeRuleListRepository.QueryListByClauseAsync(r => r.BarCodeRuleId == barCodeRule.BarCodeRuleId);
                var ruleDetails = ruleDetailsList
                    .Where(r => r.BarCodeRuleId == barCodeRule.BarCodeRuleId)
                    .OrderBy(r => r.SortOrder)
                    .ToList();

                if (ruleDetails.Count == 0)
                {
                    return (false, $"条码规则 {ruleName} 未配置明细");
                }

                int currentPosition = 0;
                foreach (var detail in ruleDetails)
                {
                    if (string.IsNullOrEmpty(detail.SegmentType))
                        continue;

                    int segmentLength = detail.FixedLength ?? (detail.Content?.Length ?? 0);
                    if (segmentLength <= 0)
                        continue;

                    if (currentPosition + segmentLength > barcode.Length)
                    {
                        return (false, $"条码长度不足，期望长度：{barCodeRule.BarCodeLength}，实际长度：{barcode.Length}");
                    }

                    string segment = barcode.Substring(currentPosition, segmentLength);

                    switch (detail.SegmentType.ToUpper())
                    {
                        case "FIXED":
                            if (!string.IsNullOrEmpty(detail.Content) && segment != detail.Content)
                            {
                                return (false, $"条码第 {currentPosition + 1} 位固定内容不匹配，期望：{detail.Content}，实际：{segment}");
                            }
                            break;

                        case "DATE":
                            string expectedFormat = detail.Content?.Trim() ?? "yyyyMMdd";
                            if (!DateTime.TryParseExact(segment, expectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                return (false, $"条码第 {currentPosition + 1} 位日期格式不正确，期望格式：{expectedFormat}，实际：{segment}");
                            }
                            break;

                        case "SEQUENCE":
                            if (!long.TryParse(segment, out _))
                            {
                                return (false, $"条码第 {currentPosition + 1} 位流水号格式不正确：{segment}");
                            }
                            break;
                    }

                    currentPosition += segmentLength;
                }

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
    }
}