using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("WipBarcodeHistory")]
    public class WipBarcodeHistory
    {
        /// <summary>
        /// 历史ID（主键，自增）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long HistoryId { get; set; }

        /// <summary>
        /// 条码ID
        /// </summary>
        public long BarCodeId { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// RFID编码
        /// </summary>
        public string RfidCode { get; set; }

        /// <summary>
        /// 型号编码
        /// </summary>
        public string ModelCode { get; set; }

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
        /// 状态
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

        /// <summary>
        /// 是否返修
        /// </summary>
        public int? IsRepair { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// 快照时间
        /// </summary>
        public DateTime SnapshotTime { get; set; }
    }
}
