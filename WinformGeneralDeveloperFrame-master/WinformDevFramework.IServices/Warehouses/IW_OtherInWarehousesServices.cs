using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models;


namespace WinformDevFramework.IServices
{
    public interface Iw_OtherInWarehouseServices: IBaseServices<w_OtherInWarehouse>
    {
        int AddOtherInWarehouseInfo(w_OtherInWarehouse buy, List<w_OtherInWarehouseDetail> detail);
        bool UpdateOtherInWarehouseInfo(w_OtherInWarehouse buy, List<w_OtherInWarehouseDetail> detail);
        bool DeleteOtherInWarehouseInfo(w_OtherInWarehouse buy);

    }
}