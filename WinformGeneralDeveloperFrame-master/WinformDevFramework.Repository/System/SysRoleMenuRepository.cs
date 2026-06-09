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
    public class sysRoleMenuRepository : BaseRepository<sysRoleMenu>, IsysRoleMenuRepository
    {
        public sysRoleMenuRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}