using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数上下限配置表
    /// </summary>
    [SugarTable("PLC_ParameterLimit")]
    public partial class PLC_ParameterLimit
    {
        public PLC_ParameterLimit()
        {

        }
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }
        /// <summary>
        /// 工站Code
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 型号Code
        /// </summary>
        public string ProductModelCode { get; set; }
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; }
        /// <summary>
        /// 上限
        /// </summary>
        public decimal? UpperLimit { get; set; }
        /// <summary>
        /// 下限
        /// </summary>
        public decimal? LowerLimit { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

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