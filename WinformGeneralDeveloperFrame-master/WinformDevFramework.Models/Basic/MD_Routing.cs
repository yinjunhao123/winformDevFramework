using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework.Models.Basic
{
    ///<summary>
    ///工艺路线表（产品与工站关系）
    ///</summary>
    [SugarTable("MD_Routing")]
    public partial class MD_Routing
    {
        public MD_Routing()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long RoutingID { get; set; }

        /// <summary>
        /// Desc:工艺路线编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string RoutingCode { get; set; }

        /// <summary>
        /// Desc:工艺路线名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string RoutingName { get; set; }


        /// <summary>
        /// Desc:是否启用
        /// Default:1
        /// Nullable:True
        /// </summary>           
        public int Status { get; set; }

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
