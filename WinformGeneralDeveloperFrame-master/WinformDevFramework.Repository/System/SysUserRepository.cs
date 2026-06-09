using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.Models;

namespace WinformDevFramework.Repository.System
{
    public class SysUserRepository : BaseRepository<sysUser>, ISysUserRepository
    {
        public SysUserRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}
