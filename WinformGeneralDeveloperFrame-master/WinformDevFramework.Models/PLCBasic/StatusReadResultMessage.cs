using System;
using System.Collections.Generic;

namespace PLCBasic
{
    /// <summary>
    /// 状态读取结果消息（用于消息队列传递）
    /// </summary>
    public class StatusReadResultMessage
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 读取的状态映射列表（包含地址信息）
        /// </summary>
        public List<StatusMappingItem> Mappings { get; set; } = new List<StatusMappingItem>();

        /// <summary>
        /// 读取的状态值数组（与Mappings索引一一对应）
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>
        /// 读取时间
        /// </summary>
        public DateTime ReadTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 状态映射项
    /// </summary>
    public class StatusMappingItem
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 地址编码
        /// </summary>
        public string AddressCode { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquipmentCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }
    }

    /// <summary>
    /// 报警读取结果消息（用于消息队列传递）
    /// </summary>
    public class AlarmReadResultMessage
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 触发的报警列表（值为true的报警）
        /// </summary>
        public List<AlarmMappingItem> TriggeredAlarms { get; set; } = new List<AlarmMappingItem>();

        /// <summary>
        /// 读取时间
        /// </summary>
        public DateTime ReadTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 报警映射项
    /// </summary>
    public class AlarmMappingItem
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 地址编码
        /// </summary>
        public string AddressCode { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }
    }
}
