using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.PLCBasic
{
    [SugarTable("PLC_CollectParametersDetail")]

    public class PLC_CollectParametersDetail
    {
        public PLC_CollectParametersDetail()
        {

        }
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ParametersDetailID {  get; set; }

        public long ParamID {  get; set; }
        /// <summary>
        /// 参数名
        /// </summary>
        public string ParamName { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public string ParamValue { get; set; }
    }
}
