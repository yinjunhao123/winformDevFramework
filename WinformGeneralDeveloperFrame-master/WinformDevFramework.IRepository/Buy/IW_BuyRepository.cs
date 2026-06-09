using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_BuyRepository:IBaseRepository<w_Buy>
    {
        int AddBuyInfo(w_Buy buy,List<w_BuyDetail> detail);
        bool UpdateBuyInfo(w_Buy buy, List<w_BuyDetail> detail);
        bool DeleteBuyInfo(w_Buy buy);

    }
}