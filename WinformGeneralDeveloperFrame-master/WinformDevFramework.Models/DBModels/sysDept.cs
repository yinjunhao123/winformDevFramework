using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysDept")]
    public partial class sysDept
    {
           public sysDept(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:机构名称
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DeptName {get;set;}

           /// <summary>
           /// Desc:上级机构
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? PDeptID {get;set;}

           /// <summary>
           /// Desc:负责人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? LeaderID {get;set;}

           /// <summary>
           /// Desc:备注
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Remark {get;set;}

    }
}
