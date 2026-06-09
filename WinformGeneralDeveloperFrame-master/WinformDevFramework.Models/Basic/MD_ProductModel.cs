using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework.Models.Basic
{
    ///<summary>
    ///产品型号表
    ///</summary>
    [SugarTable("MD_ProductModel")]
    public partial class MD_ProductModel
    {
           public MD_ProductModel(){


           }
           /// <summary>
           /// Desc:型号ID
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public long ModelID {get;set;}

           /// <summary>
           /// Desc:型号编码
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ModelCode {get;set;}

           /// <summary>
           /// Desc:型号名称
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ModelName {get;set;}

           /// <summary>
           /// Desc:规格描述
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Specification {get;set;}

           /// <summary>
           /// 工艺路线Code
           /// </summary>
           public string RoutingCode { get; set; }

           /// <summary>
           /// Desc:状态（0禁用/1启用）
           /// Default:1
           /// Nullable:True
           /// </summary>           
           public int? Status {get;set;} = 1;

           /// <summary>
           /// Desc:创建人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CreateUser {get;set;}

           /// <summary>
           /// Desc:创建时间
           /// Default:DateTime.Now
           /// Nullable:True
           /// </summary>           
           public DateTime? CreateTime {get;set;}

           /// <summary>
           /// Desc:更新人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string UpdateUser {get;set;}

           /// <summary>
           /// Desc:更改时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? UpdateTime {get;set;}

    }
}
