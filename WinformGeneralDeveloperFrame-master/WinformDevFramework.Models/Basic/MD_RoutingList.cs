using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.Basic
{
    /// <summary>
    /// 工艺路线详情表
    /// </summary>
    [SugarTable("MD_RoutingList")]
    public partial class MD_RoutingList
    {
        public MD_RoutingList()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long RoutingListId { get; set; }

        /// <summary>
        /// 工艺路线ID
        /// </summary>
        public long RoutingId { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// Plc编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 排序序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool? State { get; set; }
        /// <summary>
        /// 是否可逆工站
        /// </summary>

        public bool? IsReversible { get; set; }

        /// <summary>
        /// 是否包装工站
        /// </summary>
        public bool? IsPacking { get; set; }
        /// <summary>
        /// 最大测试次数
        /// </summary>

        public int MaxTestCount { get; set; }

        /// <summary>
        /// 主件名称
        /// </summary>
        public string MainPartsName { get; set; }

        /// <summary>
        /// 是否扫描主件
        /// </summary>
        public bool? IsScanMain { get; set; }


        /// <summary>
        /// 批次名称
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string BatchName { get; set; }
        /// <summary>
        /// 是否扫描批次
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool? IsScanBatch { get; set; }

        /// <summary>
        /// 第一零件名称
        /// </summary>
        public string FirstPartsName { get; set; }

        /// <summary>
        /// 是否扫描第一零件
        /// </summary>
        public bool? IsScanFirst { get; set; }

        /// <summary>
        /// 第二零件名称
        /// </summary>
        public string SecondPartsName { get; set; }

        /// <summary>
        /// 是否扫描第二零件
        /// </summary>
        public bool? IsScanSecond { get; set; }

        /// <summary>
        /// 第三零件名称
        /// </summary>
        public string ThirdPartsName { get; set; }

        /// <summary>
        /// 是否扫描第三零件
        /// </summary>
        public bool? IsScanThird { get; set; }

        /// <summary>
        /// 前置工站
        /// </summary>
        public string BeforeStation { get; set; }

        /// <summary>
        /// 主件规则
        /// </summary>
        public string MainPartsRule { get; set; }

        /// <summary>
        /// 批次规则
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string BatchRule { get; set; }
        
        /// <summary>
        /// 第一零件规则
        /// </summary>
        public string? FirstPartsRule { get; set; }

        /// <summary>
        /// 第二零件规则
        /// </summary>
        public string SecondPartsRule { get; set; }

        /// <summary>
        /// 第三零件规则
        /// </summary>
        public string ThirdPartsRule { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }
    }
}
