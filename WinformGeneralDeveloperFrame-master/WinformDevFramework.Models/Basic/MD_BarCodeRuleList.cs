using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.Basic
{
    /// <summary>
    /// 条码规则明细表
    /// </summary>
    [SugarTable("MD_BarCodeRuleList")]
    public partial class MD_BarCodeRuleList
    {
        public MD_BarCodeRuleList()
        {
            SortOrder = 1;
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long BarCodeRuleListId { get; set; }

        /// <summary>
        /// 条码规则主键
        /// </summary>
        public long BarCodeRuleId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int SortOrder { get; set; } = 1;

        /// <summary>
        /// 类型
        /// </summary>
        public string SegmentType { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public int? FixedLength { get; set; }

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
