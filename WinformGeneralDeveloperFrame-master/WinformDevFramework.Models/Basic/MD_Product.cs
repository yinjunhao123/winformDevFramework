using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework.Models.Basic
{
    ///<summary>
    ///产品主数据表
    ///</summary>
    [SugarTable("MD_Product")]
    public partial class MD_Product
    {
           public MD_Product(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public long ProductID {get;set;}

           /// <summary>
           /// Desc:型号
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ModelCode {get;set;}

           /// <summary>
           /// Desc:产品编码
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ProductCode {get;set;}

           /// <summary>
           /// Desc:产品名称
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ProductName {get;set;}

           /// <summary>
           /// Desc:产品规格
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ProductStandard {get;set;}

           /// <summary>
           /// Desc:产品属性
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ProductAttribute {get;set;}


           /// <summary>
           /// Desc:数量
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? InventoryQty {get;set;}

           /// <summary>
           /// Desc:版本
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Version {get;set;}

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
           /// Desc:修改人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string UpdateUser {get;set;}

           /// <summary>
           /// Desc:修改时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? UpdateTime {get;set;}

    }
}
