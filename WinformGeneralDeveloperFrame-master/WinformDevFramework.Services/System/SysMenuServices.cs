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
    public class SysMenuServices : BaseServices<sysMenu>, ISysMenuServices
    {
        private readonly ISysMenuRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public SysMenuServices(IUnitOfWork unitOfWork, ISysMenuRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}
