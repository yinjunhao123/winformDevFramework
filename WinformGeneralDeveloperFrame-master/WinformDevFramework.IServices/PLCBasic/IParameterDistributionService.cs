using PLCBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IServices.PLCBasic
{
    /// <summary>
    /// 参数下发服务接口
    /// 负责执行PLC参数下发操作，包括下发前参数快照、按顺序下发、下发结果记录等功能
    /// </summary>
    public interface IParameterDistributionService : IBaseServices<PLC_ParameterDistribution>
    {
        /// <summary>
        /// 执行参数下发
        /// 下发流程：
        /// 1. 检查PLC连接状态
        /// 2. 停止心跳服务
        /// 3. 创建下发批次记录
        /// 4. 读取PLC当前参数值并保存到历史表
        /// 5. 按顺序下发参数（BeforeDistribution -> Dispatching -> AfterDistribution）
        /// 6. 记录每个点位的下发结果
        /// 7. 更新批次记录状态和统计信息
        /// 8. 启动心跳服务
        /// </summary>
        /// <param name="distributionID">下发配置ID</param>
        /// <param name="user">下发人</param>
        /// <returns>下发结果，包含成功/失败数量、记录ID等信息</returns>
        Task<DistributionResult> ExecuteDistributionAsync(long distributionID, string user,string plcCode,string stationCode);

        /// <summary>
        /// 根据产品型号和版本号查询下发配置
        /// </summary>
        /// <param name="productModelCode">产品型号编码</param>
        /// <param name="version">版本号</param>
        /// <returns>返回下发配置主表及明细数据</returns>
        Task<Tuple<PLC_ParameterDistribution, List<PLC_ParameterDistributionDetail>>> QueryDistributionAsync(string productModelCode);

        /// <summary>
        /// 查询下发记录列表
        /// </summary>
        /// <param name="productModelCode">产品型号编码（可选）</param>
        /// <param name="plcCode">PLC编码（可选）</param>
        /// <returns>下发记录列表，按创建时间倒序排列</returns>
        Task<List<PLC_ParameterDistributionRecord>> QueryRecordsAsync(string productModelCode = null, string plcCode = null);

        /// <summary>
        /// 查询下发记录详情（每个点位的下发结果）
        /// </summary>
        /// <param name="recordID">记录ID</param>
        /// <returns>记录详情列表，包含每个点位的下发状态、错误信息等</returns>
        Task<List<PLC_ParameterDistributionRecordDetail>> QueryRecordDetailsAsync(long recordID);

        /// <summary>
        /// 查询所有下发配置列表
        /// </summary>
        /// <param name="productModelCode">产品型号编码（可选）</param>
        /// <param name="version">版本号（可选）</param>
        /// <returns>下发配置列表</returns>
        Task<List<PLC_ParameterDistribution>> QueryDistributionsAsync(string productModelCode = null, string version = null, string routingCode = null);

        /// <summary>
        /// 根据ID获取下发配置
        /// </summary>
        /// <param name="id">配置ID</param>
        /// <returns>下发配置实体</returns>
        Task<PLC_ParameterDistribution> GetDistributionByIdAsync(long id);

        /// <summary>
        /// 根据下发配置ID查询参数列表
        /// </summary>
        /// <param name="distributionID">下发配置ID</param>
        /// <returns>参数详情列表</returns>
        Task<List<PLC_ParameterDistributionDetail>> QueryParamsByDistributionAsync(long distributionID);

        /// <summary>
        /// 添加下发配置
        /// </summary>
        /// <param name="distribution">下发配置实体</param>
        /// <returns>新增记录的ID</returns>
        Task<long> AddDistributionAsync(PLC_ParameterDistribution distribution);

        /// <summary>
        /// 更新下发配置
        /// </summary>
        /// <param name="distribution">下发配置实体</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateDistributionAsync(PLC_ParameterDistribution distribution);

        /// <summary>
        /// 删除下发配置
        /// </summary>
        /// <param name="id">配置ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteDistributionAsync(long id);

        /// <summary>
        /// 添加参数下发明细
        /// </summary>
        /// <param name="detail">明细实体</param>
        /// <returns>新增记录的ID</returns>
        Task<long> AddDetailAsync(PLC_ParameterDistributionDetail detail);

        /// <summary>
        /// 更新参数下发明细
        /// </summary>
        /// <param name="detail">明细实体</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateDetailAsync(PLC_ParameterDistributionDetail detail);

        /// <summary>
        /// 删除参数下发明细
        /// </summary>
        /// <param name="id">明细ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteDetailAsync(long id);

        /// <summary>
        /// 根据下发配置ID删除所有明细（级联删除）
        /// </summary>
        /// <param name="distributionID">下发配置ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteDetailsByDistributionAsync(long distributionID);

        /// <summary>
        /// 批量添加参数下发明细
        /// </summary>
        /// <param name="details">明细实体列表</param>
        /// <returns>是否添加成功</returns>
        Task<bool> BatchAddDetailsAsync(List<PLC_ParameterDistributionDetail> details);

    }

    /// <summary>
    /// 参数下发结果
    /// </summary>
    public class DistributionResult
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 下发时间
        /// </summary>
        public DateTime? DistributionTime { get; set; }

        /// <summary>
        /// 耗时（毫秒）
        /// </summary>
        public long DurationMs { get; set; }
        /// <summary>
        /// 工站代码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 是否全部成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 下发记录ID
        /// </summary>
        public long RecordID { get; set; }

        /// <summary>
        /// 下发记录编码
        /// </summary>
        public string RecordCode { get; set; }

        /// <summary>
        /// 下发参数总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 成功数量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败数量
        /// </summary>
        public int FailedCount { get; set; }
    }
}
