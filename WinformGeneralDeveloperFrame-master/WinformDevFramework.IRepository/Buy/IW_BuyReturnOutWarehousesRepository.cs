using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_BuyReturnOutWarehouseRepository:IBaseRepository<w_BuyReturnOutWarehouse>
    {
        int AddBuyOutWarehouseInfo(w_BuyReturnOutWarehouse buy, List<w_BuyReturnOutWarehouseDetail> detail);
        bool UpdateBuyOutWarehouseInfo(w_BuyReturnOutWarehouse buy, List<w_BuyReturnOutWarehouseDetail> detail);
        bool DeleteBuyOutWarehouseInfo(w_BuyReturnOutWarehouse buy);

    }
}