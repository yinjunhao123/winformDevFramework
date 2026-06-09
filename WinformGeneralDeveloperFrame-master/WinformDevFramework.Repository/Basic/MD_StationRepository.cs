using SqlSugar;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Repository;
using WinformDevFramework.Basic.IRepository;

namespace WinformDevFramework.Basic.Repository
{
    public class MD_StationRepository : BaseRepository<MD_Station>, IMD_StationRepository
    {
        public MD_StationRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}