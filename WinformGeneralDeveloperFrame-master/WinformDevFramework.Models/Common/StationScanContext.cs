using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.Common
{
    public class StationScanContext
    {
        public string StationCode {  get; set; }
        public string MainBarcode { get; set; }
        public string ModelCode { get; set; }

        public string PlcCode {  get; set; }
        public DateTime MainScanTime { get; set; }
        public DateTime StartTime { get; set; }
        public List<string> SubBarcodes { get; set; } = new List<string>();
        public int ExpectedSubCount { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// 验证结果
        /// </summary>
        public string CheckResult {  get; set; }
        /// <summary>
        /// 参数采集验证结果（OK/NG）
        /// </summary>
        public string ParamResult { get; set; }
    }
}
