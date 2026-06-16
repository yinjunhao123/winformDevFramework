using PLCBasic;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.PLCBasic;

namespace WinformDevFramework.Repository.PLCBasic
{
    /// <summary>
    /// 条码历史表仓储实现
    /// </summary>
    public class WipBarcodeHistoryRepository : GenericRepository<WipBarcodeHistory>, IWipBarcodeHistoryRepository
    {
        public WipBarcodeHistoryRepository(ISqlSugarClient db) : base(db)
        {
        }

        /// <summary>
        /// 根据条码ID查询历史记录
        /// </summary>
        public async Task<List<WipBarcodeHistory>> QueryByBarCodeIdAsync(long barCodeId)
        {
            return await DbBaseClient.Queryable<WipBarcodeHistory>()
                .Where(h => h.BarCodeId == barCodeId)
                .OrderByDescending(h => h.SnapshotTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据条码查询历史记录
        /// </summary>
        public async Task<List<WipBarcodeHistory>> QueryByBarCodeAsync(string barCode)
        {
            return await DbBaseClient.Queryable<WipBarcodeHistory>()
                .Where(h => h.BarCode == barCode)
                .OrderByDescending(h => h.SnapshotTime)
                .ToListAsync();
        }

        /// <summary>
        /// 保存条码历史记录（将WipBarCode数据复制到历史表）
        /// </summary>
        public async Task<bool> SaveHistoryAsync(WipBarCode wipBarCode, string operationType)
        {
            if (wipBarCode == null)
                return false;

            var history = new WipBarcodeHistory
            {
                BarCodeId = wipBarCode.BarCodeId,
                RFIDCode = wipBarCode.RFIDCode,
                BarCode = wipBarCode.BarCode,
                ModelCode = wipBarCode.ModelCode,
                BarCodeType = wipBarCode.BarCodeType,
                StationCode = wipBarCode.StationCode,
                InputTime = wipBarCode.InputTime,
                OutputTime = wipBarCode.OutputTime,
                CreateUser = wipBarCode.CreateUser,
                CreateTime = wipBarCode.CreateTime,
                UpdateUser = wipBarCode.UpdateUser,
                UpdateTime = wipBarCode.UpdateTime,
                Status = wipBarCode.Status,
                RepairCount = wipBarCode.RepairCount,
                PrStationCode = wipBarCode.PrStationCode,
                NextStationCode = wipBarCode.NextStationCode,
                BatchCode = wipBarCode.BatchCode,
                OperationType = operationType,
                SnapshotTime = DateTime.Now
            };

            return await InsertAsync(history) > 0;
        }
    }
}
