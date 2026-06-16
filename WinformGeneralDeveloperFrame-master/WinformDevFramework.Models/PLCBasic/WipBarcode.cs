using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("WipBarCode")]
    public class WipBarCode
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "BarCodeId")]
        public long BarCodeId { get; set; }

        /// <summary>
        /// RFID条码
        /// </summary>
        public string RFIDCode { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 型号编码
        /// </summary>
        public string ModelCode { get; set; }
        /// <summary>
        /// 程序号
        /// </summary>
        public string RecipeCode { get; set; }

        /// <summary>
        /// 0:成功，1:失败，2：待处理
        /// </summary>
        public string BarCodeType { get; set; }

        /// <summary>
        /// 当前工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 进站时间
        /// </summary>
        public DateTime? InputTime { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        public DateTime? OutputTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新用户
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 条码类型 0=初始值，正常件=1，返修件=2，防错样件-OK=3
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 返修次数
        /// </summary>
        public int? RepairCount { get; set; }

        /// <summary>
        /// 上一工站编码
        /// </summary>
        public string PrStationCode { get; set; }

        /// <summary>
        /// 下一工站编码
        /// </summary>
        public string NextStationCode { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string BatchCode { get; set; }
    }
}