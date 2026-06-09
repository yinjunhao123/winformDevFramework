using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数下发记录主表（每次下发生成一条批次记录）
    /// </summary>
    [SugarTable("PLC_ParameterDistributionRecord")]
    public partial class PLC_ParameterDistributionRecord
    {
        public PLC_ParameterDistributionRecord()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 参数下发主表ID
        /// </summary>
        public long DistributionID { get; set; }

        /// <summary>
        /// 下发记录编码（格式：DR+时间戳）
        /// </summary>
        public string RecordCode { get; set; }

        /// <summary>
        /// 产品型号编码
        /// </summary>
        public string ProductModelCode { get; set; }
        /// <summary>
        /// 参数版本号（即RecipeCode）
        /// </summary>
        public string RecipeCode { get; set; }

        /// <summary>
        /// 下发类型 (BeforeDistribution: 前置下发, Dispatching: 调度下发, AfterDistribution: 后置下发)
        /// </summary>
        public string DistributionType { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquipmentCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 总体下发状态 (Pending: 待下发, Success: 全部成功, PartialSuccess: 部分成功, Failed: 全部失败)
        /// </summary>
        public string Status { get; set; }

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

        /// <summary>
        /// 下发结果消息（总体描述）
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 下发开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 下发结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 下发耗时（毫秒）
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// 下发人
        /// </summary>
        public string DistributionUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
    }
}
