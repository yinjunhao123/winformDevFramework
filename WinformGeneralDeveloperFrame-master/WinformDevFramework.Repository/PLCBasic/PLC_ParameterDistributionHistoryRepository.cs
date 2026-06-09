using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_ParameterDistributionHistoryRepository : BaseRepository<PLC_ParameterDistributionHistory>, IPLC_ParameterDistributionHistoryRepository
    {
        public PLC_ParameterDistributionHistoryRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
