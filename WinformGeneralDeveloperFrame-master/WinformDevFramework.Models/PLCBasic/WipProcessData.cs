using SqlSugar;

namespace PLCBasic
{
    [SugarTable("WipProcessData")]
    public class WipProcessData
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 程序号
        /// </summary>
        public string Receipe { get; set; }

        /// <summary>
        /// 工站Code
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        public string BarCodeType { get; set; }
        /// <summary>
        /// 曲线类型   跳动/拧紧/位压 曲线
        /// </summary>
        public string CurveTypes { get; set; }
        /// <summary>
        /// 保存路径
        /// </summary>
        public string SavePath { get; set; }
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