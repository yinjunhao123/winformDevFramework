using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("sysMessage")]
    public partial class sysMessage
    {
           public sysMessage(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int ID {get;set;}

           /// <summary>
           /// Desc:消息标题
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Title {get;set;}

           /// <summary>
           /// Desc:消息内容
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Content {get;set;}

           /// <summary>
           /// Desc:发送人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? FromUserID {get;set;}

           /// <summary>
           /// Desc:接收人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? ToUserID {get;set;}

           /// <summary>
           /// Desc:是否发送
           /// Default:
           /// Nullable:True
           /// </summary>           
           [SugarColumn(ColumnDataType = "bit")]
           public int? IsSend {get;set;}

           /// <summary>
           /// Desc:创建时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? CreateTime {get;set;}

           /// <summary>
           /// Desc:发送时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? SendTime {get;set;}

    }
}
