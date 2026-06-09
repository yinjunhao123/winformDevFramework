using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_ParameterDistributionRepository : BaseRepository<PLC_ParameterDistribution>, IPLC_ParameterDistributionRepository
    {
        public PLC_ParameterDistributionRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
