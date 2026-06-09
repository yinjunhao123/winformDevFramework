using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数下发主表
    /// </summary>
    [SugarTable("PLC_ParameterDistribution")]
    public partial class PLC_ParameterDistribution
    {
        public PLC_ParameterDistribution()
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
        /// 工艺路线编码（Recipe可独立指定，为空时使用型号默认工艺路线）
        /// </summary>
        public string RoutingCode { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
