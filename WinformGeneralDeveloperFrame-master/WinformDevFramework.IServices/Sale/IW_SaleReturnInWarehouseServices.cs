using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models;


namespace WinformDevFramework.IServices
{
    public interface Iw_SaleReturnInWarehouseServices: IBaseServices<w_SaleReturnInWarehouse>
    {
        int AddSaleReturnInWarehouseInfo(w_SaleReturnInWarehouse buy, List<w_SaleReturnInWarehouseDetail> detail);
        bool UpdateSaleReturnInWarehouseInfo(w_SaleReturnInWarehouse buy, List<w_SaleReturnInWarehouseDetail> detail);
        bool DeleteSaleReturnInWarehouseInfo(w_SaleReturnInWarehouse buy);
    }
}