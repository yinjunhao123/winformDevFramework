using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// PLC断路器管理器
    /// 为每个PLC实例维护独立的断路器状态
    /// </summary>
    public class PlcCircuitBreakerManager
    {
        /// <summary>
        /// 断路器字典（按PLC编码分组）
        /// </summary>
        private readonly ConcurrentDictionary<string, CircuitBreaker> _circuitBreakers = new ConcurrentDictionary<string, CircuitBreaker>();

        /// <summary>
        /// 默认断路器配置
        /// </summary>
        private readonly CircuitBreakerOptions _defaultOptions;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger _logger;

        public PlcCircuitBreakerManager(CircuitBreakerOptions defaultOptions = null, ILogger logger = null)
        {
            _defaultOptions = defaultOptions ?? new CircuitBreakerOptions
            {
                FailureThreshold = 5,
                OpenDurationMs = 30000,
                HalfOpenSampleCount = 3,
                HalfOpenSuccessThreshold = 2
            };
            _logger = logger;
        }

        /// <summary>
        /// 获取或创建PLC的断路器
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>断路器实例</returns>
        public CircuitBreaker GetOrCreateBreaker(string plcCode)
        {
            return _circuitBreakers.GetOrAdd(plcCode, code => 
                new CircuitBreaker($"PLC_{code}", _defaultOptions, _logger));
        }

        /// <summary>
        /// 检查PLC是否允许请求
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>true=允许请求，false=熔断中</returns>
        public bool IsRequestAllowed(string plcCode)
        {
            var breaker = GetOrCreateBreaker(plcCode);
            return breaker.IsAllowed();
        }

        /// <summary>
        /// 记录PLC操作成功
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        public void RecordSuccess(string plcCode)
        {
            var breaker = GetOrCreateBreaker(plcCode);
            breaker.RecordSuccess();
        }

        /// <summary>
        /// 记录PLC操作失败
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        public void RecordFailure(string plcCode)
        {
            var breaker = GetOrCreateBreaker(plcCode);
            breaker.RecordFailure();
        }

        /// <summary>
        /// 获取PLC断路器状态
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>断路器状态</returns>
        public CircuitBreakerState GetState(string plcCode)
        {
            var breaker = GetOrCreateBreaker(plcCode);
            return breaker.State;
        }

        /// <summary>
        /// 获取PLC断路器状态描述
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        /// <returns>状态描述文本</returns>
        public string GetStateDescription(string plcCode)
        {
            var breaker = GetOrCreateBreaker(plcCode);
            var state = breaker.State;
            
            switch (state)
            {
                case CircuitBreakerState.Closed:
                    return "正常运行";
                case CircuitBreakerState.Open:
                    var remaining = breaker.GetRemainingOpenTimeMs();
                    return $"熔断中，剩余 {remaining}ms";
                case CircuitBreakerState.HalfOpen:
                    return "尝试恢复中";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 重置指定PLC的断路器
        /// </summary>
        /// <param name="plcCode">PLC编码</param>
        public void ResetBreaker(string plcCode)
        {
            if (_circuitBreakers.TryGetValue(plcCode, out var breaker))
            {
                breaker.Reset();
                _logger?.LogInformation($"已重置PLC[{plcCode}]的断路器");
            }
        }

        /// <summary>
        /// 重置所有断路器
        /// </summary>
        public void ResetAllBreakers()
        {
            foreach (var breaker in _circuitBreakers.Values)
            {
                breaker.Reset();
            }
            _logger?.LogInformation("已重置所有PLC断路器");
        }

        /// <summary>
        /// 获取熔断中PLC的数量
        /// </summary>
        public int GetOpenCircuitCount()
        {
            int count = 0;
            foreach (var breaker in _circuitBreakers.Values)
            {
                if (breaker.State == CircuitBreakerState.Open)
                    count++;
            }
            return count;
        }
    }
}
