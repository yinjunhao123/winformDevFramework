using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_ReceivableRepository:IBaseRepository<w_Receivable>
    {
        int AddReceivableInfo(w_Receivable buy, List<w_ReceivableDetail> detail);
        bool UpdateReceivableInfo(w_Receivable buy, List<w_ReceivableDetail> detail);
        bool DeleteReceivableInfo(w_Receivable buy);
    }
}