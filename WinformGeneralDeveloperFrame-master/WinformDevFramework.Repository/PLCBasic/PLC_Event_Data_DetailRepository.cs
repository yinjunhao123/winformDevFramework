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
    public class PLC_Event_Data_DetailRepository : BaseRepository<PLC_Event_Data_Detail>, IPLC_Event_Data_DetailRepository
    {
        public PLC_Event_Data_DetailRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}