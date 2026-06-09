using SqlSugar;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Repository;
using WinformDevFramework.Basic.IRepository;

namespace WinformDevFramework.Basic.Repository
{
    public class MD_StationPartRepository : BaseRepository<MD_StationPart>, IMD_StationPartRepository
    {
        public MD_StationPartRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}