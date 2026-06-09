using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("WipProcessingIrreversible")]
    public class WipProcessingIrreversible
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 不可逆条码（加工条码）
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 配方编码
        /// </summary>
        public string RecipeCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUser { get; set; }
    }
}