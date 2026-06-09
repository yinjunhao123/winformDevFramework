using PLCBasic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository.PLCBasic
{
    /// <summary>
    /// 包装明细表仓储接口
    /// </summary>
    public interface IPackInfoDetailRepository : IBaseRepository<PackInfoDetail>
    {
        /// <summary>
        /// 根据包装主表ID查询明细列表
        /// </summary>
        /// <param name="packInfoId">包装主表ID</param>
        /// <returns>明细列表</returns>
        Task<List<PackInfoDetail>> QueryByPackInfoIdAsync(long packInfoId);

        /// <summary>
        /// 检查条码是否已存在于指定包装中
        /// </summary>
        /// <param name="packInfoId">包装主表ID</param>
        /// <param name="barCode">条码</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsBarCodeAsync(long packInfoId, string barCode);

        /// <summary>
        /// 检查条码是否已存在于任何包装中
        /// </summary>
        /// <param name="barCode">条码</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsBarCodeInAnyPackAsync(string barCode);

        /// <summary>
        /// 根据条码查询所属包装信息
        /// </summary>
        /// <param name="barCode">条码</param>
        /// <returns>包装明细信息</returns>
        Task<PackInfoDetail> QueryByBarCodeAsync(string barCode);

        /// <summary>
        /// 删除指定包装主表ID下的所有明细
        /// </summary>
        /// <param name="packInfoId">包装主表ID</param>
        /// <returns>删除数量</returns>
        Task<int> DeleteByPackInfoIdAsync(long packInfoId);
    }
}
