using PLCBasic;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;

namespace WinformDevFramework.Repository.PLCBasic
{
    /// <summary>
    /// 包装明细表仓储实现
    /// </summary>
    public class PackInfoDetailRepository : GenericRepository<PackInfoDetail>, IPackInfoDetailRepository
    {
        public PackInfoDetailRepository(ISqlSugarClient db) : base(db)
        {
        }

        /// <summary>
        /// 根据包装主表ID查询明细列表
        /// </summary>
        public async Task<List<PackInfoDetail>> QueryByPackInfoIdAsync(long packInfoId)
        {
            return await DbBaseClient.Queryable<PackInfoDetail>()
                .Where(p => p.PackInfoId == packInfoId)
                .OrderBy(p => p.CreateTime)
                .ToListAsync();
        }

        /// <summary>
        /// 检查条码是否已存在于指定包装中
        /// </summary>
        public async Task<bool> ExistsBarCodeAsync(long packInfoId, string barCode)
        {
            return await DbBaseClient.Queryable<PackInfoDetail>()
                .AnyAsync(p => p.PackInfoId == packInfoId && p.BarCode == barCode);
        }

        /// <summary>
        /// 检查条码是否已存在于任何包装中
        /// </summary>
        public async Task<bool> ExistsBarCodeInAnyPackAsync(string barCode)
        {
            return await DbBaseClient.Queryable<PackInfoDetail>()
                .AnyAsync(p => p.BarCode == barCode);
        }

        /// <summary>
        /// 根据条码查询所属包装信息
        /// </summary>
        public async Task<PackInfoDetail> QueryByBarCodeAsync(string barCode)
        {
            return await DbBaseClient.Queryable<PackInfoDetail>()
                .Where(p => p.BarCode == barCode)
                .FirstAsync();
        }

        /// <summary>
        /// 删除指定包装主表ID下的所有明细
        /// </summary>
        public async Task<int> DeleteByPackInfoIdAsync(long packInfoId)
        {
            return await DbBaseClient.Deleteable<PackInfoDetail>()
                .Where(p => p.PackInfoId == packInfoId)
                .ExecuteCommandAsync();
        }
    }
}
