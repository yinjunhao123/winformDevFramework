using Microsoft.Extensions.Logging;
using PLCBasic;
using PLCBasic.IRepository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.IServices.PLCBasic;

namespace WinformDevFramework.Services.PLCBasic
{
    public class ReworkRecordService : IReworkRecordService
    {
        private readonly IReworkRecordRepository _reworkRecordRepository;
        private readonly ILogger<ReworkRecordService> _logger;

        public ReworkRecordService(IReworkRecordRepository reworkRecordRepository, ILogger<ReworkRecordService> logger)
        {
            _reworkRecordRepository = reworkRecordRepository;
            _logger = logger;
        }

        public async Task<bool> AddReworkRecordAsync(ReworkRecord record)
        {
            try
            {
                record.CreateTime = DateTime.Now;
                record.UpdateTime = DateTime.Now;
                await _reworkRecordRepository.InsertAsync(record);
                _logger.LogInformation($"添加返工记录成功: RfidBarCode={record.RfidBarCode}, FromStationCode={record.FromStationCode}, ToStationCode={record.ToStationCode}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"添加返工记录失败: RfidBarCode={record.RfidBarCode}");
                return false;
            }
        }

        public async Task<bool> UpdateReworkRecordAsync(ReworkRecord record)
        {
            try
            {
                record.UpdateTime = DateTime.Now;
                await _reworkRecordRepository.UpdateAsync(record);
                _logger.LogInformation($"修改返工记录成功: ReworkId={record.ReworkId}, RfidBarCode={record.RfidBarCode}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"修改返工记录失败: ReworkId={record.ReworkId}");
                return false;
            }
        }

        public async Task<bool> DeleteReworkRecordAsync(long reworkId)
        {
            try
            {
                await _reworkRecordRepository.DeleteByIdAsync(reworkId);
                _logger.LogInformation($"删除返工记录成功: ReworkId={reworkId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除返工记录失败: ReworkId={reworkId}");
                return false;
            }
        }

        public async Task<bool> DeleteReworkRecordByRfidCodeAsync(string rfidCode)
        {
            try
            {
                var records = await _reworkRecordRepository.QueryListByClauseAsync(r => r.RfidBarCode == rfidCode);
                foreach (var record in records)
                {
                    await _reworkRecordRepository.DeleteAsync(record);
                }
                _logger.LogInformation($"删除返工记录成功: RfidBarCode={rfidCode}, 删除数量={records.Count}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除返工记录失败: RfidBarCode={rfidCode}");
                return false;
            }
        }

        public async Task<ReworkRecord> GetReworkRecordByIdAsync(long reworkId)
        {
            try
            {
                return await _reworkRecordRepository.QueryByClauseAsync(r => r.ReworkId == reworkId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询返工记录失败: ReworkId={reworkId}");
                return null;
            }
        }

        public async Task<List<ReworkRecord>> GetReworkRecordsByRfidCodeAsync(string rfidCode)
        {
            try
            {
                return await _reworkRecordRepository.QueryListByClauseAsync(r => r.RfidBarCode == rfidCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询返工记录失败: RfidBarCode={rfidCode}");
                return new List<ReworkRecord>();
            }
        }

        public async Task<List<ReworkRecord>> GetReworkRecordsByProductModelAsync(string productModel)
        {
            try
            {
                return await _reworkRecordRepository.QueryListByClauseAsync(r => r.ProductModel == productModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询返工记录失败: ProductModel={productModel}");
                return new List<ReworkRecord>();
            }
        }

        public async Task<List<ReworkRecord>> GetAllReworkRecordsAsync()
        {
            try
            {
                return await _reworkRecordRepository.QueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询所有返工记录失败");
                return new List<ReworkRecord>();
            }
        }
    }
}