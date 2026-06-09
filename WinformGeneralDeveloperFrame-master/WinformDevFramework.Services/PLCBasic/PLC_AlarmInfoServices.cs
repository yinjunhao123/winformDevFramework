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
    public class PLC_AlarmInfoServices : BaseServices<PLC_AlarmInfo>, IPLC_AlarmInfoServices
    {
        private readonly IPLC_AlarmInfoRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public PLC_AlarmInfoServices(IUnitOfWork unitOfWork, IPLC_AlarmInfoRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}