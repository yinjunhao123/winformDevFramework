using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices.System;
using WinformDevFramework.IServices;
using WinformDevFramework.IRepository;
using WinformDevFramework.Models;
using WinformDevFramework.Services;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Basic.IServices;
using WinformDevFramework.Basic.IRepository;
namespace WinformDevFramework.Basic.Services
{
    public class MD_RoutingServices : BaseServices<MD_Routing>, IMD_RoutingServices
    {
        private readonly IMD_RoutingRepository _dal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlSugarClient _db;

        public MD_RoutingServices(IUnitOfWork unitOfWork, IMD_RoutingRepository dal, ISqlSugarClient db)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public List<MD_RoutingList> QueryRoutingDetails(long routingId)
        {
            return _db.Queryable<MD_RoutingList>()
                .Where(r => r.RoutingId == routingId)
                .OrderBy(r => r.SortOrder)
                .ToList();
        }

        public void InsertRoutingDetails(List<MD_RoutingList> details)
        {
            if (details == null || details.Count == 0)
                return;
            _db.Insertable(details).ExecuteCommand();
        }

        public void DeleteRoutingDetails(long routingId)
        {
            _db.Deleteable<MD_RoutingList>()
                .Where(r => r.RoutingId == routingId)
                .ExecuteCommand();
        }
    }
}