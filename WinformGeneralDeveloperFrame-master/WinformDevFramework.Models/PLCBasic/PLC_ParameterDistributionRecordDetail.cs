using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数下发记录明细表（记录每个参数/点位的下发结果）
    /// </summary>
    [SugarTable("PLC_ParameterDistributionRecordDetail")]
    public partial class PLC_ParameterDistributionRecordDetail
    {
        public PLC_ParameterDistributionRecordDetail()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 下发记录主表ID
        /// </summary>
        public long RecordID { get; set; }

        /// <summary>
        /// 参数下发明细表ID（关联原始配置）
        /// </summary>
        public long DetailID { get; set; }

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
        /// PLC地址编码（如：DB1.DBW0）
        /// </summary>
        public string AddressCode { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 下发的值
        /// </summary>
        public string ParamValue { get; set; }

        /// <summary>
        /// 下发状态 (Success: 成功, Failed: 失败, Skipped: 跳过)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 错误信息（失败时记录）
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 下发耗时（毫秒）
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// 下发时间
        /// </summary>
        public DateTime? DistributionTime { get; set; }
    }
}
