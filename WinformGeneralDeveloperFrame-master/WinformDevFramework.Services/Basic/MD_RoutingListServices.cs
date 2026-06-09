using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Services;

namespace WinformDevFramework.Basic.Services
{
    public class MD_RoutingListServices : BaseServices<MD_RoutingList>, IMD_RoutingListServices
    {
        public MD_RoutingListServices(IMD_RoutingListRepository baseRepository)
        {
            BaseDal = baseRepository;
        }
    }
}