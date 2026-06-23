using SqlSugar;
using System;

namespace PLCBasic
{
    /// <summary>
    /// PLC实时参数采集表（每5分钟采集一次）
    /// </summary>
    [SugarTable("PLC_RealTimeParamCollect")]
    public class PLC_RealTimeParamCollect
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime { get; set; }

        #region 分项结果1

        public string ParamName1 { get; set; }
        public int SubResult1 { get; set; }
        public double SubUSL1 { get; set; }
        public double SubResultValue1 { get; set; }
        public double SubLSL1 { get; set; }

        #endregion

        #region 分项结果2

        public string ParamName2 { get; set; }
        public int SubResult2 { get; set; }
        public double SubUSL2 { get; set; }
        public double SubResultValue2 { get; set; }
        public double SubLSL2 { get; set; }

        #endregion

        #region 分项结果3

        public string ParamName3 { get; set; }
        public int SubResult3 { get; set; }
        public double SubUSL3 { get; set; }
        public double SubResultValue3 { get; set; }
        public double SubLSL3 { get; set; }

        #endregion

        #region 分项结果4

        public string ParamName4 { get; set; }
        public int SubResult4 { get; set; }
        public double SubUSL4 { get; set; }
        public double SubResultValue4 { get; set; }
        public double SubLSL4 { get; set; }

        #endregion

        #region 分项结果5

        public string ParamName5 { get; set; }
        public int SubResult5 { get; set; }
        public double SubUSL5 { get; set; }
        public double SubResultValue5 { get; set; }
        public double SubLSL5 { get; set; }

        #endregion

        #region 分项结果6

        public string ParamName6 { get; set; }
        public int SubResult6 { get; set; }
        public double SubUSL6 { get; set; }
        public double SubResultValue6 { get; set; }
        public double SubLSL6 { get; set; }

        #endregion
    }
}
