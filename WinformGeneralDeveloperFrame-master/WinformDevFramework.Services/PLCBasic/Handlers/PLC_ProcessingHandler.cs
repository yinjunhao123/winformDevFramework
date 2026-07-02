using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.PLCBasic;
using WinformDevFramework.IRepository.PLCBasic;
using PLCBasic.IRepository;
using PLCBasic;

namespace WinformDevFramework.Services.PLCBasic.Handlers
{
    public class PLC_ProcessingHandler : IPLC_ProcessingHandler
    {
        private readonly ILogger<PLC_ProcessingHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IWipBarCodeRepository _wipBarcodeRepository;
        private readonly IWipProcessingIrreversibleRepository _wipProcessingIrreversibleRepository;
        private readonly IPLC_StationRecipeCurrentRepository _stationRecipeCurrentRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_ProcessingHandler(ILogger<PLC_ProcessingHandler> logger,
                                      IPlcCommunicationService plcCommunicationService,
                                      IPLC_Event_Data_DetailRepository eventDetailRepository,
                                      IWipBarCodeRepository wipBarcodeRepository,
                                      IWipProcessingIrreversibleRepository wipProcessingIrreversibleRepository,
                                      IPLC_StationRecipeCurrentRepository stationRecipeCurrentRepository,
                                      IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _wipBarcodeRepository = wipBarcodeRepository;
            _wipProcessingIrreversibleRepository = wipProcessingIrreversibleRepository;
            _stationRecipeCurrentRepository = stationRecipeCurrentRepository;
            _plcAddressRepository = plcAddressRepository;
        }

        /// <summary>
        /// 不可逆事件处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isRisingEdge"></param>
        /// <returns></returns>
        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
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

        private async Task HandleProcessingUpAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理不可逆加工事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                int deviceStatus = await PlcHandlerHelper.CheckDeviceStatusAsync(_plcCommunicationService, _logger, _plcAddressRepository, e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    await PlcHandlerHelper.SendDeviceStatusErrorAsync(_plcCommunicationService, _logger, e.PlcCode, e.StationCode, writeDataPoints, dataToWrite);
                    return;
                }

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                await HandleProcessingEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"不可逆加工事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理不可逆加工事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task HandleProcessingDownAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理不可逆加工事件(下降沿): EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                await HandleProcessingDownEvent(e.PlcCode, e.StationCode, writeDataPoints);

                _logger.LogInformation($"不可逆加工事件(下降沿)处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理不可逆加工事件(下降沿)时发生错误: EventId={e.EventId}");
            }
        }

        private async Task HandleProcessingEvent(string plcCode, string stationCode,
            List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                string msg = string.Empty;
                string processingPartId = PlcHandlerHelper.CleanPlcString(readDataPointsDict.TryGetValue("ProcessingPartID", out string pid) ? pid : string.Empty);
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                if (string.IsNullOrWhiteSpace(processingPartId))
                {
                    msg = "不可逆条码为空，跳过处理";
                    _logger.LogWarning(msg);
                    dataToWrite.Add("ProcessingDone", false);
                    dataToWrite.Add("ProcessingErrorMsg", msg);
                    dataToWrite.Add("ProcessingErrorCode", 101);
                    await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, ProcessingErrorCode=101");
                    DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, false, msg);
                    return;
                }

                var wipBarcode = await _wipBarcodeRepository.QueryByClauseAsync(o => o.RFIDCode == processingPartId && o.StationCode == stationCode);
                if (wipBarcode == null)
                {
                    msg = $"条码 {processingPartId} 在工站 {stationCode} 没有在制信息";
                    _logger.LogWarning(msg);
                    dataToWrite.Add("ProcessingDone", false);
                    dataToWrite.Add("ProcessingErrorMsg", msg);
                    dataToWrite.Add("ProcessingErrorCode", 102);
                    await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, false, msg);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, ProcessingErrorCode=102");
                    return;
                }

                var process = await _wipProcessingIrreversibleRepository.QueryByClauseAsync(o => o.RFIDCode == processingPartId && o.StationCode == stationCode);
                if (process != null)
                {
                    msg = $"条码 {processingPartId} 在工站 {stationCode} 已存在不可逆加工记录，跳过处理";
                    _logger.LogWarning(msg);
                    dataToWrite.Add("ProcessingDone", false);
                    dataToWrite.Add("ProcessingErrorMsg", msg);
                    dataToWrite.Add("ProcessingErrorCode", 102);
                    await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                    DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, false, msg);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, ProcessingErrorCode=102");
                    return;
                }

                var stationRecipe = await _stationRecipeCurrentRepository.QueryByClauseAsync(sr => sr.PlcCode == plcCode && sr.StationCode == stationCode);
                string productModel = stationRecipe?.ProductModelCode;
                string recipeCode = stationRecipe?.RecipeCode;

                _logger.LogInformation($"不可逆加工标记: BarCode={processingPartId}, ProductModel={productModel}, StationCode={stationCode}, RecipeCode={recipeCode}");

                var irreversibleRecord = new WipProcessingIrreversible
                {
                    BarCode = wipBarcode.BarCode,
                    RFIDCode = processingPartId,
                    ProductModel = productModel,
                    StationCode = stationCode,
                    RecipeCode = recipeCode,
                    CreateTime = DateTime.Now,
                    CreateUser = "System"
                };

                dataToWrite.Add("ProcessingDone", true);
                await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                DataPushBus.PublishMainPartStationCheck(stationCode, processingPartId, true, "可逆工站已标注成功");
                _logger.LogInformation($"已向PLC下发ProcessingDone=true");
                await _wipProcessingIrreversibleRepository.InsertAsync(irreversibleRecord);
                _logger.LogInformation($"不可逆加工标记记录已保存: BarCode={processingPartId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理不可逆加工事件时发生错误");

                try
                {
                    Dictionary<string, object> errorData = new Dictionary<string, object>
                    {
                        { "ProcessingDone", false },
                        { "SubCheckInErrorMsg", ex.Message }
                    };

                    await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, errorData);
                    _logger.LogInformation($"向PLC下发错误状态: ProcessingDone=false, SubCheckInErrorMsg={ex.Message}");
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "向PLC下发错误状态失败");
                }
            }
        }

        private async Task HandleProcessingDownEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints)
        {
            try
            {
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "ProcessingDone", false }
                };

                await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                _logger.LogInformation($"已向PLC下发ProcessingDone=false");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理不可逆加工下降沿事件时发生错误");
            }
        }
    }
}