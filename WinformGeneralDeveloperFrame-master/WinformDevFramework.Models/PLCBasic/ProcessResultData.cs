using SqlSugar;
using System;

namespace PLCBasic
{
    /// <summary>
    /// PLC发往MES的结果数据 (ProcessData)
    /// 对应PLC地址: 6000-7999
    /// </summary>
    [SugarTable("ProcessResultData")]
    public class ProcessResultData
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        #region 基础信息

        /// <summary>
        /// RFID条码
        /// </summary>
        public string RfidBarcode { get; set; }

        /// <summary>
        /// 主条码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 生产线代号 String[18]
        /// </summary>
        public string LineID { get; set; }

        /// <summary>
        /// 工位代号 String[18]
        /// </summary>
        public string StationID { get; set; }

        /// <summary>
        /// 托盘编号 Int
        /// </summary>
        public int PalletNo { get; set; }

        /// <summary>
        /// 托盘类型 Byte (1-LOOP1托盘, 2-LOOP2托盘等)
        /// </summary>
        public int PalletType { get; set; }

        /// <summary>
        /// 托盘状态 int
        /// </summary>
        public int PalletStatus { get; set; }

        #endregion

        #region 节拍时间

        /// <summary>
        /// 总循环时间(秒) Real
        /// 当前工件开始加工后由PLC累加加工时长，工件离开后停止累加计时
        /// </summary>
        public double CT_Total { get; set; }


        /// <summary>
        /// 工步分项节拍时间1(秒)
        /// </summary>
        public double CT_Sub1 { get; set; }

        /// <summary>
        /// 工步分项节拍时间2(秒)
        /// </summary>
        public double CT_Sub2 { get; set; }

        /// <summary>
        /// 工步分项节拍时间3(秒)
        /// </summary>
        public double CT_Sub3 { get; set; }
        /// <summary>
        /// 工步分项节拍时间4(秒)
        /// </summary>
        public double CT_Sub4 { get; set; }

        /// <summary>
        /// 工步分项节拍时间5(秒)
        /// </summary>
        public double CT_Sub5 { get; set; } 

        /// <summary>
        /// 工步分项节拍时间6(秒)
        /// </summary>
        public double CT_Sub6 { get; set; }


        /// <summary>
        /// 工步分项节拍时间7(秒)
        /// </summary>
        public double CT_Sub7 { get; set; }

        #endregion

        #region 质量终值数据 (P-Data Array[1..64])


        // P-Data[1]
        public int SubResult1 { get; set; }

        public double SubUSL1 { get; set; }

        public double SubResultValue1 { get; set; }

        public double SubLSL1 {  get; set; }


        // P-Data[2]
        public int SubResult2 { get; set; }

        public double SubUSL2 { get; set; }

        public double SubResultValue2 { get; set; }
        public double SubLSL2 { get; set; }

        // P-Data[3]
        public byte SubResult3 { get; set; }

        public double SubUSL3 { get; set; }

        public double SubResultValue3 { get; set; }

        public double SubLSL3 { get;set ; }


        // P-Data[4]
        public byte SubResult4 { get; set; }

        public double SubUSL4 { get; set; }

        public double SubResultValue4 { get; set; }

        public double SubLSL4 { get; set; }

        // P-Data[5]
        public byte SubResult5 { get; set; }
        public double SubUSL5 { get; set; }
        public double SubResultValue5 { get; set; }
        public double SubLSL5 { get; set; }

        // P-Data[6]
        public double SubResult6 { get; set; }
        public double SubUSL6 { get; set; }
        public double SubResultValue6 { get; set; }
        public double SubLSL6 { get; set; }

        // P-Data[7]
        public byte SubResult7 { get; set; }
        public double SubUSL7 { get; set; }
        public double SubResultValue7 { get; set; }
        public double SubLSL7 { get; set; }

        // P-Data[8]
        public byte SubResult8 { get; set; }
        public double SubUSL8 { get; set; }
        public double SubResultValue8 { get; set; }
        public double SubLSL8 { get; set; }

        // P-Data[9]
        public byte SubResult9 { get; set; }
        public double SubUSL9 { get; set; }
        public double SubResultValue9 { get; set; }
        public double SubLSL9 { get; set; }

        // P-Data[10]
        public byte SubResult10 { get; set; }
        public double SubUSL10 { get; set; }
        public double SubResultValue10 { get; set; }
        public double SubLSL10 { get; set; }

        #endregion
        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime ColletTime { get; set; }
    }
}
