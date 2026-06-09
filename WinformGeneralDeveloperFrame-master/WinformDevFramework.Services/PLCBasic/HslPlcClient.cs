using HslCommunication;
using HslCommunication.Core.Types;
using HslCommunication.Profinet.Melsec;
using HslCommunication.Profinet.Siemens;
using PLCBasic;
using System;
using System.Threading;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC客户端封装类，基于HslCommunication实现多协议PLC通信
    /// 支持：西门子S7系列、三菱Melsec系列、Modbus TCP
    /// </summary>
    public class HslPlcClient : IDisposable
    {
        /// <summary>
        /// 西门子PLC客户端
        /// </summary>
        private SiemensS7Net _siemensClient;

        /// <summary>
        /// 三菱PLC客户端
        /// </summary>
        private MelsecMcNet _mitsubishiClient;

        /// <summary>
        /// Modbus TCP客户端
        /// </summary>
        private HslCommunication.ModBus.ModbusTcpNet _modbusClient;

        /// <summary>
        /// 线程同步锁对象
        /// </summary>
        private object _lockObj = new object();

        /// <summary>
        /// PLC编码（唯一标识）
        /// </summary>
        public string PlcCode { get; }

        /// <summary>
        /// 当前连接状态
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 最后连接时间
        /// </summary>
        public DateTime LastConnectTime { get; private set; }

        /// <summary>
        /// 连续错误计数
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// PLC IP地址
        /// </summary>
        private string ipAddress;

        /// <summary>
        /// PLC端口号
        /// </summary>
        private int port;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">PLC配置信息</param>
        public HslPlcClient(PLC_Config config)
        {
            PlcCode = config.PlcCode;
            ipAddress = config.IpAddress;
            port = config.Port ?? 102;

            InitializeClient(config.PlcType?.ToLower());
        }

        /// <summary>
        /// 根据协议类型初始化对应的PLC客户端
        /// </summary>
        /// <param name="protocol">协议类型（s7/siemens、fx/mitsubishi/melsec、modbus）</param>
        private void InitializeClient(string protocol)
        {
            switch (protocol)
            {
                case "s7":
                case "siemens":
                    _siemensClient = new SiemensS7Net(SiemensPLCS.S1500, ipAddress);
                    _siemensClient.Port = port;
                    break;
                case "fx":
                case "mitsubishi":
                case "melsec":
                    _mitsubishiClient = new MelsecMcNet(ipAddress, port);
                    break;
                case "modbus":
                    _modbusClient = new HslCommunication.ModBus.ModbusTcpNet(ipAddress, port);
                    break;
                default:
                    _siemensClient = new SiemensS7Net(SiemensPLCS.S1500, ipAddress);
                    _siemensClient.Port = port;
                    break;
            }
        }

        /// <summary>
        /// 建立PLC连接
        /// </summary>
        /// <returns>连接操作结果</returns>
        public OperateResult Connect()
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult result = null;

                    if (_siemensClient != null)
                    {
                        result = _siemensClient.ConnectServer();
                    }
                    else if (_mitsubishiClient != null)
                    {
                        result = _mitsubishiClient.ConnectServer();
                    }
                    else if (_modbusClient != null)
                    {
                        result = _modbusClient.ConnectServer();
                    }

                    if (result != null && result.IsSuccess)
                    {
                        IsConnected = true;
                        LastConnectTime = DateTime.Now;
                        ErrorCount = 0;
                    }
                    else
                    {
                        IsConnected = false;
                        ErrorCount++;
                    }
                    return result ?? new OperateResult("未初始化客户端");
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    ErrorCount++;
                    return new OperateResult(ex.Message);
                }
            }
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        public void Disconnect()
        {
            lock (_lockObj)
            {
                try
                {
                    _siemensClient?.ConnectClose();
                    _mitsubishiClient?.ConnectClose();
                    _modbusClient?.ConnectClose();
                    IsConnected = false;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 读取单个布尔值
        /// </summary>
        /// <param name="address">PLC地址（如：M1.0、X0.0、%M1.0）</param>
        /// <returns>读取结果，包含布尔值</returns>
        public OperateResult<bool> ReadBool(string address)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.ReadBool(address);
                if (_mitsubishiClient != null) return _mitsubishiClient.ReadBool(address);
                if (_modbusClient != null) return _modbusClient.ReadBool(address);
                return new OperateResult<bool>("未初始化客户端");
            });
        }

        /// <summary>
        /// 读取布尔数组
        /// </summary>
        /// <param name="address">起始PLC地址</param>
        /// <param name="length">读取长度</param>
        /// <returns>读取结果，包含布尔数组</returns>
        public OperateResult<bool[]> ReadBool(string address, ushort length)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.ReadBool(address, length);
                if (_mitsubishiClient != null) return _mitsubishiClient.ReadBool(address, length);
                if (_modbusClient != null) return _modbusClient.ReadBool(address, length);
                return new OperateResult<bool[]>("未初始化客户端");
            });
        }

        /// <summary>
        /// 读取32位整数
        /// </summary>
        /// <param name="address">PLC地址（如：DB1.DBW0、D0、4x0001）</param>
        /// <returns>读取结果，包含整数值</returns>
        public OperateResult<int> ReadInt32(string address)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.ReadInt32(address);
                if (_mitsubishiClient != null) return _mitsubishiClient.ReadInt32(address);
                if (_modbusClient != null) return _modbusClient.ReadInt32(address);
                return new OperateResult<int>("未初始化客户端");
            });
        }

        /// <summary>
        /// 读取单精度浮点数
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <returns>读取结果，包含浮点数值</returns>
        public OperateResult<float> ReadFloat(string address)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.ReadFloat(address);
                if (_mitsubishiClient != null) return _mitsubishiClient.ReadFloat(address);
                if (_modbusClient != null) return _modbusClient.ReadFloat(address);
                return new OperateResult<float>("未初始化客户端");
            });
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="length">字符串长度</param>
        /// <returns>读取结果，包含字符串</returns>
        public OperateResult<string> ReadString(string address, ushort length)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.ReadString(address, length);
                if (_mitsubishiClient != null) return _mitsubishiClient.ReadString(address, length);
                if (_modbusClient != null) return _modbusClient.ReadString(address, length);
                return new OperateResult<string>("未初始化客户端");
            });
        }

        /// <summary>
        /// 写入布尔值
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的布尔值</param>
        /// <returns>写入操作结果</returns>
        public OperateResult Write(string address, bool value)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.Write(address, value);
                if (_mitsubishiClient != null) return _mitsubishiClient.Write(address, value);
                if (_modbusClient != null) return _modbusClient.Write(address, value);
                return new OperateResult("未初始化客户端");
            });
        }

        /// <summary>
        /// 批量写入布尔数组（连续地址）
        /// </summary>
        /// <param name="address">起始PLC地址</param>
        /// <param name="values">要写入的布尔数组</param>
        /// <returns>写入操作结果</returns>
        public OperateResult Write(string address, bool[] values)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.Write(address, values);
                if (_mitsubishiClient != null) return _mitsubishiClient.Write(address, values);
                if (_modbusClient != null) return _modbusClient.Write(address, values);
                return new OperateResult("未初始化客户端");
            });
        }


        /// <summary>
        /// 写入32位整数
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的整数值</param>
        /// <returns>写入操作结果</returns>
        public OperateResult Write(string address, int value)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.Write(address, value);
                if (_mitsubishiClient != null) return _mitsubishiClient.Write(address, value);
                if (_modbusClient != null) return _modbusClient.Write(address, value);
                return new OperateResult("未初始化客户端");
            });
        }

        /// <summary>
        /// 写入单精度浮点数
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的浮点数值</param>
        /// <returns>写入操作结果</returns>
        public OperateResult Write(string address, float value)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.Write(address, value);
                if (_mitsubishiClient != null) return _mitsubishiClient.Write(address, value);
                if (_modbusClient != null) return _modbusClient.Write(address, value);
                return new OperateResult("未初始化客户端");
            });
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的字符串</param>
        /// <returns>写入操作结果</returns>
        public OperateResult Write(string address, string value)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.Write(address, value);
                if (_mitsubishiClient != null) return _mitsubishiClient.Write(address, value);
                if (_modbusClient != null) return _modbusClient.Write(address, value);
                return new OperateResult("未初始化客户端");
            });
        }


        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的双精度浮点数</param>
        /// <returns>写入操作结果</returns>
        public OperateResult Write(string address, double value)
        {
            return ExecuteWithRetry(() =>
            {
                if (_siemensClient != null) return _siemensClient.Write(address, value);
                if (_mitsubishiClient != null) return _mitsubishiClient.Write(address, value);
                if (_modbusClient != null) return _modbusClient.Write(address, value);
                return new OperateResult("未初始化客户端");
            });
        }

        /// <summary>
        /// 带重试机制的操作执行方法
        /// </summary>
        /// <typeparam name="T">操作结果类型</typeparam>
        /// <param name="operation">要执行的操作</param>
        /// <returns>操作结果</returns>
        private T ExecuteWithRetry<T>(Func<T> operation) where T : OperateResult
        {
            int retryCount = 3;
            int delayMs = 100;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    var result = operation();
                    if (result.IsSuccess)
                    {
                        ErrorCount = 0;
                        return result;
                    }

                    if (!IsConnected && i < retryCount - 1)
                    {
                        Connect();
                        Thread.Sleep(delayMs);
                    }
                    else if (i < retryCount - 1)
                    {
                        Thread.Sleep(delayMs);
                    }
                }
                catch (Exception)
                {
                    if (i < retryCount - 1)
                    {
                        Thread.Sleep(delayMs);
                    }
                }
            }

            ErrorCount++;
            return operation();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }
}
