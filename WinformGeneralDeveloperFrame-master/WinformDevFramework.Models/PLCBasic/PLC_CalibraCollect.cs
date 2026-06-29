using SqlSugar;
using System;

namespace PLCBasic
{
    /// <summary>
    /// PLC校准数据采集表
    /// </summary>
    [SugarTable("PLC_CalibraCollect")]
    public class PLC_CalibraCollect
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        [SugarColumn(Length = 50)]
        public string StationCode { get; set; }

        /// <summary>
        /// 线体编码
        /// </summary>
        [SugarColumn(Length = 50)]
        public string LineCode { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        [SugarColumn(Length = 50)]
        public string PlcCode { get; set; }

        /// <summary>
        /// 校准时间字符串
        /// </summary>
        [SugarColumn(Length = 50)]
        public string CalibraTime { get; set; }

        /// <summary>
        /// 校准结果
        /// </summary>
        public byte? CalibraResult { get; set; }

        /// <summary>
        /// 校准数据1
        /// </summary>
        public decimal? Calibra1Data1 { get; set; }

        /// <summary>
        /// 校准数据2
        /// </summary>
        public decimal? Calibra1Data2 { get; set; }

        /// <summary>
        /// 校准数据3
        /// </summary>
        public decimal? Calibra1Data3 { get; set; }

        /// <summary>
        /// 校准数据4
        /// </summary>
        public decimal? Calibra1Data4 { get; set; }

        /// <summary>
        /// 校准数据5
        /// </summary>
        public decimal? Calibra1Data5 { get; set; }

        /// <summary>
        /// 校准数据6
        /// </summary>
        public decimal? Calibra1Data6 { get; set; }

        /// <summary>
        /// 校准数据7
        /// </summary>
        public decimal? Calibra1Data7 { get; set; }

        /// <summary>
        /// 校准数据8
        /// </summary>
        public decimal? Calibra1Data8 { get; set; }

        /// <summary>
        /// 校准数据9
        /// </summary>
        public decimal? Calibra1Data9 { get; set; }

        /// <summary>
        /// 校准数据10
        /// </summary>
        public decimal? Calibra1Data10 { get; set; }

        /// <summary>
        /// 校准数据11
        /// </summary>
        public decimal? Calibra1Data11 { get; set; }

        /// <summary>
        /// 校准数据12
        /// </summary>
        public decimal? Calibra1Data12 { get; set; }

        /// <summary>
        /// 校准数据13
        /// </summary>
        public decimal? Calibra1Data13 { get; set; }

        /// <summary>
        /// 校准数据14
        /// </summary>
        public decimal? Calibra1Data14 { get; set; }

        /// <summary>
        /// 校准数据15
        /// </summary>
        public decimal? Calibra1Data15 { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        [SugarColumn(Length = 50)]
        public string CreateUser { get; set; }
    }
}
