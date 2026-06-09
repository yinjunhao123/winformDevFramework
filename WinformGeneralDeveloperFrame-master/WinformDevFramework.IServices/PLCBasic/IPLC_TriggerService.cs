using PLCBasic;
using System;
using System.Collections.Generic;
using WinformDevFramework.Models.PLCBasic;

namespace WinformDevFramework.IServices.PLCBasic
{
    /// <summary>
    /// PLC事件触发服务接口
    /// 负责监听PLC触发位，实现上升沿检测，执行对应的业务事件
    /// </summary>
    public interface IPLC_TriggerService
    {
        /// <summary>
        /// 启动触发服务
        /// </summary>
        void Start();

        /// <summary>
        /// 停止触发服务
        /// </summary>
        void Stop();

        /// <summary>
        /// 服务是否运行中
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 轮询间隔（毫秒）
        /// </summary>
        int PollingIntervalMs { get; set; }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        void ReloadConfig();

        /// <summary>
        /// 获取当前所有事件配置
        /// </summary>
        /// <returns></returns>
        List<PLC_Event_Trigger_Master> GetAllEventConfigs();

        /// <summary>
        /// 事件触发事件
        /// 当检测到上升沿触发时触发
        /// </summary>
        event EventHandler<PlcEventTriggeredEventArgs> EventTriggered;
    }

    /// <summary>
    /// PLC事件触发事件参数
    /// </summary>
    public class PlcEventTriggeredEventArgs : EventArgs
    {
        /// <summary>
        /// 事件ID
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// 触发地址
        /// </summary>
        public string TriggerAddress { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime TriggerTime { get; set; }

        /// <summary>
        /// 是否上升沿触发（true=上升沿，false=下降沿）
        /// </summary>
        public bool IsRisingEdge { get; set; }
    }
}