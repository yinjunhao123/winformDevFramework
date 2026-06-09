using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using WinformDevFramework.Models.Common;

namespace WinformDevFramework.Services.PLCBasic
{
    /// <summary>
    /// RabbitMQ服务
    /// 提供消息队列的发送和消费功能，支持消息持久化、死信队列和重试机制
    /// 采用延迟连接模式，仅在实际使用时才建立连接
    /// </summary>
    public class RabbitMqService : IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _virtualHost;
        private readonly ILogger<RabbitMqService> _logger;
        private bool _disposed = false;
        private bool _isConnected = false;
        private readonly object _lockObj = new object();

        // 消息TTL配置（毫秒）
        private const int MessageTtlMs = 300000; // 5分钟

        // 队列名称常量
        public const string MainBarcodeQueue = "mainbarcode_queue";
        public const string SubBarcodeQueue = "subbarcode_queue";
        public const string ParameterQueue = "parameter_queue";
        public const string DeadLetterExchange = "dlx_exchange";
        public const string DeadLetterQueue = "dlx_queue";

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public bool IsConnected => _isConnected && _connection != null && _connection.IsOpen;

        public RabbitMqService(ILogger<RabbitMqService> logger, string hostName = "localhost", 
            int port = 5672, string userName = "guest", string password = "guest", string virtualHost = "/")
        {
            _logger = logger;
            _hostName = hostName;
            _port = port;
            _userName = userName;
            _password = password;
            _virtualHost = virtualHost;

            // 延迟连接：不在构造函数中建立连接
            _logger?.LogInformation("RabbitMQ服务已初始化（延迟连接模式）");
        }

        /// <summary>
        /// 建立连接（延迟连接，按需调用）
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool Connect()
        {
            // 双重检查锁定，确保线程安全
            if (_isConnected && _connection != null && _connection.IsOpen)
            {
                return true;
            }

            lock (_lockObj)
            {
                if (_isConnected && _connection != null && _connection.IsOpen)
                {
                    return true;
                }

                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _hostName,
                        Port = _port,
                        UserName = _userName,
                        Password = _password,
                        VirtualHost = _virtualHost,
                        RequestedHeartbeat = TimeSpan.FromSeconds(60),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                        DispatchConsumersAsync = true,
                        ClientProvidedName = "WinformDevFramework-RabbitMQ-Client"
                    };

                    _connection = factory.CreateConnection();
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _channel = _connection.CreateModel();
                    _channel.ConfirmSelect();
                    _channel.BasicAcks += OnBasicAck;
                    _channel.BasicNacks += OnBasicNack;

                    DeclareQueues();

                    _isConnected = true;
                    _logger?.LogInformation($"RabbitMQ连接成功: {_hostName}:{_port}/{_virtualHost}");
                    return true;
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger?.LogError(ex, $"无法连接到RabbitMQ服务器: {_hostName}:{_port}");
                    _isConnected = false;
                    return false;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "RabbitMQ连接失败");
                    _isConnected = false;
                    return false;
                }
            }
        }

        /// <summary>
        /// 尝试重新连接
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool Reconnect()
        {
            lock (_lockObj)
            {
                // 先释放现有连接
                try
                {
                    _channel?.Close();
                    _channel?.Dispose();
                    _connection?.Close();
                    _connection?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "释放旧连接时发生异常");
                }

                _channel = null;
                _connection = null;
                _isConnected = false;

                // 尝试重新连接
                return Connect();
            }
        }

        #region 连接事件处理

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger?.LogWarning($"RabbitMQ连接断开: {e.ReplyText}");
            _isConnected = false;
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "RabbitMQ回调异常");
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger?.LogWarning($"RabbitMQ连接被阻塞: {e.Reason}");
        }

        private void OnBasicAck(object sender, BasicAckEventArgs e)
        {
            _logger?.LogDebug($"消息已确认: DeliveryTag={e.DeliveryTag}, Multiple={e.Multiple}");
        }

        private void OnBasicNack(object sender, BasicNackEventArgs e)
        {
            _logger?.LogWarning($"消息未被确认: DeliveryTag={e.DeliveryTag}, Multiple={e.Multiple}, Requeue={e.Requeue}");
        }

        #endregion

        /// <summary>
        /// 声明队列（支持持久化和死信队列）
        /// </summary>
        private void DeclareQueues()
        {
            try
            {
                // 死信交换器配置
                _channel.ExchangeDeclare(exchange: DeadLetterExchange,
                                       type: ExchangeType.Direct,
                                       durable: true,
                                       autoDelete: false,
                                       arguments: null);

                // 死信队列配置
                _channel.QueueDeclare(queue: DeadLetterQueue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                _channel.QueueBind(queue: DeadLetterQueue,
                                  exchange: DeadLetterExchange,
                                  routingKey: DeadLetterQueue);

                // 队列通用参数配置
                var queueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", DeadLetterExchange},
                    {"x-dead-letter-routing-key", DeadLetterQueue},
                    {"x-message-ttl", MessageTtlMs},
                    {"x-max-priority", 10},
                    {"x-queue-mode", "lazy"}
                };

                // 声明主条码队列
                _channel.QueueDeclare(queue: MainBarcodeQueue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: queueArgs);

                // 声明子条码队列
                _channel.QueueDeclare(queue: SubBarcodeQueue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: queueArgs);

                // 声明参数队列
                _channel.QueueDeclare(queue: ParameterQueue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: queueArgs);

                _logger?.LogInformation("RabbitMQ队列声明完成");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "声明队列失败");
                throw;
            }
        }

        /// <summary>
        /// 消费队列消息（带重试机制，泛型版本）
        /// </summary>
        public void ConsumeMessagesWithRetry<T>(string queueName, Func<T, Task<bool>> handler,
            CancellationToken cancellationToken, int maxRetries = 5, ushort prefetchCount = 1)
        {
            // 确保连接已建立
            if (!Connect())
            {
                _logger?.LogError($"无法启动消费者，RabbitMQ未连接: {queueName}");
                return;
            }

            try
            {
                _channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogWarning($"消费被取消: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var message = JsonConvert.DeserializeObject<T>(json);

                        if (message != null)
                        {
                            _logger?.LogDebug($"收到消息: Queue={queueName}, MessageId={ea.BasicProperties?.MessageId}");

                            bool success = await handler(message);

                            if (success)
                            {
                                _channel.BasicAck(ea.DeliveryTag, false);
                                _logger?.LogDebug($"消息处理成功: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                            }
                            else if (maxRetries <= 0)
                            {
                                _logger?.LogWarning($"消息处理失败，已拒绝: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                                _channel.BasicNack(ea.DeliveryTag, false, false);
                            }
                            else
                            {
                                _logger?.LogWarning($"消息处理失败，重新入队: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                                _channel.BasicNack(ea.DeliveryTag, false, true);
                            }
                        }
                        else
                        {
                            _logger?.LogWarning($"消息反序列化失败: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogError(ex, $"消息反序列化异常: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"处理消息失败: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                _channel.BasicConsume(queue: queueName,
                                     autoAck: false,
                                     consumer: consumer);

                _logger?.LogInformation($"开始消费队列(带重试泛型): {queueName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"启动消费失败: Queue={queueName}");
            }
        }

        /// <summary>
        /// 发送消息到队列（支持持久化）
        /// </summary>
        public async Task<bool> SendMessageAsync<T>(string queueName, T message, byte priority = 5)
        {
            // 确保连接已建立
            if (!Connect())
            {
                _logger?.LogError($"无法发送消息，RabbitMQ未连接: {queueName}");
                return false;
            }

            try
            {
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Priority = priority;
                properties.ContentType = "application/json";
                properties.ContentEncoding = "UTF-8";

                _channel.TxSelect();
                try
                {
                    _channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: properties,
                                         body: body);
                    _channel.TxCommit();
                }
                catch
                {
                    _channel.TxRollback();
                    throw;
                }

                _logger?.LogDebug($"消息发送成功: Queue={queueName}, MessageId={properties.MessageId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"发送消息失败: Queue={queueName}");
                return false;
            }
        }

        /// <summary>
        /// 消费队列消息
        /// </summary>
        public void ConsumeMessages(string queueName, Action<ValidationTask> handler,
            CancellationToken cancellationToken, ushort prefetchCount = 1)
        {
            // 确保连接已建立
            if (!Connect())
            {
                _logger?.LogError($"无法启动消费者，RabbitMQ未连接: {queueName}");
                return;
            }

            try
            {
                _channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogWarning($"消费被取消: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var task = JsonConvert.DeserializeObject<ValidationTask>(json);

                        if (task != null)
                        {
                            _logger?.LogDebug($"收到消息: Queue={queueName}, MessageId={ea.BasicProperties?.MessageId}");
                            await Task.Run(() => handler(task));
                            _channel.BasicAck(ea.DeliveryTag, false);
                            _logger?.LogDebug($"消息处理完成: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        }
                        else
                        {
                            _logger?.LogWarning($"消息反序列化失败: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogError(ex, $"消息反序列化异常: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"处理消息失败: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                _channel.BasicConsume(queue: queueName,
                                     autoAck: false,
                                     consumer: consumer);

                _logger?.LogInformation($"开始消费队列: {queueName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"启动消费失败: Queue={queueName}");
            }
        }

        /// <summary>
        /// 消费队列消息（带重试机制）
        /// </summary>
        public void ConsumeMessagesWithRetry(string queueName, Func<ValidationTask, Task<bool>> handler,
            CancellationToken cancellationToken, int maxRetries = 5, ushort prefetchCount = 1)
        {
            // 确保连接已建立
            if (!Connect())
            {
                _logger?.LogError($"无法启动消费者，RabbitMQ未连接: {queueName}");
                return;
            }

            try
            {
                _channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogWarning($"消费被取消: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var task = JsonConvert.DeserializeObject<ValidationTask>(json);

                        if (task != null)
                        {
                            _logger?.LogDebug($"收到消息: Queue={queueName}, MessageId={ea.BasicProperties?.MessageId}, RetryCount={task.RetryCount}");

                            bool success = await handler(task);

                            if (success)
                            {
                                _channel.BasicAck(ea.DeliveryTag, false);
                                _logger?.LogDebug($"消息处理成功: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                            }
                            else if (task.RetryCount >= maxRetries)
                            {
                                _logger?.LogWarning($"消息达到最大重试次数，已拒绝: Queue={queueName}, DeliveryTag={ea.DeliveryTag}, RetryCount={task.RetryCount}");
                                _channel.BasicNack(ea.DeliveryTag, false, false);
                            }
                            else
                            {
                                task.RetryCount++;
                                var updatedJson = JsonConvert.SerializeObject(task);
                                var updatedBody = Encoding.UTF8.GetBytes(updatedJson);

                                var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, task.RetryCount));
                                await Task.Delay(delay);

                                var properties = _channel.CreateBasicProperties();
                                properties.Persistent = true;
                                properties.DeliveryMode = 2;
                                properties.MessageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
                                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                                _channel.BasicNack(ea.DeliveryTag, false, false);
                                _channel.BasicPublish(exchange: "",
                                                     routingKey: queueName,
                                                     basicProperties: properties,
                                                     body: updatedBody);

                                _logger?.LogDebug($"消息重试: Queue={queueName}, DeliveryTag={ea.DeliveryTag}, RetryCount={task.RetryCount}");
                            }
                        }
                        else
                        {
                            _logger?.LogWarning($"消息反序列化失败: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogError(ex, $"消息反序列化异常: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"处理消息失败: Queue={queueName}, DeliveryTag={ea.DeliveryTag}");
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                _channel.BasicConsume(queue: queueName,
                                     autoAck: false,
                                     consumer: consumer);

                _logger?.LogInformation($"开始消费队列(带重试): {queueName}, MaxRetries={maxRetries}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"启动消费失败: Queue={queueName}");
            }
        }

        /// <summary>
        /// 获取队列消息数量
        /// </summary>
        public long GetQueueMessageCount(string queueName)
        {
            if (!Connect())
            {
                _logger?.LogError($"无法获取队列消息数量，RabbitMQ未连接: {queueName}");
                return 0;
            }

            try
            {
                var queueInfo = _channel.QueueDeclarePassive(queueName);
                return queueInfo.MessageCount;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"获取队列消息数量失败: {queueName}");
                return 0;
            }
        }

        /// <summary>
        /// 安全释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    _channel?.Close();
                    _channel?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "关闭Channel失败");
                }

                try
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "关闭Connection失败");
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~RabbitMqService()
        {
            Dispose(false);
        }
    }
}
