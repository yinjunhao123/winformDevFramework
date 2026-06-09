using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.Basic
{
    /// <summary>
    /// 工站零件绑定表
    /// </summary>
    [SugarTable("MD_StationPart")]
    public partial class MD_StationPart
    {
        public MD_StationPart()
        {
            State = true;
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long StationPartId { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 零件名称
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// 零件类型（Main-主零件，Sub-子零件,Batch-批次零件）
        /// </summary>
        public string PartType { get; set; }


        /// <summary>
        /// 状态（1-启用，0-禁用）
        /// </summary>
        public bool? State { get; set; } = true;

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
