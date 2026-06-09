using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework.Models.Basic
{
    ///<summary>
    ///工站管理表
    ///</summary>
    [SugarTable("MD_Station")]
    public partial class MD_Station
    {
        public MD_Station()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long StationID { get; set; }

        /// <summary>
        /// Desc:工站编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StationCode { get; set; }

        /// <summary>
        /// Desc:工站名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string StationName { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// PLC编码
        /// 数据来源：PLC_CONFIG 表的 PlcCode 字段
        /// </summary>
        public string PlcCode { get; set; }


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUser { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string UpdateUser { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? UpdateTime { get; set; }

    }
}
