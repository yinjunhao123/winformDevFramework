using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic.IRepository;
using SqlSugar;

namespace PLCBasic.Repository
{
    public class PLC_TriggerParamRepository : BaseRepository<PLC_TriggerParam>, IPLC_TriggerParamRepository
    {
        public PLC_TriggerParamRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}