using SqlSugar;
using System;

namespace PLCBasic
{
    /// <summary>
    /// 包装明细表
    /// 存储每个包装箱下的条码明细信息
    /// </summary>
    [SugarTable("PackInfoDetail")]
    public class PackInfoDetail
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 包装主表ID
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long PackInfoId { get; set; }

        /// <summary>
        /// 条码信息
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(100)", IsNullable = false)]
        public string BarCode { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(50)", IsNullable = false)]
        public string ProductModel { get; set; }

        /// <summary>
        /// 配方编码
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(50)", IsNullable = false)]
        public string RecipeCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(50)", IsNullable = false)]
        public string CreateUser { get; set; }
    }
}