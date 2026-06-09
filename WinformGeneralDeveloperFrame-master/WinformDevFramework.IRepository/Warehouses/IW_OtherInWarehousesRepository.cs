using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_OtherInWarehouseRepository:IBaseRepository<w_OtherInWarehouse>
    {
        int AddOtherInWarehouseInfo(w_OtherInWarehouse buy, List<w_OtherInWarehouseDetail> detail);
        bool UpdateOtherInWarehouseInfo(w_OtherInWarehouse buy, List<w_OtherInWarehouseDetail> detail);
        bool DeleteOtherInWarehouseInfo(w_OtherInWarehouse buy);

    }
}