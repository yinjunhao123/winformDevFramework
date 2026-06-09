using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices.System;
using WinformDevFramework.Models;

namespace WinformDevFramework.Services.System
{
    public class SysUserServices : BaseServices<sysUser>, ISysUserServices
    {
        private readonly ISysUserRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public SysUserServices(IUnitOfWork unitOfWork, ISysUserRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}
