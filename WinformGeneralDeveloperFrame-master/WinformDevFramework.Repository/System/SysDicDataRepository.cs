using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository;
using WinformDevFramework.Models;

namespace WinformDevFramework.Repository
{
    public class sysDicDataRepository : BaseRepository<sysDicData>, IsysDicDataRepository
    {
        public sysDicDataRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}