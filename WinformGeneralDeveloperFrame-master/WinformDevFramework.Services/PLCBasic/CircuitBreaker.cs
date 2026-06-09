using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// 断路器状态枚举
    /// </summary>
    public enum CircuitBreakerState
    {
        /// <summary>
        /// 闭合状态 - 正常工作，允许所有请求通过
        /// </summary>
        Closed,
        
        /// <summary>
        /// 打开状态 - 熔断中，拒绝所有请求
        /// </summary>
        Open,
        
        /// <summary>
        /// 半开状态 - 尝试恢复，允许部分请求通过测试
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// 断路器配置
    /// </summary>
    public class CircuitBreakerOptions
    {
        /// <summary>
        /// 连续失败阈值，超过此值触发熔断
        /// </summary>
        public int FailureThreshold { get; set; } = 5;

        /// <summary>
        /// 熔断持续时间（毫秒）
        /// </summary>
        public int OpenDurationMs { get; set; } = 30000; // 30秒

        /// <summary>
        /// 半开状态下允许的请求数
        /// </summary>
        public int HalfOpenSampleCount { get; set; } = 3;

        /// <summary>
        /// 半开状态下成功阈值（达到此成功数则认为服务恢复）
        /// </summary>
        public int HalfOpenSuccessThreshold { get; set; } = 2;
    }

    /// <summary>
    /// 断路器模式
    /// 用于保护系统免受持续故障的影响
    /// </summary>
    public class CircuitBreaker
    {
        private readonly CircuitBreakerOptions _options;
        private readonly ILogger _logger;
        private readonly string _name;
        
        private int _failureCount;
        private int _successCount;
        private int _halfOpenAttempts;
        private DateTime _openTime;
        private CircuitBreakerState _state = CircuitBreakerState.Closed;
        private readonly object _lock = new object();

        public CircuitBreaker(string name, CircuitBreakerOptions options = null, ILogger logger = null)
        {
            _name = name;
            _options = options ?? new CircuitBreakerOptions();
            _logger = logger;
        }

        /// <summary>
        /// 当前断路器状态
        /// </summary>
        public CircuitBreakerState State => GetState();

        /// <summary>
        /// 连续失败次数
        /// </summary>
        public int FailureCount => _failureCount;

        /// <summary>
        /// 记录成功
        /// </summary>
        public void RecordSuccess()
        {
            lock (_lock)
            {
                switch (_state)
                {
                    case CircuitBreakerState.Closed:
                        // 重置失败计数
                        _failureCount = 0;
                        break;
                    
                    case CircuitBreakerState.HalfOpen:
                        _successCount++;
                        _halfOpenAttempts++;
                        
                        // 检查是否达到恢复阈值
                        if (_successCount >= _options.HalfOpenSuccessThreshold)
                        {
                            Reset();
                            _logger?.LogInformation($"断路器[{_name}]从半开状态恢复到闭合状态");
                        }
                        else if (_halfOpenAttempts >= _options.HalfOpenSampleCount)
                        {
                            // 样本用完但未达到成功阈值，重新熔断
                            Trip();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 记录失败
        /// </summary>
        public void RecordFailure()
        {
            lock (_lock)
            {
                switch (_state)
                {
                    case CircuitBreakerState.Closed:
                        _failureCount++;

                        // 检查是否达到熔断阈值
                        if (_failureCount >= _options.FailureThreshold)
                        {
                            Trip();
                            _logger?.LogWarning($"断路器[{_name}]触发熔断，连续失败{_failureCount}次");
                        }
                        break;

                    case CircuitBreakerState.HalfOpen:
                        // 半开状态下失败，立即重新熔断
                        _halfOpenAttempts++;
                        Trip();
                        _logger?.LogWarning($"断路器[{_name}]半开测试失败，重新熔断");
                        break;
                }
            }
        }

        /// <summary>
        /// 检查是否允许请求通过
        /// </summary>
        /// <returns>true=允许通过，false=拒绝（熔断中）</returns>
        public bool IsAllowed()
        {
            lock (_lock)
            {
                // 先检查是否需要从Open状态转换到HalfOpen状态
                if (_state == CircuitBreakerState.Open)
                {
                    var elapsed = (DateTime.Now - _openTime).TotalMilliseconds;
                    if (elapsed >= _options.OpenDurationMs)
                    {
                        // 进入半开状态
                        _state = CircuitBreakerState.HalfOpen;
                        _halfOpenAttempts = 0;
                        _successCount = 0;
                        _logger?.LogInformation($"断路器[{_name}]进入半开状态，尝试恢复");
                    }
                }

                // 根据最终状态判断是否允许请求
                switch (_state)
                {
                    case CircuitBreakerState.Closed:
                        return true;

                    case CircuitBreakerState.Open:
                        return false;

                    case CircuitBreakerState.HalfOpen:
                        // 半开状态下，限制请求数量
                        if (_halfOpenAttempts < _options.HalfOpenSampleCount)
                        {
                            return true;
                        }
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 获取当前状态（考虑时间因素）
        /// </summary>
        private CircuitBreakerState GetState()
        {
            lock (_lock)
            {
                // 检查是否需要从Open状态转换到HalfOpen状态
                if (_state == CircuitBreakerState.Open)
                {
                    var elapsed = (DateTime.Now - _openTime).TotalMilliseconds;
                    if (elapsed >= _options.OpenDurationMs)
                    {
                        // 进入半开状态
                        _state = CircuitBreakerState.HalfOpen;
                        _halfOpenAttempts = 0;
                        _successCount = 0;
                        _logger?.LogInformation($"断路器[{_name}]进入半开状态，尝试恢复");
                    }
                }

                return _state;
            }
        }

        /// <summary>
        /// 触发熔断（打开断路器）
        /// </summary>
        private void Trip()
        {
            _state = CircuitBreakerState.Open;
            _openTime = DateTime.Now;
            _failureCount = 0;
            _halfOpenAttempts = 0;
            _successCount = 0;
        }

        /// <summary>
        /// 重置断路器（恢复正常状态）
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
                _halfOpenAttempts = 0;
                _successCount = 0;
            }
        }

        /// <summary>
        /// 获取熔断剩余时间（毫秒）
        /// </summary>
        public int GetRemainingOpenTimeMs()
        {
            lock (_lock)
            {
                if (_state != CircuitBreakerState.Open)
                    return 0;
                
                var elapsed = (DateTime.Now - _openTime).TotalMilliseconds;
                var remaining = (int)(_options.OpenDurationMs - elapsed);
                return Math.Max(0, remaining);
            }
        }
    }
}
