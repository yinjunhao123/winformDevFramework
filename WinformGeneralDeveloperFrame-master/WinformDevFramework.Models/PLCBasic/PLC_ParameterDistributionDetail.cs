using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数下发明细表
    /// </summary>
    [SugarTable("PLC_ParameterDistributionDetail")]
    public partial class PLC_ParameterDistributionDetail
    {
        public PLC_ParameterDistributionDetail()
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
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 型号Code
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquipmentCode { get; set; }

        /// <summary>
        /// 地址编码
        /// </summary>
        public string AddressCode { get; set; }

        /// <summary>
        /// 数据类型 (Bool, Int, Float, String等)
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParamValue { get; set; }

        /// <summary>
        /// 下发类型 (BeforeDistribution: 前置下发, Dispatching: 调度下发, AfterDistribution: 后置下发)
        /// </summary>
        public string DistributionType { get; set; }

        /// <summary>
        /// 排序序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
