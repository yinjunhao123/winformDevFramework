using System;

namespace WinformDevFramework.IServices.PLCBasic
{
    /// <summary>
    /// PLC实时参数采集服务接口
    /// 定时采集TreatmentType为real类型的参数，每5分钟采集一次，保存到PLC_RealTimeParamCollect表
    /// </summary>
    public interface IPlcRealTimeParamCollectService
    {
        /// <summary>
        /// 启动采集服务
        /// </summary>
        void Start();

        /// <summary>
        /// 停止采集服务
        /// </summary>
        void Stop();

        /// <summary>
        /// 服务是否运行中
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 重新加载配置缓存
        /// </summary>
        void ReloadConfig();
    }
}
