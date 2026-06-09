using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Services;

namespace WinformDevFramework.Basic.Services
{
    public class MD_StationPartServices : BaseServices<MD_StationPart>, IMD_StationPartServices
    {
        public MD_StationPartServices(IMD_StationPartRepository baseRepository)
        {
            BaseDal = baseRepository;
        }
    }
}
