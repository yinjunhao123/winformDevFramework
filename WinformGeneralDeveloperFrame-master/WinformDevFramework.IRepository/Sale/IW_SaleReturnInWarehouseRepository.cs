using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_SaleReturnInWarehouseRepository:IBaseRepository<w_SaleReturnInWarehouse>
    {
        int AddSaleReturnInWarehouseInfo(w_SaleReturnInWarehouse buy, List<w_SaleReturnInWarehouseDetail> detail);
        bool UpdateSaleReturnInWarehouseInfo(w_SaleReturnInWarehouse buy, List<w_SaleReturnInWarehouseDetail> detail);
        bool DeleteSaleReturnInWarehouseInfo(w_SaleReturnInWarehouse buy);

    }
}