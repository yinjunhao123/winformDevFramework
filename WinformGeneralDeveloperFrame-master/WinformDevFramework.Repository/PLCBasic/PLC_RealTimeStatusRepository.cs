using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class PLC_RealTimeStatusRepository : BaseRepository<PLC_RealTimeStatus>, IPLC_RealTimeStatusRepository
    {
        public PLC_RealTimeStatusRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}