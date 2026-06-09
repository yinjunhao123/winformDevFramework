using PLCBasic;
using PLCBasic.IRepository;
using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;

namespace PLCBasic.Repository
{
    public class PLC_StationRecipeCurrentRepository : BaseRepository<PLC_StationRecipeCurrent>, IPLC_StationRecipeCurrentRepository
    {
        public PLC_StationRecipeCurrentRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
