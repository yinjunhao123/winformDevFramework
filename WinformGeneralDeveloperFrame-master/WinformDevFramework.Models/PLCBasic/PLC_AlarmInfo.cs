using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    ///<summary>
    ///PLC报警详细信息定义表
    ///</summary>
    [SugarTable("PLC_AlarmInfo")]
    public partial class PLC_AlarmInfo
    {
        public PLC_AlarmInfo()
        {


        }
        /// <summary>
        /// Desc:报警信息主键
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long AlarmInfoID { get; set; }

        /// <summary>
        /// Desc:PLC编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PlcCode { get; set; }

        /// <summary>
        /// Desc:关联地址配置
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string AddressCode { get; set; }


        /// <summary>
        /// Desc:关联工站信息
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StationCode { get; set; }

        /// <summary>
        /// Desc:报警代码/位号 (如: 1001, 或 Bit0)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string AlarmCode { get; set; }

        /// <summary>
        /// Desc:报警名称 (如: 电机过载)
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string AlarmName { get; set; }

        /// <summary>
        /// Desc:报警级别 (如: Info, Warning, Error, Critical)
        /// Default:Error
        /// Nullable:True
        /// </summary>           
        public string AlarmLevel { get; set; }
        /// <summary>
        /// 报警详情
        /// </summary>
        public string AlarmDesc { get; set; }

        /// <summary>
        /// Desc:处理建议
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Suggestion { get; set; }

        /// <summary>
        /// Desc:是否启用
        /// Default:1
        /// Nullable:True
        /// </summary>           
        public bool IsActive { get; set; }

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
