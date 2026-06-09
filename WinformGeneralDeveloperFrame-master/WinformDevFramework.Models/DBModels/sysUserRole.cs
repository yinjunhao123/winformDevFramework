using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysUserRole")]
    public partial class sysUserRole
    {
           public sysUserRole(){


           }
           /// <summary>
           /// Desc:用户ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? UserID {get;set;}

           /// <summary>
           /// Desc:角色ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? RoleID {get;set;}

    }
}
