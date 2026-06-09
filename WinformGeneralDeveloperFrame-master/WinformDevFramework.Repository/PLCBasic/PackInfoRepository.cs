using PLCBasic;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;

namespace WinformDevFramework.Repository.PLCBasic
{
    /// <summary>
    /// 包装主表仓储实现
    /// </summary>
    public class PackInfoRepository : GenericRepository<PackInfo>, IPackInfoRepository
    {
        public PackInfoRepository(ISqlSugarClient db) : base(db)
        {
        }

        /// <summary>
        /// 根据包装箱号查询包装信息
        /// </summary>
        public async Task<PackInfo> QueryByPackBarcodeAsync(string packBarcode)
        {
            return await DbBaseClient.Queryable<PackInfo>()
                .Where(p => p.PackBarcode == packBarcode)
                .FirstAsync();
        }

        /// <summary>
        /// 检查包装箱号是否存在
        /// </summary>
        public async Task<bool> ExistsByPackBarcodeAsync(string packBarcode)
        {
            return await DbBaseClient.Queryable<PackInfo>()
                .AnyAsync(p => p.PackBarcode == packBarcode);
        }

        /// <summary>
        /// 更新已打包数量和待打包数量
        /// </summary>
        public async Task<bool> UpdateQtyAsync(long id, int haveqty, int unqty, string updateUser)
        {
            return await DbBaseClient.Updateable<PackInfo>()
                .SetColumns(p => new PackInfo
                {
                    Haveqty = haveqty,
                    Unqty = unqty,
                    UpdateTime = DateTime.Now,
                    UpdateUser = updateUser
                })
                .Where(p => p.ID == id)
                .ExecuteCommandAsync() > 0;
        }
    }
}
