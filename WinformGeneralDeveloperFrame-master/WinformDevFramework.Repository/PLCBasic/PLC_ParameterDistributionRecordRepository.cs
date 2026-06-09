using PLCBasic;
using PLCBasic.IRepository;
using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;

namespace PLCBasic.Repository
{
    public class PLC_ParameterDistributionRecordRepository : BaseRepository<PLC_ParameterDistributionRecord>, IPLC_ParameterDistributionRecordRepository
    {
        public PLC_ParameterDistributionRecordRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
