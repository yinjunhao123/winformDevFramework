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
    public class PLC_StatusDefServices : BaseServices<PLC_StatusDef>, IPLC_StatusDefServices
    {
        private readonly IPLC_StatusDefRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public PLC_StatusDefServices(IUnitOfWork unitOfWork, IPLC_StatusDefRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}