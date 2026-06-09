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
    public class PLC_ConfigRepository : BaseRepository<PLC_Config>, IPLC_ConfigRepository  
    {
        public PLC_ConfigRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}