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
using PLCBasic.Repository;
namespace PLCBasic.Services
{
    public class PLC_ConfigServices : BaseServices<PLC_Config>, IPLC_ConfigServices
    {
        private readonly IPLC_ConfigRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public PLC_ConfigServices(IUnitOfWork unitOfWork, IPLC_ConfigRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}