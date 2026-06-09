using System;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 当前参数下发状态表
    /// 用于记录当前正在进行的参数下发任务，支持页面重新打开时恢复状态
    /// </summary>
    [SugarTable("PLC_ParameterDistributionCurrent")]
    public partial class PLC_ParameterDistributionCurrent
    {
        public PLC_ParameterDistributionCurrent()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }
        /// <summary>
        /// 工艺参数版本号
        /// </summary>

        public string RecipeCode { get; set; }

        /// <summary>
        /// 产品型号编码
        /// </summary>
        public string ProductModelCode { get; set; }
        /// <summary>
        /// 工艺路线编码
        /// </summary>
        public string RouteCode { get; set; }

        /// <summary>
        /// 当前参数下发状态
        /// 状态值说明：
        /// 0 - 等待下发（初始状态，创建记录时的默认值）
        /// 1 - 下发中（正在逐个工站下发参数）
        /// 2 - 下发完成（所有工站下发成功，正常完成）
        /// 3 - 失败（下发过程中出现错误）
        /// 4 - 暂停（手动暂停下发任务）
        /// 5 - 结束（切换另一个型号时强制结束当前任务，非正常完成）
        /// 对应枚举：DistributionCurrentStatus
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间（完成时更新）
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 当前下发到第几个工站（从0开始）
        /// </summary>
        public int CurrentStationIndex { get; set; }

        /// <summary>
        /// 总工站数
        /// </summary>
        public int TotalStations { get; set; }

        /// <summary>
        /// 下发人
        /// </summary>
        public string DistributionUser { get; set; }

        /// <summary>
        /// 下发配置ID
        /// </summary>
        public long DistributionID { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
        /// 当前下发状态枚举
        /// </summary>
        public enum DistributionCurrentStatus
        {
            /// <summary>
            /// 等待下发（初始状态）
            /// </summary>
            Pending = 0,
            
            /// <summary>
            /// 下发中（正在逐个工站下发）
            /// </summary>
            Distributing = 1,

            /// <summary>
            /// 下发完成（所有工站下发成功，正常完成）
            /// </summary>
            Completed = 2,
            
            /// <summary>
            /// 失败（下发过程中出现错误）
            /// </summary>
            Failed = 3,
            
            /// <summary>
            /// 暂停（手动暂停下发）
            /// </summary>
            Paused = 4,

            /// <summary>
            /// 结束（切换另一个型号时强制结束，非正常完成）
            /// </summary>
            Ended = 5
        }

        /// <summary>
        /// 状态扩展方法
        /// </summary>
        public static class DistributionCurrentStatusExtensions
        {
            public static string GetDescription(this DistributionCurrentStatus status)
            {
                switch (status)
                {
                    case DistributionCurrentStatus.Pending:
                        return "等待下发";
                    case DistributionCurrentStatus.Distributing:
                        return "下发中";
                    case DistributionCurrentStatus.Completed:
                        return "已完成";
                    case DistributionCurrentStatus.Failed:
                        return "失败";
                    case DistributionCurrentStatus.Paused:
                        return "暂停";
                    case DistributionCurrentStatus.Ended:
                        return "已结束";
                    default:
                        return "未知";
                }
            }
        }
}