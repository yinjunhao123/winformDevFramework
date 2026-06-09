using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_SaleReturnRepository:IBaseRepository<w_SaleReturn>
    {
        int AddSaleReturnInfo(w_SaleReturn buy, List<w_SaleReturnDetail> detail);
        bool UpdateSaleReturnInfo(w_SaleReturn buy, List<w_SaleReturnDetail> detail);
    }
}