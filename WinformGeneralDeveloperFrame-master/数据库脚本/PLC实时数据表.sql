-- =============================================
-- PLC实时数据表创建脚本
-- 数据库类型: SQL Server
-- =============================================

-- 1. PLC设备实时状态表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_PlcRealTimeStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MD_PlcRealTimeStatus](
        [StatusID] [bigint] IDENTITY(1,1) NOT NULL,
        [PlcID] [bigint] NOT NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [PlcName] [nvarchar](100) NOT NULL,
        [AddressCode] [nvarchar](50) NOT NULL,
        [StatusValue] [nvarchar](50) NOT NULL,
        [StatusName] [nvarchar](100) NOT NULL,
        [CollectTime] [datetime] NOT NULL,
        [IsOnline] [bit] NOT NULL,
        CONSTRAINT [PK_MD_PlcRealTimeStatus] PRIMARY KEY CLUSTERED ([StatusID] ASC)
    )
    PRINT '表 MD_PlcRealTimeStatus 创建成功'
END
ELSE
BEGIN
    PRINT '表 MD_PlcRealTimeStatus 已存在'
END
GO

-- 2. PLC实时报警记录表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_PlcRealTimeAlarm]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MD_PlcRealTimeAlarm](
        [AlarmRecordID] [bigint] IDENTITY(1,1) NOT NULL,
        [PlcID] [bigint] NOT NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [AddressCode] [nvarchar](50) NOT NULL,
        [StationCode] [nvarchar](50) NOT NULL,
        [AlarmCode] [nvarchar](50) NOT NULL,
        [AlarmName] [nvarchar](100) NOT NULL,
        [AlarmLevel] [nvarchar](20) NULL,
        [Suggestion] [nvarchar](500) NULL,
        [TriggerTime] [datetime] NOT NULL,
        [IsHandled] [bit] NOT NULL,
        [HandleTime] [datetime] NULL,
        [HandleUser] [nvarchar](50) NULL,
        CONSTRAINT [PK_MD_PlcRealTimeAlarm] PRIMARY KEY CLUSTERED ([AlarmRecordID] ASC)
    )
    PRINT '表 MD_PlcRealTimeAlarm 创建成功'
END
ELSE
BEGIN
    PRINT '表 MD_PlcRealTimeAlarm 已存在'
END
GO

-- 3. PLC触发参数采集表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_PlcTriggerParam]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MD_PlcTriggerParam](
        [ParamID] [bigint] IDENTITY(1,1) NOT NULL,
        [PlcID] [bigint] NOT NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [TriggerPoint] [nvarchar](50) NOT NULL,
        [ParamName] [nvarchar](100) NOT NULL,
        [ParamValue] [nvarchar](100) NOT NULL,
        [Unit] [nvarchar](20) NULL,
        [CollectTime] [datetime] NOT NULL,
        CONSTRAINT [PK_MD_PlcTriggerParam] PRIMARY KEY CLUSTERED ([ParamID] ASC)
    )
    PRINT '表 MD_PlcTriggerParam 创建成功'
END
ELSE
BEGIN
    PRINT '表 MD_PlcTriggerParam 已存在'
END
GO

-- =============================================
-- 创建索引以提高查询性能
-- =============================================

-- 为实时状态表创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_PlcRealTimeStatus_PlcID' AND object_id = OBJECT_ID('[dbo].[MD_PlcRealTimeStatus]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_PlcRealTimeStatus_PlcID] ON [dbo].[MD_PlcRealTimeStatus] ([PlcID] ASC)
    PRINT '索引 IX_MD_PlcRealTimeStatus_PlcID 创建成功'
END
GO

-- 为实时报警表创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_PlcRealTimeAlarm_PlcID' AND object_id = OBJECT_ID('[dbo].[MD_PlcRealTimeAlarm]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_PlcRealTimeAlarm_PlcID] ON [dbo].[MD_PlcRealTimeAlarm] ([PlcID] ASC)
    PRINT '索引 IX_MD_PlcRealTimeAlarm_PlcID 创建成功'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_PlcRealTimeAlarm_IsHandled' AND object_id = OBJECT_ID('[dbo].[MD_PlcRealTimeAlarm]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_PlcRealTimeAlarm_IsHandled] ON [dbo].[MD_PlcRealTimeAlarm] ([IsHandled] ASC)
    PRINT '索引 IX_MD_PlcRealTimeAlarm_IsHandled 创建成功'
END
GO

-- 为触发参数表创建索引
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_PlcTriggerParam_PlcID' AND object_id = OBJECT_ID('[dbo].[MD_PlcTriggerParam]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_PlcTriggerParam_PlcID] ON [dbo].[MD_PlcTriggerParam] ([PlcID] ASC)
    PRINT '索引 IX_MD_PlcTriggerParam_PlcID 创建成功'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_PlcTriggerParam_CollectTime' AND object_id = OBJECT_ID('[dbo].[MD_PlcTriggerParam]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_PlcTriggerParam_CollectTime] ON [dbo].[MD_PlcTriggerParam] ([CollectTime] DESC)
    PRINT '索引 IX_MD_PlcTriggerParam_CollectTime 创建成功'
END
GO

PRINT '==========================================='
PRINT '所有表和索引创建完成！'
PRINT '==========================================='
