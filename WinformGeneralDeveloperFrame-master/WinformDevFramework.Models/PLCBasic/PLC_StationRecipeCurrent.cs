using SqlSugar;
using System;

namespace PLCBasic
{
    /// <summary>
    /// 工站当前生产Recipe表
    /// 存储每个工站当前正在生产的Recipe、型号和工艺路线信息
    /// 工站参数下发成功后同步更新此表
    /// </summary>
    [SugarTable("PLC_StationRecipeCurrent")]
    public class PLC_StationRecipeCurrent
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 当前Recipe编码
        /// </summary>
        public string RecipeCode { get; set; }

        /// <summary>
        /// 当前产品型号编码
        /// </summary>
        public string ProductModelCode { get; set; }

        /// <summary>
        /// 当前工艺路线编码
        /// </summary>
        public string RoutingCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
    }
}