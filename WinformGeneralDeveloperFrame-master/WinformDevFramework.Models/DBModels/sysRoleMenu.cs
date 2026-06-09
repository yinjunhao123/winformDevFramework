using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysRoleMenu")]
    public partial class sysRoleMenu
    {
           public sysRoleMenu(){


           }
           /// <summary>
           /// Desc:角色ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? RoleID {get;set;}

           /// <summary>
           /// Desc:菜单ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? MenuID {get;set;}

    }
}
