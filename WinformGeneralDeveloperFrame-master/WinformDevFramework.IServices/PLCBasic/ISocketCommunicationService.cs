using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinformDevFramework.IServices.PLCBasic
{
    /// <summary>
    /// Socket通讯服务接口
    /// 用于与外部设备进行TCP Socket通信
    /// </summary>
    public interface ISocketCommunicationService
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接到Socket服务器
        /// </summary>
        /// <param name="ipAddress">服务器IP地址</param>
        /// <param name="port">服务器端口</param>
        /// <returns>连接是否成功</returns>
        Task<bool> ConnectAsync(string ipAddress, int port);

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <returns>发送是否成功</returns>
        Task<bool> SendDataAsync(byte[] data);

        /// <summary>
        /// 发送字符串数据
        /// </summary>
        /// <param name="message">要发送的字符串</param>
        /// <returns>发送是否成功</returns>
        Task<bool> SendStringAsync(string message);

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns>接收到的数据</returns>
        Task<byte[]> ReceiveDataAsync();

        /// <summary>
        /// 接收字符串数据
        /// </summary>
        /// <returns>接收到的字符串</returns>
        Task<string> ReceiveStringAsync();

        /// <summary>
        /// 发送命令并等待响应
        /// </summary>
        /// <param name="command">命令数据</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>响应数据</returns>
        Task<byte[]> SendAndReceiveAsync(byte[] command, int timeoutMs = 5000);

        /// <summary>
        /// 当接收到数据时触发
        /// </summary>
        event EventHandler<SocketDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// 当连接状态变化时触发
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// 将接收到的数据保存为图片
        /// </summary>
        /// <param name="imageData">图片数据（字节数组）</param>
        /// <param name="filePath">保存路径</param>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <param name="imageFormat">图片格式（如 "png", "jpg", "bmp"）</param>
        /// <returns>完整的文件路径</returns>
        string SaveImage(byte[] imageData, string filePath, string fileName, string imageFormat = "png");

        /// <summary>
        /// 发送触发命令并接收图片数据
        /// </summary>
        /// <param name="triggerCommand">触发命令</param>
        /// <param name="savePath">图片保存路径</param>
        /// <param name="fileNamePrefix">文件名前缀</param>
        /// <returns>保存的图片文件路径列表</returns>
        Task<List<string>> TriggerAndSaveImagesAsync(byte[] triggerCommand, string savePath, string fileNamePrefix);
    }

    /// <summary>
    /// Socket数据接收事件参数
    /// </summary>
    public class SocketDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 接收到的数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceiveTime { get; set; }

        /// <summary>
        /// 远程端点信息
        /// </summary>
        public string RemoteEndPoint { get; set; }
    }
}