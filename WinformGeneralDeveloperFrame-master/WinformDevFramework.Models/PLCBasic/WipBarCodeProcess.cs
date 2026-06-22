using SqlSugar;
using System;

namespace PLCBasic
{
    [SugarTable("WipBarCodeProcess")]
    public class WipBarCodeProcess
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 主零件条码
        /// </summary>
        public string BarCode { get; set; }


        /// <summary>
        /// RFID条码
        /// </summary>
        public string RfidCode { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string ProductMode { get; set; }

        /// <summary>
        /// 程序编码
        /// </summary>
        public string RecipeCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 第一子零件
        /// </summary>
        public string FirstSubBarCode { get; set; }

        /// <summary>
        /// 第二子零件
        /// </summary>
        public string SecondSubBarCode { get; set; }

        /// <summary>
        /// 第三子零件
        /// </summary>
        public string ThirdSubBarCode { get; set; }

        /// <summary>
        /// 加工结果
        /// </summary>
        public int PartResult { get; set; }

        /// <summary>
        /// 测试模式
        /// </summary>
        public int TestCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUser { get; set; }
    }
}