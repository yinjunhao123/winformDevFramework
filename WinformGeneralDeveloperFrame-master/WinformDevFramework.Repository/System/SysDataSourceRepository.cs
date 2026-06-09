using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.Models;


namespace WinformDevFramework.Repository
{
    public class SysDataSourceRepository : BaseRepository<sysDataSource>, ISysDataSourceRepository
    {
        public SysDataSourceRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
