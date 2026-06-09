using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Repository;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Models.Basic;


namespace WinformDevFramework.Basic.Repository
{
    public class MD_RoutingRepository : BaseRepository<MD_Routing>, IMD_RoutingRepository
    {
        public MD_RoutingRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}