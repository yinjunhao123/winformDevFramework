-- =============================================
-- 主条码过站相关表结构
-- 创建日期: 2025
-- =============================================

-- WipBarCodeProcess 表 - 主条码过站记录表
-- 用于记录主条码的过站信息，包含PLC采集的结果
IF OBJECT_ID('WipBarCodeProcess', 'U') IS NOT NULL
    DROP TABLE WipBarCodeProcess
GO

CREATE TABLE WipBarCodeProcess (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    BarCode VARCHAR(50) NOT NULL,              -- 主条码
    ProductMode VARCHAR(50) NULL,              -- 型号
    ProcessVersion VARCHAR(50) NULL,           -- 工艺参数版本号
    StationCode VARCHAR(50) NULL,              -- 工站编码
    FirstSubBarCode VARCHAR(50) NULL,          -- 第一子零件
    SecondSubBarCode VARCHAR(50) NULL,         -- 第二子零件
    ThirdSubBarCode VARCHAR(50) NULL,          -- 第三子零件
    ProcessResult VARCHAR(50) NULL,            -- 加工结果（PLC采集结果）
    TestCode VARCHAR(50) NULL,                 -- 测试模式
    CreateTime DATETIME NULL,                  -- 创建时间
    CreateUser VARCHAR(50) NULL                -- 创建用户
)
GO

-- WipBarCode 表 - 主条码表（添加缺失字段）
-- 用于记录主条码的基本信息和状态
IF OBJECT_ID('WipBarCode', 'U') IS NOT NULL
BEGIN
    -- 检查并添加缺失的字段
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WipBarCode') AND name = 'Status')
        ALTER TABLE WipBarCode ADD Status INT NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WipBarCode') AND name = 'RepairCount')
        ALTER TABLE WipBarCode ADD RepairCount INT NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WipBarCode') AND name = 'PrStationCode')
        ALTER TABLE WipBarCode ADD PrStationCode VARCHAR(50) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WipBarCode') AND name = 'NextStationCode')
        ALTER TABLE WipBarCode ADD NextStationCode VARCHAR(50) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WipBarCode') AND name = 'BatchCode')
        ALTER TABLE WipBarCode ADD BatchCode VARCHAR(50) NULL
END
GO

-- =============================================
-- 创建索引以提高查询性能
-- =============================================

-- WipBarCodeProcess 表索引
CREATE INDEX IX_WipBarCodeProcess_BarCode ON WipBarCodeProcess(BarCode)
CREATE INDEX IX_WipBarCodeProcess_StationCode ON WipBarCodeProcess(StationCode)
CREATE INDEX IX_WipBarCodeProcess_CreateTime ON WipBarCodeProcess(CreateTime)
CREATE INDEX IX_WipBarCodeProcess_ProductMode ON WipBarCodeProcess(ProductMode)

-- WipBarCode 表索引（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WipBarCode_BarCode' AND object_id = OBJECT_ID('WipBarCode'))
    CREATE INDEX IX_WipBarCode_BarCode ON WipBarCode(BarCode)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WipBarCode_StationCode' AND object_id = OBJECT_ID('WipBarCode'))
    CREATE INDEX IX_WipBarCode_StationCode ON WipBarCode(StationCode)

GO

-- =============================================
-- 表说明
-- =============================================
/*
1. WipBarCodeProcess: 记录主条码的过站历史，包含主条码、型号、工艺版本、工站、子零件、加工结果等信息
   - ProcessResult 字段直接存储 PLC 采集的加工结果
2. WipBarCode: 主条码主表，记录条码的基本信息和当前状态
*/