using PLCBasic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository.PLCBasic
{
    /// <summary>
    /// 条码历史表仓储接口
    /// </summary>
    public interface IWipBarcodeHistoryRepository : IBaseRepository<WipBarcodeHistory>
    {
        /// <summary>
        /// 根据条码ID查询历史记录
        /// </summary>
        /// <param name="barCodeId">条码ID</param>
        /// <returns>历史记录列表</returns>
        Task<List<WipBarcodeHistory>> QueryByBarCodeIdAsync(long barCodeId);

        /// <summary>
        /// 根据条码查询历史记录
        /// </summary>
        /// <param name="barCode">条码</param>
        /// <returns>历史记录列表</returns>
        Task<List<WipBarcodeHistory>> QueryByBarCodeAsync(string barCode);

        /// <summary>
        /// 保存条码历史记录（将WipBarCode数据复制到历史表）
        /// </summary>
        /// <param name="wipBarCode">条码实体</param>
        /// <param name="operationType">操作类型</param>
        /// <returns>是否保存成功</returns>
        Task<bool> SaveHistoryAsync(WipBarCode wipBarCode, string operationType);
    }
}
