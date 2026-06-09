using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.Basic
{
    /// <summary>
    /// 条码规则主表
    /// </summary>
    [SugarTable("MD_BarCodeRule")]
    public partial class MD_BarCodeRule
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long BarCodeRuleId { get; set; }

        /// <summary>
        /// 条码规则名称
        /// </summary>
        public string BarCodeName { get; set; }

        /// <summary>
        /// 工站
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 条码规则类型 Main/Sub/Batch
        /// </summary>
        public string BarCodeRuleType { get; set; }

        /// <summary>
        /// 条码位数
        /// </summary>
        public int BarCodeLength { get; set; }

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
