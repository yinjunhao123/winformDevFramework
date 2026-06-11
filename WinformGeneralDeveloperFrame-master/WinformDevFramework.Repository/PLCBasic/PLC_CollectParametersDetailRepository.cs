using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.Models.PLCBasic;

namespace WinformDevFramework.Repository.PLCBasic
{
    public class PLC_CollectParametersDetailRepository : BaseRepository<PLC_CollectParametersDetail>, IPLC_CollectParametersDetailRepository
    {
        public PLC_CollectParametersDetailRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
