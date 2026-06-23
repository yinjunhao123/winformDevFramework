using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCBasic
{
    /// <summary>
    /// PLC采集参数
    /// </summary>
    [SugarTable("PLC_TriggerParam")]
    public partial class PLC_TriggerParam
    {
        public PLC_TriggerParam()
        {

        }

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ParamID { get; set; }
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站Code
        /// </summary>
        public string StationCode { get; set; }


        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquipmentCode { get; set; }

        /// <summary>
        /// 地址编码
        /// </summary>
        public string AddressCode { get; set; }
        /// <summary>
        /// Desc:数据类型 (如: Bool, Int, Float, String)
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 处理方式 collect/MSA/real
        /// </summary>
        public string TreatmentType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateUser { get; set; }
    }
}
