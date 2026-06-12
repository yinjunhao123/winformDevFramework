using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("WipMaterialInfo")]
    public class WipMaterialInfo
    {
        /// <summary>
        /// 物料信息ID
        /// </summary>
        public string MaterialInfoId { get; set; }

        /// <summary>
        /// RFID编码
        /// </summary>
        public string RfidCode { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料类型 SubPart/Batch
        /// </summary>
        public string MaterialType { get; set; }
        /// <summary>
        /// 工站编码
        /// </summary>

        public string StationCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUser { get; set; }

    }
}