using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_OtherOutWarehouseRepository:IBaseRepository<w_OtherOutWarehouse>
    {
        int AddOtherOutWarehouseInfo(w_OtherOutWarehouse buy, List<w_OtherOutWarehouseDetail> detail);
        bool UpdateOtherOutWarehouseInfo(w_OtherOutWarehouse buy, List<w_OtherOutWarehouseDetail> detail);
        bool DeleteOtherOutWarehouseInfo(w_OtherOutWarehouse buy);
    }
}