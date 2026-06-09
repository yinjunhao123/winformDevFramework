# RabbitMQ 消息队列集成说明

## 架构概述

本系统采用 RabbitMQ 消息队列实现 PLC 数据采集的异步处理，将触发监听与业务逻辑解耦，提高系统稳定性和可扩展性。

### 整体流程

```
PLC触发 → 读取数据 → 发送到队列 → 立即复位标志位
                                    ↓
                            队列消费者异步处理
                                    ↓
                    验证站点 → 读取参数 → 验证参数 → 更新状态 → 发布事件
```

## 核心组件

### 1. 消息模型 (ValidationTask.cs)

定义了三种任务类型：
- **MainBarcode**: 主条码扫描任务
- **SubBarcode**: 子条码扫描任务
- **ParameterCollect**: 参数采集任务

### 2. RabbitMQ 服务 (RabbitMqService.cs)

提供消息队列的基础功能：
- 连接管理（自动重连）
- 队列声明（持久化、死信队列）
- 消息发送（支持重试）
- 消息消费（支持手动确认）

### 3. 队列消费者 (ValidationQueueConsumer.cs)

处理队列中的验证任务：
- 验证站点配置
- 读取和验证参数
- 更新数据库记录
- 发布处理结果

### 4. 触发监听服务 (PlcTriggerMonitorService.cs)

负责监听 PLC 标志位：
- 检测标志位变化
- 读取条码数据
- 发送任务到队列
- 立即复位标志位

## 队列设计

### 队列列表

| 队列名称 | 用途 | 持久化 |
|---------|------|--------|
| mainbarcode_queue | 主条码验证任务 | 是 |
| subbarcode_queue | 子条码验证任务 | 是 |
| parameter_queue | 参数采集任务 | 是 |
| dlx_queue | 死信队列（失败消息） | 是 |

### 死信队列配置

- 消息存活时间（TTL）：300秒（5分钟）
- 超时后自动转入死信队列
- 支持人工审核和重试

## 使用步骤

### 1. 安装 RabbitMQ

#### Windows 安装

```powershell
# 1. 下载并安装 Erlang
# https://www.erlang.org/downloads

# 2. 下载并安装 RabbitMQ
# https://www.rabbitmq.com/install-windows.html

# 3. 启用管理插件
cd "C:\Program Files\RabbitMQ Server\rabbitmq_server-3.12.0\sbin"
rabbitmq-plugins enable rabbitmq_management

# 4. 启动服务
net start RabbitMQ

# 5. 访问管理界面
# http://localhost:15672
# 默认用户名/密码：guest/guest
```

#### Docker 安装（推荐）

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 2. 配置连接

修改 `appsettings.rabbitmq.json`：

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

### 3. 注册服务

在依赖注入容器中注册服务：

```csharp
// 注册 RabbitMQ 服务
services.AddSingleton<RabbitMqService>();

// 注册队列消费者
services.AddSingleton<ValidationQueueConsumer>();

// 修改 PlcTriggerMonitorService 构造函数注入
services.AddSingleton<IPlcTriggerMonitorService, PlcTriggerMonitorService>();
```

### 4. 启动服务

```csharp
// 启动触发监听服务
var triggerMonitorService = serviceProvider.GetService<IPlcTriggerMonitorService>();
triggerMonitorService.Start();

// 启动队列消费者
var queueConsumer = serviceProvider.GetService<ValidationQueueConsumer>();
queueConsumer.Start();
```

## 关键特性

### 1. 立即复位标志位

**重要**：读取数据后立即复位 PLC 标志位，避免阻塞 PLC 侧。

```csharp
// 读取数据
string barcode = await ReadBarcodeDataAsync(plcCode, barcodeConfig);

// 发送到队列
await _rabbitMqService.SendMessageAsync("mainbarcode_queue", task);

// 立即复位标志位（关键！）
await WriteBarcodeResetDataAsync(plcCode, barcodeConfig);
```

### 2. 重试机制

- 最大重试次数：5次
- 重试策略：失败后重新入队
- 超过重试次数：转入死信队列

### 3. 消息确认

- 手动确认模式（autoAck = false）
- 处理成功：BasicAck
- 处理失败：BasicNack（重新入队或死信队列）

### 4. 死信队列

- 存储无法处理的消息
- 支持人工审核
- 可定时重试或手动处理

## 监控与管理

### 1. RabbitMQ 管理界面

访问 `http://localhost:15672` 查看队列状态：
- 队列深度（Ready/Unacked/Total）
- 消息速率
- 消费者状态

### 2. 关键指标

| 指标 | 说明 | 正常值 |
|------|------|--------|
| Ready | 等待消费的消息数 | < 100 |
| Unacked | 已消费但未确认的消息数 | < 10 |
| Total | 队列总消息数 | Ready + Unacked |
| Redelivered | 重新投递的消息数 | < 5% |

### 3. 日志监控

```csharp
// 消费者日志
_logger.LogInformation($"开始处理主条码任务: TaskId={task.TaskId}");
_logger.LogWarning($"工站验证失败: StationCode={task.StationCode}");
_logger.LogError(ex, $"处理任务失败: TaskId={task.TaskId}");
```

## 故障排查

### 问题1：队列消息只增不减

**可能原因**：
- 消费者未启动
- 消费者处理异常但未确认消息
- 消息一直重投递

**排查步骤**：
1. 检查消费者是否启动
2. 查看 Unacked 消息数（持续增加说明消费者卡死）
3. 查看日志中的异常信息
4. 检查消息是否在持续 Redelivered

### 问题2：消息丢失

**可能原因**：
- 队列未持久化
- 消息未持久化
- RabbitMQ 服务重启

**解决方案**：
- 确保队列声明时 `durable = true`
- 确保消息发送时 `Persistent = true`
- 使用集群部署保证高可用

### 问题3：处理延迟高

**可能原因**：
- 消费者数量不足
- 单个任务处理时间过长
- 数据库响应慢

**解决方案**：
- 增加消费者数量
- 优化数据库查询
- 使用批量操作

## 性能优化

### 1. 并发控制

```csharp
// 控制并发消费者数量
var semaphore = new SemaphoreSlim(4); // 最多4个并发任务

await semaphore.WaitAsync();
try
{
    await ProcessTaskAsync(task);
}
finally
{
    semaphore.Release();
}
```

### 2. 批量处理

```csharp
// 批量读取PLC数据
var addresses = triggerParams.Select(p => p.AddressCode).ToList();
var results = await _plcCommunicationService.BatchRead(addresses);
```

### 3. 连接池

```csharp
// 使用连接池避免频繁建立连接
private readonly ObjectPool<IPlcConnection> _connectionPool;
```

## 最佳实践

1. **立即复位标志位**：读取数据后立即复位，避免阻塞 PLC
2. **幂等性设计**：使用唯一 ID 避免重复处理
3. **完善的异常处理**：记录日志并适当重试
4. **监控告警**：设置队列深度和处理延迟告警
5. **定期清理**：清理死信队列和过期数据

## 扩展建议

1. **分布式部署**：使用 RabbitMQ 集群支持多实例
2. **消息追踪**：添加消息追踪功能，便于问题排查
3. **性能监控**：集成 Prometheus/Grafana 监控
4. **自动扩缩容**：根据队列深度自动调整消费者数量

## 联系支持

如有问题，请查看：
- RabbitMQ 官方文档：https://www.rabbitmq.com/documentation.html
- 项目日志文件
- RabbitMQ 管理界面