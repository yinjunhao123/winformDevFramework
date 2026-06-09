using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysDicType")]
    public partial class sysDicType
    {
           public sysDicType(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:字典类型名称
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DicTypeName {get;set;}

           /// <summary>
           /// Desc:备注
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Remark {get;set;}

           /// <summary>
           /// Desc:字典类型编码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DicTypeCode {get;set;}

    }
}
