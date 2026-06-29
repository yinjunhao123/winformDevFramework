using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// Socket通讯服务实现
    /// 用于与外部设备进行TCP Socket通信，接收图片数据并保存
    /// </summary>
    public class SocketCommunicationService : ISocketCommunicationService, IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Thread _receiveThread;
        private volatile bool _isRunning;
        private readonly ILogger<SocketCommunicationService> _logger;
        private string _remoteEndPoint;

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected => _tcpClient?.Connected ?? false;

        /// <summary>
        /// 当接收到数据时触发
        /// </summary>
        public event EventHandler<SocketDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 当连接状态变化时触发
        /// </summary>
        public event EventHandler<bool> ConnectionStatusChanged;

        public SocketCommunicationService(ILogger<SocketCommunicationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 连接到Socket服务器
        /// </summary>
        /// <param name="ipAddress">服务器IP地址</param>
        /// <param name="port">服务器端口</param>
        /// <returns>连接是否成功</returns>
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            try
            {
                // 如果已有连接，先断开
                if (_tcpClient != null)
                {
                    Disconnect();
                }

                _tcpClient = new TcpClient();
                _logger?.LogInformation($"正在连接到 {ipAddress}:{port}");

                await _tcpClient.ConnectAsync(ipAddress, port);

                if (_tcpClient.Connected)
                {
                    _remoteEndPoint = $"{ipAddress}:{port}";
                    _networkStream = _tcpClient.GetStream();
                    _isRunning = true;

                    // 启动接收线程
                    _receiveThread = new Thread(ReceiveLoop);
                    _receiveThread.IsBackground = true;
                    _receiveThread.Start();

                    _logger?.LogInformation($"成功连接到 {ipAddress}:{port}");
                    OnConnectionStatusChanged(true);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"连接到 {ipAddress}:{port} 失败");
                OnConnectionStatusChanged(false);
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _isRunning = false;

                if (_networkStream != null)
                {
                    _networkStream.Close();
                    _networkStream.Dispose();
                    _networkStream = null;
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }

                if (_receiveThread != null && _receiveThread.IsAlive)
                {
                    _receiveThread.Join(1000);
                    _receiveThread = null;
                }

                _logger?.LogInformation("Socket连接已断开");
                OnConnectionStatusChanged(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "断开连接时发生错误");
            }
        }

        /// <summary>
        /// 数据接收循环
        /// </summary>
        private void ReceiveLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (_networkStream == null || !_networkStream.CanRead)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    // 读取数据长度（假设前4字节为长度）
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = _networkStream.Read(lengthBuffer, 0, 4);

                    if (bytesRead == 0)
                    {
                        // 连接已关闭
                        _logger?.LogWarning("Socket连接已被远程关闭");
                        Disconnect();
                        return;
                    }

                    if (bytesRead < 4)
                    {
                        _logger?.LogWarning("接收到不完整的数据长度");
                        continue;
                    }

                    // 将字节数组转换为整数（大端序）
                    int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                    if (dataLength <= 0 || dataLength > 10 * 1024 * 1024) // 最大10MB
                    {
                        _logger?.LogWarning($"无效的数据长度: {dataLength}");
                        continue;
                    }

                    // 读取实际数据
                    byte[] dataBuffer = new byte[dataLength];
                    int totalRead = 0;

                    while (totalRead < dataLength)
                    {
                        bytesRead = _networkStream.Read(dataBuffer, totalRead, dataLength - totalRead);
                        if (bytesRead == 0)
                        {
                            _logger?.LogWarning("数据读取中断");
                            break;
                        }
                        totalRead += bytesRead;
                    }

                    if (totalRead == dataLength)
                    {
                        // 触发数据接收事件
                        OnDataReceived(dataBuffer, dataLength);
                    }
                    else
                    {
                        _logger?.LogWarning($"数据读取不完整: 预期 {dataLength} 字节，实际读取 {totalRead} 字节");
                    }
                }
                catch (IOException ex)
                {
                    if (_isRunning)
                    {
                        _logger?.LogError(ex, "Socket读取异常，可能连接已断开");
                        Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "数据接收循环异常");
                }
            }
        }

        /// <summary>
        /// 触发数据接收事件
        /// </summary>
        protected virtual void OnDataReceived(byte[] data, int length)
        {
            DataReceived?.Invoke(this, new SocketDataReceivedEventArgs
            {
                Data = data,
                Length = length,
                ReceiveTime = DateTime.Now,
                RemoteEndPoint = _remoteEndPoint
            });
        }

        /// <summary>
        /// 触发连接状态变化事件
        /// </summary>
        protected virtual void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <returns>发送是否成功</returns>
        public async Task<bool> SendDataAsync(byte[] data)
        {
            try
            {
                if (!IsConnected || _networkStream == null)
                {
                    _logger?.LogWarning("Socket未连接，无法发送数据");
                    return false;
                }

                // 先发送数据长度（4字节）
                byte[] lengthBytes = BitConverter.GetBytes(data.Length);
                await _networkStream.WriteAsync(lengthBytes, 0, 4);

                // 再发送实际数据
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();

                _logger?.LogDebug($"成功发送 {data.Length} 字节数据");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发送数据失败");
                return false;
            }
        }

        /// <summary>
        /// 发送字符串数据
        /// </summary>
        /// <param name="message">要发送的字符串</param>
        /// <returns>发送是否成功</returns>
        public async Task<bool> SendStringAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            return await SendDataAsync(data);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns>接收到的数据</returns>
        public async Task<byte[]> ReceiveDataAsync()
        {
            try
            {
                if (!IsConnected || _networkStream == null)
                {
                    _logger?.LogWarning("Socket未连接，无法接收数据");
                    return null;
                }

                // 读取数据长度
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await _networkStream.ReadAsync(lengthBuffer, 0, 4);

                if (bytesRead < 4)
                {
                    _logger?.LogWarning("接收到不完整的数据长度");
                    return null;
                }

                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (dataLength <= 0 || dataLength > 10 * 1024 * 1024)
                {
                    _logger?.LogWarning($"无效的数据长度: {dataLength}");
                    return null;
                }

                // 读取实际数据
                byte[] dataBuffer = new byte[dataLength];
                int totalRead = 0;

                while (totalRead < dataLength)
                {
                    bytesRead = await _networkStream.ReadAsync(dataBuffer, totalRead, dataLength - totalRead);
                    if (bytesRead == 0)
                    {
                        _logger?.LogWarning("数据读取中断");
                        return null;
                    }
                    totalRead += bytesRead;
                }

                return dataBuffer;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "接收数据失败");
                return null;
            }
        }

        /// <summary>
        /// 接收字符串数据
        /// </summary>
        /// <returns>接收到的字符串</returns>
        public async Task<string> ReceiveStringAsync()
        {
            byte[] data = await ReceiveDataAsync();
            return data != null ? Encoding.UTF8.GetString(data) : null;
        }

        /// <summary>
        /// 发送命令并等待响应
        /// </summary>
        /// <param name="command">命令数据</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>响应数据</returns>
        public async Task<byte[]> SendAndReceiveAsync(byte[] command, int timeoutMs = 5000)
        {
            try
            {
                if (!IsConnected)
                {
                    _logger?.LogWarning("Socket未连接，无法执行发送接收操作");
                    return null;
                }

                // 发送命令
                bool sendSuccess = await SendDataAsync(command);
                if (!sendSuccess)
                {
                    _logger?.LogWarning("发送命令失败");
                    return null;
                }

                // 使用带超时的接收
                var receiveTask = ReceiveDataAsync();
                var timeoutTask = Task.Delay(timeoutMs);

                var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logger?.LogWarning("接收响应超时");
                    return null;
                }

                return receiveTask.Result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发送命令并接收响应失败");
                return null;
            }
        }

        /// <summary>
        /// 发送原始字节数据（不带长度头）并读取指定长度的响应
        /// </summary>
        /// <param name="data">要发送的原始字节</param>
        /// <param name="responseLength">期望读取的响应字节数</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>响应数据</returns>
        public async Task<byte[]> SendRawAndReceiveAsync(byte[] data, int responseLength, int timeoutMs = 5000)
        {
            try
            {
                if (!IsConnected || _networkStream == null)
                {
                    _logger?.LogWarning("Socket未连接，无法执行发送接收操作");
                    return null;
                }

                // 直接发送原始字节（不带长度头）
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();

                _logger?.LogDebug($"成功发送 {data.Length} 字节原始数据: {Encoding.UTF8.GetString(data)}");

                // 读取指定长度的响应
                byte[] buffer = new byte[responseLength];
                int totalRead = 0;

                var readTask = Task.Run(async () =>
                {
                    while (totalRead < responseLength)
                    {
                        int bytesRead = await _networkStream.ReadAsync(buffer, totalRead, responseLength - totalRead);
                        if (bytesRead == 0)
                        {
                            _logger?.LogWarning("数据读取中断");
                            return null;
                        }
                        totalRead += bytesRead;
                    }
                    return buffer;
                });

                var timeoutTask = Task.Delay(timeoutMs);
                var completedTask = await Task.WhenAny(readTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logger?.LogWarning($"接收响应超时（{timeoutMs}ms）");
                    return null;
                }

                byte[] result = await readTask;
                if (result != null)
                {
                    string hexStr = BitConverter.ToString(result).Replace("-", "").ToLower();
                    _logger?.LogInformation($"接收到 {result.Length} 字节响应，十六进制: {hexStr}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发送原始数据并接收响应失败");
                return null;
            }
        }

        /// <summary>
        /// 发送原始字节数据（不带长度头）并读取一行响应（以换行符结尾）
        /// </summary>
        /// <param name="data">要发送的原始字节</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>响应字符串（不含换行符）</returns>
        public async Task<string> SendRawAndReceiveLineAsync(byte[] data, int timeoutMs = 10000)
        {
            try
            {
                if (!IsConnected || _networkStream == null)
                {
                    _logger?.LogWarning("Socket未连接，无法执行发送接收操作");
                    return null;
                }

                // 直接发送原始字节（不带长度头）
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();

                _logger?.LogDebug($"成功发送 {data.Length} 字节原始数据: {Encoding.UTF8.GetString(data)}");

                // 逐字节读取直到遇到换行符
                var sb = new StringBuilder();
                var cts = new CancellationTokenSource(timeoutMs);

                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        byte[] oneByte = new byte[1];
                        int bytesRead = await _networkStream.ReadAsync(oneByte, 0, 1, cts.Token);
                        if (bytesRead == 0)
                        {
                            _logger?.LogWarning("连接已关闭");
                            break;
                        }

                        char ch = (char)oneByte[0];
                        if (ch == '\n')
                        {
                            // 遇到换行符，结束读取
                            break;
                        }
                        if (ch != '\r')
                        {
                            sb.Append(ch);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogWarning($"读取响应超时（{timeoutMs}ms）");
                }

                string result = sb.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    _logger?.LogInformation($"接收到行响应: {result}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "发送原始数据并读取行响应失败");
                return null;
            }
        }

        /// <summary>
        /// 将接收到的数据保存为图片
        /// </summary>
        /// <param name="imageData">图片数据（字节数组）</param>
        /// <param name="filePath">保存路径</param>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <param name="imageFormat">图片格式（如 "png", "jpg", "bmp"）</param>
        /// <returns>完整的文件路径</returns>
        public string SaveImage(byte[] imageData, string filePath, string fileName, string imageFormat = "png")
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                    _logger?.LogDebug($"创建目录: {filePath}");
                }

                // 生成文件名（包含时间戳）
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string fullFileName = $"{fileName}_{timestamp}.{imageFormat.ToLower()}";
                string fullPath = Path.Combine(filePath, fullFileName);

                // 保存图片
                File.WriteAllBytes(fullPath, imageData);
                _logger?.LogInformation($"图片已保存: {fullPath}");

                return fullPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"保存图片失败: {fileName}");
                return null;
            }
        }

        /// <summary>
        /// 发送触发命令并接收图片数据
        /// </summary>
        /// <param name="triggerCommand">触发命令</param>
        /// <param name="savePath">图片保存路径</param>
        /// <param name="fileNamePrefix">文件名前缀</param>
        /// <returns>保存的图片文件路径列表</returns>
        public async Task<List<string>> TriggerAndSaveImagesAsync(byte[] triggerCommand, string savePath, string fileNamePrefix)
        {
            var savedFiles = new List<string>();

            try
            {
                _logger?.LogInformation($"发送触发命令，准备接收图片数据");

                // 发送触发命令
                bool sendSuccess = await SendDataAsync(triggerCommand);
                if (!sendSuccess)
                {
                    _logger?.LogWarning("发送触发命令失败");
                    return savedFiles;
                }

                // 等待并接收图片数据（假设会连续接收多张图片）
                // 这里简化处理，只接收一张图片
                byte[] imageData = await ReceiveDataAsync();

                if (imageData != null && imageData.Length > 0)
                {
                    // 尝试检测图片格式
                    string imageFormat = DetectImageFormat(imageData);
                    string savedPath = SaveImage(imageData, savePath, fileNamePrefix, imageFormat);

                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        savedFiles.Add(savedPath);
                    }
                }
                else
                {
                    _logger?.LogWarning("未接收到图片数据");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "触发并保存图片失败");
            }

            return savedFiles;
        }

        /// <summary>
        /// 检测图片格式
        /// </summary>
        /// <param name="imageData">图片数据</param>
        /// <returns>图片格式</returns>
        private string DetectImageFormat(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 4)
                return "png";

            // JPEG: FF D8 FF
            if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                return "jpg";

            // PNG: 89 50 4E 47
            if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
                return "png";

            // BMP: 42 4D
            if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                return "bmp";

            // GIF: 47 49 46 38
            if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46 && imageData[3] == 0x38)
                return "gif";

            return "png";
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