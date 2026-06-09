using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Services;

namespace WinformDevFramework.Basic.Services
{
    public class MD_BarCodeRuleListServices : BaseServices<MD_BarCodeRuleList>, IMD_BarCodeRuleListServices
    {
        public MD_BarCodeRuleListServices(IMD_BarCodeRuleListRepository baseRepository)
        {
            BaseDal = baseRepository;
        }
    }
}
