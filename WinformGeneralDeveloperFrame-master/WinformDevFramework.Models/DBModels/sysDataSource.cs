using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysDataSource")]
    public partial class sysDataSource
    {
           public sysDataSource(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:连接名
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ConnectName {get;set;}

           /// <summary>
           /// Desc:主机
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Host {get;set;}

           /// <summary>
           /// Desc:数据库
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DataBaseName {get;set;}

           /// <summary>
           /// Desc:用户名
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Username {get;set;}

           /// <summary>
           /// Desc:密码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Password {get;set;}

    }
}
