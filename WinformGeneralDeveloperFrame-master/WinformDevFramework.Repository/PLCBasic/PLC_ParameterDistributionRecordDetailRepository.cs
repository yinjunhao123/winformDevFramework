using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_ParameterDistributionRecordDetailRepository : BaseRepository<PLC_ParameterDistributionRecordDetail>, IPLC_ParameterDistributionRecordDetailRepository
    {
        public PLC_ParameterDistributionRecordDetailRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
