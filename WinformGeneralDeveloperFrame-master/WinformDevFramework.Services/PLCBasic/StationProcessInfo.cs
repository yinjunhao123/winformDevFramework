using System;
using System.Collections.Generic;
using WinformDevFramework.Models.Basic;

namespace WinformDevFramework.Services.PLCBasic
{
    public class StationProcessInfo
    {
        public string PlcCode { get; set; }

        public string StationCode { get; set; }

        public string ProductModel { get; set; }

        public string Recipe { get; set; }

        public string RoutingCode { get; set; }

        public List<MD_RoutingList> RoutingList { get; set; }

        public MD_BarCodeRule MainBarCodeRule { get; set; }

        public List<MD_BarCodeRuleList> MainBarCodeRuleList { get; set; }

        public MD_BarCodeRule FirstBarCodeRule { get; set; }

        public List<MD_BarCodeRuleList> FirstBarCodeRuleList { get; set; }

        public MD_BarCodeRule SecondBarCodeRule { get; set; }

        public List<MD_BarCodeRuleList> SecondBarCodeRuleList { get; set; }

        public MD_BarCodeRule ThirdBarCodeRule { get; set; }

        public List<MD_BarCodeRuleList> ThirdBarCodeRuleList { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}