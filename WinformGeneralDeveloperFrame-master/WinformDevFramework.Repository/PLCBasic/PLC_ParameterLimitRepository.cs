using SqlSugar;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.Repository;
using PLCBasic;

namespace PLCBasic.Repository
{
    public class PLC_ParameterLimitRepository : BaseRepository<PLC_ParameterLimit>, IPLC_ParameterLimitRepository
    {
        public PLC_ParameterLimitRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }

        public PLC_ParameterLimit GetByStationAndProductAndParam(string stationCode, string productModelCode, string parameterName)
        {
            return DbBaseClient
                .Queryable<PLC_ParameterLimit>()
                .Where(p => p.StationCode == stationCode && 
                           p.ProductModelCode == productModelCode && 
                           p.ParameterName == parameterName)
                .First();
        }
    }
}