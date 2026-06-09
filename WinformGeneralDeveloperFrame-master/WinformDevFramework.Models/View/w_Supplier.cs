using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework
{
    public partial class w_Supplier
    {
        /// <summary>
        /// 应付款总额
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal TotalPayable { get; set; } = 100;
        /// <summary>
        /// 退货款总额
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal TotalAmountReturned { get; set; } = 100;
        /// <summary>
        /// 已结款总额
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal TotalAmountPaid{ get; set; } = 100;
        /// <summary>
        /// 未结款总额
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal TotalAmountOutstanding { get; set; } = 100;
    }
}
