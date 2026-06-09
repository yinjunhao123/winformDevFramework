USE [WinformGeneralDeveloperFrame]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- 检查表是否已存在，不存在则创建
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PLC_ParameterLimit')
BEGIN
    CREATE TABLE [dbo].[PLC_ParameterLimit](
        [ID] [bigint] IDENTITY(1,1) NOT NULL,
        [StationCode] [varchar](50) NOT NULL,
        [ProductModelCode] [varchar](50) NOT NULL,
        [ParameterName] [varchar](100) NOT NULL,
        [UpperLimit] [decimal](18, 4) NULL,
        [LowerLimit] [decimal](18, 4) NULL,
        [Remark] [varchar](500) NULL,
        [CreateUser] [varchar](50) NULL,
        [CreateTime] [datetime] NULL,
        [UpdateUser] [varchar](50) NULL,
        [UpdateTime] [datetime] NULL,
    CONSTRAINT [PK_PLC_ParameterLimit] PRIMARY KEY CLUSTERED 
    (
        [ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- 检查索引是否已存在，不存在则创建
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLC_ParameterLimit_StationCode' AND object_id = OBJECT_ID('PLC_ParameterLimit'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PLC_ParameterLimit_StationCode] ON [dbo].[PLC_ParameterLimit]
    (
        [StationCode] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLC_ParameterLimit_ProductModelCode' AND object_id = OBJECT_ID('PLC_ParameterLimit'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PLC_ParameterLimit_ProductModelCode] ON [dbo].[PLC_ParameterLimit]
    (
        [ProductModelCode] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLC_ParameterLimit_ParameterName' AND object_id = OBJECT_ID('PLC_ParameterLimit'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PLC_ParameterLimit_ParameterName] ON [dbo].[PLC_ParameterLimit]
    (
        [ParameterName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- 创建唯一约束，确保同一工站、产品型号、参数名称只能有一条记录
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_PLC_ParameterLimit_Station_Product_Param' AND object_id = OBJECT_ID('PLC_ParameterLimit'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_PLC_ParameterLimit_Station_Product_Param] ON [dbo].[PLC_ParameterLimit]
    (
        [StationCode] ASC,
        [ProductModelCode] ASC,
        [ParameterName] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO