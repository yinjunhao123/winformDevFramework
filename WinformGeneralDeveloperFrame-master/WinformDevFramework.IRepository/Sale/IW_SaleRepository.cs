using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_SaleRepository:IBaseRepository<w_Sale>
    {
        int AddSaleInfo(w_Sale buy, List<w_SaleDetail> detail);
        bool UpdateSaleInfo(w_Sale buy, List<w_SaleDetail> detail);
    }
}