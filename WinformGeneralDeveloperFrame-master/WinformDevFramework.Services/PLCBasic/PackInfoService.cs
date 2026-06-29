using Microsoft.Extensions.Logging;
using PLCBasic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Repository.UnitOfWork;
using WinformDevFramework.IServices;
using WinformDevFramework.IRepository;
using WinformDevFramework.IRepository.UnitOfWork;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// 包装服务实现
    /// </summary>
    public class PackInfoService : BaseServices<PackInfo>, IPackInfoService
    {
        private readonly IPackInfoRepository _packInfoRepository;
        private readonly IPackInfoDetailRepository _packInfoDetailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PackInfoService> _logger;

        public PackInfoService(
            IPackInfoRepository packInfoRepository,
            IPackInfoDetailRepository packInfoDetailRepository,
            IUnitOfWork unitOfWork,
            ILogger<PackInfoService> logger,
            IBaseRepository<PackInfo> baseRepository)
        {
            _packInfoRepository = packInfoRepository;
            _packInfoDetailRepository = packInfoDetailRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            BaseDal = baseRepository;
        }

        /// <summary>
        /// 创建包装信息
        /// </summary>
        public async Task<bool> CreatePackInfoAsync(PackInfo packInfo)
        {
            try
            {
                _unitOfWork.BeginTran();
                
                // 设置待打包数量等于最大数量
                packInfo.Unqty = packInfo.Maxqty;
                packInfo.Haveqty = 0;

                var result = _packInfoRepository.Insert(packInfo);
                
                if (result > 0)
                {
                    _unitOfWork.CommitTran();
                    _logger.LogInformation($"创建包装信息成功: PackBarcode={packInfo.PackBarcode}");
                    return true;
                }

                _unitOfWork.RollbackTran();
                return false;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTran();
                _logger.LogError(ex, $"创建包装信息失败: PackBarcode={packInfo.PackBarcode}");
                throw;
            }
        }

        /// <summary>
        /// 根据包装箱号查询包装信息
        /// </summary>
        public async Task<PackInfo> GetPackInfoByBarcodeAsync(string packBarcode)
        {
            return await _packInfoRepository.QueryByPackBarcodeAsync(packBarcode);
        }

        /// <summary>
        /// 检查包装箱号是否存在
        /// </summary>
        public async Task<bool> PackBarcodeExistsAsync(string packBarcode)
        {
            return await _packInfoRepository.ExistsByPackBarcodeAsync(packBarcode);
        }

        /// <summary>
        /// 添加条码到包装
        /// </summary>
        public async Task<bool> AddBarCodeToPackAsync(long packInfoId, string barCode, string productModel, string recipeCode, string createUser)
        {
            try
            {
                _unitOfWork.BeginTran();

                // 检查条码是否已存在于当前包装中
                var existsInPack = await _packInfoDetailRepository.ExistsBarCodeAsync(packInfoId, barCode);
                if (existsInPack)
                {
                    _logger.LogWarning($"条码已存在于当前包装中: BarCode={barCode}, PackInfoId={packInfoId}");
                    return false;
                }

                // 创建明细记录
                var detail = new PackInfoDetail
                {
                    PackInfoId = packInfoId,
                    BarCode = barCode,
                    ProductModel = productModel,
                    RecipeCode = recipeCode,
                    CreateUser = createUser,
                    CreateTime = DateTime.Now
                };

                var result = _packInfoDetailRepository.Insert(detail);
                if (result <= 0)
                {
                    _unitOfWork.RollbackTran();
                    return false;
                }

                // 更新主表的已打包数量和待打包数量
                var packInfo = await _packInfoRepository.QueryByIdAsync(packInfoId);
                if (packInfo != null)
                {
                    int newHaveQty = packInfo.Haveqty + 1;
                    int newUnQty = packInfo.Unqty - 1;

                    await _packInfoRepository.UpdateQtyAsync(packInfoId, newHaveQty, newUnQty, createUser);
                }

                _unitOfWork.CommitTran();
                _logger.LogInformation($"条码添加到包装成功: BarCode={barCode}, PackInfoId={packInfoId}");
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTran();
                _logger.LogError(ex, $"添加条码到包装失败: BarCode={barCode}, PackInfoId={packInfoId}");
                throw;
            }
        }

        /// <summary>
        /// 检查条码是否已存在于任何包装中
        /// </summary>
        public async Task<bool> BarCodeExistsInAnyPackAsync(string barCode)
        {
            return await _packInfoDetailRepository.ExistsBarCodeInAnyPackAsync(barCode);
        }

        /// <summary>
        /// 查询包装明细列表
        /// </summary>
        public async Task<List<PackInfoDetail>> GetPackDetailListAsync(long packInfoId)
        {
            return await _packInfoDetailRepository.QueryByPackInfoIdAsync(packInfoId);
        }

        /// <summary>
        /// 根据条码查询所属包装主表ID列表
        /// </summary>
        public async Task<List<long>> GetPackInfoIdsByBarCodeAsync(string barCode)
        {
            var details =  _packInfoDetailRepository.Query()
                .Where(d => d.BarCode.Contains(barCode))
                .Select(d => d.PackInfoId)
                .Distinct()
                .ToList();
            return details;
        }

        /// <summary>
        /// 删除包装信息（级联删除明细）
        /// </summary>
        public async Task<bool> DeletePackInfoAsync(long id)
        {
            try
            {
                _unitOfWork.BeginTran();

                // 先删除明细
                await _packInfoDetailRepository.DeleteByPackInfoIdAsync(id);

                // 再删除主表
                var result = _packInfoRepository.DeleteById(id);

                if (result)
                {
                    _unitOfWork.CommitTran();
                    _logger.LogInformation($"删除包装信息成功: ID={id}");
                    return true;
                }

                _unitOfWork.RollbackTran();
                return false;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTran();
                _logger.LogError(ex, $"删除包装信息失败: ID={id}");
                throw;
            }
        }
    }
}
