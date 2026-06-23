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
    public class PLC_RealTimeParamCollectRepository : BaseRepository<PLC_RealTimeParamCollect>, IPLC_RealTimeParamCollectRepository
    {
        public PLC_RealTimeParamCollectRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}
