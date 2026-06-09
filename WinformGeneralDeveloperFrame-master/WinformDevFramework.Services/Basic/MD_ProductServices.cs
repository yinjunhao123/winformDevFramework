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
    public class MD_ProductServices : BaseServices<MD_Product>, IMD_ProductServices
    {
        private readonly IMD_ProductRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public MD_ProductServices(IUnitOfWork unitOfWork, IMD_ProductRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}