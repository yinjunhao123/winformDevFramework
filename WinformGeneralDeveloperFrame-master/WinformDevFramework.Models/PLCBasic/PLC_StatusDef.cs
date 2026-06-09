using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    ///<summary>
    ///PLC设备状态值定义表（值与含义的映射）
    ///</summary>
    [SugarTable("PLC_StatusDef")]
    public partial class PLC_StatusDef
    {
        public PLC_StatusDef()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long StatusDefID { get; set; }

        /// <summary>
        /// Desc:PLC编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PlcCode { get; set; }

        /// <summary>
        /// Desc:PLC地址编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string AddressCode { get; set; }

        /// <summary>
        /// 工站Code
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// Desc:PLC状态编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StatusCode { get; set; }

        /// <summary>
        /// Desc:PLC状态名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StatusName { get; set; }

        /// <summary>
        /// Desc:创建用户
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
        /// Desc:修改用户
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
