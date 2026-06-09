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
    public class WipProcessDataRepository : BaseRepository<WipProcessData>, IWipProcessDataRepository
    {
        public WipProcessDataRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}