using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("WipBatch")]
    public class WipBatch
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 批次编码
        /// </summary>
        public string BatchCode { get; set; }

        /// <summary>
        /// 状态  1.初始值 2.验证失败 3.验证成功 4完成
        /// </summary>
        public int? Status { get; set; }

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