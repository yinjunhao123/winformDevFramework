using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic.IRepository;
using WinformDevFramework.Models.PLCBasic;

namespace PLCBasic.Repository
{
    public class PLC_Event_Trigger_MasterRepository : BaseRepository<PLC_Event_Trigger_Master>, IPLC_Event_Trigger_MasterRepository
    {
        public PLC_Event_Trigger_MasterRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}