using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("ReworkRecord")]
    public class ReworkRecord
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ReworkId { get; set; }

        [SugarColumn(Length = 50)]
        public string RfidBarCode { get; set; }

        [SugarColumn(Length = 50)]
        public string ProductModel { get; set; }

        [SugarColumn(Length = 50)]
        public string FromStationCode { get; set; }

        [SugarColumn(Length = 50)]
        public string ToStationCode { get; set; }

        [SugarColumn(Length = 500)]
        public string ReworkReason { get; set; }

        [SugarColumn(Length = 50)]
        public string CreateUser { get; set; }

        public DateTime CreateTime { get; set; }

        [SugarColumn(Length = 50)]
        public string UpdateUser { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}