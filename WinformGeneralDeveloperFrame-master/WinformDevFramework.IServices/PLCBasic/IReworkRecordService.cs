using PLCBasic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinformDevFramework.IServices.PLCBasic
{
    public interface IReworkRecordService
    {
        Task<bool> AddReworkRecordAsync(ReworkRecord record);
        Task<bool> UpdateReworkRecordAsync(ReworkRecord record);
        Task<bool> DeleteReworkRecordAsync(long reworkId);
        Task<bool> DeleteReworkRecordByRfidCodeAsync(string rfidCode);
        Task<ReworkRecord> GetReworkRecordByIdAsync(long reworkId);
        Task<List<ReworkRecord>> GetReworkRecordsByRfidCodeAsync(string rfidCode);
        Task<List<ReworkRecord>> GetReworkRecordsByProductModelAsync(string productModel);
        Task<List<ReworkRecord>> GetAllReworkRecordsAsync();
    }
}