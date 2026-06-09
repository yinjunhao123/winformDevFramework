using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数下发历史明细表（保存下发前的设备参数快照明细）
    /// </summary>
    [SugarTable("PLC_ParameterDistributionDetailHistory")]
    public partial class PLC_ParameterDistributionDetailHistory
    {
        public PLC_ParameterDistributionDetailHistory()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 历史主表ID
        /// </summary>
        public long HistoryID { get; set; }

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
        /// 地址编码
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
        /// 下发前的原始值（从PLC读取的当前值）
        /// </summary>
        public string OriginalValue { get; set; }

        /// <summary>
        /// 计划下发的新值
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// 下发类型
        /// </summary>
        public string DistributionType { get; set; }
    }
}
