using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace WinformDevFramework
{

    public partial class sysDicData
    {
        [SugarColumn(IsIgnore = true)]
        public string DicTypeName {get;set;}

    }
}
