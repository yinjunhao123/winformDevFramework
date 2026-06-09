using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_ParameterDistributionDetailHistoryRepository : BaseRepository<PLC_ParameterDistributionDetailHistory>, IPLC_ParameterDistributionDetailHistoryRepository
    {
        public PLC_ParameterDistributionDetailHistoryRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
