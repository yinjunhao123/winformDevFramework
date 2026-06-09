using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_SaleOutWarehouseRepository:IBaseRepository<w_SaleOutWarehouse>
    {
        int AddSaleOutWarehouseInfo(w_SaleOutWarehouse buy, List<w_SaleOutWarehouseDetail> detail);
        bool UpdateSaleOutWarehouseInfo(w_SaleOutWarehouse buy, List<w_SaleOutWarehouseDetail> detail);
        bool DeleteSaleOutWarehouseInfo(w_SaleOutWarehouse buy);

    }
}