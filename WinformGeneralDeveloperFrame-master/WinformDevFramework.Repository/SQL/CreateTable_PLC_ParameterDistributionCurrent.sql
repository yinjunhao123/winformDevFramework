USE [WinformGeneralDeveloperFrame]
GO

/****** Object:  Table [dbo].[PLC_ParameterDistributionCurrent]    Script Date: 2026/05/14 10:00:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- 检查表是否已存在，不存在则创建
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PLC_ParameterDistributionCurrent')
BEGIN
    CREATE TABLE [dbo].[PLC_ParameterDistributionCurrent](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [ProductModelCode] [varchar](50) NOT NULL,
        [Status] [tinyint] NOT NULL,
        [StartTime] [datetime] NULL,
        [EndTime] [datetime] NULL,
        [CurrentStationIndex] [int] NOT NULL,
        [TotalStations] [int] NOT NULL,
        [DistributionUser] [varchar](50) NULL,
        [DistributionID] [bigint] NOT NULL,
        [Remark] [varchar](500) NULL,
        [CreateTime] [datetime] NULL,
        [UpdateTime] [datetime] NULL,
    CONSTRAINT [PK_PLC_ParameterDistributionCurrent] PRIMARY KEY CLUSTERED 
    (
        [ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- 检查索引是否已存在，不存在则创建
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLC_ParameterDistributionCurrent_ProductModelCode' AND object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PLC_ParameterDistributionCurrent_ProductModelCode] ON [dbo].[PLC_ParameterDistributionCurrent]
    (
        [ProductModelCode] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLC_ParameterDistributionCurrent_Status' AND object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PLC_ParameterDistributionCurrent_Status] ON [dbo].[PLC_ParameterDistributionCurrent]
    (
        [Status] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- 检查默认约束是否已存在，不存在则添加
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_PLC_ParameterDistributionCurrent_Status' AND parent_object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    ALTER TABLE [dbo].[PLC_ParameterDistributionCurrent] ADD  CONSTRAINT [DF_PLC_ParameterDistributionCurrent_Status]  DEFAULT ((1)) FOR [Status]
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_PLC_ParameterDistributionCurrent_CurrentStationIndex' AND parent_object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    ALTER TABLE [dbo].[PLC_ParameterDistributionCurrent] ADD  CONSTRAINT [DF_PLC_ParameterDistributionCurrent_CurrentStationIndex]  DEFAULT ((0)) FOR [CurrentStationIndex]
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_PLC_ParameterDistributionCurrent_TotalStations' AND parent_object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    ALTER TABLE [dbo].[PLC_ParameterDistributionCurrent] ADD  CONSTRAINT [DF_PLC_ParameterDistributionCurrent_TotalStations]  DEFAULT ((0)) FOR [TotalStations]
END
GO

-- 检查外键约束是否已存在，不存在则添加
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PLC_ParameterDistributionCurrent_PLC_ParameterDistribution' AND parent_object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    ALTER TABLE [dbo].[PLC_ParameterDistributionCurrent]  WITH CHECK ADD  CONSTRAINT [FK_PLC_ParameterDistributionCurrent_PLC_ParameterDistribution] FOREIGN KEY([DistributionID])
    REFERENCES [dbo].[PLC_ParameterDistribution] ([ID])
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PLC_ParameterDistributionCurrent_PLC_ParameterDistribution' AND parent_object_id = OBJECT_ID('PLC_ParameterDistributionCurrent'))
BEGIN
    ALTER TABLE [dbo].[PLC_ParameterDistributionCurrent] CHECK CONSTRAINT [FK_PLC_ParameterDistributionCurrent_PLC_ParameterDistribution]
END
GO
