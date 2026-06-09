using SqlSugar;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_ParameterDistributionDetailRepository : BaseRepository<PLC_ParameterDistributionDetail>, IPLC_ParameterDistributionDetailRepository
    {
        public PLC_ParameterDistributionDetailRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
