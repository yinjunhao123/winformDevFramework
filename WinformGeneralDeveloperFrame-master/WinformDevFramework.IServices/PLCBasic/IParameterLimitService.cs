using PLCBasic;
using WinformDevFramework.IServices;

namespace WinformDevFramework.IServices.PLCBasic
{
    public interface IParameterLimitService : IBaseServices<PLC_ParameterLimit>
    {
        PLC_ParameterLimit GetByStationAndProductAndParam(string stationCode, string productModelCode, string parameterName);
    }
}