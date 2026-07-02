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
    public class PLC_BatchCheckHandler : IPLC_BatchCheckHandler
    {
        private readonly ILogger<PLC_BatchCheckHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IWipBatchRepository _wipBatchRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_BatchCheckHandler(ILogger<PLC_BatchCheckHandler> logger,
                                      IPlcCommunicationService plcCommunicationService,
                                      IPLC_Event_Data_DetailRepository eventDetailRepository,
                                      IWipBatchRepository wipBatchRepository,
                                      IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _wipBatchRepository = wipBatchRepository;
            _plcAddressRepository = plcAddressRepository;
        }

        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
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

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    string msg = $"未找到该事件{e.EventId}的数据点配置";
                    _logger.LogError(msg);
                    return;
                }

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                int deviceStatus = await PlcHandlerHelper.CheckDeviceStatusAsync(_plcCommunicationService, _logger, _plcAddressRepository, e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    dataToWrite.Add("BatchCheckDone", 1);
                    dataToWrite.Add("BatchCheckNG", 1);
                    dataToWrite.Add("BatchCheckErrorCode", 102);
                    await PlcHandlerHelper.SendDeviceStatusErrorAsync(_plcCommunicationService, _logger, e.PlcCode, e.StationCode, writeDataPoints, dataToWrite);
                    return;
                }

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                string batchCode = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("BatchCheckID") ? readDataPointsDict["BatchCheckID"] : string.Empty);

                await BatchCheckInWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, batchCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理批次码验证事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task BatchCheckInWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, string batchCode)
        {
            try
            {
                string msg = string.Empty;
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                if (string.IsNullOrWhiteSpace(batchCode))
                {
                    msg = "批次码为空，无法继续处理";
                    _logger.LogError(msg);
                }

                if (string.IsNullOrEmpty(msg))
                {
                    var existingBatch = await _wipBatchRepository.QueryByClauseAsync(w => w.BatchCode == batchCode);
                    if (existingBatch != null)
                    {
                        msg = $"批次码 {batchCode} 已存在于 WipBatch 表中，当前状态: {existingBatch.Status}";
                        _logger.LogError(msg);
                    }
                }

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

                if (string.IsNullOrEmpty(msg))
                {
                    dataToWrite.Add("BatchCheckDone", true);
                    dataToWrite.Add("BatchCheckOK", true);
                    _logger.LogInformation($"批次码验证成功: BatchCode={batchCode}");
                }
                else
                {
                    dataToWrite.Add("BatchCheckDone", 1);
                    dataToWrite.Add("BatchCheckNG", 1);
                    dataToWrite.Add("BatchCheckErrorMsg", msg);
                    dataToWrite.Add("BatchCheckErrorCode", 102);
                    _logger.LogWarning($"批次码验证失败: {msg}");
                }

                bool writeSuccess = await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);

                if (!writeSuccess)
                {
                    _logger.LogWarning($"批次进站PLC写入部分失败: StationCode={stationCode}, BatchCode={batchCode}");
                }

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
                    _logger.LogInformation($"新增批次记录: BatchCode={batchCode}, Status=2");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次码验证写入PLC时发生错误");
            }
        }

        private async Task HandleBatchCheckOutAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理批次码验证完成事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                await BatchCheckOutWriteToPLCAsync(e.PlcCode, e.StationCode, writeDataPoints);

                _logger.LogInformation($"批次码验证完成事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理批次码验证完成事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task BatchCheckOutWriteToPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints)
        {
            try
            {
                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                dataToWrite.Add("BatchCheckDone", false);
                dataToWrite.Add("BatchCheckOK", false);
                dataToWrite.Add("BatchCheckNG", false);
                dataToWrite.Add("BatchCheckErrorCode", 0);

                bool writeSuccess = await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);

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
    }
}