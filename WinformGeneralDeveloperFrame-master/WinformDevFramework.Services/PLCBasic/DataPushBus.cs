using PLCBasic;
using System;
using System.Collections.Generic;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC数据推送总线（发布-订阅模式）
    /// 用于在后台服务和UI界面之间传递PLC实时数据
    /// </summary>
    public static class DataPushBus
    {
        /// <summary>
        /// 设备状态更新事件委托
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">状态更新事件参数</param>
        public delegate void StatusUpdatedEventHandler(object sender, PlcStatusUpdateEventArgs e);

        /// <summary>
        /// 设备状态更新事件
        /// UI界面订阅此事件以接收设备状态更新
        /// </summary>
        public static event StatusUpdatedEventHandler StatusUpdated;

        /// <summary>
        /// 发布设备状态更新（单个）
        /// </summary>
        /// <param name="status">状态信息</param>
        public static void PublishStatus(PLC_RealTimeStatus status)
        {
            StatusUpdated?.Invoke(null, new PlcStatusUpdateEventArgs(new List<PLC_RealTimeStatus> { status }));
        }

        /// <summary>
        /// 发布设备状态更新（批量）
        /// </summary>
        /// <param name="statusList">状态信息列表</param>
        public static void PublishStatusCollection(List<PLC_RealTimeStatus> statusList)
        {
            StatusUpdated?.Invoke(null, new PlcStatusUpdateEventArgs(statusList));
        }

        /// <summary>
        /// 设备报警触发事件委托
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">报警事件参数</param>
        public delegate void AlarmTriggeredEventHandler(object sender, PlcAlarmEventArgs e);

        /// <summary>
        /// 设备报警触发事件
        /// UI界面订阅此事件以接收报警信息
        /// </summary>
        public static event AlarmTriggeredEventHandler AlarmTriggered;

        /// <summary>
        /// 发布设备报警信息（单个）
        /// </summary>
        /// <param name="alarm">报警信息</param>
        public static void PublishAlarm(PLC_RealTimeAlarm alarm)
        {
            AlarmTriggered?.Invoke(null, new PlcAlarmEventArgs(alarm));
        }

        /// <summary>
        /// 发布设备报警信息（批量）
        /// </summary>
        /// <param name="alarmList">报警信息列表</param>
        public static void PublishAlarmCollection(List<PLC_RealTimeAlarm> alarmList)
        {
            foreach (var alarm in alarmList)
            {
                AlarmTriggered?.Invoke(null, new PlcAlarmEventArgs(alarm));
            }
        }

        /// <summary>
        /// 条码采集完成事件委托
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">条码事件参数</param>
        public delegate void BarcodeCollectedEventHandler(object sender, PlcBarcodeEventArgs e);

        /// <summary>
        /// 条码采集完成事件
        /// UI界面订阅此事件以接收条码采集结果
        /// </summary>
        public static event BarcodeCollectedEventHandler BarcodeCollected;

        /// <summary>
        /// 发布条码采集结果
        /// </summary>
        /// <param name="collectParam">采集参数</param>
        public static void PublishBarcodeCollection(PLC_MainBarcode mainBarcode)
        {
            BarcodeCollected?.Invoke(null, new PlcBarcodeEventArgs(mainBarcode));
        }

        /// <summary>
        /// 子条码采集完成事件委托
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">子条码事件参数</param>
        public delegate void PartBarcodeCollectedEventHandler(object sender, PlcPartBarcodeEventArgs e);

        /// <summary>
        /// 子条码采集完成事件
        /// UI界面订阅此事件以接收子条码采集结果
        /// </summary>
        public static event PartBarcodeCollectedEventHandler PartBarcodeCollected;

        /// <summary>
        /// 发布子条码采集结果
        /// </summary>
        /// <param name="collectParam">子条码采集参数</param>
        public static void PublishPartBarcodeCollection(PLC_PartCollectParameters collectParam)
        {
            PartBarcodeCollected?.Invoke(null, new PlcPartBarcodeEventArgs(collectParam));
        }

        #region PLC参数采集更新事件

        /// <summary>
        /// PLC参数采集更新事件委托
        /// </summary>
        public delegate void ParamUpdatedEventHandler(object sender, PlcParamUpdatedEventArgs e);

        /// <summary>
        /// PLC参数采集更新事件
        /// UI界面订阅此事件以接收采集到的参数值
        /// </summary>
        public static event ParamUpdatedEventHandler ParamUpdated;

        /// <summary>
        /// 发布PLC参数采集结果
        /// </summary>
        /// <param name="stationCode">工站编码</param>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="paramValues">参数名值对字典</param>
        public static void PublishParamUpdated(string stationCode, string plcCode, Dictionary<string, string> paramValues)
        {
            ParamUpdated?.Invoke(null, new PlcParamUpdatedEventArgs
            {
                StationCode = stationCode,
                PlcCode = plcCode,
                ParamValues = paramValues,
                CollectTime = DateTime.Now
            });
        }

        #endregion

        #region PLC状态变更事件

        /// <summary>
        /// PLC状态变更事件委托
        /// </summary>
        public delegate void PlcStatusChangedEventHandler(object sender, PlcStatusChangedEventArgs e);

        /// <summary>
        /// PLC状态变更事件
        /// UI界面订阅此事件以接收PLC连接状态变更通知
        /// </summary>
        public static event PlcStatusChangedEventHandler PlcStatusChanged;

        /// <summary>
        /// 发布PLC状态变更事件
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <param name="plcName">PLC名称</param>
        /// <param name="isOnline">是否在线</param>
        public static void PublishPlcStatusChanged(string plcCode, string plcName, bool isOnline)
        {
            PlcStatusChanged?.Invoke(null, new PlcStatusChangedEventArgs
            {
                PlcCode = plcCode,
                PlcName = plcName,
                IsOnline = isOnline,
                ChangeTime = DateTime.Now
            });
        }

        #endregion

        #region 参数下发进度事件

        /// <summary>
        /// 参数下发进度更新事件委托
        /// </summary>
        public delegate void DistributionProgressUpdatedEventHandler(object sender, DistributionProgressEventArgs e);

        /// <summary>
        /// 参数下发进度更新事件
        /// UI界面订阅此事件以接收参数下发进度更新
        /// </summary>
        public static event DistributionProgressUpdatedEventHandler DistributionProgressUpdated;

        /// <summary>
        /// 发布参数下发进度更新
        /// </summary>
        /// <param name="progressInfo">进度信息</param>
        public static void PublishDistributionProgress(DistributionProgressInfo progressInfo)
        {
            DistributionProgressUpdated?.Invoke(null, new DistributionProgressEventArgs(progressInfo));
        }

        #endregion

        #region PLC发起的参数下发事件

        /// <summary>
        /// PLC发起的参数下发事件委托
        /// </summary>
        public delegate void PlcParameterDistributionEventHandler(object sender, PlcParameterDistributionEventArgs e);

        /// <summary>
        /// PLC发起的参数下发事件
        /// UI界面订阅此事件以接收PLC发起的参数下发通知
        /// </summary>
        public static event PlcParameterDistributionEventHandler PlcParameterDistribution;

        /// <summary>
        /// 发布PLC发起的参数下发事件
        /// </summary>
        /// <param name="info">参数下发信息</param>
        public static void PublishPlcParameterDistribution(PlcParameterDistributionInfo info)
        {
            PlcParameterDistribution?.Invoke(null, new PlcParameterDistributionEventArgs(info));
        }

        #endregion

        #region 主零件进站校验事件

        /// <summary>
        /// 主零件进站校验事件委托
        /// </summary>
        public delegate void MainPartStationCheckEventHandler(object sender, MainPartStationCheckEventArgs e);

        /// <summary>
        /// 主零件进站校验事件
        /// UI界面订阅此事件以接收主零件进站校验结果
        /// </summary>
        public static event MainPartStationCheckEventHandler MainPartStationCheck;

        /// <summary>
        /// 发布主零件进站校验结果
        /// </summary>
        /// <param name="stationCode">工站编码</param>
        /// <param name="barcode">主零件条码</param>
        /// <param name="isSuccess">是否校验成功</param>
        /// <param name="message">校验消息</param>
        public static void PublishMainPartStationCheck(string stationCode, string barcode, bool isSuccess, string message)
        {
            MainPartStationCheck?.Invoke(null, new MainPartStationCheckEventArgs
            {
                StationCode = stationCode,
                Barcode = barcode,
                IsSuccess = isSuccess,
                Message = message,
                CheckTime = DateTime.Now
            });
        }

        #endregion

        #region 子零件绑定事件

        /// <summary>
        /// 子零件绑定事件委托
        /// </summary>
        public delegate void PartBindingEventHandler(object sender, PartBindingEventArgs e);

        /// <summary>
        /// 子零件绑定事件
        /// UI界面订阅此事件以接收子零件绑定结果
        /// </summary>
        public static event PartBindingEventHandler PartBinding;

        /// <summary>
        /// 发布子零件绑定结果
        /// </summary>
        /// <param name="stationCode">工站编码</param>
        /// <param name="partName">子零件名称</param>
        /// <param name="barcode">子零件条码</param>
        /// <param name="isSuccess">是否绑定成功</param>
        /// <param name="message">绑定消息</param>
        public static void PublishPartBinding(string stationCode, string partName, string barcode, bool isSuccess, string message)
        {
            PartBinding?.Invoke(null, new PartBindingEventArgs
            {
                StationCode = stationCode,
                PartName = partName,
                Barcode = barcode,
                IsSuccess = isSuccess,
                Message = message,
                BindingTime = DateTime.Now
            });
        }

        #endregion

        #region 通用错误/异常事件

        /// <summary>
        /// 通用错误/异常事件委托
        /// </summary>
        public delegate void ErrorMessageEventHandler(object sender, ErrorMessageEventArgs e);

        /// <summary>
        /// 通用错误/异常事件
        /// UI界面订阅此事件以接收错误信息、报警信息、出入站异常等
        /// </summary>
        public static event ErrorMessageEventHandler ErrorMessage;

        /// <summary>
        /// 发布通用错误/异常消息
        /// </summary>
        /// <param name="stationCode">工站编码</param>
        /// <param name="errorType">错误类型</param>
        /// <param name="message">错误消息</param>
        /// <param name="level">消息级别</param>
        public static void PublishErrorMessage(string stationCode, string errorType, string message, string level = "Error")
        {
            ErrorMessage?.Invoke(null, new ErrorMessageEventArgs
            {
                StationCode = stationCode,
                ErrorType = errorType,
                Message = message,
                Level = level,
                OccurTime = DateTime.Now
            });
        }

        #endregion
    }

    #region 参数下发进度相关类

    /// <summary>
    /// 参数下发进度事件参数
    /// </summary>
    public class DistributionProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 进度信息
        /// </summary>
        public DistributionProgressInfo ProgressInfo { get; }

        public DistributionProgressEventArgs(DistributionProgressInfo progressInfo)
        {
            ProgressInfo = progressInfo;
        }
    }

    /// <summary>
    /// 参数下发进度信息
    /// </summary>
    public class DistributionProgressInfo
    {
        /// <summary>
        /// 下发记录ID
        /// </summary>
        public long RecordID { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 进度状态（CreatingRecord/SavingSnapshot/WritingParameters/RollingBack/SavingResults/Completed）
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 当前进度百分比（0-100）
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// 进度消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }

        public string ProductMode { get; set; }
    }

    #endregion

    #region PLC参数下发相关类

    /// <summary>
    /// PLC发起的参数下发事件参数
    /// </summary>
    public class PlcParameterDistributionEventArgs : EventArgs
    {
        /// <summary>
        /// 参数下发信息
        /// </summary>
        public PlcParameterDistributionInfo DistributionInfo { get; }

        public PlcParameterDistributionEventArgs(PlcParameterDistributionInfo info)
        {
            DistributionInfo = info;
        }
    }

    /// <summary>
    /// PLC发起的参数下发信息
    /// </summary>
    public class PlcParameterDistributionInfo
    {
        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 当前Recipe版本
        /// </summary>
        public string CurrentRecipeVer { get; set; }

        /// <summary>
        /// 目标Recipe版本
        /// </summary>
        public string TargetRecipeVer { get; set; }

        /// <summary>
        /// 产品型号
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 处理状态（Success/Failed/NotFound）
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }
    }

    #endregion

    /// <summary>
    /// PLC状态更新事件参数
    /// </summary>
    public class PlcStatusUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 设备状态列表
        /// </summary>
        public List<PLC_RealTimeStatus> StatusList { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="statusList">设备状态列表</param>
        public PlcStatusUpdateEventArgs(List<PLC_RealTimeStatus> statusList)
        {
            StatusList = statusList;
        }
    }

    /// <summary>
    /// PLC报警事件参数
    /// </summary>
    public class PlcAlarmEventArgs : EventArgs
    {
        /// <summary>
        /// 报警信息
        /// </summary>
        public PLC_RealTimeAlarm Alarm { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="alarm">报警信息</param>
        public PlcAlarmEventArgs(PLC_RealTimeAlarm alarm)
        {
            Alarm = alarm;
        }
    }

    /// <summary>
    /// PLC参数采集事件参数
    /// </summary>
    public class PlcParamEventArgs : EventArgs
    {
        /// <summary>
        /// 参数列表
        /// </summary>
        public List<PLC_TriggerParam> ParamList { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="paramList">参数列表</param>
        public PlcParamEventArgs(List<PLC_TriggerParam> paramList)
        {
            ParamList = paramList;
        }
    }

    /// <summary>
    /// PLC条码采集事件参数
    /// </summary>
    public class PlcBarcodeEventArgs : EventArgs
    {
        /// <summary>
        /// 采集参数
        /// </summary>
        public PLC_MainBarcode MainBarcode { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collectParam">采集参数</param>
        public PlcBarcodeEventArgs(PLC_MainBarcode mainBarcode)
        {
            MainBarcode = mainBarcode;
        }
    }
    /// <summary>
    /// PLC子条码采集参数
    /// </summary>
    public class PLC_MainBarcode
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 主条码信息
        /// </summary>
        public string MainBarcode { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime { get; set; }
    }



    /// <summary>
    /// PLC子条码采集事件参数
    /// </summary>
    public class PlcPartBarcodeEventArgs : EventArgs
    {
        /// <summary>
        /// 子条码采集参数
        /// </summary>
        public PLC_PartCollectParameters CollectParam { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collectParam">子条码采集参数</param>
        public PlcPartBarcodeEventArgs(PLC_PartCollectParameters collectParam)
        {
            CollectParam = collectParam;
        }
    }

    /// <summary>
    /// PLC子条码采集参数
    /// </summary>
    public class PLC_PartCollectParameters
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 零件名称
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// 子条码值
        /// </summary>
        public string PartBarcode { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime { get; set; }
    }

    /// <summary>
    /// PLC参数采集报错
    /// </summary>
    public class PlcParametersEventArgs : EventArgs
    {
        /// <summary>
        /// 子条码采集参数
        /// </summary>
        public PlcParameters plcParameters { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collectParam">子条码采集参数</param>
        public PlcParametersEventArgs(PlcParameters plcParameters)
        {
            this.plcParameters = plcParameters;
        }
    }
    public class PlcParameters
    {
        public string PlcCode { get; set; }
        public string StationCode { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 条码过站错误事件参数
    /// </summary>
    public class BarcodePassErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public BarcodePassErrorInfo ErrorInfo { get; }

        public BarcodePassErrorEventArgs(BarcodePassErrorInfo errorInfo)
        {
            ErrorInfo = errorInfo;
        }
    }

    /// <summary>
    /// 条码过站错误信息
    /// </summary>
    public class BarcodePassErrorInfo
    {
        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 条码值
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误时间
        /// </summary>
        public DateTime ErrorTime { get; set; }
    }

    #region 新事件参数类

    /// <summary>
    /// 主零件进站校验事件参数
    /// </summary>
    public class MainPartStationCheckEventArgs : EventArgs
    {
        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 主零件条码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 是否校验成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 校验消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 校验时间
        /// </summary>
        public DateTime CheckTime { get; set; }
    }

    /// <summary>
    /// 子零件绑定事件参数
    /// </summary>
    public class PartBindingEventArgs : EventArgs
    {
        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 子零件名称
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// 子零件条码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 是否绑定成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 绑定消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public DateTime BindingTime { get; set; }
    }

    /// <summary>
    /// 通用错误/异常事件参数
    /// </summary>
    public class ErrorMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 错误类型
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 消息级别（Info, Warning, Error, Critical）
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime OccurTime { get; set; }
    }

    /// <summary>
    /// PLC参数采集更新事件参数
    /// </summary>
    public class PlcParamUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// 工站编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// PLC编码
        /// </summary>
        public string PlcCode { get; set; }

        /// <summary>
        /// 参数名值对字典
        /// </summary>
        public Dictionary<string, string> ParamValues { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime { get; set; }
    }

    #endregion
}
