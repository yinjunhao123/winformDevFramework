-- =============================================
-- 表名: PLC_CalibraCollect
-- 描述: PLC校准数据采集表
-- 创建时间: 2026-06-23
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PLC_CalibraCollect]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PLC_CalibraCollect]
    (
        [ID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [StationCode] NVARCHAR(50) NULL,
        [LineCode] NVARCHAR(50) NULL,
        [PlcCode] NVARCHAR(50) NULL,
        [CalibraTime] NVARCHAR(50) NULL,
        [CalibraResult] TINYINT NULL,
        [Calibra1Data1] DECIMAL(18,6) NULL,
        [Calibra1Data2] DECIMAL(18,6) NULL,
        [Calibra1Data3] DECIMAL(18,6) NULL,
        [Calibra1Data4] DECIMAL(18,6) NULL,
        [Calibra1Data5] DECIMAL(18,6) NULL,
        [Calibra1Data6] DECIMAL(18,6) NULL,
        [Calibra1Data7] DECIMAL(18,6) NULL,
        [Calibra1Data8] DECIMAL(18,6) NULL,
        [Calibra1Data9] DECIMAL(18,6) NULL,
        [Calibra1Data10] DECIMAL(18,6) NULL,
        [Calibra1Data11] DECIMAL(18,6) NULL,
        [Calibra1Data12] DECIMAL(18,6) NULL,
        [Calibra1Data13] DECIMAL(18,6) NULL,
        [Calibra1Data14] DECIMAL(18,6) NULL,
        [Calibra1Data15] DECIMAL(18,6) NULL,
        [CollectTime] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreateUser] NVARCHAR(50) NULL
    );

    -- 创建索引
    CREATE INDEX [IX_PLC_CalibraCollect_StationCode] ON [dbo].[PLC_CalibraCollect]([StationCode]);
    CREATE INDEX [IX_PLC_CalibraCollect_CollectTime] ON [dbo].[PLC_CalibraCollect]([CollectTime] DESC);
    CREATE INDEX [IX_PLC_CalibraCollect_PlcCode] ON [dbo].[PLC_CalibraCollect]([PlcCode]);

    PRINT '表 PLC_CalibraCollect 创建成功';
END
ELSE
BEGIN
    PRINT '表 PLC_CalibraCollect 已存在';
END
GO
