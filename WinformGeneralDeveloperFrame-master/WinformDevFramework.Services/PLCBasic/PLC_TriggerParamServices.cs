using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices;
using WinformDevFramework.Services;
using PLCBasic.IServices;
using PLCBasic.IRepository;

namespace PLCBasic.Services
{
    public class PLC_TriggerParamServices : BaseServices<PLC_TriggerParam>, IPLC_TriggerParamServices
    {
        private readonly IPLC_TriggerParamRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public PLC_TriggerParamServices(IUnitOfWork unitOfWork, IPLC_TriggerParamRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}