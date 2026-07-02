using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WinformDevFramework.Services
{
    /// <summary>
    /// 性能日志记录器
    /// 用于记录详细的性能指标，便于性能分析和优化
    /// </summary>
    public static class PerformanceLogger
    {
        /// <summary>
        /// 性能指标缓存
        /// Key = "ServiceName.MethodName", Value = Dictionary(MetricName, MetricValue)
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _metricsCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        /// <summary>
        /// 日志记录器实例
        /// </summary>
        private static ILogger _logger;

        /// <summary>
        /// 设置日志记录器实例
        /// </summary>
        /// <param name="logger">日志记录器</param>
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        /// <param name="serviceName">服务名称（如：TriggerService.CheckAllTriggers）</param>
        /// <param name="metricName">指标名称（如：ProcessingTimeMs）</param>
        /// <param name="value">指标值</param>
        public static void Log(string serviceName, string metricName, object value)
        {
            try
            {
                // 更新缓存
                var serviceMetrics = _metricsCache.GetOrAdd(serviceName, _ => new ConcurrentDictionary<string, object>());
                serviceMetrics[metricName] = value;

                // 如果配置了日志记录器，记录日志
                if (_logger != null)
                {
                    if (value is long longValue)
                    {
                        // 对于数值型指标，根据名称判断日志级别
                        if (metricName.EndsWith("TimeMs") || metricName.EndsWith("DurationMs"))
                        {
                            // 时间类指标：超过阈值才记录
                            if (longValue > 100)
                            {
                               // _logger.LogDebug($"[性能指标] {serviceName}.{metricName} = {longValue}ms");
                            }
                        }
                        else if (metricName.EndsWith("Count") || metricName.EndsWith("Total"))
                        {
                            // 计数类指标：只记录非零值
                            if (longValue > 0)
                            {
                               // _logger.LogDebug($"[性能指标] {serviceName}.{metricName} = {longValue}");
                            }
                        }
                    }
                    else
                    {
                        // 非数值型指标全部记录
                       // _logger.LogDebug($"[性能指标] {serviceName}.{metricName} = {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                // 性能日志记录失败不影响主流程
                Debug.WriteLine($"[PerformanceLogger] 记录性能指标失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 记录性能指标（带时间戳）
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="metricName">指标名称</param>
        /// <param name="value">指标值</param>
        /// <param name="timestamp">时间戳</param>
        public static void Log(string serviceName, string metricName, object value, DateTime timestamp)
        {
            Log($"{serviceName}.{timestamp:HH:mm:ss.fff}", metricName, value);
        }

        /// <summary>
        /// 记录耗时
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="elapsedMilliseconds">耗时（毫秒）</param>
        public static void LogDuration(string serviceName, string operationName, long elapsedMilliseconds)
        {
            Log(serviceName, $"{operationName}.DurationMs", elapsedMilliseconds);

            // 如果耗时超过阈值，记录警告
            if (elapsedMilliseconds > 1000)
            {
                _logger?.LogWarning($"[性能警告] {serviceName}.{operationName} 耗时 {elapsedMilliseconds}ms，超过1秒");
            }
        }

        /// <summary>
        /// 获取性能指标
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="metricName">指标名称</param>
        /// <returns>指标值，如果不存在返回null</returns>
        public static object GetMetric(string serviceName, string metricName)
        {
            if (_metricsCache.TryGetValue(serviceName, out var serviceMetrics))
            {
                if (serviceMetrics.TryGetValue(metricName, out var value))
                {
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有性能指标
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>所有指标字典</returns>
        public static Dictionary<string, object> GetAllMetrics(string serviceName)
        {
            if (_metricsCache.TryGetValue(serviceName, out var serviceMetrics))
            {
                return serviceMetrics.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// 清空指定服务的性能指标
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        public static void ClearMetrics(string serviceName)
        {
            _metricsCache.TryRemove(serviceName, out _);
        }

        /// <summary>
        /// 清空所有性能指标
        /// </summary>
        public static void ClearAllMetrics()
        {
            _metricsCache.Clear();
        }

        /// <summary>
        /// 获取性能报告
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>性能报告字符串</returns>
        public static string GetPerformanceReport(string serviceName)
        {
            var metrics = GetAllMetrics(serviceName);
            if (metrics.Count == 0)
            {
                return $"[{serviceName}] 无性能数据";
            }

            var lines = new List<string> { $"[{serviceName}] 性能报告:" };
            foreach (var metric in metrics.OrderBy(m => m.Key))
            {
                lines.Add($"  - {metric.Key} = {metric.Value}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// 生成性能摘要（用于日志输出）
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>性能摘要字符串</returns>
        public static string GetPerformanceSummary(string serviceName)
        {
            var metrics = GetAllMetrics(serviceName);
            if (metrics.Count == 0)
            {
                return string.Empty;
            }

            var summary = new List<string>();
            foreach (var metric in metrics)
            {
                summary.Add($"{metric.Key}={metric.Value}");
            }

            return string.Join(", ", summary);
        }
    }
}
