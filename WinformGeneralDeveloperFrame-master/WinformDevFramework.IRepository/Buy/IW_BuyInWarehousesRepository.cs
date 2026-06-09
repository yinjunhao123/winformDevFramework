using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_BuyInWarehouseRepository:IBaseRepository<w_BuyInWarehouse>
    {
        int AddBuyInWarehouseInfo(w_BuyInWarehouse buy, List<w_BuyInWarehouseDetail> detail);
        bool UpdateBuyInWarehouseInfo(w_BuyInWarehouse buy, List<w_BuyInWarehouseDetail> detail);
        bool DeleteBuyInWarehouseInfo(w_BuyInWarehouse buy);
    }
}