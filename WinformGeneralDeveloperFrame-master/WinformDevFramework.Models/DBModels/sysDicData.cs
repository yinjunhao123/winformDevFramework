using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysDicData")]
    public partial class sysDicData
    {
           public sysDicData(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:字典类型ID
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? DicTypeID {get;set;}

           /// <summary>
           /// Desc:内容
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DicData {get;set;}

           /// <summary>
           /// Desc:备注
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Remark {get;set;}

           /// <summary>
           /// Desc:编码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DicDataCode {get;set;}

    }
}
