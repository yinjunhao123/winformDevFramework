using SqlSugar;
using System;

namespace PLCBasic
{
    /// <summary>
    /// 包装主表
    /// 存储包装箱信息，包含包装箱号、产品型号、工艺配方等
    /// </summary>
    [SugarTable("PackInfo")]
    public class PackInfo
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 包装箱号
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(100)", IsNullable = false)]
        public string PackBarcode { get; set; }

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
        /// 最大包装数量
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Maxqty { get; set; }

        /// <summary>
        /// 已打包数量
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Haveqty { get; set; } = 0;

        /// <summary>
        /// 待打包数量（未打包数量）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Unqty { get; set; } = 0;

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

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(50)")]
        public string UpdateUser { get; set; }
    }
}