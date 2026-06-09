using System;
using System.Collections.Generic;

namespace WinformDevFramework.Models.Common
{
    public enum TaskType
    {
        MainBarcode,
        SubBarcode,
        ParameterCollect
    }

    public enum TaskStatus
    {
        Pending,
        Processing,
        Success,
        Failed,
        Timeout
    }

    public class ValidationTask
    {
        public string TaskId { get; set; }
        public TaskType TaskType { get; set; }
        public string PlcCode { get; set; }
        public string StationCode { get; set; }
        public string Barcode { get; set; }
        public string ModelCode { get; set; }
        public DateTime EnqueueTime { get; set; }
        public TaskStatus Status { get; set; }
        public int RetryCount { get; set; }
        public string ErrorMessage { get; set; }

        public Dictionary<string, object> ExtendedData { get; set; } = new Dictionary<string, object>();

        public ValidationTask()
        {
            TaskId = Guid.NewGuid().ToString();
            EnqueueTime = DateTime.Now;
            Status = TaskStatus.Pending;
            RetryCount = 0;
        }
    }

    public class ValidationResult
    {
        public string TaskId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DateTime ProcessTime { get; set; }
        public Dictionary<string, object> ResultData { get; set; } = new Dictionary<string, object>();

        public ValidationResult()
        {
            ProcessTime = DateTime.Now;
        }
    }

    public class SubBarcodeValidationTask : ValidationTask
    {
        public int SubPartIndex { get; set; }
        public string SubBarcode { get; set; }
        public bool IsMaterialValid { get; set; }
    }

    public class ParameterCollectTask : ValidationTask
    {
        public Dictionary<string, string> CollectedParameters { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, bool> ParameterValidationResults { get; set; } = new Dictionary<string, bool>();
    }
}