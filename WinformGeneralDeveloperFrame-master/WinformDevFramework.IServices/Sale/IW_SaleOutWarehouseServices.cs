using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models;


namespace WinformDevFramework.IServices
{
    public interface Iw_SaleOutWarehouseServices: IBaseServices<w_SaleOutWarehouse>
    {
        int AddSaleOutWarehouseInfo(w_SaleOutWarehouse buy, List<w_SaleOutWarehouseDetail> detail);
        bool UpdateSaleOutWarehouseInfo(w_SaleOutWarehouse buy, List<w_SaleOutWarehouseDetail> detail);
        bool DeleteSaleOutWarehouseInfo(w_SaleOutWarehouse buy);
    }
}