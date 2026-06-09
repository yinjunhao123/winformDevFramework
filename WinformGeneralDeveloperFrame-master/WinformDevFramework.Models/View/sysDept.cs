using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework
{
    public partial class sysDept
    {
        [SugarColumn(IsIgnore = true)]
        public string PDeptName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string LeaderName { get; set; }
    }
}
