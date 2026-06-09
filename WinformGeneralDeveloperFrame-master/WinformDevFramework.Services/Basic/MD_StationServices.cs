using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices.System;
using WinformDevFramework.IServices;
using WinformDevFramework.IRepository;
using WinformDevFramework.Models;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Services;
using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
namespace WinformDevFramework.Basic.Services
{
    public class MD_StationServices : BaseServices<MD_Station>, IMD_StationServices
    {
        private readonly IMD_StationRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public MD_StationServices(IUnitOfWork unitOfWork, IMD_StationRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}