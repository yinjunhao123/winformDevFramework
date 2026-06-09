using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCBasic
{
    [SugarTable("PLC_RealTimeStatus")]
    public partial class PLC_RealTimeStatus
    {
        public PLC_RealTimeStatus()
        {

        }
        /// <summary>
        /// 主键ID
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long StatusID { get; set; }
        /// <summary>
        /// plc编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// PLC地址编码
        /// </summary>
        public string AddressCode { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentCode { get; set; }

        /// <summary>
        /// 工站编号
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 对应值
        /// </summary>
        public string StatusValue { get; set; }
        /// <summary>
        /// 值名称
        /// </summary>
        public string StatusName { get; set; }
        /// <summary>
        /// 收集时间
        /// </summary>
        public DateTime CollectTime { get; set; }
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }
    }
}
