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
    public class sysDicTypeRepository : BaseRepository<sysDicType>, IsysDicTypeRepository
    {
        public sysDicTypeRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}