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
using WinformDevFramework.Services;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
namespace WinformDevFramework.Basic.Services
{
    public class MD_ProductModelServices : BaseServices<MD_ProductModel>, IMD_ProductModelServices
    {
        private readonly IMD_ProductModelRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public MD_ProductModelServices(IUnitOfWork unitOfWork, IMD_ProductModelRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}