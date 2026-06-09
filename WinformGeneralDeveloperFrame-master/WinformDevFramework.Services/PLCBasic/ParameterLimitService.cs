using PLCBasic;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Services;

namespace WinformDevFramework.Services.PLCBasic
{
    public class ParameterLimitService : BaseServices<PLC_ParameterLimit>, IParameterLimitService
    {
        private readonly IPLC_ParameterLimitRepository _limitRepository;

        public ParameterLimitService(IPLC_ParameterLimitRepository limitRepository)
        {
            _limitRepository = limitRepository;
            BaseDal = limitRepository;
        }

        public PLC_ParameterLimit GetByStationAndProductAndParam(string stationCode, string productModelCode, string parameterName)
        {
            return _limitRepository.GetByStationAndProductAndParam(stationCode, productModelCode, parameterName);
        }
    }
}