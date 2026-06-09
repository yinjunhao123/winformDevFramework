using SqlSugar;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Repository;
using WinformDevFramework.Basic.IRepository;

namespace WinformDevFramework.Basic.Repository
{
    public class MD_BarCodeRuleRepository : BaseRepository<MD_BarCodeRule>, IMD_BarCodeRuleRepository
    {
        public MD_BarCodeRuleRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}