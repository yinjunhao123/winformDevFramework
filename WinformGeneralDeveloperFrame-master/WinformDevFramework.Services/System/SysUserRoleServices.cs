using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices.System;
using WinformDevFramework.IServices;
using WinformDevFramework.Models;
namespace WinformDevFramework.Services
{
    public class sysUserRoleServices : BaseServices<sysUserRole>, IsysUserRoleServices
    {
        private readonly IsysUserRoleRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public sysUserRoleServices(IUnitOfWork unitOfWork, IsysUserRoleRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}