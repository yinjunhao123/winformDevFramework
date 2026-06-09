using WinformDevFramework.Core;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Core.Repository;

namespace WinformDevFramework.Basic.Services
{
    public class MD_EquipmentServices : BaseServices<MD_Equipment>, IMD_EquipmentServices
    {
        public MD_EquipmentServices(IBaseRepository<MD_Equipment> baseRepository) : base(baseRepository)
        {
        }
    }
}