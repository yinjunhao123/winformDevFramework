using SqlSugar;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Repository;
using WinformDevFramework.Basic.IRepository;

namespace WinformDevFramework.Basic.Repository
{
    public class MD_BarCodeRuleListRepository : BaseRepository<MD_BarCodeRuleList>, IMD_BarCodeRuleListRepository
    {
        public MD_BarCodeRuleListRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}