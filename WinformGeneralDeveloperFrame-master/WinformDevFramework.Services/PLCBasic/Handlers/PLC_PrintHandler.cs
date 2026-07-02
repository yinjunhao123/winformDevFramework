using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.PLCBasic;
using WinformDevFramework.IRepository.PLCBasic;
using PLCBasic.IRepository;

namespace WinformDevFramework.Services.PLCBasic.Handlers
{
    public class PLC_PrintHandler : IPLC_PrintHandler
    {
        private readonly ILogger<PLC_PrintHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_PrintHandler(ILogger<PLC_PrintHandler> logger,
                                  IPlcCommunicationService plcCommunicationService,
                                  IPLC_Event_Data_DetailRepository eventDetailRepository,
                                  IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _plcAddressRepository = plcAddressRepository;
        }

        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            await HandlePrintExchangeAsync(e);
        }

        private async Task HandlePrintExchangeAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理打印相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                //int deviceStatus = await PlcHandlerHelper.CheckDeviceStatusAsync(_plcCommunicationService, _logger, _plcAddressRepository, e.PlcCode, e.StationCode);
                //if (deviceStatus == -1)
                //{
                //    await PlcHandlerHelper.SendDeviceStatusErrorAsync(_plcCommunicationService, _logger, e.PlcCode, e.StationCode, writeDataPoints, dataToWrite);
                //    return;
                //}

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                await HandleCalibraUpEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"打印相关事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"打印相关事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task HandleCalibraUpEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                _logger.LogInformation($"开始处理校准事件(上升沿): PlcCode={plcCode}, StationCode={stationCode}");

                string calibraTime = readDataPointsDict.ContainsKey("P-Calibra_Time") ? readDataPointsDict["P-Calibra_Time"] : string.Empty;
                byte? calibraResult = null;
                if (readDataPointsDict.ContainsKey("P-Calibra_Result") && byte.TryParse(readDataPointsDict["P-Calibra_Result"], out byte result))
                {
                    calibraResult = result;
                }

                decimal?[] calibraData = new decimal?[15];
                for (int i = 1; i <= 15; i++)
                {
                    string key = $"P-Calibra1_Data{i}";
                    if (readDataPointsDict.ContainsKey(key) && decimal.TryParse(readDataPointsDict[key], out decimal val))
                    {
                        calibraData[i - 1] = val;
                    }
                }

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "CalibraDataDone", true }
                };

                await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                _logger.LogInformation($"已向PLC下发CalibraDataDone=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理校准事件时发生错误");
            }
        }
    }
}