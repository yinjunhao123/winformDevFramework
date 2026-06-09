using PLCBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Models.Basic;

namespace WinformDevFramework.IServices.PLCBasic
{
    public interface IPlcHeartbeatService
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        int IntervalMs { get; set; }
        
        /// <summary>
        /// 检查指定PLC是否已连接
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>true表示已连接，false表示未连接</returns>
        bool IsPlcConnected(string plcCode);
        
        /// <summary>
        /// 当PLC连接状态发生变化时触发
        /// </summary>
        event EventHandler<PlcStatusChangedEventArgs> PlcStatusChanged;

        /// <summary>
        /// 发布当前所有PLC的状态
        /// UI页面可以调用此方法获取当前所有PLC的连接状态
        /// 通过事件订阅方式返回，不直接返回结果
        /// </summary>
        void PublishCurrentPlcStatus();
    }
}
