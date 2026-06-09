using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository;
using WinformDevFramework.Models;
using WinformDevFramework.Repository;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_PlcAlarmInfoRepository : BaseRepository<PLC_AlarmInfo>, IPLC_AlarmInfoRepository
    {
        public PLC_PlcAlarmInfoRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}