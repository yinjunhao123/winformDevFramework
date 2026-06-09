-- 返工记录表
CREATE TABLE ReworkRecord (
    ReworkId BIGINT IDENTITY(1,1) PRIMARY KEY,
    RfidBarCode NVARCHAR(50) NOT NULL,
    ProductModel NVARCHAR(50) NOT NULL,
    FromStationCode NVARCHAR(50) NOT NULL,
    ToStationCode NVARCHAR(50) NOT NULL,
    ReworkReason NVARCHAR(500) NOT NULL,
    CreateUser NVARCHAR(50) NOT NULL,
    CreateTime DATETIME NOT NULL DEFAULT GETDATE(),
    UpdateUser NVARCHAR(50),
    UpdateTime DATETIME
);

-- 添加索引
CREATE INDEX IX_ReworkRecord_RfidBarCode ON ReworkRecord(RfidBarCode);
CREATE INDEX IX_ReworkRecord_ProductModel ON ReworkRecord(ProductModel);
CREATE INDEX IX_ReworkRecord_FromStationCode ON ReworkRecord(FromStationCode);
CREATE INDEX IX_ReworkRecord_ToStationCode ON ReworkRecord(ToStationCode);
CREATE INDEX IX_ReworkRecord_CreateTime ON ReworkRecord(CreateTime);

-- 添加注释
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'返工记录ID', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'ReworkId';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'RFID条码', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'RfidBarCode';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'产品型号', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'ProductModel';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'来源工站代码', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'FromStationCode';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'目标工站代码', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'ToStationCode';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'返工原因', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'ReworkReason';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'创建用户', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'CreateUser';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'创建时间', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'CreateTime';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'更新用户', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'UpdateUser';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'更新时间', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord',
    @level2type = N'COLUMN', @level2name = N'UpdateTime';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'返工记录表', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'ReworkRecord';