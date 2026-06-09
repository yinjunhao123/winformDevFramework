using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models;


namespace WinformDevFramework.IServices
{
    public interface Iw_SaleReturnServices: IBaseServices<w_SaleReturn>
    {
        int AddSaleReturnInfo(w_SaleReturn buy, List<w_SaleReturnDetail> detail);
        bool UpdateSaleReturnInfo(w_SaleReturn buy, List<w_SaleReturnDetail> detail);
    }
}