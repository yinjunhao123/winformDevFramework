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
using PLCBasic.IServices;
using PLCBasic.IRepository;
namespace PLCBasic.Services
{
    public class PLC_AddressServices : BaseServices<PLC_Address>, IPLC_AddressServices
    {
        private readonly IPLC_AddressRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public PLC_AddressServices(IUnitOfWork unitOfWork, IPLC_AddressRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}