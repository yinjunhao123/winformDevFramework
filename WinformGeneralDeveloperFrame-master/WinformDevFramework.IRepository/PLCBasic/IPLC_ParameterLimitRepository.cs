using PLCBasic;
using WinformDevFramework.IRepository;

namespace WinformDevFramework.IRepository.PLCBasic
{
    public interface IPLC_ParameterLimitRepository : IBaseRepository<PLC_ParameterLimit>
    {
        PLC_ParameterLimit GetByStationAndProductAndParam(string stationCode, string productModelCode, string parameterName);
    }
}