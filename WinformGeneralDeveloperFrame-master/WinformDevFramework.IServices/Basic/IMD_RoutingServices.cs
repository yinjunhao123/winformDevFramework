using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IServices;
using WinformDevFramework.Models.Basic;

namespace WinformDevFramework.Basic.IServices
{
    public interface IMD_RoutingServices: IBaseServices<MD_Routing>
    {
        List<MD_RoutingList> QueryRoutingDetails(long routingId);
        void InsertRoutingDetails(List<MD_RoutingList> details);
        void DeleteRoutingDetails(long routingId);
    }
}