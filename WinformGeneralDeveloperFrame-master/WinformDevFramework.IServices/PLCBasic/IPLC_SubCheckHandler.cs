using System.Threading.Tasks;
using WinformDevFramework.Models.PLCBasic;

namespace WinformDevFramework.IServices.PLCBasic
{
    public interface IPLC_SubCheckHandler
    {
        Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge);
    }
}