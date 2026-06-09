using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    ///<summary>
    ///PLC采集地址配置表（区分报警地址和状态地址）
    ///</summary>
    [SugarTable("PLC_Address")]
    public partial class PLC_Address
    {
        public PLC_Address()
        {


        }
        /// <summary>
        /// Desc:地址ID主键
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long AddressID { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>

        public string PlcCode { get; set; }

        /// <summary>
        /// Desc:地址编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string AddressCode { get; set; }

        /// <summary>
        /// Desc:地址类型 (如: Coil, Input, HoldingRegister)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string AddressType { get; set; }

        /// <summary>
        /// Desc:数据类型 (如: Bool, Int, Float, String)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string DataType { get; set; }

        /// <summary>
        /// Desc:cc (枚举: 'DeviceAlarm' 设备报警, 'DeviceStatus' 设备状态，'Trigger' 触发地址码,'OperationMethod'运行方式)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Category { get; set; }


        /// <summary>
        /// Desc:工站编码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string StationCode { get; set; }

        /// <summary>
        /// Desc:设备编码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string EquipmentCode { get; set; }

        /// <summary>
        /// Desc:地址含义描述
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Description { get; set; }

        /// <summary>
        /// Desc:创建人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUser { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:DateTime.Now
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Desc:修改人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string UpdateUser { get; set; }

        /// <summary>
        /// Desc:修改时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? UpdateTime { get; set; }

    }
}
