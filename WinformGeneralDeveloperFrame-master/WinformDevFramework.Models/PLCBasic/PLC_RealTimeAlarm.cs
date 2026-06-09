using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCBasic
{
    [SugarTable("PLC_RealTimeAlarm")]
    public partial class PLC_RealTimeAlarm
    {
        public PLC_RealTimeAlarm()
        {

        }
        /// <summary>
        /// 报警主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long AlarmRecordID { get; set; }
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }
        /// <summary>
        /// 地址Code
        /// </summary>
        public string AddressCode { get; set; }
        /// <summary>
        /// 关联设备编码
        /// </summary>
        public string EquipmentCode { get; set; }
        /// <summary>
        /// 关联工站信息
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 报警代码/位号 (如: 1001, 或 Bit0)
        /// </summary>
        public string AlarmCode { get; set; }
        /// <summary>
        /// 报警名称
        /// </summary>
        public string AlarmName { get; set; }
        /// <summary>
        /// 报警等级 (如: 1-一般, 2-重要, 3-严重)
        /// </summary>
        public string AlarmLevel { get; set; }
        /// <summary>
        /// 报警描述/详情 (如: 电机过载，当前电流150A，超过设定值100A)
        /// </summary>
        public string AlarmDesc { get; set; }
        /// <summary>
        /// 处理建议/措施 (如: 停机检查电机，联系维护人员)
        /// </summary>
        public string Suggestion { get; set; }
        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime TriggerTime { get; set; }
        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool IsHandled { get; set; } 
        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? HandleTime { get; set; }
        /// <summary>
        /// 处理人员
        /// </summary>
        public string HandleUser { get; set; }
    }
}
