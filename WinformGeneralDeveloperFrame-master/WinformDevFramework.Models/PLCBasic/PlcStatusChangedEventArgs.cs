using System;

namespace PLCBasic
{
    /// <summary>
    /// PLC状态变更事件参数
    /// </summary>
    public class PlcStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// PLC名称
        /// </summary>
        public string PlcName { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 状态变更时间
        /// </summary>
        public DateTime ChangeTime { get; set; }
    }
}