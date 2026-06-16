using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace PLCBasic
{
    /// <summary>
    /// 参数下发历史主表（保存下发前的设备参数快照）
    /// </summary>
    [SugarTable("PLC_ParameterDistributionHistory")]
    public partial class PLC_ParameterDistributionHistory
    {
        public PLC_ParameterDistributionHistory()
        {
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ID { get; set; }

        /// <summary>
        /// 关联下发记录ID
        /// </summary>
        public long RecordID { get; set; }

        /// <summary>
        /// 产品型号编码
        /// </summary>
        public string ProductModelCode { get; set; }
        /// <summary>
        /// 参数版本号（即RecipeCode）
        /// </summary>
        public string RecipeCode { get; set; }

        /// <summary>
        /// 快照时间（即下发前保存的时间）
        /// </summary>
        public DateTime? SnapshotTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
    }
}
