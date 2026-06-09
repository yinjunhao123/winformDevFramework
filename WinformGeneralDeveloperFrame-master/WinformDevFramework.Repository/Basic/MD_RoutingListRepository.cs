using SqlSugar;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Repository;
using WinformDevFramework.Basic.IRepository;

namespace WinformDevFramework.Basic.Repository
{
    public class MD_RoutingListRepository : BaseRepository<MD_RoutingList>, IMD_RoutingListRepository
    {
        public MD_RoutingListRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}