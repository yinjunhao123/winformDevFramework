using HslCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models.Common;

namespace WinformDevFramework.IServices.PLCBasic
{
    /// <summary>
    /// 批量读取请求
    /// </summary>
    public class BatchReadRequest
    {
        /// <summary>
        /// 起始地址
        /// </summary>
        public string StartAddress { get; set; }

        /// <summary>
        /// 读取长度
        /// </summary>
        public ushort Length { get; set; }

        /// <summary>
        /// 对应的映射列表（索引与读取结果一一对应）
        /// </summary>
        public List<int> MappingIndices { get; set; } = new List<int>();
    }

    public interface IPlcCommunicationService
    {
        bool Connect(string plcCode);
        bool Disconnect(string plcCode);
        bool IsConnected(string plcCode);

        OperateResult<bool> ReadBool(string plcCode, string address);
        OperateResult<bool[]> ReadBool(string plcCode, string address, ushort length);
        OperateResult<int> ReadInt32(string plcCode, string address);
        OperateResult<float> ReadFloat(string plcCode, string address);
        OperateResult<string> ReadString(string plcCode, string address, ushort length);

        OperateResult<byte> ReadByte(string plcCode, string address); 


        /// <summary>
        /// 批量读取多个布尔地址
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="addresses">地址列表</param>
        /// <returns>布尔值数组，顺序与地址列表对应</returns>
        Task<bool[]> ReadBatchAsync(string plcCode, List<string> addresses);

        /// <summary>
        /// 批量读取多个布尔地址（支持连续地址批量读取）
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="requests">批量读取请求列表（起始地址+长度）</param>
        /// <param name="totalCount">总结果数量</param>
        /// <returns>布尔值数组，顺序与请求中的映射索引对应</returns>
        Task<bool[]> ReadBatchRequestsAsync(string plcCode, List<BatchReadRequest> requests, int totalCount);

        OperateResult Write(string plcCode, string address, double value);

        OperateResult Write(string plcCode, string address, bool value);
        OperateResult Write(string plcCode, string address, int value);
        OperateResult Write(string plcCode, string address, float value);
        OperateResult Write(string plcCode, string address, string value);

        Task<bool[]> WriteBatchAsync(string plcCode, List<WinformDevFramework.Models.Common.BatchWriteRequest> requests);
        void InitializeConnections();
        void Cleanup();
        int GetAddressBitOffset(string address);
        
        /// <summary>
        /// 重新连接指定的PLC
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否重连成功</returns>
        Task<bool> ReconnectAsync(string plcCode);
    }
}

