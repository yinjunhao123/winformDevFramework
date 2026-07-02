using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.PLCBasic;
using WinformDevFramework.IRepository.PLCBasic;
using PLCBasic.IRepository;
using PLCBasic;

namespace WinformDevFramework.Services.PLCBasic.Handlers
{
    public class PLC_ParameterDistributionHandler : IPLC_ParameterDistributionHandler
    {
        private readonly ILogger<PLC_ParameterDistributionHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IPLC_ParameterDistributionRepository _parameterDistributionRepository;
        private readonly IPLC_ParameterDistributionDetailRepository _parameterDistributionDetailRepository;
        private readonly IPLC_ParameterDistributionHistoryRepository _parameterDistributionHistoryRepository;
        private readonly IPLC_ParameterDistributionDetailHistoryRepository _parameterDistributionDetailHistoryRepository;
        private readonly IPLC_ParameterDistributionRecordRepository _parameterDistributionRecordRepository;
        private readonly IPLC_ParameterDistributionRecordDetailRepository _parameterDistributionRecordDetailRepository;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_ParameterDistributionHandler(ILogger<PLC_ParameterDistributionHandler> logger,
                                                   IPlcCommunicationService plcCommunicationService,
                                                   IPLC_Event_Data_DetailRepository eventDetailRepository,
                                                   IPLC_ParameterDistributionRepository parameterDistributionRepository,
                                                   IPLC_ParameterDistributionDetailRepository parameterDistributionDetailRepository,
                                                   IPLC_ParameterDistributionHistoryRepository parameterDistributionHistoryRepository,
                                                   IPLC_ParameterDistributionDetailHistoryRepository parameterDistributionDetailHistoryRepository,
                                                   IPLC_ParameterDistributionRecordRepository parameterDistributionRecordRepository,
                                                   IPLC_ParameterDistributionRecordDetailRepository parameterDistributionRecordDetailRepository,
                                                   IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _parameterDistributionRepository = parameterDistributionRepository;
            _parameterDistributionDetailRepository = parameterDistributionDetailRepository;
            _parameterDistributionHistoryRepository = parameterDistributionHistoryRepository;
            _parameterDistributionDetailHistoryRepository = parameterDistributionDetailHistoryRepository;
            _parameterDistributionRecordRepository = parameterDistributionRecordRepository;
            _parameterDistributionRecordDetailRepository = parameterDistributionRecordDetailRepository;
            _plcAddressRepository = plcAddressRepository;
        }

        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            await HandleParameterDistributionDownAsync(e);
        }

        private async Task HandleParameterDistributionDownAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理在线MES参数下发相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

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

                await MesOnlineParameterDistributionPLCAsync(e.PlcCode, e.StationCode, writeDataPoints, readDataPointsDict);

                _logger.LogInformation($"MES在线参数下发事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MES在线参数下发事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task MesOnlineParameterDistributionPLCAsync(string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, string> readDataPointsDict)
        {
            string partType = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("PartType") ? readDataPointsDict["PartType"] : string.Empty);
            string recipeVer = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("RecipeVer") ? readDataPointsDict["RecipeVer"] : string.Empty);

            try
            {
                _logger.LogInformation($"MES在线模式参数下发开始: PlcCode={plcCode}, StationCode={stationCode}, PartType={partType}, RecipeVer={recipeVer}");

                var parameterDistribution = await _parameterDistributionRepository.QueryByClauseAsync(p => p.ProductModelCode == partType && p.RecipeCode == recipeVer);

                PLC_ParameterDistribution distributionToUse;
                List<PLC_ParameterDistributionDetail> distributionDetails;

                if (parameterDistribution == null)
                {
                    _logger.LogInformation($"未找到匹配的参数配置，使用第一条配置: PartType={partType}, RecipeVer={recipeVer}");

                    var allDistributions = await _parameterDistributionRepository.QueryAsync();
                    var firstDistribution = allDistributions?.FirstOrDefault();
                    if (firstDistribution == null)
                    {
                        _logger.LogError($"PLC_ParameterDistribution表为空，无法进行参数下发");
                        return;
                    }

                    distributionToUse = new PLC_ParameterDistribution
                    {
                        RecipeCode = firstDistribution.RecipeCode,
                        ProductModelCode = partType,
                        RoutingCode = recipeVer,
                        IsEnabled = firstDistribution.IsEnabled,
                        Description = firstDistribution.Description,
                        CreateUser = "System",
                        CreateDate = DateTime.Now
                    };

                    distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(
                        d => d.DistributionID == firstDistribution.ID && d.StationCode == stationCode);

                    if (distributionDetails == null || !distributionDetails.Any())
                    {
                        _logger.LogWarning($"未找到工站{stationCode}的参数下发明细配置");
                        return;
                    }

                    List<PLC_ParameterDistributionDetail> list = new List<PLC_ParameterDistributionDetail>();
                    foreach (var detail in distributionDetails)
                    {
                        try
                        {
                            string actualValue = PlcHandlerHelper.ReadPlcData(_plcCommunicationService, _logger, plcCode, detail.AddressCode, detail.DataType);

                            var newDetail = new PLC_ParameterDistributionDetail
                            {
                                DistributionID = 0,
                                StationCode = detail.StationCode,
                                ModelCode = detail.ModelCode,
                                PlcCode = detail.PlcCode,
                                EquipmentCode = detail.EquipmentCode,
                                AddressCode = detail.AddressCode,
                                DataType = detail.DataType,
                                ParamName = detail.ParamName,
                                ParamValue = actualValue,
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

                    await _parameterDistributionRepository.InsertAsync(distributionToUse);
                    _logger.LogInformation($"创建新的参数配置记录: DistributionID={distributionToUse.ID}");

                    foreach (var detail in list)
                    {
                        detail.DistributionID = distributionToUse.ID;
                    }
                    await _parameterDistributionDetailRepository.InsertAsync(list);
                    _logger.LogInformation($"保存参数下发明细: Count={list.Count}");
                }
                else
                {
                    _logger.LogInformation($"找到匹配的参数配置: DistributionID={parameterDistribution.ID}");

                    distributionToUse = parameterDistribution;
                    distributionDetails = await _parameterDistributionDetailRepository.QueryListByClauseAsync(
                        d => d.DistributionID == parameterDistribution.ID && d.StationCode == stationCode);

                    if (distributionDetails == null || !distributionDetails.Any())
                    {
                        _logger.LogWarning($"未找到工站{stationCode}的参数下发明细配置");
                        return;
                    }

                    bool hasChanges = false;
                    List<PLC_ParameterDistributionDetail> changedDetails = new List<PLC_ParameterDistributionDetail>();

                    foreach (var detail in distributionDetails)
                    {
                        try
                        {
                            string actualValue = PlcHandlerHelper.ReadPlcData(_plcCommunicationService, _logger, plcCode, detail.AddressCode, detail.DataType);

                            if (detail.ParamValue != actualValue)
                            {
                                hasChanges = true;
                                _logger.LogInformation($"参数值发生变化: ParamName={detail.ParamName}, 原值={detail.ParamValue}, 新值={actualValue}");

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
                                    ParamValue = detail.ParamValue,
                                    DistributionType = detail.DistributionType,
                                    SortOrder = detail.SortOrder,
                                    UpdateUser = "System",
                                    UpdateTime = DateTime.Now
                                });

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

                    if (hasChanges)
                    {
                        var history = new PLC_ParameterDistributionHistory
                        {
                            ProductModelCode = partType,
                            RecipeCode = recipeVer,
                            SnapshotTime = DateTime.Now,
                            CreateUser = "System",
                            CreateTime = DateTime.Now
                        };
                        await _parameterDistributionHistoryRepository.InsertAsync(history);

                        var insertedHistory = await _parameterDistributionHistoryRepository.QueryByClauseAsync(
                            h => h.ProductModelCode == partType && h.RecipeCode == recipeVer && h.SnapshotTime == history.SnapshotTime);
                        long actualHistoryID = insertedHistory?.ID ?? 0;

                        List<PLC_ParameterDistributionDetailHistory> historyDetails = new List<PLC_ParameterDistributionDetailHistory>();
                        foreach (var changedDetail in changedDetails)
                        {
                            string newValue = changedDetail.ParamValue;
                            string currentValue = PlcHandlerHelper.ReadPlcData(_plcCommunicationService, _logger, plcCode, changedDetail.AddressCode, changedDetail.DataType);

                            var historyDetail = new PLC_ParameterDistributionDetailHistory
                            {
                                HistoryID = actualHistoryID,
                                PlcCode = changedDetail.PlcCode,
                                EquipmentCode = changedDetail.EquipmentCode,
                                StationCode = changedDetail.StationCode,
                                AddressCode = changedDetail.AddressCode,
                                DataType = changedDetail.DataType,
                                ParamName = changedDetail.ParamName,
                                OriginalValue = newValue,
                                NewValue = currentValue,
                                DistributionType = changedDetail.DistributionType
                            };
                            historyDetails.Add(historyDetail);
                        }
                        await _parameterDistributionDetailHistoryRepository.InsertAsync(historyDetails);
                        _logger.LogInformation($"创建历史记录成功: HistoryID={actualHistoryID}, DetailCount={historyDetails.Count}");

                        await _parameterDistributionDetailRepository.UpdateAsync(distributionDetails);
                        _logger.LogInformation($"更新参数下发明细: Count={distributionDetails.Count}");
                    }
                    else
                    {
                        _logger.LogInformation($"参数值未发生变化，无需更新");
                    }
                }
                _logger.LogInformation($"MES在线模式参数下发完成: PlcCode={plcCode}, StationCode={stationCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MES在线模式参数下实时发失败: PlcCode={plcCode}, StationCode={stationCode}");
            }
        }
    }
}