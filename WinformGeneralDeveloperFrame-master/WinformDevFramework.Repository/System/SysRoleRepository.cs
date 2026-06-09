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
    public class sysRoleRepository : BaseRepository<sysRole>, IsysRoleRepository
    {
        public sysRoleRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}