using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_BuyReturnRepository:IBaseRepository<w_BuyReturn>
    {
        int AddBuyReturnInfo(w_BuyReturn buyReturn, List<w_BuyReturnDetail> detail);
        bool UpdateBuyReturnInfo(w_BuyReturn buyReturn, List<w_BuyReturnDetail> detail);
    }
}