using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository
{
    public interface Iw_PaymentRepository:IBaseRepository<w_Payment>
    {
        int AddPaymentInfo(w_Payment buy, List<w_PaymentDetail> detail);
        bool UpdatePaymentInfo(w_Payment buy, List<w_PaymentDetail> detail);
        bool DeletePaymentInfo(w_Payment buy);
    }
}