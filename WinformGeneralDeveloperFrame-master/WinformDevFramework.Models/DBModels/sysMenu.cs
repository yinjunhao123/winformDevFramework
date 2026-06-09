using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysMenu")]
    public partial class sysMenu
    {
           public sysMenu(){


           }
           /// <summary>
           /// Desc:菜单ID
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:父菜单ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? ParentID {get;set;}

           /// <summary>
           /// Desc:菜单标题
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Title {get;set;}

           /// <summary>
           /// Desc:菜单链接地址
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string URL {get;set;}

           /// <summary>
           /// Desc:菜单图标
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Icon {get;set;}

           /// <summary>
           /// Desc:菜单排序顺序
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? SortOrder {get;set;}

           /// <summary>
           /// Desc:菜单类型
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MenuType {get;set;}

           /// <summary>
           /// Desc:创建人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? CreatedBy {get;set;}

           /// <summary>
           /// Desc:创建时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? CreatedDate {get;set;}

           /// <summary>
           /// Desc:更新者
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? UpdatedBy {get;set;}

           /// <summary>
           /// Desc:更新时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? UpdatedDate {get;set;}

           /// <summary>
           /// Desc:菜单编码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MenuCode {get;set;}

           /// <summary>
           /// Desc:权限标识
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PermissionCode {get;set;}

    }
}
