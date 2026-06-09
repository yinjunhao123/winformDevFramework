using HslCommunication;
using Microsoft.Extensions.Logging;
using PLCBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Models.Common;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// 参数下发服务实现
    /// 负责执行 PLC 参数下发操作，包括下发前参数快照、按顺序下发、下发结果记录等功能
    /// </summary>
    public class ParameterDistributionService : BaseServices<PLC_ParameterDistribution>,IParameterDistributionService
    {
        /// <summary>
        /// PLC 通信服务
        /// </summary>
        private readonly IPlcCommunicationService _plcCommunicationService;

        /// <summary>
        /// PLC 连接管理器
        /// </summary>
        private readonly PlcConnectionManager _connectionManager;

        /// <summary>
        /// 心跳服务
        /// </summary>
        private readonly IPlcHeartbeatService _heartbeatService;

        /// <summary>
        /// 下发配置主表仓储
        /// </summary>
        private readonly IBaseRepository<PLC_ParameterDistribution> _distributionRepository;

        /// <summary>
        /// 下发配置明细表仓储
        /// </summary>
        private readonly IBaseRepository<PLC_ParameterDistributionDetail> _detailRepository;

        /// <summary>
        /// 下发记录主表仓储
        /// </summary>
        private readonly IBaseRepository<PLC_ParameterDistributionRecord> _recordRepository;

        /// <summary>
        /// 下发记录明细表仓储
        /// </summary>
        private readonly IBaseRepository<PLC_ParameterDistributionRecordDetail> _recordDetailRepository;

        /// <summary>
        /// 下发历史主表仓储
        /// </summary>
        private readonly IBaseRepository<PLC_ParameterDistributionHistory> _historyRepository;

        /// <summary>
        /// 下发历史明细表仓储
        /// </summary>
        private readonly IBaseRepository<PLC_ParameterDistributionDetailHistory> _historyDetailRepository;

        /// <summary>
        /// 产品型号仓储
        /// </summary>
        private readonly IBaseRepository<MD_ProductModel> _productModelRepository;


        /// <summary>
        /// 分布式锁（按工站隔离）
        /// Key: PlcCode_StationCode, Value: 信号量
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _stationLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// 下发请求去重缓存
        /// Key: DistributionID_StationCode, Value: 最近一次下发记录 ID
        /// 缓存时间：5 分钟
        /// </summary>
        private readonly ConcurrentDictionary<string, (long RecordID, DateTime CreateTime)> _deduplicationCache = new ConcurrentDictionary<string, (long, DateTime)>();

        /// <summary>
        /// 去重缓存有效期（分钟）
        /// </summary>
        private const int DeduplicationCacheExpireMinutes = 5;

        private readonly ILogger<ParameterDistributionService> _logger;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="plcCommunicationService">PLC通信服务</param>
        /// <param name="connectionManager">PLC连接管理器</param>
        /// <param name="heartbeatService">心跳服务</param>
        /// <param name="distributionRepository">下发配置主表仓储</param>
        /// <param name="detailRepository">下发配置明细表仓储</param>
        /// <param name="recordRepository">下发记录主表仓储</param>
        /// <param name="recordDetailRepository">下发记录明细表仓储</param>
        /// <param name="historyRepository">下发历史主表仓储</param>
        /// <param name="historyDetailRepository">下发历史明细表仓储</param>
        /// <param name="productModelRepository">产品型号仓储</param>
        public ParameterDistributionService(IPlcCommunicationService plcCommunicationService,
            PlcConnectionManager connectionManager,
            IPlcHeartbeatService heartbeatService,
            IBaseRepository<PLC_ParameterDistribution> distributionRepository,
            IBaseRepository<PLC_ParameterDistributionDetail> detailRepository,
            IBaseRepository<PLC_ParameterDistributionRecord> recordRepository,
            IBaseRepository<PLC_ParameterDistributionRecordDetail> recordDetailRepository,
            IBaseRepository<PLC_ParameterDistributionHistory> historyRepository,
            IBaseRepository<PLC_ParameterDistributionDetailHistory> historyDetailRepository,
            IBaseRepository<MD_ProductModel> productModelRepository,
            ILogger<ParameterDistributionService> logger)
        {
            // 初始化父类的 BaseDal 属性
            BaseDal = distributionRepository;
            
            _plcCommunicationService = plcCommunicationService;
            _connectionManager = connectionManager;
            _heartbeatService = heartbeatService;
            _distributionRepository = distributionRepository;
            _detailRepository = detailRepository;
            _recordRepository = recordRepository;
            _recordDetailRepository = recordDetailRepository;
            _historyRepository = historyRepository;
            _historyDetailRepository = historyDetailRepository;
            _productModelRepository = productModelRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取或创建工站锁
        /// </summary>
        private SemaphoreSlim GetStationLock(string plcCode, string stationCode)
        {
            var key = $"{plcCode}_{stationCode}";
            return _stationLocks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
        }

        /// <summary>
        /// 检查重复请求
        /// </summary>
        private bool IsDuplicateRequest(long distributionID, string stationCode, out long existingRecordID)
        {
            existingRecordID = 0;
            var key = $"{distributionID}_{stationCode}";
            
            if (_deduplicationCache.TryGetValue(key, out var cached))
            {
                // 检查缓存是否过期
                if (DateTime.Now - cached.CreateTime < TimeSpan.FromMinutes(DeduplicationCacheExpireMinutes))
                {
                    existingRecordID = cached.RecordID;
                    return true;
                }
                else
                {
                    // 缓存过期，移除
                    _deduplicationCache.TryRemove(key, out _);
                }
            }
            return false;
        }

        /// <summary>
        /// 进度报告消息队列名称
        /// </summary>
        private const string ProgressQueueName = "distribution_progress_queue";

        /// <summary>
        /// 记录下发请求到缓存
        /// </summary>
        private void RecordDistributionRequest(long distributionID, string stationCode, long recordID)
        {
            var key = $"{distributionID}_{stationCode}";
            _deduplicationCache.AddOrUpdate(key, (recordID, DateTime.Now), (k, v) => (recordID, DateTime.Now));
            _logger?.LogInformation($"记录下发请求: DistributionID={distributionID}, StationCode={stationCode}, RecordID={recordID}");
        }

        /// <summary>
        /// 清理过期的去重缓存
        /// </summary>
        private void CleanupExpiredCache()
        {
            var now = DateTime.Now;
            var expiredKeys = _deduplicationCache
                .Where(kv => now - kv.Value.CreateTime >= TimeSpan.FromMinutes(DeduplicationCacheExpireMinutes))
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _deduplicationCache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 发送进度报告到消息队列并发布到事件总线
        /// </summary>
        /// <param name="recordID">下发记录 ID</param>
        /// <param name="stationCode">工站代码</param>
        /// <param name="status">进度状态</param>
        /// <param name="progress">当前进度（0-100）</param>
        /// <param name="message">进度消息</param>
        private void ReportProgress(long recordID, string stationCode, string status, int progress, string message)
        {
            try
            {
                var progressInfo = new DistributionProgressInfo
                {
                    RecordID = recordID,
                    StationCode = stationCode,
                    Status = status,
                    Progress = progress,
                    Message = message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // 发布到事件总线，供前端订阅
                DataPushBus.PublishDistributionProgress(progressInfo);

                _logger?.LogInformation($"下发进度: RecordID={recordID}, StationCode={stationCode}, Status={status}, Progress={progress}%, Message={message}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发送进度报告失败");
            }
        }

        /// <summary>
        /// 验证参数值是否有效
        /// </summary>
        /// <param name="detail">下发配置明细</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>验证通过返回 true</returns>
        private bool ValidateParameterValue(PLC_ParameterDistributionDetail detail, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (detail == null)
            {
                errorMessage = "下发配置明细为空";
                return false;
            }

            // 参数值非空验证
            if (string.IsNullOrWhiteSpace(detail.ParamValue))
            {
                errorMessage = $"参数值为空 [{detail.ParamName}]";
                return false;
            }

            // 数据类型格式验证
            try
            {
                switch (detail.DataType?.ToLower())
                {
                    case "bool":
                        bool.Parse(detail.ParamValue);
                        break;
                    case "int":
                    case "int32":
                        int.Parse(detail.ParamValue);
                        break;
                    case "int16":
                        short.Parse(detail.ParamValue);
                        break;
                    case "float":
                        float.Parse(detail.ParamValue);
                        break;
                    case "double":
                        double.Parse(detail.ParamValue);
                        break;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"参数值格式错误 [{detail.ParamName}]: {ex.Message}";
                _logger?.LogError(ex, $"参数值格式验证失败: {detail.ParamName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证 PLC 地址格式是否正确
        /// </summary>
        /// <param name="addressCode">地址编码</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>验证通过返回 true</returns>
        private bool ValidateAddressFormat(string addressCode, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(addressCode))
            {
                errorMessage = "地址编码为空";
                return false;
            }

            // 简单的地址格式验证
            var addressPatterns = new[]
            {
                @"^[IMQ][0-9]+\.[0-7]$",      // 位地址如 I0.0, M10.5, Q2.7
                @"^[IMQ][0-9]+$",             // 字节地址如 I0, M10, Q2
                @"^DB[0-9]+\.DBX[0-9]+\.[0-7]$", // DB位地址如 DB1.DBX0.0
                @"^DB[0-9]+\.DBB[0-9]+$",      // DB字节地址如 DB1.DBB0
                @"^DB[0-9]+\.DBW[0-9]+$",      // DB字地址如 DB1.DBW0
                @"^DB[0-9]+\.DBD[0-9]+$",      // DB双字地址如 DB1.DBD0
                @"^[VWCT][0-9]+$",             // 变量地址如 V0, W100, C5
                @"^[LW][0-9]+$"                // L地址如 L0, W0
            };

            foreach (var pattern in addressPatterns)
            {
                if (Regex.IsMatch(addressCode, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            errorMessage = $"地址格式不正确: {addressCode}";
            _logger?.LogWarning($"地址格式验证失败: {addressCode}");
            return false;
        }

        /// <summary>
        /// 执行参数下发
        /// 下发流程：
        /// 1. 检查下发配置是否存在
        /// 2. 检查 PLC 连接状态
        /// 3. 停止心跳服务（避免下发过程中心跳干扰）
        /// 4. 创建下发批次记录
        /// 5. 读取 PLC 当前参数值并保存到历史表（快照）
        /// 6. 按顺序下发参数（BeforeDistribution -> Dispatching -> AfterDistribution）
        /// 7. 记录每个点位的下发结果
        /// 8. 更新批次记录状态和统计信息
        /// 9. 启动心跳服务
        /// </summary>
        /// <param name="distributionID">下发配置 ID</param>
        /// <param name="user">下发人</param>
        /// <returns>下发结果，包含成功/失败数量、记录 ID 等信息</returns>
        public async Task<DistributionResult> ExecuteDistributionAsync(long distributionID, string user, string plcCode, string stationCode)
        {
            var startTime = DateTime.Now;
            var result = new DistributionResult
            {
                Success = false,
                Message = "下发失败",
                PlcCode = plcCode,
                StationCode = stationCode,
                DistributionTime = startTime
            };

            // 确保心跳服务最终会重启
            bool heartbeatStopped = false;
            SemaphoreSlim stationLock = null;
            
            try
            {
                // 1. 参数验证
                if (distributionID <= 0)
                {
                    result.Message = "下发配置 ID 无效";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(user))
                {
                    result.Message = "下发人不能为空";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(plcCode))
                {
                    result.Message = "PLC 编码不能为空";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(stationCode))
                {
                    result.Message = "工站代码不能为空";
                    return result;
                }

                // 2. 检查重复请求（幂等性检查）
                if (IsDuplicateRequest(distributionID, stationCode, out long existingRecordID))
                {
                    result.Message = $"检测到重复请求，{DeduplicationCacheExpireMinutes}分钟内已下发过该配置 [记录 ID:{existingRecordID}]";
                    result.RecordID = existingRecordID;
                    result.Success = true; // 视为成功，避免重复下发
                    return result;
                }

                // 3. 获取工站锁（并发控制）
                stationLock = GetStationLock(plcCode, stationCode);
                var lockAcquired = await stationLock.WaitAsync(TimeSpan.FromSeconds(30));
                if (!lockAcquired)
                {
                    result.Message = $"工站 [{stationCode}] 正在执行下发，请等待完成后再试";
                    return result;
                }

                // 4. 查询下发配置
                var distribution = _distributionRepository.QueryById(distributionID);
                if (distribution == null)
                {
                    result.Message = $"下发配置不存在 [ID:{distributionID}]";
                    return result;
                }

                // 5. 查询该工站的下发配置明细（单工站过滤）
                var details = _detailRepository.QueryListByClause(d => 
                    d.DistributionID == distributionID && 
                    d.StationCode == stationCode).ToList();
                
                if (!details.Any())
                {
                    result.Message = $"工站 [{stationCode}] 没有下发配置明细";
                    return result;
                }

                // 6. 参数值和地址格式验证
                foreach (var detail in details)
                {
                    // 验证地址格式
                    if (!ValidateAddressFormat(detail.AddressCode, out string addressError))
                    {
                        result.Message = $"地址格式错误: {addressError}";
                        return result;
                    }

                    // 验证参数值范围
                    if (!ValidateParameterValue(detail, out string valueError))
                    {
                        result.Message = $"参数值验证失败 [{detail.ParamName}]: {valueError}";
                        return result;
                    }
                }
                _logger?.LogInformation($"参数验证通过，共 {details.Count} 个点位");

                // 7. 检查 PLC 连接状态（包含心跳检测）
                string connectionError;
                if (!ValidatePlcConnection(plcCode, out connectionError))
                {
                    result.Message = connectionError;
                    return result;
                }

                // 8. 停止心跳服务，避免下发过程中产生干扰
                _heartbeatService.Stop();
                heartbeatStopped = true;
                _logger?.LogInformation($"心跳服务已停止");

                // 9. 创建下发批次记录（单工站）
                ReportProgress(0, stationCode, "CreatingRecord", 10, "正在创建下发批次记录");
                var record = await CreateDistributionRecordAsync(distribution, details, user);
                _logger?.LogInformation($"创建下发批次记录成功，记录 ID: {record.ID}");

                // 10. 保存下发前参数快照到历史表
                ReportProgress(record.ID, stationCode, "SavingSnapshot", 20, "正在保存参数快照");
                var historyID = await SaveParameterHistoryAsync(record.ID, distribution, details);
                _logger?.LogInformation($"参数快照保存成功，历史 ID: {historyID}");

                // 11. 按下发类型排序（BeforeDistribution -> Dispatching -> AfterDistribution）
                var sortedDetails = details.OrderBy(d => GetDistributionOrder(d.DistributionType)).ToList();

                // 12. 执行参数下发（批量写入优化，提升效率）
                ReportProgress(record.ID, stationCode, "WritingParameters", 40, "正在下发参数");
                _logger?.LogInformation($"开始批量下发参数，共 {sortedDetails.Count} 个点位");

                var recordDetails = new List<PLC_ParameterDistributionRecordDetail>();
                int successCount = 0;
                int failedCount = 0;
                bool shouldRollback = false;

                // 构建批量写入请求
                var writeRequests = sortedDetails.Select((detail, index) => new BatchWriteRequest
                {
                    Index = index,
                    Address = detail.AddressCode,
                    Value = detail.ParamValue,
                    DataType = detail.DataType
                }).ToList();

                // 执行批量写入
                ReportProgress(record.ID, stationCode, "WritingParameters", 50, "正在执行批量写入");
                var writeResults = await _plcCommunicationService.WriteBatchAsync(plcCode, writeRequests);
                _logger?.LogInformation($"批量写入完成，结果数: {writeResults.Length}");

                // 处理写入结果
                for (int i = 0; i < sortedDetails.Count && i < writeResults.Length; i++)
                {
                    var detail = sortedDetails[i];
                    bool writeSuccess = writeResults[i];

                    var detailResult = new PLC_ParameterDistributionRecordDetail
                    {
                        RecordID = record.ID,
                        PlcCode = detail.PlcCode,
                        StationCode = detail.StationCode,
                        AddressCode = detail.AddressCode,
                        ParamName = detail.ParamName,
                        ParamValue = detail.ParamValue,
                        DataType = detail.DataType,
                        Status = writeSuccess ? "Success" : "Failed",
                        ErrorMessage = writeSuccess ? null : "写入失败"
                    };

                    recordDetails.Add(detailResult);

                    if (writeSuccess)
                        successCount++;
                    else
                    {
                        failedCount++;
                        shouldRollback = true;
                    }
                }

                // 13. 如果有失败，执行回滚
                if (shouldRollback)
                {
                    ReportProgress(record.ID, stationCode, "RollingBack", 70, "下发失败，正在执行回滚");
                    _logger?.LogWarning($"下发失败，开始回滚参数，历史 ID: {historyID}");
                    try
                    {
                        bool rollbackSuccess = await RollbackParametersAsync(historyID, details);
                        if (rollbackSuccess)
                        {
                            result.Message = $"工站 [{stationCode}] 下发失败，已执行回滚";
                            _logger?.LogInformation("回滚成功");
                        }
                        else
                        {
                            result.Message = $"工站 [{stationCode}] 下发失败，回滚部分失败";
                            _logger?.LogWarning("回滚部分失败");
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        result.Message = $"工站 [{stationCode}] 下发失败，回滚也失败: {rollbackEx.Message}";
                        _logger?.LogError(rollbackEx, "回滚异常");
                    }
                }

                // 14. 保存点位下发结果
                ReportProgress(record.ID, stationCode, "SavingResults", 80, "正在保存下发结果");
                if (recordDetails.Any())
                {
                    _recordDetailRepository.Insert(recordDetails);
                    _logger?.LogInformation($"保存点位下发结果成功，共 {recordDetails.Count} 条");
                }

                // 15. 更新批次记录统计信息
                var endTime = DateTime.Now;
                record.SuccessCount = successCount;
                record.FailedCount = failedCount;
                record.EndTime = endTime;
                record.DurationMs = (endTime - record.StartTime.Value).Milliseconds;
                record.Status = failedCount == 0 ? "Success" : (successCount > 0 ? "PartialSuccess" : "Failed");
                record.Message = shouldRollback 
                    ? $"工站 [{stationCode}] 下发失败，已回滚" 
                    : $"工站 [{stationCode}] 下发完成: 成功{successCount}条, 失败{failedCount}条";
                _recordRepository.Update(record);
                _logger?.LogInformation($"更新批次记录成功，状态: {record.Status}");

                // 16. 记录下发请求到缓存（幂等性）
                RecordDistributionRequest(distributionID, stationCode, record.ID);

                // 17. 设置返回结果
                result.Success = failedCount == 0;
                result.Message = record.Message;
                result.RecordID = record.ID;
                result.RecordCode = record.RecordCode;
                result.TotalCount = successCount + failedCount;
                result.SuccessCount = successCount;
                result.FailedCount = failedCount;
                result.DurationMs = record.DurationMs;
                result.DistributionTime = endTime;
            }
            catch (Exception ex)
            {
                result.Message = $"工站 [{stationCode}] 下发异常：{ex.Message}";
                _logger?.LogError(ex, $"下发异常: StationCode={stationCode}, DistributionID={distributionID}");
            }
            finally
            {
                // 释放工站锁
                if (stationLock != null)
                {
                    stationLock.Release();
                    _logger?.LogInformation($"释放工站锁: {plcCode}_{stationCode}");
                }

                // 确保心跳服务重启
                if (heartbeatStopped)
                {
                    try
                    {
                        _heartbeatService.Start();
                        _logger?.LogInformation("心跳服务已重启");
                    }
                    catch (Exception ex)
                    {
                        result.Message += $"（心跳服务重启失败：{ex.Message}）";
                        _logger?.LogError(ex, "心跳服务重启失败");
                    }
                }

                // 清理过期缓存
                CleanupExpiredCache();

                // 发送完成进度报告
                ReportProgress(result.RecordID, stationCode, "Completed", 100, result.Message);
            }
            return result;
        }

        /// <summary>
        /// 根据产品型号和版本号查询下发配置
        /// </summary>
        /// <param name="productModelCode">产品型号编码</param>
        /// <param name="version">版本号</param>
        /// <returns>返回下发配置主表及明细数据</returns>
        public async Task<Tuple<PLC_ParameterDistribution, List<PLC_ParameterDistributionDetail>>> QueryDistributionAsync(string productModelCode)
        {
            var distribution = _distributionRepository
                .QueryListByClause(d => d.ProductModelCode == productModelCode)
                .FirstOrDefault();

            if (distribution == null)
            {
                return Tuple.Create((PLC_ParameterDistribution)null, new List<PLC_ParameterDistributionDetail>());
            }

            var details = _detailRepository
                .QueryListByClause(d => d.DistributionID == distribution.ID)
                .ToList();

            return Tuple.Create(distribution, details);
        }

        /// <summary>
        /// 查询下发记录列表
        /// </summary>
        /// <param name="productModelCode">产品型号编码（可选）</param>
        /// <param name="plcCode">PLC编码（可选）</param>
        /// <returns>下发记录列表，按创建时间倒序排列</returns>
        public async Task<List<PLC_ParameterDistributionRecord>> QueryRecordsAsync(string productModelCode = null, string plcCode = null)
        {
            var records = _recordRepository.QueryListByClause(r => 
                (string.IsNullOrEmpty(productModelCode) || r.ProductModelCode == productModelCode) &&
                (string.IsNullOrEmpty(plcCode) || r.PlcCode == plcCode))
                .OrderByDescending(r => r.CreateTime)
                .ToList();

            return records;
        }

        /// <summary>
        /// 查询下发记录详情（每个点位的下发结果）
        /// </summary>
        /// <param name="recordID">记录ID</param>
        /// <returns>记录详情列表，包含每个点位的下发状态、错误信息等</returns>
        public async Task<List<PLC_ParameterDistributionRecordDetail>> QueryRecordDetailsAsync(long recordID)
        {
            return _recordDetailRepository
                .QueryListByClause(d => d.RecordID == recordID)
                .ToList();
        }

        /// <summary>
        /// 创建下发批次记录
        /// </summary>
        /// <param name="distribution">下发配置</param>
        /// <param name="details">下发配置明细</param>
        /// <param name="user">操作人</param>
        /// <returns>下发批次记录</returns>
        private async Task<PLC_ParameterDistributionRecord> CreateDistributionRecordAsync(PLC_ParameterDistribution distribution,
            List<PLC_ParameterDistributionDetail> details, string user)
        {
            var plcCode = details.First().PlcCode;
            var record = new PLC_ParameterDistributionRecord
            {
                DistributionID = distribution.ID,
                RecordCode = $"DR{DateTime.Now:yyyyMMddHHmmss}",
                ProductModelCode = distribution.ProductModelCode,
                DistributionType = "Combined",
                PlcCode = plcCode,
                EquipmentCode = details.FirstOrDefault()?.EquipmentCode,
                StationCode = details.FirstOrDefault()?.StationCode,
                Status = "Pending",
                StartTime = DateTime.Now,
                DistributionUser = user,
                CreateTime = DateTime.Now,
                CreateUser = user
            };

            _recordRepository.Insert(record);
            return record;
        }

        /// <summary>
        /// 保存下发前参数快照到历史表
        /// 读取PLC当前参数值，并与计划下发的新值一起保存
        /// 采用批量读取方式提高效率
        /// </summary>
        /// <param name="recordID">下发记录ID</param>
        /// <param name="distribution">下发配置</param>
        /// <param name="details">下发配置明细</param>
        /// <returns>历史记录ID</returns>
        private async Task<long> SaveParameterHistoryAsync(long recordID, PLC_ParameterDistribution distribution,
            List<PLC_ParameterDistributionDetail> details)
        {
            var history = new PLC_ParameterDistributionHistory
            {
                RecordID = recordID,
                ProductModelCode = distribution.ProductModelCode,
                SnapshotTime = DateTime.Now,
                CreateTime = DateTime.Now
            };

            _historyRepository.Insert(history);

            // 使用批量读取方式获取PLC参数值
            var valuesByAddress = await BatchReadPlcValuesAsync(details);

            var historyDetails = new List<PLC_ParameterDistributionDetailHistory>();

            foreach (var detail in details)
            {
                string originalValue = null;
                var key = $"{detail.PlcCode}_{detail.StationCode}_{detail.AddressCode}";
                if (valuesByAddress.TryGetValue(key, out var value))
                {
                    originalValue = value;
                }
                else
                {
                    originalValue = "读取失败";
                }

                historyDetails.Add(new PLC_ParameterDistributionDetailHistory
                {
                    HistoryID = history.ID,
                    PlcCode = detail.PlcCode,
                    EquipmentCode = detail.EquipmentCode,
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
                _historyDetailRepository.Insert(historyDetails);
            }

            return history.ID;
        }

        /// <summary>
        /// 批量读取PLC参数值
        /// 采用并行读取方式提高效率：
        /// 1. 按PLC分组
        /// 2. 每组内按数据类型并行读取
        /// 3. 布尔值支持连续地址批量读取
        /// </summary>
        /// <param name="details">下发配置明细</param>
        /// <returns>参数值字典，Key为 "PlcCode_AddressCode"</returns>
        private async Task<Dictionary<string, string>> BatchReadPlcValuesAsync(List<PLC_ParameterDistributionDetail> details)
        {
            var result = new ConcurrentDictionary<string, string>();

            if (!details.Any())
                return result.ToDictionary(kv => kv.Key, kv => kv.Value);

            // 按PLC分组
            var groupedByPlc = details.GroupBy(d => d.PlcCode).ToList();

            foreach (var plcGroup in groupedByPlc)
            {
                var plcCode = plcGroup.Key;

                // 按数据类型分组
                var groupedByType = plcGroup.GroupBy(d => d.DataType?.ToLower()).ToList();

                var tasks = new List<Task>();

                foreach (var typeGroup in groupedByType)
                {
                    var dataType = typeGroup.Key ?? "int";
                    var addresses = typeGroup.ToList();

                    tasks.Add(Task.Run(async () =>
                    {
                        await ReadValuesByTypeParallelAsync(plcCode, dataType, addresses, result);
                    }));
                }

                await Task.WhenAll(tasks);
            }

            return result.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// 按数据类型并行读取PLC值
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="addresses">地址列表</param>
        /// <param name="result">结果字典</param>
        private async Task ReadValuesByTypeParallelAsync(string plcCode, string dataType, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            switch (dataType)
            {
                case "bool":
                    await ReadBoolBatchOptimizedAsync(plcCode, addresses, result);
                    break;
                case "int":
                case "int32":
                    await ReadInt32ParallelAsync(plcCode, addresses, result);
                    break;
                case "int16":
                    await ReadInt16ParallelAsync(plcCode, addresses, result);
                    break;
                case "float":
                    await ReadFloatParallelAsync(plcCode, addresses, result);
                    break;
                case "double":
                    await ReadDoubleParallelAsync(plcCode, addresses, result);
                    break;
                case "string":
                    await ReadStringParallelAsync(plcCode, addresses, result);
                    break;
                default:
                    await ReadInt32ParallelAsync(plcCode, addresses, result);
                    break;
            }
        }

        /// <summary>
        /// 优化的布尔值批量读取
        /// 尝试将连续地址合并为批量读取，非连续地址并行读取
        /// </summary>
        private async Task ReadBoolBatchOptimizedAsync(string plcCode, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            // 按地址排序并分组连续地址
            var sortedAddresses = addresses.OrderBy(a => ParseAddressOffset(a.AddressCode)).ToList();
            var consecutiveGroups = GroupConsecutiveAddresses(sortedAddresses);

            foreach (var group in consecutiveGroups)
            {
                if (group.Count == 1)
                {
                    // 单个地址，直接读取
                    try
                    {
                        var address = group.First();
                        var boolResult = await Task.Run(() => _plcCommunicationService.ReadBool(plcCode, address.AddressCode));
                        result[$"{plcCode}_{address.AddressCode}"] = boolResult.IsSuccess ? boolResult.Content.ToString() : "读取失败";
                    }
                    catch
                    {
                        result[$"{plcCode}_{group.First().AddressCode}"] = "读取失败";
                    }
                }
                else
                {
                    // 连续地址，批量读取
                    try
                    {
                        var firstAddress = group.First();
                        var length = (ushort)group.Count;
                        var batchResult = await Task.Run(() => _plcCommunicationService.ReadBool(plcCode, firstAddress.AddressCode, length));

                        if (batchResult.IsSuccess && batchResult.Content != null)
                        {
                            for (int i = 0; i < group.Count && i < batchResult.Content.Length; i++)
                            {
                                result[$"{plcCode}_{group[i].AddressCode}"] = batchResult.Content[i].ToString();
                            }
                        }
                        else
                        {
                            // 批量读取失败，降级为逐个读取
                            foreach (var address in group)
                            {
                                try
                                {
                                    var boolResult = await Task.Run(() => _plcCommunicationService.ReadBool(plcCode, address.AddressCode));
                                    result[$"{plcCode}_{address.AddressCode}"] = boolResult.IsSuccess ? boolResult.Content.ToString() : "读取失败";
                                }
                                catch
                                {
                                    result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 批量读取失败，降级为逐个读取
                        foreach (var address in group)
                        {
                            try
                            {
                                var boolResult = await Task.Run(() => _plcCommunicationService.ReadBool(plcCode, address.AddressCode));
                                result[$"{plcCode}_{address.AddressCode}"] = boolResult.IsSuccess ? boolResult.Content.ToString() : "读取失败";
                            }
                            catch
                            {
                                result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将连续地址分组
        /// </summary>
        private List<List<PLC_ParameterDistributionDetail>> GroupConsecutiveAddresses(List<PLC_ParameterDistributionDetail> addresses)
        {
            var result = new List<List<PLC_ParameterDistributionDetail>>();
            if (!addresses.Any()) return result;

            var currentGroup = new List<PLC_ParameterDistributionDetail> { addresses[0] };

            for (int i = 1; i < addresses.Count; i++)
            {
                var prevOffset = ParseAddressOffset(currentGroup.Last().AddressCode);
                var currOffset = ParseAddressOffset(addresses[i].AddressCode);

                if (currOffset == prevOffset + 1)
                {
                    currentGroup.Add(addresses[i]);
                }
                else
                {
                    result.Add(currentGroup);
                    currentGroup = new List<PLC_ParameterDistributionDetail> { addresses[i] };
                }
            }

            result.Add(currentGroup);
            return result;
        }

        /// <summary>
        /// 解析地址偏移量
        /// </summary>
        private int ParseAddressOffset(string address)
        {
            try
            {
                var numberPart = new string(address.Where(c => char.IsDigit(c) || c == '.').ToArray());
                if (double.TryParse(numberPart, out var value))
                {
                    return (int)value;
                }
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// 并行读取Int32值
        /// </summary>
        private async Task ReadInt32ParallelAsync(string plcCode, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            var tasks = addresses.Select(address => Task.Run(() =>
            {
                try
                {
                    var intResult = _plcCommunicationService.ReadInt32(plcCode, address.AddressCode);
                    result[$"{plcCode}_{address.AddressCode}"] = intResult.IsSuccess ? intResult.Content.ToString() : "读取失败";
                }
                catch
                {
                    result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                }
            })).ToList();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 并行读取Int16值
        /// </summary>
        private async Task ReadInt16ParallelAsync(string plcCode, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            var tasks = addresses.Select(address => Task.Run(() =>
            {
                try
                {
                    var shortResult = _plcCommunicationService.ReadInt32(plcCode, address.AddressCode);
                    result[$"{plcCode}_{address.AddressCode}"] = shortResult.IsSuccess ? ((short)shortResult.Content).ToString() : "读取失败";
                }
                catch
                {
                    result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                }
            })).ToList();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 并行读取Float值
        /// </summary>
        private async Task ReadFloatParallelAsync(string plcCode, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            var tasks = addresses.Select(address => Task.Run(() =>
            {
                try
                {
                    var floatResult = _plcCommunicationService.ReadFloat(plcCode, address.AddressCode);
                    result[$"{plcCode}_{address.AddressCode}"] = floatResult.IsSuccess ? floatResult.Content.ToString("F2") : "读取失败";
                }
                catch
                {
                    result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                }
            })).ToList();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 并行读取Double值
        /// </summary>
        private async Task ReadDoubleParallelAsync(string plcCode, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            var tasks = addresses.Select(address => Task.Run(() =>
            {
                try
                {
                    var doubleResult = _plcCommunicationService.ReadFloat(plcCode, address.AddressCode);
                    result[$"{plcCode}_{address.AddressCode}"] = doubleResult.IsSuccess ? doubleResult.Content.ToString("F2") : "读取失败";
                }
                catch
                {
                    result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                }
            })).ToList();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 并行读取String值
        /// </summary>
        private async Task ReadStringParallelAsync(string plcCode, List<PLC_ParameterDistributionDetail> addresses, ConcurrentDictionary<string, string> result)
        {
            var tasks = addresses.Select(address => Task.Run(() =>
            {
                try
                {
                    var stringResult = _plcCommunicationService.ReadString(plcCode, address.AddressCode, 256);
                    result[$"{plcCode}_{address.AddressCode}"] = stringResult.IsSuccess ? stringResult.Content : "读取失败";
                }
                catch
                {
                    result[$"{plcCode}_{address.AddressCode}"] = "读取失败";
                }
            })).ToList();

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 向PLC写入单个参数
        /// </summary>
        /// <param name="recordID">下发记录ID</param>
        /// <param name="detail">下发配置明细</param>
        /// <returns>点位下发结果记录</returns>
        private async Task<PLC_ParameterDistributionRecordDetail> WriteParameterToPlcAsync(long recordID, PLC_ParameterDistributionDetail detail)
        {
            var recordDetail = new PLC_ParameterDistributionRecordDetail
            {
                RecordID = recordID,
                DetailID = detail.ID,
                PlcCode = detail.PlcCode,
                EquipmentCode = detail.EquipmentCode,
                StationCode = detail.StationCode,
                AddressCode = detail.AddressCode,
                DataType = detail.DataType,
                ParamName = detail.ParamName,
                ParamValue = detail.ParamValue,
                DistributionTime = DateTime.Now
            };

            const int maxRetryCount = 3;
            const int retryDelayMs = 1000;
            int retryCount = 0;
            bool success = false;
            string lastErrorMessage = string.Empty;
            var startTime = DateTime.Now;

            while (retryCount < maxRetryCount && !success)
            {
                try
                {
                    const int writeTimeoutMs = 5000;
                    const int readTimeoutMs = 3000;
                    
                    OperateResult writeResult;
                    
                    // 根据数据类型转换并写入（带超时控制）
                    switch (detail.DataType?.ToLower())
                    {
                        case "bool":
                            writeResult = await WriteWithTimeoutAsync(detail.PlcCode, detail.AddressCode, bool.Parse(detail.ParamValue), writeTimeoutMs);
                            break;
                        case "int":
                        case "int32":
                            writeResult = await WriteWithTimeoutAsync(detail.PlcCode, detail.AddressCode, int.Parse(detail.ParamValue), writeTimeoutMs);
                            break;
                        case "int16":
                            writeResult = await WriteWithTimeoutAsync(detail.PlcCode, detail.AddressCode, (int)short.Parse(detail.ParamValue), writeTimeoutMs);
                            break;
                        case "float":
                            writeResult = await WriteWithTimeoutAsync(detail.PlcCode, detail.AddressCode, float.Parse(detail.ParamValue), writeTimeoutMs);
                            break;
                        case "double":
                            writeResult = await WriteWithTimeoutAsync(detail.PlcCode, detail.AddressCode, float.Parse(detail.ParamValue), writeTimeoutMs);
                            break;
                        case "string":
                        default:
                            writeResult = await WriteWithTimeoutAsync(detail.PlcCode, detail.AddressCode, detail.ParamValue, writeTimeoutMs);
                            break;
                    }

                    if (writeResult.IsSuccess)
                    {
                        // 写入验证：读取回来确认写入成功（带超时控制）
                        bool verifySuccess = await VerifyWriteWithTimeoutAsync(detail, readTimeoutMs);
                        if (verifySuccess)
                        {
                            recordDetail.Status = "Success";
                            success = true;
                        }
                        else
                        {
                            lastErrorMessage = "写入验证失败，值未正确写入PLC";
                        }
                    }
                    else
                    {
                        lastErrorMessage = writeResult.Message;
                    }
                }
                catch (Exception ex)
                {
                    lastErrorMessage = ex.Message;
                }
                
                if (!success && retryCount < maxRetryCount - 1)
                {
                    retryCount++;
                    await Task.Delay(retryDelayMs * retryCount); // 指数退避
                }
            }
            
            recordDetail.DurationMs = (DateTime.Now - startTime).Milliseconds;
            
            if (!success)
            {
                recordDetail.Status = "Failed";
                recordDetail.ErrorMessage = lastErrorMessage;
            }

            return recordDetail;
        }

        /// <summary>
        /// 带超时控制的写入操作
        /// </summary>
        private async Task<OperateResult> WriteWithTimeoutAsync(string plcCode, string addressCode, object value, int timeoutMs)
        {
            try
            {
                var cts = new CancellationTokenSource(timeoutMs);
                var task = Task.Run(() => 
                {
                    if (value is bool boolValue)
                        return _plcCommunicationService.Write(plcCode, addressCode, boolValue);
                    else if (value is int intValue)
                        return _plcCommunicationService.Write(plcCode, addressCode, intValue);
                    else if (value is float floatValue)
                        return _plcCommunicationService.Write(plcCode, addressCode, floatValue);
                    else if (value is string stringValue)
                        return _plcCommunicationService.Write(plcCode, addressCode, stringValue);
                    else
                        return new OperateResult { IsSuccess = false, Message = $"不支持的数据类型：{value.GetType()}" };
                }, cts.Token);
                
                if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
                {
                    return await task;
                }
                else
                {
                    cts.Cancel();
                    return new OperateResult { IsSuccess = false, Message = $"写入超时（{timeoutMs}ms）" };
                }
            }
            catch (OperationCanceledException)
            {
                return new OperateResult { IsSuccess = false, Message = "写入被取消" };
            }
            catch (Exception ex)
            {
                return new OperateResult { IsSuccess = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 带超时控制的写入验证（写后读）
        /// </summary>
        /// <param name="detail">下发配置明细</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>验证成功返回 true，否则返回 false</returns>
        private async Task<bool> VerifyWriteWithTimeoutAsync(PLC_ParameterDistributionDetail detail, int timeoutMs)
        {
            try
            {
                var cts = new CancellationTokenSource(timeoutMs);
                var task = Task.Run(() => 
                {
                    return ReadPlcValueAsync(detail.PlcCode, detail.AddressCode, detail.DataType).Result;
                }, cts.Token);
                
                string readValue;
                if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
                {
                    readValue = await task;
                }
                else
                {
                    cts.Cancel();
                    return false;
                }
                
                if (string.IsNullOrEmpty(readValue))
                    return false;
                
                // 比较读取值与写入值
                return string.Equals(readValue, detail.ParamValue, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从PLC读取参数值
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="addressCode">地址编码</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>读取到的值</returns>
        private async Task<string> ReadPlcValueAsync(string plcCode, string addressCode, string dataType)
        {
            return await Task.Run(() =>
            {
                switch (dataType?.ToLower())
                {
                    case "bool":
                        var boolResult = _plcCommunicationService.ReadBool(plcCode, addressCode);
                        return boolResult.IsSuccess ? boolResult.Content.ToString() : null;
                    case "int":
                    case "int32":
                        var intResult = _plcCommunicationService.ReadInt32(plcCode, addressCode);
                        return intResult.IsSuccess ? intResult.Content.ToString() : null;
                    case "int16":
                        var shortResult = _plcCommunicationService.ReadInt32(plcCode, addressCode);
                        return shortResult.IsSuccess ? ((short)shortResult.Content).ToString() : null;
                    case "float":
                        var floatResult = _plcCommunicationService.ReadFloat(plcCode, addressCode);
                        return floatResult.IsSuccess ? floatResult.Content.ToString("F2") : null;
                    case "double":
                        var doubleResult = _plcCommunicationService.ReadFloat(plcCode, addressCode);
                        return doubleResult.IsSuccess ? doubleResult.Content.ToString("F2") : null;
                    case "string":
                        var stringResult = _plcCommunicationService.ReadString(plcCode, addressCode, 256);
                        return stringResult.IsSuccess ? stringResult.Content : null;
                    default:
                        var defaultResult = _plcCommunicationService.ReadInt32(plcCode, addressCode);
                        return defaultResult.IsSuccess ? defaultResult.Content.ToString() : null;
                }
            });
        }

        /// <summary>
        /// 验证PLC连接状态（包含心跳检测）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="errorMessage">错误信息输出</param>
        /// <returns>连接有效返回true，否则返回false</returns>
        private bool ValidatePlcConnection(string plcCode, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // 1. 基础连接检查
            if (!_connectionManager.IsConnected(plcCode))
            {
                errorMessage = $"PLC [{plcCode}] 未连接";
                return false;
            }
            
            // 2. 心跳检测（尝试读取一个已知的状态位）
            try
            {
                const int heartbeatTimeoutMs = 2000;
                var cts = new global::System.Threading.CancellationTokenSource(heartbeatTimeoutMs);
                var task = global::System.Threading.Tasks.Task.Run(() =>    
                {
                    return _plcCommunicationService.ReadBool(plcCode, "M0.0");
                }, cts.Token);
                
                HslCommunication.OperateResult heartbeatResult;
                if (global::System.Threading.Tasks.Task.WhenAny(task, global::System.Threading.Tasks.Task.Delay(heartbeatTimeoutMs)).Result == task)
                {
                    heartbeatResult = task.Result;
                }
                else
                {
                    cts.Cancel();
                    errorMessage = $"PLC [{plcCode}] 连接超时，无法通信";
                    return false;
                }
                
                if (!heartbeatResult.IsSuccess)
                {
                    errorMessage = $"PLC [{plcCode}] 连接异常，无法通信: {heartbeatResult.Message}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"PLC [{plcCode}] 通信检测失败: {ex.Message}";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 获取下发类型的排序值
        /// 确保按顺序下发：BeforeDistribution -> Dispatching -> AfterDistribution
        /// </summary>
        /// <param name="distributionType">下发类型</param>
        /// <returns>排序值</returns>
        private int GetDistributionOrder(string distributionType)
        {
            return distributionType switch
            {
                "BeforeDistribution" => 1,
                "Dispatching" => 2,
                "AfterDistribution" => 3,
                _ => 2
            };
        }

        /// <summary>
        /// 回滚已写入的参数（恢复到下发前的快照值）
        /// </summary>
        /// <param name="recordID">下发记录 ID</param>
        /// <param name="details">下发配置明细</param>
        /// <returns>回滚结果</returns>
        private async Task<bool> RollbackParametersAsync(long recordID, List<PLC_ParameterDistributionDetail> details)
        {
            try
            {
                // 查询下发前的历史快照
                var historyDetails = _historyDetailRepository
                    .QueryListByClause(h => h.HistoryID == recordID)
                    .ToList();
                
                if (!historyDetails.Any())
                {
                    return false;
                }

                // 批量回滚参数
                var rollbackRequests = new List<BatchWriteRequest>();
                foreach (var detail in details)
                {
                    var history = historyDetails.FirstOrDefault(h => h.AddressCode == detail.AddressCode);
                    if (history != null && !string.IsNullOrEmpty(history.OriginalValue) && history.OriginalValue != "读取失败")
                    {
                        rollbackRequests.Add(new BatchWriteRequest
                        {
                            Index = rollbackRequests.Count,
                            Address = detail.AddressCode,
                            Value = history.OriginalValue,
                            DataType = detail.DataType
                        });
                    }
                }

                if (rollbackRequests.Any())
                {
                    // 批量写回历史值
                    var firstDetail = details.First();
                    var rollbackResults = await _plcCommunicationService.WriteBatchAsync(firstDetail.PlcCode, rollbackRequests);
                    
                    int rollbackSuccessCount = rollbackResults.Count(r => r);
                    int rollbackFailedCount = rollbackResults.Length - rollbackSuccessCount;
                    
                    if (rollbackFailedCount > 0)
                    {
                        // 记录回滚失败的点位
                        for (int i = 0; i < rollbackRequests.Count; i++)
                        {
                            if (!rollbackResults[i])
                            {
                                // 回滚失败，记录日志（实际项目中应该写入日志系统）
                                _logger.LogError($"回滚失败：{rollbackRequests[i].Address}");
                            }
                        }
                    }
                    
                    return rollbackFailedCount == 0;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"回滚异常：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 查询所有下发配置列表
        /// </summary>
        public async Task<List<PLC_ParameterDistribution>> QueryDistributionsAsync(string productModelCode = null, string version = null, string routingCode = null)
        {
            List<PLC_ParameterDistribution> distributions;
            
            if (!string.IsNullOrEmpty(productModelCode) || !string.IsNullOrEmpty(version) || !string.IsNullOrEmpty(routingCode))
            {
                distributions = _distributionRepository.QueryListByClause(d => 
                    (string.IsNullOrEmpty(productModelCode) || d.ProductModelCode == productModelCode) &&
                    (string.IsNullOrEmpty(version) || d.RecipeCode == version) &&
                    (string.IsNullOrEmpty(routingCode) || d.RoutingCode == routingCode), 
                    "CreateDate DESC");
            }
            else
            {
                distributions = _distributionRepository.Query().OrderByDescending(d => d.CreateDate).ToList();
            }
            
            return distributions;
        }

        /// <summary>
        /// 根据下发配置ID查询参数列表
        /// </summary>
        /// <param name="distributionID">下发配置ID</param>
        /// <returns>参数详情列表</returns>
        public async Task<List<PLC_ParameterDistributionDetail>> QueryParamsByDistributionAsync(long distributionID)
        {
            var details = _detailRepository.QueryListByClause(d => d.DistributionID == distributionID, "SortOrder ASC");
            return details.ToList();
        }

        /// <summary>
        /// 根据ID获取下发配置
        /// </summary>
        public async Task<PLC_ParameterDistribution> GetDistributionByIdAsync(long id)
        {
            return _distributionRepository.QueryById(id);
        }

        /// <summary>
        /// 添加下发配置
        /// </summary>
        public async Task<long> AddDistributionAsync(PLC_ParameterDistribution distribution)
        {
            distribution.CreateDate = DateTime.Now;
            distribution.UpdateTime = DateTime.Now;
            return _distributionRepository.Insert(distribution);
        }

        /// <summary>
        /// 更新下发配置
        /// </summary>
        public async Task<bool> UpdateDistributionAsync(PLC_ParameterDistribution distribution)
        {
            distribution.UpdateTime = DateTime.Now;
            return _distributionRepository.Update(distribution);
        }

        /// <summary>
        /// 删除下发配置（级联删除明细）
        /// </summary>
        public async Task<bool> DeleteDistributionAsync(long id)
        {
            // 先删除明细
            await DeleteDetailsByDistributionAsync(id);
            return _distributionRepository.DeleteById(id);
        }

        /// <summary>
        /// 添加参数下发明细
        /// </summary>
        /// <param name="detail">明细实体</param>
        /// <returns>新增记录的ID</returns>
        public async Task<long> AddDetailAsync(PLC_ParameterDistributionDetail detail)
        {
            detail.CreateTime = DateTime.Now;
            detail.UpdateTime = DateTime.Now;
            return _detailRepository.Insert(detail);
        }

        /// <summary>
        /// 更新参数下发明细
        /// </summary>
        /// <param name="detail">明细实体</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateDetailAsync(PLC_ParameterDistributionDetail detail)
        {
            detail.UpdateTime = DateTime.Now;
            return _detailRepository.Update(detail);
        }

        /// <summary>
        /// 删除参数下发明细
        /// </summary>
        /// <param name="id">明细ID</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> DeleteDetailAsync(long id)
        {
            return _detailRepository.DeleteById(id);
        }

        /// <summary>
        /// 根据下发配置ID删除所有明细（级联删除）
        /// </summary>
        /// <param name="distributionID">下发配置ID</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> DeleteDetailsByDistributionAsync(long distributionID)
        {
            var details = _detailRepository.QueryListByClause(d => d.DistributionID == distributionID);
            if (details.Any())
            {
                _detailRepository.Delete(details);
            }
            return true;
        }

        /// <summary>
        /// 批量添加参数下发明细
        /// </summary>
        /// <param name="details">明细实体列表</param>
        /// <returns>是否添加成功</returns>
        public async Task<bool> BatchAddDetailsAsync(List<PLC_ParameterDistributionDetail> details)
        {
            if (details == null || !details.Any())
                return true;

            var now = DateTime.Now;
            foreach (var detail in details)
            {
                detail.CreateTime = now;
                detail.UpdateTime = now;
            }

            _detailRepository.Insert(details);
            return true;
        }

    }
}
