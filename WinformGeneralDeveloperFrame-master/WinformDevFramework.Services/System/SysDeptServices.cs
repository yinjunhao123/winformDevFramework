using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices.System;
using WinformDevFramework.IServices;
using WinformDevFramework.IRepository;
using WinformDevFramework.Models;
namespace WinformDevFramework.Services
{
    public class sysDeptServices : BaseServices<sysDept>, IsysDeptServices
    {
        private readonly IsysDeptRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public sysDeptServices(IUnitOfWork unitOfWork, IsysDeptRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}