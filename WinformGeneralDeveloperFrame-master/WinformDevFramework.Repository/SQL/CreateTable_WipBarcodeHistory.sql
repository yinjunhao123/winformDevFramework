-- 创建条码历史表
CREATE TABLE [dbo].[WipBarcodeHistory](
    [HistoryId] [bigint] IDENTITY(1,1) NOT NULL,
    [BarCodeId] [bigint] NOT NULL,
    [BarCode] [nvarchar](100) NOT NULL,
    [RfidCode] [nvarchar](100) NULL,
    [ModelCode] [nvarchar](50) NULL,
    [BarCodeType] [nvarchar](20) NULL,
    [StationCode] [nvarchar](50) NULL,
    [InputTime] [datetime] NULL,
    [OutputTime] [datetime] NULL,
    [CreateUser] [nvarchar](50) NULL,
    [CreateTime] [datetime] NULL,
    [UpdateUser] [nvarchar](50) NULL,
    [UpdateTime] [datetime] NULL,
    [Status] [int] NOT NULL DEFAULT 0,
    [RepairCount] [int] NULL,
    [PrStationCode] [nvarchar](50) NULL,
    [NextStationCode] [nvarchar](50) NULL,
    [BatchCode] [nvarchar](50) NULL,
    [IsRepair] [int] NULL,
    [OperationType] [nvarchar](50) NULL,
    [SnapshotTime] [datetime] NOT NULL,
 CONSTRAINT [PK_WipBarcodeHistory] PRIMARY KEY CLUSTERED 
(
    [HistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 创建索引
CREATE NONCLUSTERED INDEX [IX_WipBarcodeHistory_BarCodeId] ON [dbo].[WipBarcodeHistory]
(
    [BarCodeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_WipBarcodeHistory_BarCode] ON [dbo].[WipBarcodeHistory]
(
    [BarCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_WipBarcodeHistory_SnapshotTime] ON [dbo].[WipBarcodeHistory]
(
    [SnapshotTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_WipBarcodeHistory_StationCode] ON [dbo].[WipBarcodeHistory]
(
    [StationCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
