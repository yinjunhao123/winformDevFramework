using PLCBasic;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;

namespace WinformDevFramework.Repository.PLCBasic
{
    public class PLC_CalibraCollectRepository : BaseRepository<PLC_CalibraCollect>, IPLC_CalibraCollectRepository
    {
        public PLC_CalibraCollectRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
