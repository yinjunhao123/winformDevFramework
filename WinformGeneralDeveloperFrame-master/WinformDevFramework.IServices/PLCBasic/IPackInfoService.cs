using PLCBasic;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IServices;

namespace WinformDevFramework.IServices.PLCBasic
{
    /// <summary>
    /// 包装服务接口
    /// </summary>
    public interface IPackInfoService : IBaseServices<PackInfo>
    {
        /// <summary>
        /// 创建包装信息
        /// </summary>
        /// <param name="packInfo">包装信息</param>
        /// <returns>是否创建成功</returns>
        Task<bool> CreatePackInfoAsync(PackInfo packInfo);

        /// <summary>
        /// 根据包装箱号查询包装信息
        /// </summary>
        /// <param name="packBarcode">包装箱号</param>
        /// <returns>包装信息</returns>
        Task<PackInfo> GetPackInfoByBarcodeAsync(string packBarcode);

        /// <summary>
        /// 检查包装箱号是否存在
        /// </summary>
        /// <param name="packBarcode">包装箱号</param>
        /// <returns>是否存在</returns>
        Task<bool> PackBarcodeExistsAsync(string packBarcode);

        /// <summary>
        /// 添加条码到包装
        /// </summary>
        /// <param name="packInfoId">包装主表ID</param>
        /// <param name="barCode">条码</param>
        /// <param name="productModel">产品型号</param>
        /// <param name="recipeCode">配方编码</param>
        /// <param name="createUser">创建人</param>
        /// <returns>是否添加成功</returns>
        Task<bool> AddBarCodeToPackAsync(long packInfoId, string barCode, string productModel, string recipeCode, string createUser);

        /// <summary>
        /// 检查条码是否已存在于任何包装中
        /// </summary>
        /// <param name="barCode">条码</param>
        /// <returns>是否存在</returns>
        Task<bool> BarCodeExistsInAnyPackAsync(string barCode);

        /// <summary>
        /// 查询包装明细列表
        /// </summary>
        /// <param name="packInfoId">包装主表ID</param>
        /// <returns>明细列表</returns>
        Task<List<PackInfoDetail>> GetPackDetailListAsync(long packInfoId);

        /// <summary>
        /// 删除包装信息（级联删除明细）
        /// </summary>
        /// <param name="id">包装主表ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeletePackInfoAsync(long id);
    }
}
