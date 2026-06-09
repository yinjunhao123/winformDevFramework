using PLCBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC连接管理器
    /// 管理所有PLC设备的连接状态，提供线程安全的连接信息管理
    /// </summary>
    public class PlcConnectionManager
    {
        /// <summary>
        /// PLC连接信息字典（线程安全）
        /// Key: PlcID, Value: 连接信息
        /// </summary>
        private readonly ConcurrentDictionary<long, PlcConnectionInfo> _connections;

        /// <summary>
        /// 初始化锁对象
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public PlcConnectionManager()
        {
            _connections = new ConcurrentDictionary<long, PlcConnectionInfo>();
        }

        /// <summary>
        /// 添加或更新PLC连接信息
        /// </summary>
        /// <param name="plcConfig">PLC配置信息</param>
        public void AddOrUpdateConnection(PLC_Config plcConfig)
        {
            var connectionInfo = new PlcConnectionInfo
            {
                PlcID = plcConfig.PlcID,
                PlcCode = plcConfig.PlcCode,
                IpAddress = plcConfig.IpAddress,
                Port = plcConfig.Port ?? 102,
                PlcType = plcConfig.PlcType,
                LastConnectTime = DateTime.Now,
                IsOnline = false,
                ErrorCount = 0
            };

            _connections.AddOrUpdate(plcConfig.PlcID, connectionInfo, (key, old) => connectionInfo);
        }

        /// <summary>
        /// 尝试获取PLC连接信息
        /// </summary>
        /// <param name="plcId">PLC唯一标识</param>
        /// <param name="connection">输出连接信息</param>
        /// <returns>是否获取成功</returns>
        public bool TryGetConnection(long plcId, out PlcConnectionInfo connection)
        {
            return _connections.TryGetValue(plcId, out connection);
        }

        /// <summary>
        /// 获取所有PLC连接信息
        /// </summary>
        /// <returns>连接信息列表</returns>
        public IEnumerable<PlcConnectionInfo> GetAllConnections()
        {
            return _connections.Values.ToList();
        }

        /// <summary>
        /// 更新PLC连接状态
        /// </summary>
        /// <param name="plcId">PLC唯一标识</param>
        /// <param name="isOnline">是否在线</param>
        public void UpdateConnectionStatus(long plcId, bool isOnline)
        {
            if (_connections.TryGetValue(plcId, out var connection))
            {
                connection.IsOnline = isOnline;
                connection.LastConnectTime = DateTime.Now;

                // 如果离线，增加错误计数；如果在线，重置错误计数
                if (!isOnline)
                {
                    connection.ErrorCount++;
                }
                else
                {
                    connection.ErrorCount = 0;
                }
            }
        }

        /// <summary>
        /// 移除PLC连接信息
        /// </summary>
        /// <param name="plcId">PLC唯一标识</param>
        public void RemoveConnection(long plcId)
        {
            _connections.TryRemove(plcId, out _);
        }

        /// <summary>
        /// 获取在线PLC数量
        /// </summary>
        /// <returns>在线数量</returns>
        public int GetOnlineCount()
        {
            return _connections.Values.Count(c => c.IsOnline);
        }

        /// <summary>
        /// 获取离线PLC数量
        /// </summary>
        /// <returns>离线数量</returns>
        public int GetOfflineCount()
        {
            return _connections.Values.Count(c => !c.IsOnline);
        }

        /// <summary>
        /// 根据PLC编码判断连接状态
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>是否在线</returns>
        public bool IsConnected(string plcCode)
        {
            var connection = _connections.Values.FirstOrDefault(c => c.PlcCode == plcCode);
            return connection?.IsOnline ?? false;
        }

        /// <summary>
        /// 根据PLC ID判断连接状态
        /// </summary>
        /// <param name="plcId">PLC唯一标识</param>
        /// <returns>是否在线</returns>
        public bool IsConnected(long plcId)
        {
            if (_connections.TryGetValue(plcId, out var connection))
            {
                return connection.IsOnline;
            }
            return false;
        }

        /// <summary>
        /// 获取所有PLC配置信息
        /// </summary>
        /// <returns>PLC配置列表</returns>
        public IEnumerable<PLC_Config> GetAllPlcConfigs()
        {
            return _connections.Values.Select(c => new PLC_Config
            {
                PlcID = c.PlcID,
                PlcCode = c.PlcCode,
                IpAddress = c.IpAddress,
                Port = c.Port,
                PlcType = c.PlcType
            }).ToList();
        }

        /// <summary>
        /// 根据PLC编码获取连接信息
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>连接信息，未找到返回null</returns>
        public PlcConnectionInfo GetConnectionByCode(string plcCode)
        {
            return _connections.Values.FirstOrDefault(c => c.PlcCode == plcCode);
        }
    }

    /// <summary>
    /// PLC连接信息
    /// </summary>
    public class PlcConnectionInfo
    {
        /// <summary>
        /// PLC唯一标识
        /// </summary>
        public long PlcID { get; set; }

        /// <summary>
        /// PLC编码（唯一标识）
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// PLC IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// PLC端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 通信协议
        /// </summary>
        public string PlcType { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 最后连接时间
        /// </summary>
        public DateTime LastConnectTime { get; set; }

        /// <summary>
        /// 连续错误计数
        /// </summary>
        public int ErrorCount { get; set; }
    }
}
