using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Models.Common
{
    public class BatchWriteRequest
    {
        public int Index { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}
