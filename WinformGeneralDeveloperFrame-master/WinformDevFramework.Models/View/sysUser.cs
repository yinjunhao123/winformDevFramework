using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework
{
    public partial class sysUser
    {
        [SugarColumn(IsIgnore = true)]
        public string DeptName { get; set; }
    }
}
