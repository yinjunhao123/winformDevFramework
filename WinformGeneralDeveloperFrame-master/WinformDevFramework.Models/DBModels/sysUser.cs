using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysUser")]
    public partial class sysUser
    {
           public sysUser(){


           }
           /// <summary>
           /// Desc:唯一标识用户的主键
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:用户登录名
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Username {get;set;}

           /// <summary>
           /// Desc:用户登录密码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Password {get;set;}

           /// <summary>
           /// Desc:用户姓名
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Fullname {get;set;}

           /// <summary>
           /// Desc:用户电子邮件
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Email {get;set;}

           /// <summary>
           /// Desc:用户手机号码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PhoneNumber {get;set;}

           /// <summary>
           /// Desc:账号创建时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? CreateTime {get;set;}

           /// <summary>
           /// Desc:最后登陆时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? LastLoginTime {get;set;}

           /// <summary>
           /// Desc:用户状态 启用 禁用
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool? Status {get;set;}

           /// <summary>
           /// Desc:性别
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Sex {get;set;}

           /// <summary>
           /// Desc:部门ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? DeptID {get;set;}

    }
}
