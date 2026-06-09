using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace WinformDevFramework
{
    public partial class sysMenu
    {
        /// <summary>
        /// 父菜单
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string PMenuName { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string CreateByName { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UpdateByName { get; set; }
    }
}
