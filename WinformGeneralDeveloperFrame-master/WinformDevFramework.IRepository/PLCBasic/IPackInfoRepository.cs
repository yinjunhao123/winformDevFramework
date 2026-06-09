using PLCBasic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinformDevFramework.IRepository.PLCBasic
{
    /// <summary>
    /// 包装主表仓储接口
    /// </summary>
    public interface IPackInfoRepository : IBaseRepository<PackInfo>
    {
        /// <summary>
        /// 根据包装箱号查询包装信息
        /// </summary>
        /// <param name="packBarcode">包装箱号</param>
        /// <returns>包装信息</returns>
        Task<PackInfo> QueryByPackBarcodeAsync(string packBarcode);

        /// <summary>
        /// 检查包装箱号是否存在
        /// </summary>
        /// <param name="packBarcode">包装箱号</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsByPackBarcodeAsync(string packBarcode);

        /// <summary>
        /// 更新已打包数量和待打包数量
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="haveqty">已打包数量</param>
        /// <param name="unqty">待打包数量</param>
        /// <param name="updateUser">更新人</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateQtyAsync(long id, int haveqty, int unqty, string updateUser);
    }
}
