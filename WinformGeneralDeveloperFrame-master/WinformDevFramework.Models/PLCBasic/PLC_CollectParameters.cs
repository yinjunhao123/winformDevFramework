using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCBasic
{
    /// <summary>
    /// PLC参数信息
    /// </summary>
    [SugarTable("PLC_CollectParameters")]
    public partial class PLC_CollectParameters
    {
        public PLC_CollectParameters()
        {

        }

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ParamID { get; set; }
        /// <summary>
        /// 主条码RFID
        /// </summary>
        public string RFIDCode { get; set; }

        /// <summary>
        /// 条码编码
        /// </summary>
        public string BarCode { get; set; }
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
        /// <summary>
        /// 采集结果
        /// </summary>
        public string Result {  get; set; }
        /// <summary>
        /// 提示消息
        /// </summary>
        public string Message {  get; set; }
    }
}
