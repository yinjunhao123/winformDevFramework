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
    public class PLC_CollectParametersRepository : BaseRepository<PLC_CollectParameters>, IPLC_CollectParametersRepository
    {
        public PLC_CollectParametersRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
