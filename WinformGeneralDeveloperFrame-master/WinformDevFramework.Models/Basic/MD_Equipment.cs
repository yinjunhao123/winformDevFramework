using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework.Models.Basic
{
    ///<summary>
    ///设备主数据表
    ///</summary>
    [SugarTable("MD_Equipment")]
    public partial class MD_Equipment
    {
        public MD_Equipment()
        {


        }
        /// <summary>
        /// Desc:设备id
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long EquipmentID { get; set; }

        /// <summary>
        /// Desc:设备编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string EquipmentCode { get; set; }

        /// <summary>
        /// Desc:设备名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string EquipmentName { get; set; }
        /// <summary>
        /// PLC编码
        /// </summary>

        public string PlcCode { get; set; }

        /// <summary>
        /// 工站Code
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// Desc:设备状态
        /// Default:1代表使用/0代表停用
        /// Nullable:True
        /// </summary>           
        public int? Status { get; set; }

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
