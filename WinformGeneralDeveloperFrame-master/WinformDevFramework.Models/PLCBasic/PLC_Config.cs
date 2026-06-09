using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    ///<summary>
    ///PLC基础信息配置表
    ///</summary>
    [SugarTable("PLC_Config")]
    public partial class PLC_Config
    {
        public PLC_Config()
        {


        }
        /// <summary>
        /// Desc:PLC主键
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long PlcID { get; set; }

        /// <summary>
        /// Desc:PLC编码 (唯一标识)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PlcCode { get; set; }

        /// <summary>
        /// Desc:PLC名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PlcName { get; set; }

        /// <summary>
        /// Desc:IP地址
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string IpAddress { get; set; }

        /// <summary>
        /// Desc:端口
        /// Default:102
        /// Nullable:True
        /// </summary>           
        public int? Port { get; set; }

        /// <summary>
        /// Desc:协议类型 (如: S7, ModbusTCP, Omron)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PlcType { get; set; }

        /// <summary>0
        /// Desc:描述
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
