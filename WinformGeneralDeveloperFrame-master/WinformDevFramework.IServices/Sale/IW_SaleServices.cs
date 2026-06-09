using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models;


namespace WinformDevFramework.IServices
{
    public interface Iw_SaleServices: IBaseServices<w_Sale>
    {
        int AddSaleInfo(w_Sale buy, List<w_SaleDetail> detail);
        bool UpdateSaleInfo(w_Sale buy, List<w_SaleDetail> detail);
    }
}