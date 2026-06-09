-- =====================================================
-- 参数下发相关表 SQL Server 脚本
-- 创建时间: 2026-04-30
-- =====================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. 参数下发主表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MD_ParameterDistribution')
BEGIN
    CREATE TABLE [dbo].[MD_ParameterDistribution](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [ProductModelCode] [nvarchar](50) NOT NULL,
        [Version] [nvarchar](50) NOT NULL,
        [CreateUser] [nvarchar](50) NULL,
        [CreateDate] [datetime] NULL,
        [UpdateUser] [nvarchar](50) NULL,
        [UpdateTime] [datetime] NULL,
        CONSTRAINT [PK_MD_ParameterDistribution] PRIMARY KEY CLUSTERED 
        (
            [ID] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistribution_ProductModelCode' AND object_id = OBJECT_ID('MD_ParameterDistribution'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistribution_ProductModelCode] ON [dbo].[MD_ParameterDistribution]
    (
        [ProductModelCode] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- 2. 参数下发明细表
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MD_ParameterDistributionDetail')
BEGIN
    CREATE TABLE [dbo].[MD_ParameterDistributionDetail](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [DistributionID] [bigint] NOT NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [EquipmentCode] [nvarchar](50) NULL,
        [StationCode] [nvarchar](50) NULL,
        [AddressCode] [nvarchar](100) NOT NULL,
        [DataType] [nvarchar](20) NOT NULL,
        [ParamName] [nvarchar](100) NOT NULL,
        [ParamValue] [nvarchar](500) NOT NULL,
        [DistributionType] [nvarchar](50) NOT NULL,
        [CreateUser] [nvarchar](50) NULL,
        [CreateTime] [datetime] NULL,
        [UpdateUser] [nvarchar](50) NULL,
        [UpdateTime] [datetime] NULL,
        CONSTRAINT [PK_MD_ParameterDistributionDetail] PRIMARY KEY CLUSTERED 
        (
            [ID] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionDetail_DistributionID' AND object_id = OBJECT_ID('MD_ParameterDistributionDetail'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionDetail_DistributionID] ON [dbo].[MD_ParameterDistributionDetail]
    (
        [DistributionID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionDetail_PlcCode' AND object_id = OBJECT_ID('MD_ParameterDistributionDetail'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionDetail_PlcCode] ON [dbo].[MD_ParameterDistributionDetail]
    (
        [PlcCode] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- 3. 参数下发记录主表（批次记录）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MD_ParameterDistributionRecord')
BEGIN
    CREATE TABLE [dbo].[MD_ParameterDistributionRecord](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [DistributionID] [bigint] NOT NULL,
        [RecordCode] [nvarchar](50) NOT NULL,
        [ProductModelCode] [nvarchar](50) NOT NULL,
        [Version] [nvarchar](50) NOT NULL,
        [DistributionType] [nvarchar](50) NOT NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [EquipmentCode] [nvarchar](50) NULL,
        [StationCode] [nvarchar](50) NULL,
        [Status] [nvarchar](20) NOT NULL,
        [TotalCount] [int] NOT NULL DEFAULT(0),
        [SuccessCount] [int] NOT NULL DEFAULT(0),
        [FailedCount] [int] NOT NULL DEFAULT(0),
        [Message] [nvarchar](500) NULL,
        [StartTime] [datetime] NULL,
        [EndTime] [datetime] NULL,
        [DurationMs] [bigint] NOT NULL DEFAULT(0),
        [DistributionUser] [nvarchar](50) NULL,
        [CreateTime] [datetime] NULL,
        [CreateUser] [nvarchar](50) NULL,
        CONSTRAINT [PK_MD_ParameterDistributionRecord] PRIMARY KEY CLUSTERED 
        (
            [ID] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionRecord_DistributionID' AND object_id = OBJECT_ID('MD_ParameterDistributionRecord'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionRecord_DistributionID] ON [dbo].[MD_ParameterDistributionRecord]
    (
        [DistributionID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionRecord_RecordCode' AND object_id = OBJECT_ID('MD_ParameterDistributionRecord'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionRecord_RecordCode] ON [dbo].[MD_ParameterDistributionRecord]
    (
        [RecordCode] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- 4. 参数下发记录明细表（每个点位的详细结果）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MD_ParameterDistributionRecordDetail')
BEGIN
    CREATE TABLE [dbo].[MD_ParameterDistributionRecordDetail](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [RecordID] [bigint] NOT NULL,
        [DetailID] [bigint] NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [EquipmentCode] [nvarchar](50) NULL,
        [StationCode] [nvarchar](50) NULL,
        [AddressCode] [nvarchar](100) NOT NULL,
        [DataType] [nvarchar](20) NOT NULL,
        [ParamName] [nvarchar](100) NOT NULL,
        [ParamValue] [nvarchar](500) NOT NULL,
        [Status] [nvarchar](20) NOT NULL,
        [ErrorMessage] [nvarchar](500) NULL,
        [RetryCount] [int] NOT NULL DEFAULT(0),
        [DurationMs] [bigint] NOT NULL DEFAULT(0),
        [DistributionTime] [datetime] NULL,
        CONSTRAINT [PK_MD_ParameterDistributionRecordDetail] PRIMARY KEY CLUSTERED 
        (
            [ID] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionRecordDetail_RecordID' AND object_id = OBJECT_ID('MD_ParameterDistributionRecordDetail'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionRecordDetail_RecordID] ON [dbo].[MD_ParameterDistributionRecordDetail]
    (
        [RecordID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- 5. 参数下发历史主表（保存下发前的设备参数快照）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MD_ParameterDistributionHistory')
BEGIN
    CREATE TABLE [dbo].[MD_ParameterDistributionHistory](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [RecordID] [bigint] NOT NULL,
        [ProductModelCode] [nvarchar](50) NOT NULL,
        [Version] [nvarchar](50) NOT NULL,
        [SnapshotTime] [datetime] NULL,
        [CreateUser] [nvarchar](50) NULL,
        [CreateTime] [datetime] NULL,
        CONSTRAINT [PK_MD_ParameterDistributionHistory] PRIMARY KEY CLUSTERED 
        (
            [ID] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionHistory_RecordID' AND object_id = OBJECT_ID('MD_ParameterDistributionHistory'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionHistory_RecordID] ON [dbo].[MD_ParameterDistributionHistory]
    (
        [RecordID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- 6. 参数下发历史明细表（保存下发前的设备参数快照明细）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MD_ParameterDistributionDetailHistory')
BEGIN
    CREATE TABLE [dbo].[MD_ParameterDistributionDetailHistory](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [HistoryID] [bigint] NOT NULL,
        [PlcCode] [nvarchar](50) NOT NULL,
        [EquipmentCode] [nvarchar](50) NULL,
        [StationCode] [nvarchar](50) NULL,
        [AddressCode] [nvarchar](100) NOT NULL,
        [DataType] [nvarchar](20) NOT NULL,
        [ParamName] [nvarchar](100) NOT NULL,
        [OriginalValue] [nvarchar](500) NULL,
        [NewValue] [nvarchar](500) NOT NULL,
        [DistributionType] [nvarchar](50) NOT NULL,
        CONSTRAINT [PK_MD_ParameterDistributionDetailHistory] PRIMARY KEY CLUSTERED 
        (
            [ID] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MD_ParameterDistributionDetailHistory_HistoryID' AND object_id = OBJECT_ID('MD_ParameterDistributionDetailHistory'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MD_ParameterDistributionDetailHistory_HistoryID] ON [dbo].[MD_ParameterDistributionDetailHistory]
    (
        [HistoryID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- =====================================================
-- 说明:
-- DistributionType 下发类型:
--   - BeforeDistribution: 前置下发（工步开始前）
--   - Dispatching: 调度下发（生产过程中）
--   - AfterDistribution: 后置下发（工步完成后）
--
-- Record.Status 总体状态:
--   - Pending: 待下发
--   - Success: 全部成功
--   - PartialSuccess: 部分成功
--   - Failed: 全部失败
--
-- RecordDetail.Status 点位状态:
--   - Success: 成功
--   - Failed: 失败
--   - Skipped: 跳过
-- =====================================================
