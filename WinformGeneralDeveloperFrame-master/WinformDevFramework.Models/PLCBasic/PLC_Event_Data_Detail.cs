using SqlSugar;
using System;

namespace WinformDevFramework.Models.PLCBasic
{
    [SugarTable("PLC_Event_Data_Detail")]
    public class PLC_Event_Data_Detail
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 触发ID
        /// </summary>
        public int EventId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ParamName { get; set; }
        /// <summary>
        /// PLC地址
        /// </summary>
        public string DataAddress { get; set; }
        /// <summary>
        /// 数据类型 (如: Bool, Int, Float, String)
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 采集类型 result(结果), quality(质量)  可不填
        /// </summary>
        public string DataCategory { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public int? Length { get; set; }
        /// <summary>
        /// read/write
        /// </summary>
        public string IOOperation { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string ConstantValue { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? SortOrder { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        public string CreateUser { get; set; }

        public DateTime? CreateTime { get; set; }

        public string UpdateUser { get; set; }

        public DateTime? UpdateTime { get; set; }

    }
}