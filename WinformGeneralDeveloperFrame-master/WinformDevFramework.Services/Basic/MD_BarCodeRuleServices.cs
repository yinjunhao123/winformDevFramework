using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Services;

namespace WinformDevFramework.Basic.Services
{
    public class MD_BarCodeRuleServices : BaseServices<MD_BarCodeRule>, IMD_BarCodeRuleServices
    {
        public MD_BarCodeRuleServices(IMD_BarCodeRuleRepository baseRepository)
        {
            BaseDal = baseRepository;
        }
    }
}
