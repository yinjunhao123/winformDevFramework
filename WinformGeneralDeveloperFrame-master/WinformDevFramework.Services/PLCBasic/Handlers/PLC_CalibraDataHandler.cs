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
    public class PLC_CalibraDataHandler : IPLC_CalibraDataHandler
    {
        private readonly ILogger<PLC_CalibraDataHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IPLC_CalibraCollectRepository _calibraCollectRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_CalibraDataHandler(ILogger<PLC_CalibraDataHandler> logger,
                                        IPlcCommunicationService plcCommunicationService,
                                        IPLC_Event_Data_DetailRepository eventDetailRepository,
                                        IPLC_CalibraCollectRepository calibraCollectRepository,
                                        IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _calibraCollectRepository = calibraCollectRepository;
            _plcAddressRepository = plcAddressRepository;
        }

        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleCalibraUpDataAsync(e);
            }
            else
            {
                await HandleCalibraDownDataAsync(e);
            }
        }

        private async Task HandleCalibraUpDataAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理标定数据事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

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

                await HandleCalibraUpEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"标定数据事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理标定数据事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task HandleCalibraUpEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                _logger.LogInformation($"开始处理校准事件(上升沿): PlcCode={plcCode}, StationCode={stationCode}");

                string calibraTime = readDataPointsDict.ContainsKey("Calibra_Time") ? readDataPointsDict["Calibra_Time"] : string.Empty;
                byte? calibraResult = null;
                if (readDataPointsDict.ContainsKey("Calibra_Result") && byte.TryParse(readDataPointsDict["Calibra_Result"], out byte result))
                {
                    calibraResult = result;
                }

                decimal?[] calibraData = new decimal?[15];
                for (int i = 1; i <= 15; i++)
                {
                    string key = $"Calibra1_Data{i}";
                    if (readDataPointsDict.ContainsKey(key) && decimal.TryParse(readDataPointsDict[key], out decimal val))
                    {
                        calibraData[i - 1] = val;
                    }
                }

                var calibraRecord = new PLC_CalibraCollect
                {
                    StationCode = stationCode,
                    LineCode = string.Empty,
                    PlcCode = plcCode,
                    CalibraTime = calibraTime,
                    CalibraResult = calibraResult,
                    Calibra1Data1 = calibraData[0],
                    Calibra1Data2 = calibraData[1],
                    Calibra1Data3 = calibraData[2],
                    Calibra1Data4 = calibraData[3],
                    Calibra1Data5 = calibraData[4],
                    Calibra1Data6 = calibraData[5],
                    Calibra1Data7 = calibraData[6],
                    Calibra1Data8 = calibraData[7],
                    Calibra1Data9 = calibraData[8],
                    Calibra1Data10 = calibraData[9],
                    Calibra1Data11 = calibraData[10],
                    Calibra1Data12 = calibraData[11],
                    Calibra1Data13 = calibraData[12],
                    Calibra1Data14 = calibraData[13],
                    Calibra1Data15 = calibraData[14],
                    CollectTime = DateTime.Now,
                    CreateUser = "System"
                };

                await _calibraCollectRepository.InsertAsync(calibraRecord);
                _logger.LogInformation($"校准数据已保存: StationCode={stationCode}, CalibraTime={calibraTime}");

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

        private async Task HandleCalibraDownDataAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理标定数据下降沿事件: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                await HandleCalibraDownEvent(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"标定数据下降沿事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理标定数据下降沿事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task HandleCalibraDownEvent(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                _logger.LogInformation($"开始处理校准事件下降沿: PlcCode={plcCode}, StationCode={stationCode}");

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>
                {
                    { "CalibraDataRaq", false }
                };

                await PlcHandlerHelper.CollectAndWriteDataPointsAsync(_plcCommunicationService, _logger, plcCode, writeDataPoints, dataToWrite);
                _logger.LogInformation($"已向PLC下发CalibraDataRaq=false");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理校准事件下降沿时发生错误");
            }
        }
    }
}