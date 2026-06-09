using SqlSugar;
using System;

namespace WinformDevFramework.Models.PLCBasic
{
    [SugarTable("PLC_Event_Trigger_Master")]
    public class PLC_Event_Trigger_Master
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int EventId { get; set; }

        public string PlcCode { get; set; }

        public string StationCode { get; set; }

        public string TriggerAddress { get; set; }

        public string TriggerAddressType { get; set; }
        /// <summary>
        /// 事件类型：ParameterCheckIn,ParameterCheckDownIn,BatchCheckIn,BatchCheckOut,MainCheckIn,SubCheckIn,MainCheckOut,MainCheckDownOut,Processing,ProcessDowning
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// 触发类型：Rising(上升沿，默认), Falling(下降沿)
        /// </summary>
        public string TriggerEdgeType { get; set; }

        public string Description { get; set; }

        public string CreateUser { get; set; }

        public DateTime? CreateTime { get; set; }

        public string UpdateUser { get; set; }

        public DateTime? UpdateTime { get; set; }

    }
}