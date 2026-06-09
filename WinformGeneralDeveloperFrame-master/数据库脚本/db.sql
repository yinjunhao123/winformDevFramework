/*
 Navicat Premium Data Transfer

 Source Server         : sqlserver服务器
 Source Server Type    : SQL Server
 Source Server Version : 11002100
 Source Host           : 121.4.95.243:1433
 Source Catalog        : winformdevframework
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 11002100
 File Encoding         : 65001

 Date: 13/05/2024 10:44:58
*/


-- ----------------------------
-- Table structure for sysDataSource
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysDataSource]') AND type IN ('U'))
	DROP TABLE [dbo].[sysDataSource]
GO

CREATE TABLE [dbo].[sysDataSource] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [ConnectName] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Host] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [DataBaseName] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Username] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Password] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[sysDataSource] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'连接名',
'SCHEMA', N'dbo',
'TABLE', N'sysDataSource',
'COLUMN', N'ConnectName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'主机',
'SCHEMA', N'dbo',
'TABLE', N'sysDataSource',
'COLUMN', N'Host'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数据库',
'SCHEMA', N'dbo',
'TABLE', N'sysDataSource',
'COLUMN', N'DataBaseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户名',
'SCHEMA', N'dbo',
'TABLE', N'sysDataSource',
'COLUMN', N'Username'
GO

EXEC sp_addextendedproperty
'MS_Description', N'密码',
'SCHEMA', N'dbo',
'TABLE', N'sysDataSource',
'COLUMN', N'Password'
GO


-- ----------------------------
-- Records of sysDataSource
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysDataSource] ON
GO

INSERT INTO [dbo].[sysDataSource] ([ID], [ConnectName], [Host], [DataBaseName], [Username], [Password]) VALUES (N'1', N'本机', N'localhost', N'winformdevframework', N'sa', N'123456'), (N'2', N'服务器', N'121.4.95.243', N'winformdevframework', N'sa', N'wk123456W')
GO

SET IDENTITY_INSERT [dbo].[sysDataSource] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysDept
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysDept]') AND type IN ('U'))
	DROP TABLE [dbo].[sysDept]
GO

CREATE TABLE [dbo].[sysDept] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [DeptName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PDeptID] int  NULL,
  [LeaderID] int  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[sysDept] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'机构名称',
'SCHEMA', N'dbo',
'TABLE', N'sysDept',
'COLUMN', N'DeptName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'上级机构',
'SCHEMA', N'dbo',
'TABLE', N'sysDept',
'COLUMN', N'PDeptID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'负责人',
'SCHEMA', N'dbo',
'TABLE', N'sysDept',
'COLUMN', N'LeaderID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'sysDept',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of sysDept
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysDept] ON
GO

INSERT INTO [dbo].[sysDept] ([ID], [DeptName], [PDeptID], [LeaderID], [Remark]) VALUES (N'1', N'winformDevFrame', N'0', N'10', NULL), (N'2', N'销售部', N'1', N'8', N''), (N'3', N'采购部', N'1', N'10', N'')
GO

SET IDENTITY_INSERT [dbo].[sysDept] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysDicData
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysDicData]') AND type IN ('U'))
	DROP TABLE [dbo].[sysDicData]
GO

CREATE TABLE [dbo].[sysDicData] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [DicTypeID] int  NULL,
  [DicData] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [DicDataCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[sysDicData] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'字典类型ID',
'SCHEMA', N'dbo',
'TABLE', N'sysDicData',
'COLUMN', N'DicTypeID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'内容',
'SCHEMA', N'dbo',
'TABLE', N'sysDicData',
'COLUMN', N'DicData'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'sysDicData',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'编码',
'SCHEMA', N'dbo',
'TABLE', N'sysDicData',
'COLUMN', N'DicDataCode'
GO


-- ----------------------------
-- Records of sysDicData
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysDicData] ON
GO

INSERT INTO [dbo].[sysDicData] ([ID], [DicTypeID], [DicData], [Remark], [DicDataCode]) VALUES (N'1', N'1', N'Menu', N'', NULL), (N'2', N'1', N'Form', N'', NULL), (N'3', N'1', N'Button', N'', NULL), (N'1002', N'1002', N'电子产品', N'', NULL), (N'1003', N'1002', N'机械产品', N'', NULL), (N'1004', N'1002', N'生活日用品', N'', NULL), (N'1005', N'1002', N'服装', N'', NULL), (N'1006', N'1002', N'食品', N'', NULL), (N'1007', N'1002', N'医疗用品', N'', NULL), (N'1008', N'1003', N'个', N'', NULL), (N'1009', N'1003', N'件', N'', NULL), (N'1010', N'1003', N'台', N'', NULL), (N'1011', N'1003', N'箱', N'', NULL), (N'1012', N'1003', N'套', N'', NULL), (N'1013', N'1003', N'卷', N'', NULL), (N'1014', N'1003', N'克', N'', NULL), (N'1015', N'1003', N'平米', N'', NULL), (N'1016', N'1003', N'千克', N'', NULL), (N'1017', N'1003', N'吨', N'', NULL), (N'1018', N'1003', N'米', N'', NULL), (N'1019', N'1003', N'千米', N'', NULL), (N'1020', N'1003', N'厘米', N'', NULL), (N'1021', N'1003', N'PCS', N'', NULL), (N'2002', N'2002', N'未审核', N'01', NULL), (N'2003', N'2002', N'已审核', N'02', NULL), (N'2004', N'2003', N'盘亏', N'', NULL), (N'2005', N'2003', N'移库', N'', NULL), (N'2006', N'2003', N'借出', N'', NULL), (N'2007', N'2003', N'其他出库', N'', NULL), (N'2008', N'2003', N'盘盈', N'', NULL), (N'2009', N'2003', N'借入', N'', NULL), (N'2010', N'2003', N'其他入库', N'', NULL), (N'2011', N'2004', N'银行转账', N'', NULL), (N'2012', N'2004', N'支付宝', N'', NULL), (N'2013', N'2004', N'微信', N'', NULL), (N'2014', N'2004', N'现金', N'', NULL), (N'2015', N'2005', N'其他', N'', NULL), (N'2016', N'2005', N'电费', N'', NULL), (N'2017', N'2005', N'房租', N'', NULL), (N'2018', N'2006', N'其他', N'', NULL)
GO

SET IDENTITY_INSERT [dbo].[sysDicData] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysDicType
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysDicType]') AND type IN ('U'))
	DROP TABLE [dbo].[sysDicType]
GO

CREATE TABLE [dbo].[sysDicType] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [DicTypeName] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [DicTypeCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[sysDicType] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'字典类型名称',
'SCHEMA', N'dbo',
'TABLE', N'sysDicType',
'COLUMN', N'DicTypeName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'sysDicType',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'字典类型编码',
'SCHEMA', N'dbo',
'TABLE', N'sysDicType',
'COLUMN', N'DicTypeCode'
GO


-- ----------------------------
-- Records of sysDicType
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysDicType] ON
GO

INSERT INTO [dbo].[sysDicType] ([ID], [DicTypeName], [Remark], [DicTypeCode]) VALUES (N'1', N'菜单类型', N'', N'MenuType'), (N'1002', N'商品类别', N'', N'GoodsType'), (N'1003', N'计量单位', N'', N'MeasureUnit'), (N'2002', N'单据状态', N'', N'InvoicesStatus'), (N'2003', N'业务类别', N'', N'YWLB'), (N'2004', N'结算方式', N'', N'JSFS'), (N'2005', N'其他支出类型', N'', N'QTZCLX'), (N'2006', N'其他收入类型', N'', N'QTSRLX')
GO

SET IDENTITY_INSERT [dbo].[sysDicType] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysMenu
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysMenu]') AND type IN ('U'))
	DROP TABLE [dbo].[sysMenu]
GO

CREATE TABLE [dbo].[sysMenu] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [ParentID] int  NULL,
  [Title] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [URL] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Icon] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SortOrder] int  NULL,
  [MenuType] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreatedBy] int  NULL,
  [CreatedDate] datetime  NULL,
  [UpdatedBy] int  NULL,
  [UpdatedDate] datetime  NULL,
  [MenuCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PermissionCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[sysMenu] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单ID',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'ID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'父菜单ID',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'ParentID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单标题',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'Title'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单链接地址',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'URL'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单图标',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'Icon'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单排序顺序',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单类型',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'MenuType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新者',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'UpdatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'UpdatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单编码',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'MenuCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'权限标识',
'SCHEMA', N'dbo',
'TABLE', N'sysMenu',
'COLUMN', N'PermissionCode'
GO


-- ----------------------------
-- Records of sysMenu
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysMenu] ON
GO

INSERT INTO [dbo].[sysMenu] ([ID], [ParentID], [Title], [URL], [Icon], [SortOrder], [MenuType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [MenuCode], [PermissionCode]) VALUES (N'1', N'0', N'WinformDevFramework', N'', N'Resources\设置.png', N'1', N'Menu', NULL, N'2023-08-12 13:23:34.000', N'8', N'2023-08-12 13:23:34.000', N'SystemSetup', N'SystemSetup'), (N'2', N'1034', N'菜单设置', N'FrmMenu', N'Resources\菜单.png', N'1', N'Form', NULL, N'2023-08-12 13:24:06.000', N'8', N'2023-08-12 13:24:06.000', N'FrmMenu', N'FrmMenu'), (N'3', N'1034', N'用户设置', N'FrmUser', N'Resources\用户管理.png', N'2', N'Form', NULL, N'2023-08-12 13:44:33.000', N'8', N'2023-08-12 13:44:33.000', N'FrmUser', N'FrmUser'), (N'4', N'2', N'新增', NULL, NULL, NULL, N'Button', NULL, N'2023-08-12 13:45:06.000', NULL, NULL, N'btnAdd', N'FrmMenu:btnAdd'), (N'10', N'1034', N'角色管理', N'FrmRole', N'Resources\角色管理.png', N'4', N'Form', N'8', N'2023-08-15 23:43:10.000', N'8', N'2023-08-15 23:43:10.000', N'FrmRole', N'FrmRole'), (N'11', N'2', N'保存', NULL, NULL, NULL, N'Button', NULL, N'2023-08-16 13:47:57.000', NULL, NULL, N'btnSave', N'FrmMenu:btnSave'), (N'12', N'2', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-16 13:58:39.000', NULL, NULL, N'btnClose', N'FrmMenu:btnClose'), (N'13', N'2', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-16 13:59:15.000', NULL, NULL, N'btnEdit', N'FrmMenu:btnEdit'), (N'14', N'2', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:02:19.000', NULL, NULL, N'btnCanel', N'FrmMenu:btnCanel'), (N'15', N'2', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:03:13.000', NULL, NULL, N'btnDel', N'FrmMenu:btnDel'), (N'16', N'3', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:04:50.000', NULL, NULL, N'btnAdd', N'FrmUser:btnAdd'), (N'17', N'3', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:06:47.000', NULL, NULL, N'btnSave', N'FrmUser:btnSave'), (N'18', N'3', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:07:39.000', NULL, NULL, N'btnClose', N'FrmUser:btnClose'), (N'19', N'3', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:08:10.000', NULL, NULL, N'btnEdit', N'FrmUser:btnEdit'), (N'20', N'3', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:08:41.000', NULL, NULL, N'btnCanel', N'FrmUser:btnCanel'), (N'21', N'3', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:09:16.000', NULL, NULL, N'btnDel', N'FrmUser:btnDel'), (N'22', N'3', N'重置密码', N'', N'', N'1', N'Button', N'8', N'2023-08-16 14:10:16.000', NULL, NULL, N'btnResetPW', N'FrmUser:btnResetPW'), (N'23', N'1034', N'数据源维护', N'FrmDataSource', N'Resources\数据源.png', N'5', N'Form', N'8', N'2023-08-16 17:21:20.000', N'8', N'2023-08-16 17:21:20.000', N'FrmDataSource', N'FrmDataSource'), (N'24', N'23', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-16 17:49:00.000', NULL, NULL, N'btnAdd', N'FrmDataSource:btnAdd'), (N'25', N'23', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-16 17:49:42.000', NULL, NULL, N'btnSave', N'FrmDataSource:btnSave'), (N'26', N'1034', N'生成代码', N'FrmCreateCode', N'Resources\生成代码.png', N'1', N'Form', N'8', N'2023-08-16 18:18:51.000', N'8', N'2023-08-16 18:18:51.000', N'FrmCreateCode', N'FrmCreateCode'), (N'27', N'10', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-17 10:24:18.000', NULL, NULL, N'btnAdd', N'FrmRole:btnAdd'), (N'28', N'10', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-17 11:02:07.000', NULL, NULL, N'btnSave', N'FrmRole:btnSave'), (N'29', N'10', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-17 12:26:28.000', NULL, NULL, N'btnClose', N'FrmRole:btnClose'), (N'30', N'10', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-17 14:12:53.000', NULL, NULL, N'btnEdit', N'FrmRole:btnEdit'), (N'31', N'10', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-17 14:13:29.000', NULL, NULL, N'btnCanel', N'FrmRole:btnCanel'), (N'32', N'10', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-17 14:14:26.000', NULL, NULL, N'btnDel', N'FrmRole:btnDel'), (N'33', N'1034', N'消息通知', N'FrmMessage', N'Resources\消息.png', N'1', N'Form', N'8', N'2023-08-17 16:10:11.000', N'8', N'2023-08-17 16:10:11.000', N'FrmMessage', N'FrmMessage'), (N'34', N'1034', N'测试', N'Form1', N'Resources\测试.png', N'1', N'Form', N'8', N'2023-08-17 23:55:07.000', N'8', N'2023-08-17 23:55:07.000', N'Form1', N'Form1'), (N'1033', N'1', N'基础资料', N'', N'Resources\基础资料.png', N'1', N'Menu', N'8', N'2023-08-18 12:39:01.000', N'8', N'2023-08-18 12:39:01.000', N'BaseData', N'BaseData'), (N'1034', N'1', N'系统设置', N'', N'Resources\设置.png', N'1', N'Menu', NULL, N'2023-08-12 13:23:34.000', N'8', N'2023-08-12 13:23:34.000', N'SystemSetup', N'SystemSetup'), (N'1035', N'1034', N'字典类型', N'FrmDicType', N'Resources\字典类型.png', N'1', N'Form', N'8', N'2023-08-18 14:30:07.000', N'8', N'2023-08-18 14:30:07.000', N'FrmDicType', N'FrmDicType'), (N'1036', N'1034', N'字典内容', N'FrmDicData', N'Resources\字典内容.png', N'1', N'Form', N'8', N'2023-08-18 15:42:05.000', NULL, NULL, N'FrmDicData', N'FrmDicData'), (N'1037', N'23', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-18 19:12:01.000', N'8', N'2023-08-18 19:12:01.000', N'btnEdit', N'FrmDataSource:btnEdit'), (N'1038', N'23', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-18 19:15:03.000', NULL, NULL, N'btnCanel', N'FrmDataSource:btnCanel'), (N'1039', N'23', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-18 19:16:07.000', NULL, NULL, N'btnDel', N'FrmDataSource:btnDel'), (N'1040', N'23', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-18 19:16:42.000', NULL, NULL, N'btnClose', N'FrmDataSource:btnClose'), (N'2033', N'1033', N'供应商信息', N'FrmSupplier', N'Resources\供应商管理.png', N'1', N'Form', N'8', N'2023-08-22 12:12:21.000', N'8', N'2023-08-22 12:12:21.000', N'FrmSupplier', N'FrmSupplier'), (N'3033', N'1033', N'仓库管理', N'Frmw_Warehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-08-29 10:46:34.000', N'8', N'2023-08-29 10:46:34.000', N'Frmw_Warehouse', N'Frmw_Warehouse'), (N'3034', N'1033', N'客户管理', N'Frmw_Customer', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-08-29 18:55:14.000', NULL, NULL, N'Frmw_Customer', N'Frmw_Customer'), (N'3035', N'1033', N'结算账户', N'Frmw_SettlementAccount', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-08-29 19:28:43.000', NULL, NULL, N'Frmw_SettlementAccount', N'Frmw_SettlementAccount'), (N'3036', N'2033', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:07:33.000', NULL, NULL, N'btnAdd', N'FrmSupplier:btnAdd'), (N'3037', N'2033', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:08:45.000', NULL, NULL, N'btnEdit', N'FrmSupplier:btnEdit'), (N'3038', N'2033', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:09:23.000', NULL, NULL, N'btnSave', N'FrmSupplier:btnSave'), (N'3039', N'2033', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:10:39.000', NULL, NULL, N'btnCanel', N'FrmSupplier:btnCanel'), (N'3040', N'2033', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:11:14.000', NULL, NULL, N'btnDel', N'FrmSupplier:btnDel'), (N'3041', N'2033', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:11:47.000', NULL, NULL, N'btnClose', N'FrmSupplier:btnClose'), (N'3042', N'2033', N'测试', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:27:02.000', N'8', N'2023-08-30 10:27:02.000', N'button1', N'FrmSupplier:btnTest'), (N'3043', N'1036', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:33:05.000', NULL, NULL, N'btnAdd', N'FrmDicData:btnAdd'), (N'3044', N'1036', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:33:42.000', NULL, NULL, N'btnEdit', N'FrmDicData:btnEdit'), (N'3045', N'1036', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:34:07.000', NULL, NULL, N'btnSave', N'FrmDicData:btnSave'), (N'3046', N'1036', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:34:38.000', NULL, NULL, N'btnCanel', N'FrmDicData:btnCanel'), (N'3047', N'1036', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:35:07.000', NULL, NULL, N'btnDel', N'FrmDicData:btnDel'), (N'3048', N'1036', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:41:53.000', NULL, NULL, N'btnClose', N'FrmDicData:btnClose'), (N'3049', N'1035', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:42:44.000', NULL, NULL, N'btnAdd', N'FrmDicType:btnAdd'), (N'3050', N'1035', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:44:41.000', NULL, NULL, N'btnEdit', N'FrmDicType:btnEdit'), (N'3051', N'1035', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:45:04.000', NULL, NULL, N'btnSave', N'FrmDicType:btnSave'), (N'3052', N'1035', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:45:27.000', NULL, NULL, N'btnCanel', N'FrmDicType:btnCanel'), (N'3053', N'1035', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:45:54.000', NULL, NULL, N'btnDel', N'FrmDicType:btnDel'), (N'3054', N'1035', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 10:47:04.000', NULL, NULL, N'btnClose', N'FrmDicType:btnClose'), (N'3055', N'3034', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:01:30.000', NULL, NULL, N'btnAdd', N'Frmw_Customer:btnAdd'), (N'3056', N'3034', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:03:26.000', NULL, NULL, N'btnEdit', N'Frmw_Customer:btnEdit'), (N'3057', N'3034', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:03:57.000', NULL, NULL, N'btnSave', N'Frmw_Customer:btnSave'), (N'3058', N'3034', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:04:44.000', NULL, NULL, N'btnCanel', N'Frmw_Customer:btnCanel'), (N'3059', N'3034', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:05:13.000', NULL, NULL, N'btnDel', N'Frmw_Customer:btnDel'), (N'3060', N'3034', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:05:47.000', NULL, NULL, N'btnClose', N'Frmw_Customer:btnClose'), (N'3061', N'3035', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:06:30.000', NULL, NULL, N'btnAdd', N'Frmw_SettlementAccount:btnAdd'), (N'3062', N'3035', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:07:05.000', NULL, NULL, N'btnEdit', N'Frmw_SettlementAccount:btnEdit'), (N'3063', N'3035', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:07:26.000', NULL, NULL, N'btnSave', N'Frmw_SettlementAccount:btnSave'), (N'3064', N'3035', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:08:02.000', NULL, NULL, N'btnCanel', N'Frmw_SettlementAccount:btnCanel'), (N'3065', N'3035', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:09:23.000', NULL, NULL, N'btnDel', N'Frmw_SettlementAccount:btnDel'), (N'3066', N'3035', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:09:44.000', NULL, NULL, N'btnClose', N'Frmw_SettlementAccount:btnClose'), (N'3067', N'3033', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:10:22.000', NULL, NULL, N'btnAdd', N'Frmw_Warehourses:btnAdd'), (N'3068', N'3033', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:10:57.000', NULL, NULL, N'btnEdit', N'Frmw_Warehourses:btnEdit'), (N'3069', N'3033', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:12:22.000', NULL, NULL, N'btnSave', N'Frmw_Warehourses:btnSave'), (N'3070', N'3033', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:12:48.000', NULL, NULL, N'btnCanel', N'Frmw_Warehourses:btnCanel'), (N'3071', N'3033', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:13:20.000', NULL, NULL, N'btnDel', N'Frmw_Warehourses:btnDel'), (N'3072', N'3033', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:13:44.000', NULL, NULL, N'btnClose', N'Frmw_Warehourses:btnClose'), (N'3073', N'1033', N'商品管理', N'FrmW_Goods', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-08-30 11:41:29.000', NULL, NULL, N'FrmW_Goods', N'FrmW_Goods'), (N'3074', N'3073', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:42:07.000', NULL, NULL, N'btnAdd', N'FrmW_Goods:btnAdd'), (N'3075', N'3073', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:42:35.000', NULL, NULL, N'btnEdit', N'FrmW_Goods:btnEdit'), (N'3076', N'3073', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:42:58.000', NULL, NULL, N'btnSave', N'FrmW_Goods:btnSave'), (N'3077', N'3073', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:43:32.000', NULL, NULL, N'btnCanel', N'FrmW_Goods:btnCanel'), (N'3078', N'3073', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:44:00.000', NULL, NULL, N'btnDel', N'FrmW_Goods:btnDel'), (N'3079', N'3073', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-08-30 11:44:25.000', NULL, NULL, N'btnClose', N'FrmW_Goods:btnClose'), (N'3080', N'1', N'采购', N'', N'Resources\菜单.png', N'1', N'Menu', N'8', N'2023-08-31 09:05:19.000', N'8', N'2023-08-31 09:05:19.000', N'Buy', N'Buy'), (N'3081', N'3080', N'采购订单', N'Frmw_Buy', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-08-31 09:06:41.000', N'8', N'2023-08-31 09:06:41.000', N'Frmw_Buy', N'Frmw_Buy'), (N'4035', N'1034', N'机构管理', N'FrmsysDept', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-04 13:08:20.000', NULL, NULL, N'FrmsysDept', N'FrmsysDept'), (N'4036', N'3081', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-04 20:52:24.000', NULL, NULL, N'btnAdd', N'Frmw_Buy:btnAdd'), (N'4037', N'3081', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-04 20:56:51.000', NULL, NULL, N'btnEdit', N'Frmw_Buy:btnEdit'), (N'4038', N'3081', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-04 21:03:01.000', NULL, NULL, N'btnSave', N'Frmw_Buy:btnSave'), (N'4039', N'3081', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-04 21:03:26.000', NULL, NULL, N'btnCanel', N'Frmw_Buy:btnCanel'), (N'4040', N'3081', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-04 21:04:11.000', NULL, NULL, N'btnDel', N'Frmw_Buy:btnDel'), (N'4041', N'3081', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-04 21:04:44.000', NULL, NULL, N'btnClose', N'Frmw_Buy:btnClose'), (N'4042', N'1', N'销售', N'', N'Resources\菜单.png', N'1', N'Menu', N'8', N'2023-09-04 21:51:46.000', NULL, NULL, N'Sale', N'Sale'), (N'4043', N'4042', N'销售订单', N'Frmw_Sale', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-04 21:52:30.000', N'8', N'2023-09-04 21:52:30.000', N'Frmw_Sale', N'Frmw_Sale'), (N'5036', N'3080', N'采购退货', N'Frmw_BuyReturn', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-06 10:37:43.000', NULL, NULL, N'Frmw_BuyReturn', N'Frmw_BuyReturn'), (N'5037', N'1', N'仓库', N'Warehouses', N'Resources\菜单.png', N'1', N'Menu', N'8', N'2023-09-07 21:23:49.000', NULL, NULL, N'Warehouses', N'Warehouses'), (N'5038', N'5037', N'采购入库', N'Frmw_BuyInWarehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-11 11:48:37.000', N'8', N'2023-09-11 11:48:37.000', N'Frmw_BuyInWarehouse', N'Frmw_BuyInWarehouse'), (N'5039', N'5037', N'采购退货出库', N'Frmw_BuyReturnOutWarehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-11 18:25:04.000', N'8', N'2023-09-11 18:25:04.000', N'Frmw_BuyReturnOutWarehouse', N'Frmw_BuyReturnOutWarehouse')
GO

INSERT INTO [dbo].[sysMenu] ([ID], [ParentID], [Title], [URL], [Icon], [SortOrder], [MenuType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [MenuCode], [PermissionCode]) VALUES (N'5040', N'5037', N'其他入库单', N'Frmw_OtherInWarehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-12 11:54:34.000', N'8', N'2023-09-12 11:54:34.000', N'Frmw_OtherInWarehouse', N'Frmw_OtherInWarehouse'), (N'5041', N'5037', N'其他出库单', N'Frmw_OtherOutWarehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-12 11:55:11.000', N'8', N'2023-09-12 11:55:11.000', N'Frmw_OtherOutWarehouse', N'Frmw_OtherOutWarehouse'), (N'5042', N'1', N'财务', N'Finance', N'Resources\菜单.png', N'1', N'Menu', N'8', N'2023-09-13 18:51:58.000', NULL, NULL, N'Finance', N'Finance'), (N'5043', N'5042', N'收款单', N'Frmw_Receivable', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-13 18:52:35.000', NULL, NULL, N'Frmw_Receivable', N'Frmw_Receivable'), (N'5044', N'5042', N'付款单', N'Frmw_Payment', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-13 18:53:00.000', NULL, NULL, N'Frmw_Payment', N'Frmw_Payment'), (N'5045', N'5037', N'销售出库', N'Frmw_SaleOutWarehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-15 10:51:55.000', NULL, NULL, N'Frmw_SaleOutWarehouse', N'Frmw_SaleOutWarehouse'), (N'5046', N'5037', N'销售退货入库', N'Frmw_SaleReturnInWarehouse', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-15 10:52:37.000', NULL, NULL, N'Frmw_SaleReturnInWarehouse', N'Frmw_SaleReturnInWarehouse'), (N'5047', N'4042', N'销售退货', N'Frmw_SaleReturn', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-15 10:53:08.000', NULL, NULL, N'Frmw_SaleReturn', N'Frmw_SaleReturn'), (N'5048', N'5042', N'其他收入单', N'Frmw_OtherIncome', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-15 10:54:12.000', NULL, NULL, N'Frmw_OtherIncome', N'Frmw_OtherIncome'), (N'5049', N'5042', N'其他支出单', N'Frmw_OtherOutlay', N'Resources\菜单.png', N'1', N'Form', N'8', N'2023-09-15 10:54:40.000', NULL, NULL, N'Frmw_OtherOutlay', N'Frmw_OtherOutlay'), (N'5051', N'5036', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 13:15:31.000', NULL, NULL, N'btnAdd', N'Frmw_BuyReturn:btnAdd'), (N'5052', N'5036', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 13:16:17.000', NULL, NULL, N'btnEdit', N'Frmw_BuyReturn:Edit'), (N'5053', N'5036', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 13:18:31.000', NULL, NULL, N'btnSave', N'Frmw_BuyReturn:btnSave'), (N'5054', N'5036', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:17:05.000', NULL, NULL, N'btnCanel', N'Frmw_BuyReturn:btnCanel'), (N'5055', N'5036', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:18:05.000', NULL, NULL, N'btnDel', N'Frmw_BuyReturn:btnDel'), (N'5056', N'5036', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:18:36.000', NULL, NULL, N'btnClose', N'Frmw_BuyReturn:btnClose'), (N'5057', N'4035', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'FrmsysDept:btnAdd'), (N'5058', N'4035', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'FrmsysDept:btnEdit'), (N'5059', N'4035', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'FrmsysDept:btnSave'), (N'5060', N'4035', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'FrmsysDept:btnCanel'), (N'5061', N'4035', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'FrmsysDept:btnDel'), (N'5062', N'4035', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'FrmsysDept:btnClose'), (N'5063', N'5038', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_BuyInWarehouse:btnAdd'), (N'5064', N'5038', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_BuyInWarehouse:btnEdit'), (N'5065', N'5038', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_BuyInWarehouse:btnSave'), (N'5066', N'5038', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_BuyInWarehouse:btnCanel'), (N'5067', N'5038', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_BuyInWarehouse:btnDel'), (N'5068', N'5038', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_BuyInWarehouse:btnClose'), (N'5069', N'5039', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_BuyReturnOutWarehouse:btnAdd'), (N'5070', N'5039', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_BuyReturnOutWarehouse:btnEdit'), (N'5071', N'5039', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_BuyReturnOutWarehouse:btnSave'), (N'5072', N'5039', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_BuyReturnOutWarehouse:btnCanel'), (N'5073', N'5039', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_BuyReturnOutWarehouse:btnDel'), (N'5074', N'5039', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_BuyReturnOutWarehouse:btnClose'), (N'5075', N'5048', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_OtherIncome:btnAdd'), (N'5076', N'5048', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_OtherIncome:btnEdit'), (N'5077', N'5048', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_OtherIncome:btnSave'), (N'5078', N'5048', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_OtherIncome:btnCanel'), (N'5079', N'5048', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_OtherIncome:btnDel'), (N'5080', N'5048', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_OtherIncome:btnClose'), (N'5081', N'5040', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_OtherInWarehouse:btnAdd'), (N'5082', N'5040', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_OtherInWarehouse:btnEdit'), (N'5083', N'5040', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_OtherInWarehouse:btnSave'), (N'5084', N'5040', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_OtherInWarehouse:btnCanel'), (N'5085', N'5040', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_OtherInWarehouse:btnDel'), (N'5086', N'5040', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_OtherInWarehouse:btnClose'), (N'5087', N'5049', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_OtherOutlay:btnAdd'), (N'5088', N'5049', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_OtherOutlay:btnEdit'), (N'5089', N'5049', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_OtherOutlay:btnSave'), (N'5090', N'5049', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_OtherOutlay:btnCanel'), (N'5091', N'5049', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_OtherOutlay:btnDel'), (N'5092', N'5049', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_OtherOutlay:btnClose'), (N'5093', N'5041', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_OtherOutWarehouse:btnAdd'), (N'5094', N'5041', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_OtherOutWarehouse:btnEdit'), (N'5095', N'5041', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_OtherOutWarehouse:btnSave'), (N'5096', N'5041', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_OtherOutWarehouse:btnCanel'), (N'5097', N'5041', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_OtherOutWarehouse:btnDel'), (N'5098', N'5041', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_OtherOutWarehouse:btnClose'), (N'5099', N'5044', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_Payment:btnAdd'), (N'5100', N'5044', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_Payment:btnEdit'), (N'5101', N'5044', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_Payment:btnSave'), (N'5102', N'5044', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_Payment:btnCanel'), (N'5103', N'5044', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_Payment:btnDel'), (N'5104', N'5044', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_Payment:btnClose'), (N'5105', N'5043', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_Receivable:btnAdd'), (N'5106', N'5043', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_Receivable:btnEdit'), (N'5107', N'5043', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_Receivable:btnSave'), (N'5108', N'5043', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_Receivable:btnCanel'), (N'5109', N'5043', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_Receivable:btnDel'), (N'5110', N'5043', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_Receivable:btnClose'), (N'5111', N'4043', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_Sale:btnAdd'), (N'5112', N'4043', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_Sale:btnEdit'), (N'5113', N'4043', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_Sale:btnSave'), (N'5114', N'4043', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_Sale:btnCanel'), (N'5115', N'4043', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_Sale:btnDel'), (N'5116', N'4043', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_Sale:btnClose'), (N'5117', N'5045', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_SaleOutWarehouse:btnAdd'), (N'5118', N'5045', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_SaleOutWarehouse:btnEdit'), (N'5119', N'5045', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_SaleOutWarehouse:btnSave'), (N'5120', N'5045', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_SaleOutWarehouse:btnCanel'), (N'5121', N'5045', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_SaleOutWarehouse:btnDel'), (N'5122', N'5045', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_SaleOutWarehouse:btnClose'), (N'5123', N'5047', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_SaleReturn:btnAdd'), (N'5124', N'5047', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_SaleReturn:btnEdit'), (N'5125', N'5047', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_SaleReturn:btnSave'), (N'5126', N'5047', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_SaleReturn:btnCanel'), (N'5127', N'5047', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_SaleReturn:btnDel'), (N'5128', N'5047', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_SaleReturn:btnClose'), (N'5129', N'5046', N'新增', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnAdd', N'Frmw_SaleReturnInWarehouse:btnAdd'), (N'5130', N'5046', N'编辑', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnEdit', N'Frmw_SaleReturnInWarehouse:btnEdit'), (N'5131', N'5046', N'保存', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnSave', N'Frmw_SaleReturnInWarehouse:btnSave'), (N'5132', N'5046', N'撤销', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnCanel', N'Frmw_SaleReturnInWarehouse:btnCanel'), (N'5133', N'5046', N'删除', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnDel', N'Frmw_SaleReturnInWarehouse:btnDel'), (N'5134', N'5046', N'关闭', N'', N'', N'1', N'Button', N'8', N'2023-09-20 20:19:10.000', NULL, NULL, N'btnClose', N'Frmw_SaleReturnInWarehouse:btnClose'), (N'5135', N'2', N'搜索', N'', N'', N'1', N'Button', N'8', N'2023-09-20 21:04:52.000', NULL, NULL, N'btnSearch', N'FrmMenu:btnSearch')
GO

SET IDENTITY_INSERT [dbo].[sysMenu] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysMessage
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysMessage]') AND type IN ('U'))
	DROP TABLE [dbo].[sysMessage]
GO

CREATE TABLE [dbo].[sysMessage] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [Title] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Content] nvarchar(2000) COLLATE Chinese_PRC_CI_AS  NULL,
  [FromUserID] int  NULL,
  [ToUserID] int  NULL,
  [IsSend] bit  NULL,
  [CreateTime] datetime  NULL,
  [SendTime] datetime  NULL
)
GO

ALTER TABLE [dbo].[sysMessage] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'消息标题',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'Title'
GO

EXEC sp_addextendedproperty
'MS_Description', N'消息内容',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'Content'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发送人',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'FromUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'接收人',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'ToUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否发送',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'IsSend'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发送时间',
'SCHEMA', N'dbo',
'TABLE', N'sysMessage',
'COLUMN', N'SendTime'
GO


-- ----------------------------
-- Records of sysMessage
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysMessage] ON
GO

INSERT INTO [dbo].[sysMessage] ([ID], [Title], [Content], [FromUserID], [ToUserID], [IsSend], [CreateTime], [SendTime]) VALUES (N'3', N'测试', N'这是一条测试消息', N'8', N'8', N'1', N'2023-08-17 21:35:50.350', N'2023-08-17 21:52:55.763'), (N'4', N'11', N'11发发', N'8', N'8', N'1', N'2023-08-17 21:53:11.437', N'2023-08-17 21:53:11.743'), (N'5', N'11', N'11发发', N'8', N'8', N'1', N'2023-08-17 21:53:21.113', N'2023-08-17 21:53:21.773'), (N'6', N'11', N'11发发', N'8', N'8', N'1', N'2023-08-17 21:53:24.437', N'2023-08-17 21:53:24.757'), (N'7', N'11', N'11发发', N'8', N'8', N'1', N'2023-08-17 21:53:25.947', N'2023-08-17 21:53:26.757'), (N'8', N'11', N'11发发', N'8', N'8', N'1', N'2023-08-17 21:53:27.603', N'2023-08-17 21:53:27.763'), (N'9', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:53:59.210', N'2023-08-17 21:55:16.433'), (N'10', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:54:01.153', N'2023-08-17 21:55:16.443'), (N'11', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:54:02.507', N'2023-08-17 21:55:16.457'), (N'12', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:54:03.763', N'2023-08-17 21:55:16.463'), (N'13', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:54:05.260', N'2023-08-17 21:55:16.473'), (N'14', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:54:06.507', N'2023-08-17 21:55:16.480'), (N'15', N'11', N'11发发', N'8', N'10', N'1', N'2023-08-17 21:54:08.860', N'2023-08-17 21:55:16.487'), (N'16', N'来自王凯的消息', N'达瓦达瓦服务器服务器', N'8', N'10', N'1', N'2023-08-17 21:57:55.237', N'2023-08-17 21:57:55.443'), (N'17', N'来自王凯的消息', N'达瓦达瓦服务器服务器', N'8', N'8', N'1', N'2023-08-17 22:02:39.193', N'2023-08-17 22:02:40.063'), (N'18', N'来自王凯的消息', N'达瓦达瓦服务器服务器', N'8', N'10', N'1', N'2023-08-17 22:02:39.193', N'2023-08-17 22:02:39.477'), (N'19', N'来自王凯的消息', N'达瓦达瓦服务器服务器', N'8', N'11', N'1', N'2023-08-17 22:02:39.193', N'2023-08-18 00:29:46.680'), (N'1003', N'来自王凯的消息', N'这是一条测试消息', N'8', N'11', N'1', N'2023-08-18 00:30:44.810', N'2023-08-18 00:30:26.650'), (N'2003', N'111', N'111', N'8', N'8', N'1', N'2023-08-18 10:21:21.503', N'2023-08-18 10:21:22.153'), (N'2004', N'111', N'111', N'8', N'10', N'0', N'2023-08-18 10:21:21.503', NULL), (N'2005', N'111', N'111', N'8', N'11', N'1', N'2023-08-18 10:21:21.503', N'2023-09-04 21:11:48.367'), (N'2006', N'111', N'速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给', N'8', N'8', N'1', N'2023-08-18 12:38:28.083', N'2023-08-18 12:38:28.257'), (N'2007', N'111', N'速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给', N'8', N'8', N'1', N'2023-08-18 12:38:37.363', N'2023-08-18 12:38:38.257'), (N'2008', N'111', N'速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给', N'8', N'10', N'0', N'2023-08-18 12:38:37.363', NULL), (N'2009', N'111', N'速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给速度跟不上的过半数的根本VS的改变VS E给', N'8', N'11', N'1', N'2023-08-18 12:38:37.363', N'2023-09-04 21:11:48.373'), (N'2010', N'程序更新', N'程序有新版本了，重新启动即可自动更新', N'8', N'8', N'1', N'2023-08-19 01:13:45.923', N'2023-08-19 01:13:46.293'), (N'3003', N'测试', N'我是一条测试消息', N'8', N'8', N'1', N'2023-08-19 22:53:06.130', N'2023-08-19 22:53:06.587'), (N'3004', N'测试', N'我是一条测试消息', N'8', N'8', N'1', N'2023-08-20 00:27:17.673', N'2023-08-20 00:27:18.430'), (N'3005', N'1', N'1', N'8', N'8', N'1', N'2023-08-22 10:46:57.417', N'2023-08-22 10:46:57.710'), (N'3006', N'333', N'333', N'8', N'8', N'1', N'2023-08-22 19:03:16.177', N'2023-08-22 19:03:17.040'), (N'3007', N'333', N'333', N'8', N'10', N'0', N'2023-08-22 19:03:16.177', NULL), (N'3008', N'333', N'333', N'8', N'11', N'1', N'2023-08-22 19:03:16.177', N'2023-09-04 21:11:48.380'), (N'4005', N'1', N'1', N'8', N'8', N'1', N'2023-08-28 15:06:02.040', N'2023-08-28 15:06:02.167'), (N'5005', N'侧手', N'1', N'8', N'8', N'1', N'2023-09-11 17:20:44.993', N'2023-09-11 17:20:45.937'), (N'5006', N'侧手', N'1', N'8', N'10', N'0', N'2023-09-11 17:20:44.993', NULL), (N'5007', N'1', N'1', N'8', N'8', N'1', N'2023-09-20 20:41:07.073', N'2023-09-22 18:14:04.070'), (N'5008', N'1', N'1', N'8', N'8', N'1', N'2023-10-07 16:57:54.130', N'2023-10-07 16:57:54.697'), (N'5009', N'11', N'11', N'8', N'8', N'1', N'2023-12-04 00:56:05.520', N'2023-12-04 00:56:06.390')
GO

SET IDENTITY_INSERT [dbo].[sysMessage] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysRole
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysRole]') AND type IN ('U'))
	DROP TABLE [dbo].[sysRole]
GO

CREATE TABLE [dbo].[sysRole] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [RoleName] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateBy] int  NULL,
  [CreateTime] datetime  NULL,
  [UpdateBy] int  NULL,
  [UpdateTime] datetime  NULL
)
GO

ALTER TABLE [dbo].[sysRole] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'角色名称',
'SCHEMA', N'dbo',
'TABLE', N'sysRole',
'COLUMN', N'RoleName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'sysRole',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'dbo',
'TABLE', N'sysRole',
'COLUMN', N'CreateBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'dbo',
'TABLE', N'sysRole',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'dbo',
'TABLE', N'sysRole',
'COLUMN', N'UpdateBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'dbo',
'TABLE', N'sysRole',
'COLUMN', N'UpdateTime'
GO


-- ----------------------------
-- Records of sysRole
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysRole] ON
GO

INSERT INTO [dbo].[sysRole] ([ID], [RoleName], [Remark], [CreateBy], [CreateTime], [UpdateBy], [UpdateTime]) VALUES (N'1', N'管理员', N'', N'1', N'2023-08-17 10:21:56.000', N'8', N'2023-10-07 19:50:16.777'), (N'9', N'开发', N'', N'8', N'2023-08-17 15:16:33.253', NULL, NULL)
GO

SET IDENTITY_INSERT [dbo].[sysRole] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysRoleMenu
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysRoleMenu]') AND type IN ('U'))
	DROP TABLE [dbo].[sysRoleMenu]
GO

CREATE TABLE [dbo].[sysRoleMenu] (
  [RoleID] int  NULL,
  [MenuID] int  NULL
)
GO

ALTER TABLE [dbo].[sysRoleMenu] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'角色ID',
'SCHEMA', N'dbo',
'TABLE', N'sysRoleMenu',
'COLUMN', N'RoleID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'菜单ID',
'SCHEMA', N'dbo',
'TABLE', N'sysRoleMenu',
'COLUMN', N'MenuID'
GO


-- ----------------------------
-- Records of sysRoleMenu
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[sysRoleMenu] ([RoleID], [MenuID]) VALUES (N'9', N'1'), (N'9', N'2'), (N'9', N'4'), (N'9', N'11'), (N'9', N'12'), (N'9', N'13'), (N'9', N'14'), (N'9', N'15'), (N'9', N'23'), (N'9', N'24'), (N'9', N'25'), (N'9', N'26'), (N'1', N'1'), (N'1', N'1033'), (N'1', N'2033'), (N'1', N'3036'), (N'1', N'3037'), (N'1', N'3038'), (N'1', N'3039'), (N'1', N'3040'), (N'1', N'3041'), (N'1', N'3042'), (N'1', N'3033'), (N'1', N'3067'), (N'1', N'3068'), (N'1', N'3069'), (N'1', N'3070'), (N'1', N'3071'), (N'1', N'3072'), (N'1', N'3034'), (N'1', N'3055'), (N'1', N'3056'), (N'1', N'3057'), (N'1', N'3058'), (N'1', N'3059'), (N'1', N'3060'), (N'1', N'3035'), (N'1', N'3061'), (N'1', N'3062'), (N'1', N'3063'), (N'1', N'3064'), (N'1', N'3065'), (N'1', N'3066'), (N'1', N'3073'), (N'1', N'3074'), (N'1', N'3075'), (N'1', N'3076'), (N'1', N'3077'), (N'1', N'3078'), (N'1', N'3079'), (N'1', N'1034'), (N'1', N'2'), (N'1', N'4'), (N'1', N'11'), (N'1', N'12'), (N'1', N'13'), (N'1', N'14'), (N'1', N'15'), (N'1', N'5135'), (N'1', N'3'), (N'1', N'16'), (N'1', N'17'), (N'1', N'18'), (N'1', N'19'), (N'1', N'20'), (N'1', N'21'), (N'1', N'22'), (N'1', N'10'), (N'1', N'27'), (N'1', N'28'), (N'1', N'29'), (N'1', N'30'), (N'1', N'31'), (N'1', N'32'), (N'1', N'23'), (N'1', N'24'), (N'1', N'25'), (N'1', N'1037'), (N'1', N'1038'), (N'1', N'1039'), (N'1', N'1040'), (N'1', N'26'), (N'1', N'33'), (N'1', N'1035'), (N'1', N'3049'), (N'1', N'3050'), (N'1', N'3051'), (N'1', N'3052'), (N'1', N'3053'), (N'1', N'3054'), (N'1', N'1036'), (N'1', N'3043'), (N'1', N'3044'), (N'1', N'3045'), (N'1', N'3046'), (N'1', N'3047'), (N'1', N'3048'), (N'1', N'4035'), (N'1', N'5057'), (N'1', N'5058')
GO

INSERT INTO [dbo].[sysRoleMenu] ([RoleID], [MenuID]) VALUES (N'1', N'5059'), (N'1', N'5060'), (N'1', N'5061'), (N'1', N'5062'), (N'1', N'3080'), (N'1', N'3081'), (N'1', N'4036'), (N'1', N'4037'), (N'1', N'4038'), (N'1', N'4039'), (N'1', N'4040'), (N'1', N'4041'), (N'1', N'5036'), (N'1', N'5051'), (N'1', N'5052'), (N'1', N'5053'), (N'1', N'5054'), (N'1', N'5055'), (N'1', N'5056'), (N'1', N'4042'), (N'1', N'4043'), (N'1', N'5111'), (N'1', N'5112'), (N'1', N'5113'), (N'1', N'5114'), (N'1', N'5115'), (N'1', N'5116'), (N'1', N'5047'), (N'1', N'5123'), (N'1', N'5124'), (N'1', N'5125'), (N'1', N'5126'), (N'1', N'5127'), (N'1', N'5128'), (N'1', N'5037'), (N'1', N'5038'), (N'1', N'5063'), (N'1', N'5064'), (N'1', N'5065'), (N'1', N'5066'), (N'1', N'5067'), (N'1', N'5068'), (N'1', N'5039'), (N'1', N'5069'), (N'1', N'5070'), (N'1', N'5071'), (N'1', N'5072'), (N'1', N'5073'), (N'1', N'5074'), (N'1', N'5040'), (N'1', N'5081'), (N'1', N'5082'), (N'1', N'5083'), (N'1', N'5084'), (N'1', N'5085'), (N'1', N'5086'), (N'1', N'5041'), (N'1', N'5093'), (N'1', N'5094'), (N'1', N'5095'), (N'1', N'5096'), (N'1', N'5097'), (N'1', N'5098'), (N'1', N'5045'), (N'1', N'5117'), (N'1', N'5118'), (N'1', N'5119'), (N'1', N'5120'), (N'1', N'5121'), (N'1', N'5122'), (N'1', N'5046'), (N'1', N'5129'), (N'1', N'5130'), (N'1', N'5131'), (N'1', N'5132'), (N'1', N'5133'), (N'1', N'5134'), (N'1', N'5042'), (N'1', N'5043'), (N'1', N'5105'), (N'1', N'5106'), (N'1', N'5107'), (N'1', N'5108'), (N'1', N'5109'), (N'1', N'5110'), (N'1', N'5044'), (N'1', N'5099'), (N'1', N'5100'), (N'1', N'5101'), (N'1', N'5102'), (N'1', N'5103'), (N'1', N'5104'), (N'1', N'5048'), (N'1', N'5075'), (N'1', N'5076'), (N'1', N'5077'), (N'1', N'5078'), (N'1', N'5079'), (N'1', N'5080'), (N'1', N'5049')
GO

INSERT INTO [dbo].[sysRoleMenu] ([RoleID], [MenuID]) VALUES (N'1', N'5087'), (N'1', N'5088'), (N'1', N'5089'), (N'1', N'5090'), (N'1', N'5091'), (N'1', N'5092')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysUser
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysUser]') AND type IN ('U'))
	DROP TABLE [dbo].[sysUser]
GO

CREATE TABLE [dbo].[sysUser] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [Username] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Password] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Fullname] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Email] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CreateTime] datetime  NULL,
  [LastLoginTime] datetime  NULL,
  [Status] bit  NULL,
  [Sex] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [DeptID] int  NULL
)
GO

ALTER TABLE [dbo].[sysUser] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'唯一标识用户的主键',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'ID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户登录名',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'Username'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户登录密码',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'Password'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户姓名',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'Fullname'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户电子邮件',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'Email'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户手机号码',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'账号创建时间',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'CreateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'最后登陆时间',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'LastLoginTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户状态 启用 禁用',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'性别',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'Sex'
GO

EXEC sp_addextendedproperty
'MS_Description', N'部门ID',
'SCHEMA', N'dbo',
'TABLE', N'sysUser',
'COLUMN', N'DeptID'
GO


-- ----------------------------
-- Records of sysUser
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[sysUser] ON
GO

INSERT INTO [dbo].[sysUser] ([ID], [Username], [Password], [Fullname], [Email], [PhoneNumber], [CreateTime], [LastLoginTime], [Status], [Sex], [DeptID]) VALUES (N'8', N'wangkai', N'123456', N'王凯', N'648428741@qq.com', N'15029367414', N'2023-08-11 15:14:08.000', N'2024-05-13 10:35:42.967', N'1', N'男', N'2'), (N'10', N'admin', N'123456', N'管理员', N'', N'', N'2023-08-17 12:44:51.000', N'2023-09-04 21:11:38.060', N'1', N'男', N'0'), (N'11', N'test', N'123456', N'测试', N'', N'', N'2023-08-17 12:46:24.000', N'2023-09-04 21:11:47.287', N'1', N'男', N'0'), (N'13', N'zhangsan', N'123456', N'张三', N'', N'', N'2023-09-04 15:25:24.000', N'2023-09-04 15:25:24.000', N'0', N'男', N'3')
GO

SET IDENTITY_INSERT [dbo].[sysUser] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for sysUserRole
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[sysUserRole]') AND type IN ('U'))
	DROP TABLE [dbo].[sysUserRole]
GO

CREATE TABLE [dbo].[sysUserRole] (
  [UserID] int  NULL,
  [RoleID] int  NULL
)
GO

ALTER TABLE [dbo].[sysUserRole] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户ID',
'SCHEMA', N'dbo',
'TABLE', N'sysUserRole',
'COLUMN', N'UserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'角色ID',
'SCHEMA', N'dbo',
'TABLE', N'sysUserRole',
'COLUMN', N'RoleID'
GO


-- ----------------------------
-- Records of sysUserRole
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[sysUserRole] ([UserID], [RoleID]) VALUES (N'11', N'9'), (N'8', N'1')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Buy
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Buy]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Buy]
GO

CREATE TABLE [dbo].[w_Buy] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [MakeUserName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [DeliveryDate] datetime  NULL,
  [InvoicesImage] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementAccount] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] char(1) COLLATE Chinese_PRC_CI_AS DEFAULT 0 NULL,
  [SupplierCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TotalPrice] money  NULL,
  [TotalPrice1] money  NULL,
  [TotalPrice2] money  NULL
)
GO

ALTER TABLE [dbo].[w_Buy] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'SupplierName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'MakeUserName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'交货日期',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'DeliveryDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'InvoicesImage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'SettlementAccount'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'SupplierCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'总金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'已结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'TotalPrice1'
GO

EXEC sp_addextendedproperty
'MS_Description', N'未结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Buy',
'COLUMN', N'TotalPrice2'
GO


-- ----------------------------
-- Records of w_Buy
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Buy] ON
GO

INSERT INTO [dbo].[w_Buy] ([ID], [BuyCode], [SupplierName], [InvoicesDate], [MakeUserName], [DeliveryDate], [InvoicesImage], [Remark], [SettlementAccount], [Status], [SupplierCode], [TotalPrice], [TotalPrice1], [TotalPrice2]) VALUES (N'2019', N'CG20230922175131', N'供应商A', N'2023-09-22 17:51:18.087', N'王凯', N'2023-09-22 17:51:18.087', N'', N'', N'JSZH001', NULL, N'SYS001', N'79980.0000', N'2000.0000', N'77980.0000'), (N'2020', N'CG20231007195226', N'供应商A', N'2023-10-07 19:52:07.450', N'王凯', N'2023-10-07 19:52:07.450', N'', N'', N'JSZH001', NULL, N'SYS001', N'460000.0000', N'0.0000', N'460000.0000')
GO

SET IDENTITY_INSERT [dbo].[w_Buy] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyDetail]
GO

CREATE TABLE [dbo].[w_BuyDetail] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [BuyID] int  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyNumber] decimal(18)  NULL,
  [UnitPrice] decimal(18)  NULL,
  [DiscountPrice] decimal(18)  NULL,
  [TotalPrice] decimal(18)  NULL,
  [TaxRate] decimal(18)  NULL,
  [TaxUnitPrice] decimal(18)  NULL,
  [TaxPrice] decimal(18)  NULL,
  [TotalTaxPrice] decimal(18)  NULL,
  [Remark] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Warehouse] int  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单ID',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'BuyID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'BuyDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'BuyNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'购货单价',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'UnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'折扣价格',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'DiscountPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'金额',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税率',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'TaxRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税单价',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'TaxUnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税额',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'TaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税金额',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'TotalTaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'beizhu',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'Warehouse'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品单位',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyDetail',
'COLUMN', N'GoodsUnit'
GO


-- ----------------------------
-- Records of w_BuyDetail
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_BuyDetail] ON
GO

INSERT INTO [dbo].[w_BuyDetail] ([ID], [BuyID], [BuyCode], [BuyDetailCode], [GoodsName], [GoodsCode], [BuyNumber], [UnitPrice], [DiscountPrice], [TotalPrice], [TaxRate], [TaxUnitPrice], [TaxPrice], [TotalTaxPrice], [Remark], [Warehouse], [GoodsSpec], [GoodsUnit]) VALUES (N'2022', N'2019', N'CG20230922175131', N'CG20230922175131001', N'联想电脑', N'SP001', N'20', N'3999', N'0', N'79980', N'0', N'3999', N'0', N'79980', N'', NULL, NULL, NULL), (N'2023', N'2020', N'CG20231007195226', N'CG20231007195226001', N'联想电脑', N'SP001', N'200', N'2300', N'0', N'460000', N'0', N'2300', N'0', N'460000', N'', NULL, N'16G+500G', N'台')
GO

SET IDENTITY_INSERT [dbo].[w_BuyDetail] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyInWarehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyInWarehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyInWarehouse]
GO

CREATE TABLE [dbo].[w_BuyInWarehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierName] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [BuyInWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesImage] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyInWarehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购订单',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商编号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'SupplierCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'SupplierName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'BuyInWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'InvoicesImage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouse',
'COLUMN', N'Status'
GO


-- ----------------------------
-- Records of w_BuyInWarehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_BuyInWarehouse] ON
GO

INSERT INTO [dbo].[w_BuyInWarehouse] ([ID], [BuyCode], [SupplierCode], [SupplierName], [InvoicesDate], [BuyInWarehouseCode], [InvoicesImage], [Remark], [Status]) VALUES (N'20', N'CG20230922175131', N'SYS001', N'供应商A', N'2023-09-22 17:57:56.050', N'CGRK20230922175804', N'', N'', N'01')
GO

SET IDENTITY_INSERT [dbo].[w_BuyInWarehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyInWarehouseDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyInWarehouseDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyInWarehouseDetail]
GO

CREATE TABLE [dbo].[w_BuyInWarehouseDetail] (
  [BuyInWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyInWarehouseDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyInWarehouseDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'BuyInWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'BuyInWarehouseDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'BuyDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库数量',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyInWarehouseDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_BuyInWarehouseDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_BuyInWarehouseDetail] ([BuyInWarehouseCode], [BuyInWarehouseDetailCode], [WarehouseCode], [WarehouseName], [BuyDetailCode], [BuyCode], [GoodsCode], [GoodsName], [GoodsUnit], [GoodsSpec], [Number], [Remark]) VALUES (N'BUYRI20230911174910', N'BUYRI20230911174910001', N'CK001', N'成品仓', NULL, N'BUY20230907200406', N'SP003', N'华硕电脑-8G+512G', N'', N'', N'30', N''), (N'BUYRI20230911174411', N'BUYRI20230911174411001', N'CK001', N'成品仓', NULL, N'BUY20230901203124', N'SP002', N'手机', N'', N'', N'10', N''), (N'BUYRI20230911174411', N'BUYRI20230911174411002', N'CK001', N'成品仓', NULL, N'BUY20230901203124', N'SP002', N'手机', N'', N'', N'1', N''), (N'BUYRI20230911174533', N'BUYRI20230911174533001', N'CK001', N'成品仓', NULL, N'BUY20230901203124', N'SP002', N'手机', N'', N'', N'10', N''), (N'BUYRI20230911174533', N'BUYRI20230911174533002', N'CK001', N'成品仓', NULL, N'BUY20230901203124', N'SP002', N'手机', N'', N'', N'0', N''), (N'BUYRI20230911175316', N'BUYRI20230911175316001', N'CK001', N'成品仓', NULL, N'BUY20230907200406', N'SP003', N'华硕电脑-8G+512G', N'', N'', N'200', N'')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyReturn
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyReturn]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyReturn]
GO

CREATE TABLE [dbo].[w_BuyReturn] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [BuyReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierCode] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [MakeUserName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ReviewUserName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesImage] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] char(1) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierName] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyReturn] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'BuyReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'关联的采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'SupplierCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'MakeUserName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审核人',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'ReviewUserName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'InvoicesImage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturn',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_BuyReturn
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_BuyReturn] ON
GO

INSERT INTO [dbo].[w_BuyReturn] ([ID], [BuyReturnCode], [BuyCode], [SupplierCode], [InvoicesDate], [MakeUserName], [ReviewUserName], [InvoicesImage], [Status], [Remark], [SupplierName]) VALUES (N'3', N'CGTH20231007170552', N'CG20230922175131', N'SYS001', N'2023-10-07 00:00:00.000', N'王凯', N'', N'', N'1', N'', N'供应商A')
GO

SET IDENTITY_INSERT [dbo].[w_BuyReturn] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyReturnDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyReturnDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyReturnDetail]
GO

CREATE TABLE [dbo].[w_BuyReturnDetail] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [BuyReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyReturnDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [ReturnNumber] decimal(18)  NULL,
  [UnitPrice] money  NULL,
  [DiscountPrice] money  NULL,
  [TotalPrice] money  NULL,
  [TaxRate] decimal(18)  NULL,
  [TaxUnitPrice] money  NULL,
  [TaxPrice] money  NULL,
  [TotalTaxPrice] money  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyReturnDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'BuyReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'BuyReturnDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货数量',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'ReturnNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货单价',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'UnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'折扣价格',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'DiscountPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货金额',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税率',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'TaxRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税单价',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'TaxUnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税额',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'TaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税金额',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'TotalTaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_BuyReturnDetail
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_BuyReturnDetail] ON
GO

INSERT INTO [dbo].[w_BuyReturnDetail] ([ID], [BuyReturnCode], [BuyReturnDetailCode], [BuyCode], [GoodsCode], [GoodsName], [GoodsSpec], [GoodsUnit], [ReturnNumber], [UnitPrice], [DiscountPrice], [TotalPrice], [TaxRate], [TaxUnitPrice], [TaxPrice], [TotalTaxPrice], [Remark]) VALUES (N'4', N'CGTH20231007170552', N'CGTH20231007170552001', N'CG20230922175131', N'SP001', N'联想电脑', N'', N'', N'1', N'3999.0000', N'0.0000', N'3999.0000', N'0', N'3999.0000', N'0.0000', N'3999.0000', N'')
GO

SET IDENTITY_INSERT [dbo].[w_BuyReturnDetail] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyReturnOutWarehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyReturnOutWarehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyReturnOutWarehouse]
GO

CREATE TABLE [dbo].[w_BuyReturnOutWarehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [BuyReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyReturnOutWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [InvoicesImage] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyReturnOutWarehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'BuyReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货出库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'BuyReturnOutWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商编码',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'SupplierCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'SupplierName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'InvoicesImage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouse',
'COLUMN', N'Status'
GO


-- ----------------------------
-- Records of w_BuyReturnOutWarehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_BuyReturnOutWarehouse] ON
GO

INSERT INTO [dbo].[w_BuyReturnOutWarehouse] ([ID], [BuyReturnCode], [BuyCode], [BuyReturnOutWarehouseCode], [SupplierCode], [SupplierName], [InvoicesDate], [InvoicesImage], [Remark], [Status]) VALUES (N'2', N'CGTH20231007170552', N'CG20230922175131', N'CGTHCK20231007173133', N'SYS001', N'供应商A', N'2023-10-07 17:31:14.000', NULL, N'', N'01')
GO

SET IDENTITY_INSERT [dbo].[w_BuyReturnOutWarehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_BuyReturnOutWarehouseDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_BuyReturnOutWarehouseDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_BuyReturnOutWarehouseDetail]
GO

CREATE TABLE [dbo].[w_BuyReturnOutWarehouseDetail] (
  [BuyReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyReturnOutWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyReturnOutWarehouseDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_BuyReturnOutWarehouseDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货单',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'BuyReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货出库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'BuyReturnOutWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购退货出库明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'BuyReturnOutWarehouseDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'BuyDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_BuyReturnOutWarehouseDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_BuyReturnOutWarehouseDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_BuyReturnOutWarehouseDetail] ([BuyReturnCode], [BuyCode], [BuyReturnOutWarehouseCode], [BuyReturnOutWarehouseDetailCode], [BuyDetailCode], [WarehouseCode], [WarehouseName], [GoodsCode], [GoodsName], [GoodsUnit], [GoodsSpec], [Number], [Remark]) VALUES (N'BUYR20230907112028', N'BUY20230901203124', N'', N'001', NULL, N'CK001', N'成品仓', N'SP002', N'手机', N'', N'', N'10', N''), (N'BUYR20230907112028', N'BUY20230901203124', N'', N'002', NULL, N'CK001', N'成品仓', N'SP002', N'手机', N'', N'', N'0', N''), (N'CGTH20231007170552', N'CG20230922175131', N'CGTHCK20231007173133', N'CGTHCK20231007173133001', NULL, N'CK001', N'成品仓', N'SP001', N'联想电脑', N'', N'', N'1', N'')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Customer
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Customer]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Customer]
GO

CREATE TABLE [dbo].[w_Customer] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [CustomerName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ContactPerson] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Email] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BeginCollect] decimal(18,2)  NULL,
  [EndPayCollect] decimal(18,2)  NULL,
  [TaxtRate] decimal(18,4)  NULL,
  [SortOrder] int  NULL,
  [Status] bit  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Faxing] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Address] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Bank] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TaxpayerNumber] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BankAccount] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [LandlinePhone] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Customer] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'CustomerName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'联系人',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'ContactPerson'
GO

EXEC sp_addextendedproperty
'MS_Description', N'手机号码',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'邮箱',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'Email'
GO

EXEC sp_addextendedproperty
'MS_Description', N'期初应收',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'BeginCollect'
GO

EXEC sp_addextendedproperty
'MS_Description', N'期末应收',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'EndPayCollect'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税率',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'TaxtRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'传真',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'Faxing'
GO

EXEC sp_addextendedproperty
'MS_Description', N'地址',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'Address'
GO

EXEC sp_addextendedproperty
'MS_Description', N'开户行',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'Bank'
GO

EXEC sp_addextendedproperty
'MS_Description', N'纳税人识别号',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'TaxpayerNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'银行账号',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'BankAccount'
GO

EXEC sp_addextendedproperty
'MS_Description', N'固定电话',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'LandlinePhone'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer',
'COLUMN', N'CustomerCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户',
'SCHEMA', N'dbo',
'TABLE', N'w_Customer'
GO


-- ----------------------------
-- Records of w_Customer
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Customer] ON
GO

INSERT INTO [dbo].[w_Customer] ([ID], [CustomerName], [ContactPerson], [PhoneNumber], [Email], [BeginCollect], [EndPayCollect], [TaxtRate], [SortOrder], [Status], [Remark], [Faxing], [Address], [Bank], [TaxpayerNumber], [BankAccount], [LandlinePhone], [CustomerCode]) VALUES (N'5', N'客户1', N'', N'', N'', N'0.00', N'0.00', N'0.0000', N'0', N'0', N'', N'', N'', N'', N'', N'', N'', N'')
GO

SET IDENTITY_INSERT [dbo].[w_Customer] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for W_Goods
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[W_Goods]') AND type IN ('U'))
	DROP TABLE [dbo].[W_Goods]
GO

CREATE TABLE [dbo].[W_Goods] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsType] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [RestockingPrice] decimal(18)  NULL,
  [RetailPrice] decimal(18)  NULL,
  [WholesalePrice] decimal(18)  NULL,
  [SafetyStock] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsImages] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Stock] decimal(18)  NULL
)
GO

ALTER TABLE [dbo].[W_Goods] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编号',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'规格型号',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品类别',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'GoodsType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'进货价',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'RestockingPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'零售价',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'RetailPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'批发价',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'WholesalePrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'安全库存',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'SafetyStock'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品图片',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'GoodsImages'
GO

EXEC sp_addextendedproperty
'MS_Description', N'库存',
'SCHEMA', N'dbo',
'TABLE', N'W_Goods',
'COLUMN', N'Stock'
GO


-- ----------------------------
-- Records of W_Goods
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[W_Goods] ON
GO

INSERT INTO [dbo].[W_Goods] ([ID], [GoodsCode], [GoodsName], [GoodsSpec], [GoodsType], [GoodsUnit], [RestockingPrice], [RetailPrice], [WholesalePrice], [SafetyStock], [Remark], [GoodsImages], [Stock]) VALUES (N'1004', N'SP001', N'联想电脑', N'16G+500G', N'电子产品', N'台', N'1', N'1', N'1', N'100', N'', NULL, N'-11'), (N'1005', N'SP002', N'手机', N'华为手机', N'电子产品', N'台', N'9', N'9', N'9', N'100', N'', NULL, NULL)
GO

SET IDENTITY_INSERT [dbo].[W_Goods] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_HandlerManger
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_HandlerManger]') AND type IN ('U'))
	DROP TABLE [dbo].[w_HandlerManger]
GO

CREATE TABLE [dbo].[w_HandlerManger] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [UserID] int  NULL,
  [Type] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] bit  NULL,
  [SortOrder] int  NULL
)
GO

ALTER TABLE [dbo].[w_HandlerManger] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'人员ID',
'SCHEMA', N'dbo',
'TABLE', N'w_HandlerManger',
'COLUMN', N'UserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'类型',
'SCHEMA', N'dbo',
'TABLE', N'w_HandlerManger',
'COLUMN', N'Type'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态',
'SCHEMA', N'dbo',
'TABLE', N'w_HandlerManger',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'dbo',
'TABLE', N'w_HandlerManger',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'经手人管理',
'SCHEMA', N'dbo',
'TABLE', N'w_HandlerManger'
GO


-- ----------------------------
-- Records of w_HandlerManger
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_HandlerManger] ON
GO

SET IDENTITY_INSERT [dbo].[w_HandlerManger] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_OtherIncome
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_OtherIncome]') AND type IN ('U'))
	DROP TABLE [dbo].[w_OtherIncome]
GO

CREATE TABLE [dbo].[w_OtherIncome] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [OtherIncomeCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [FlatCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [FlatName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvicesDate] datetime  NULL,
  [SettlementCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [IncomeTypeID] int  NULL,
  [IncomeType] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TotalPrice] money  NULL,
  [MakeUserID] int  NULL,
  [ReviewUserID] int  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_OtherIncome] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'其他收入单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'OtherIncomeCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'对方单位编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'FlatCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'对方单位名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'FlatName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'InvicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'SettlementCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'SettlementName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收入类别ID',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'IncomeTypeID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收入类别',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'IncomeType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'金额',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审核人',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'ReviewUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherIncome',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_OtherIncome
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_OtherIncome] ON
GO

INSERT INTO [dbo].[w_OtherIncome] ([ID], [OtherIncomeCode], [FlatCode], [FlatName], [InvicesDate], [SettlementCode], [SettlementName], [IncomeTypeID], [IncomeType], [TotalPrice], [MakeUserID], [ReviewUserID], [Status], [Remark]) VALUES (N'1', N'QTSR20231007175901', N'KH001', N'客户A', N'2023-10-07 00:00:00.000', N'JSZH001', N'账户A', N'2018', N'其他', N'20.0000', N'8', N'8', N'', N''), (N'2', N'QTSR20231007181219', N'KH001', N'客户A', N'2023-10-07 00:00:00.000', N'JSZH001', N'账户A', N'2018', N'其他', N'0.0000', N'13', N'8', N'', N'')
GO

SET IDENTITY_INSERT [dbo].[w_OtherIncome] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_OtherInWarehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_OtherInWarehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_OtherInWarehouse]
GO

CREATE TABLE [dbo].[w_OtherInWarehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [TypeCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [OtherInWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesImage] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [MakeUserID] int  NULL
)
GO

ALTER TABLE [dbo].[w_OtherInWarehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'业务类别',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'TypeCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'OtherInWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'InvoicesImage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouse',
'COLUMN', N'MakeUserID'
GO


-- ----------------------------
-- Records of w_OtherInWarehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_OtherInWarehouse] ON
GO

INSERT INTO [dbo].[w_OtherInWarehouse] ([ID], [TypeCode], [InvoicesDate], [OtherInWarehouseCode], [InvoicesImage], [Remark], [Status], [MakeUserID]) VALUES (N'2', N'2004', N'2023-10-07 17:39:03.000', N'QTRK20231007173911', N'', N'', N'', N'8')
GO

SET IDENTITY_INSERT [dbo].[w_OtherInWarehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_OtherInWarehouseDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_OtherInWarehouseDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_OtherInWarehouseDetail]
GO

CREATE TABLE [dbo].[w_OtherInWarehouseDetail] (
  [OtherInWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [OtherInWarehouseDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_OtherInWarehouseDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'OtherInWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'OtherInWarehouseDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库数量',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherInWarehouseDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_OtherInWarehouseDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_OtherInWarehouseDetail] ([OtherInWarehouseCode], [OtherInWarehouseDetailCode], [WarehouseCode], [WarehouseName], [GoodsCode], [GoodsName], [GoodsUnit], [GoodsSpec], [Number], [Remark]) VALUES (N'QTRK20230912170058', N'QTRK20230912170058001', N'CK001', N'成品仓', N'SP003', N'华硕电脑-8G+512G', N'', N'', N'20', N''), (N'QTRK20231007173911', N'QTRK20231007173911001', N'CK001', N'成品仓', N'SP001', N'SP001', N'台', N'16G+500G', N'0', N'')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_OtherOutlay
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_OtherOutlay]') AND type IN ('U'))
	DROP TABLE [dbo].[w_OtherOutlay]
GO

CREATE TABLE [dbo].[w_OtherOutlay] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [OtherOutlayCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [FlatCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [FlatName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvicesDate] datetime  NULL,
  [SettlementCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [OutlayTypeID] int  NULL,
  [OutlayType] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TotalPrice] money  NULL,
  [MakeUserID] int  NULL,
  [ReviewUserID] int  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_OtherOutlay] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'其他支出单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'OtherOutlayCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'对方单位编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'FlatCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'对方单位名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'FlatName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'InvicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'SettlementCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'SettlementName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收入类别ID',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'OutlayTypeID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收入类别',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'OutlayType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'金额',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审核人',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'ReviewUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutlay',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_OtherOutlay
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_OtherOutlay] ON
GO

INSERT INTO [dbo].[w_OtherOutlay] ([ID], [OtherOutlayCode], [FlatCode], [FlatName], [InvicesDate], [SettlementCode], [SettlementName], [OutlayTypeID], [OutlayType], [TotalPrice], [MakeUserID], [ReviewUserID], [Status], [Remark]) VALUES (N'1', N'QTZC20231007165857', N'KH001', N'客户A', N'2023-10-07 16:58:54.837', N'JSZH001', N'账户A', N'2015', N'其他', N'0.0000', N'8', N'8', N'', N'')
GO

SET IDENTITY_INSERT [dbo].[w_OtherOutlay] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_OtherOutWarehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_OtherOutWarehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_OtherOutWarehouse]
GO

CREATE TABLE [dbo].[w_OtherOutWarehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [TypeCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [InvoicesImage] text COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [MakeUserID] int  NULL,
  [OtherOutWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_OtherOutWarehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'业务类别',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'TypeCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'InvoicesImage'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'出库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouse',
'COLUMN', N'OtherOutWarehouseCode'
GO


-- ----------------------------
-- Records of w_OtherOutWarehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_OtherOutWarehouse] ON
GO

INSERT INTO [dbo].[w_OtherOutWarehouse] ([ID], [TypeCode], [InvoicesDate], [InvoicesImage], [Remark], [Status], [MakeUserID], [OtherOutWarehouseCode]) VALUES (N'2', N'2004', N'2023-10-07 17:39:23.000', N'', N'', N'', N'0', N'QTCK20231007173929')
GO

SET IDENTITY_INSERT [dbo].[w_OtherOutWarehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_OtherOutWarehouseDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_OtherOutWarehouseDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_OtherOutWarehouseDetail]
GO

CREATE TABLE [dbo].[w_OtherOutWarehouseDetail] (
  [OtherOutWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [OtherOutWarehouseDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_OtherOutWarehouseDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'出库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'OtherOutWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'出库明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_OtherOutWarehouseDetail',
'COLUMN', N'OtherOutWarehouseDetailCode'
GO


-- ----------------------------
-- Records of w_OtherOutWarehouseDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_OtherOutWarehouseDetail] ([OtherOutWarehouseCode], [WarehouseCode], [WarehouseName], [GoodsCode], [GoodsName], [GoodsUnit], [GoodsSpec], [Number], [Remark], [OtherOutWarehouseDetailCode]) VALUES (N'QTCK20230912172243', N'CK001', N'成品仓', N'SP003', N'华硕电脑-8G+512G', N'', N'', N'2', N'', N'QTCK20230912172243001'), (N'QTCK20231007173929', N'CK001', N'成品仓', N'SP001', N'SP001', N'台', N'16G+500G', N'0', N'', N'QTCK20231007173929001')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Payment
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Payment]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Payment]
GO

CREATE TABLE [dbo].[w_Payment] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [PayCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PayDate] datetime  NULL,
  [MakeUserID] int  NULL,
  [SettlementType] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TotalPrice] money  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Payment] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'付款单号',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'PayCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'SupplierCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'SupplierName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'付款时间',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'PayDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算方式',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'SettlementType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'SettlementCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'SettlementName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'付款金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Payment',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_Payment
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Payment] ON
GO

INSERT INTO [dbo].[w_Payment] ([ID], [PayCode], [SupplierCode], [SupplierName], [PayDate], [MakeUserID], [SettlementType], [SettlementCode], [SettlementName], [TotalPrice], [Remark]) VALUES (N'18', N'FK20230922180306', N'SYS001', N'供应商A', N'2023-09-22 18:02:58.787', N'0', N'银行转账', N'JSZH001', N'账户A', N'2000.0000', N'')
GO

SET IDENTITY_INSERT [dbo].[w_Payment] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_PaymentDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_PaymentDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_PaymentDetail]
GO

CREATE TABLE [dbo].[w_PaymentDetail] (
  [PayCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PayDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BuyCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Amount1] money  NULL,
  [Amount2] money  NULL,
  [Amount3] money  NULL,
  [Amount4] money  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_PaymentDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'付款单号',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'PayCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'付款明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'PayDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'采购单号',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'BuyCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据金额',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'Amount1'
GO

EXEC sp_addextendedproperty
'MS_Description', N'已结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'Amount2'
GO

EXEC sp_addextendedproperty
'MS_Description', N'未结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'Amount3'
GO

EXEC sp_addextendedproperty
'MS_Description', N'本次结算金额',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'Amount4'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_PaymentDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_PaymentDetail
-- ----------------------------
BEGIN TRANSACTION
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Project
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Project]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Project]
GO

CREATE TABLE [dbo].[w_Project] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [ProjectName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ProjectType] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SortOrder] int  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Project] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'收支项目',
'SCHEMA', N'dbo',
'TABLE', N'w_Project',
'COLUMN', N'ProjectName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'类型',
'SCHEMA', N'dbo',
'TABLE', N'w_Project',
'COLUMN', N'ProjectType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'dbo',
'TABLE', N'w_Project',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Project',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收支项目',
'SCHEMA', N'dbo',
'TABLE', N'w_Project'
GO


-- ----------------------------
-- Records of w_Project
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Project] ON
GO

SET IDENTITY_INSERT [dbo].[w_Project] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Receivable
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Receivable]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Receivable]
GO

CREATE TABLE [dbo].[w_Receivable] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [ReceivableCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ReceivableDate] datetime  NULL,
  [MakeUserID] int  NULL,
  [SettlementCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SettlementType] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TotalPrice] money  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Receivable] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'收款单号',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'ReceivableCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'CustomerCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'CustomerName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收款日期',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'ReceivableDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'SettlementCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'SettlementName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算方式',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'SettlementType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'实收金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Receivable',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_Receivable
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Receivable] ON
GO

INSERT INTO [dbo].[w_Receivable] ([ID], [ReceivableCode], [CustomerCode], [CustomerName], [ReceivableDate], [MakeUserID], [SettlementCode], [SettlementName], [SettlementType], [TotalPrice], [Remark]) VALUES (N'6', N'SK20230922180046', N'KH001', N'客户A', N'2023-09-22 18:00:37.360', N'0', N'JSZH001', N'账户A', N'银行转账', N'5000.0000', N'')
GO

SET IDENTITY_INSERT [dbo].[w_Receivable] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_ReceivableDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_ReceivableDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_ReceivableDetail]
GO

CREATE TABLE [dbo].[w_ReceivableDetail] (
  [ReceivableCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ReceivableDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Amount1] money  NULL,
  [Amount2] money  NULL,
  [Amount3] money  NULL,
  [Amount4] money  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_ReceivableDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'收款单号',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'ReceivableCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'收款明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'ReceivableDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售单号',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据金额',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'Amount1'
GO

EXEC sp_addextendedproperty
'MS_Description', N'已结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'Amount2'
GO

EXEC sp_addextendedproperty
'MS_Description', N'未结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'Amount3'
GO

EXEC sp_addextendedproperty
'MS_Description', N'本次结算金额',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'Amount4'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_ReceivableDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_ReceivableDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_ReceivableDetail] ([ReceivableCode], [ReceivableDetailCode], [SaleCode], [Amount1], [Amount2], [Amount3], [Amount4], [Remark]) VALUES (N'BUY20230916171339', N'BUY20230916171339001', N'XS20230915123132', N'15996.0000', N'0.0000', N'0.0000', N'200.0000', N''), (N'BUY20230916171339', N'BUY20230916171339002', N'XS20230915123132', N'15996.0000', N'0.0000', N'0.0000', N'300.0000', N'')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Sale
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Sale]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Sale]
GO

CREATE TABLE [dbo].[w_Sale] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [CustomerCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [MakeUserID] int  NULL,
  [DeliveryDate] datetime  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesFile] varchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TotalPrice] money  NULL,
  [TotalPrice1] money  NULL,
  [TotalPrice2] money  NULL,
  [CustomerName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Sale] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'CustomerCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'交货日期',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'DeliveryDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据编号 SALE20230904212510  003',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'InvoicesFile'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'总金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'已结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'TotalPrice1'
GO

EXEC sp_addextendedproperty
'MS_Description', N'未结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'TotalPrice2'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Sale',
'COLUMN', N'CustomerName'
GO


-- ----------------------------
-- Records of w_Sale
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Sale] ON
GO

INSERT INTO [dbo].[w_Sale] ([ID], [CustomerCode], [InvoicesDate], [MakeUserID], [DeliveryDate], [SaleCode], [InvoicesFile], [Remark], [TotalPrice], [TotalPrice1], [TotalPrice2], [CustomerName]) VALUES (N'10', N'KH001', N'2023-09-22 17:58:52.093', N'0', N'2023-09-22 17:58:52.093', N'XS20230922175903', N'', N'', N'49990.0000', N'5000.0000', N'44990.0000', N'客户A'), (N'11', N'KH001', N'2023-10-07 17:13:50.727', N'8', N'2023-10-07 17:13:50.727', N'XS20231007171359', N'', N'', N'0.0000', N'0.0000', N'0.0000', N'客户A'), (N'12', N'KH001', N'2023-10-07 18:59:44.690', N'8', N'2023-10-07 18:59:44.690', N'XS20231007190005', N'', N'', N'39990.0000', N'0.0000', N'39990.0000', N'客户A'), (N'13', N'KH001', N'2023-10-07 19:00:38.107', N'8', N'2023-10-07 19:00:38.107', N'XS20231007190113', N'', N'', N'30476.1000', N'0.0000', N'30476.1000', N'客户A')
GO

SET IDENTITY_INSERT [dbo].[w_Sale] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleDetail]
GO

CREATE TABLE [dbo].[w_SaleDetail] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [SaleID] int  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [UnitPrice] decimal(18)  NULL,
  [TotalPrice] decimal(18)  NULL,
  [TaxRate] decimal(18)  NULL,
  [DiscountPrice] decimal(18)  NULL,
  [TaxUnitPrice] decimal(18)  NULL,
  [TaxPrice] decimal(18)  NULL,
  [TotalTaxPrice] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SaleDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'主表ID',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'SaleID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'主表编号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'SaleDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单价',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'UnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税率',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'TaxRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'折扣金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'DiscountPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税单价',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'TaxUnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'TaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税总价',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'TotalTaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_SaleDetail
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_SaleDetail] ON
GO

INSERT INTO [dbo].[w_SaleDetail] ([ID], [SaleID], [SaleCode], [SaleDetailCode], [GoodsCode], [GoodsName], [GoodsSpec], [GoodsUnit], [Number], [UnitPrice], [TotalPrice], [TaxRate], [DiscountPrice], [TaxUnitPrice], [TaxPrice], [TotalTaxPrice], [Remark]) VALUES (N'10', NULL, N'XS20230922175903', N'XS20230922175903001', N'SP001', N'联想电脑', N'16G+500G', N'台', N'10', N'4999', N'49990', N'0', N'0', N'4999', N'0', N'49990', N''), (N'11', NULL, N'XS20231007171359', N'XS20231007171359001', N'SP001', N'联想电脑', N'16G+500G', N'台', N'0', N'0', N'0', N'0', N'0', N'0', N'0', N'0', N''), (N'12', NULL, N'XS20231007190005', N'XS20231007190005001', N'SP001', N'联想电脑', N'16G+500G', N'台', N'10', N'3999', N'39990', N'0', N'0', N'3999', N'0', N'39990', N''), (N'13', NULL, N'XS20231007190113', N'XS20231007190113001', N'SP002', N'手机', N'华为手机', N'台', N'30', N'899', N'26970', N'13', N'0', N'1016', N'3506', N'30476', N'')
GO

SET IDENTITY_INSERT [dbo].[w_SaleDetail] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleOutWarehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleOutWarehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleOutWarehouse]
GO

CREATE TABLE [dbo].[w_SaleOutWarehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleOutWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvicesDate] datetime  NULL,
  [InvicesFile] text COLLATE Chinese_PRC_CI_AS  NULL,
  [MakeUserID] int  NULL,
  [ReviewUserID] int  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SaleOutWarehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售订单',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售出库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'SaleOutWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'CustomerCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'CustomerName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'InvicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'InvicesFile'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审核人',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'ReviewUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouse',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_SaleOutWarehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_SaleOutWarehouse] ON
GO

INSERT INTO [dbo].[w_SaleOutWarehouse] ([ID], [SaleCode], [SaleOutWarehouseCode], [CustomerCode], [CustomerName], [InvicesDate], [InvicesFile], [MakeUserID], [ReviewUserID], [Status], [Remark]) VALUES (N'5', N'XS20230922175903', N'XSCK20230922175924', N'KH001', N'客户A', N'2023-09-22 17:59:08.483', N'', N'0', N'0', N'', N'')
GO

SET IDENTITY_INSERT [dbo].[w_SaleOutWarehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleOutWarehouseDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleOutWarehouseDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleOutWarehouseDetail]
GO

CREATE TABLE [dbo].[w_SaleOutWarehouseDetail] (
  [SaleOutWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleOutWarehouseDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SaleOutWarehouseDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售出库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'SaleOutWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售出库明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'SaleOutWarehouseDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'SaleDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleOutWarehouseDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_SaleOutWarehouseDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_SaleOutWarehouseDetail] ([SaleOutWarehouseCode], [SaleOutWarehouseDetailCode], [SaleCode], [SaleDetailCode], [GoodsCode], [GoodsName], [GoodsSpec], [GoodsUnit], [WarehouseCode], [WarehouseName], [Number], [Remark]) VALUES (N'XSCK20230916152620', N'XSCK20230916152620001', N'XS20230915123132', N'XS20230915123132001', N'SP003', N'华硕电脑-8G+512G', N'', N'台', N'CK001', N'成品仓', N'30', N'')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleReturn
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleReturn]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleReturn]
GO

CREATE TABLE [dbo].[w_SaleReturn] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [SaleReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvoicesDate] datetime  NULL,
  [MakeUserID] int  NULL,
  [InvoicesFile] text COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ReviewUserID] int  NULL,
  [TotalPrice] money  NULL,
  [TotalPrice1] money  NULL,
  [TotalPrice2] money  NULL
)
GO

ALTER TABLE [dbo].[w_SaleReturn] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'SaleReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'CustomerCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'CustomerName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'InvoicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'InvoicesFile'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审核人',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'ReviewUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'总额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'已结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'TotalPrice1'
GO

EXEC sp_addextendedproperty
'MS_Description', N'未结金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturn',
'COLUMN', N'TotalPrice2'
GO


-- ----------------------------
-- Records of w_SaleReturn
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_SaleReturn] ON
GO

INSERT INTO [dbo].[w_SaleReturn] ([ID], [SaleReturnCode], [CustomerCode], [CustomerName], [InvoicesDate], [MakeUserID], [InvoicesFile], [SaleCode], [Remark], [Status], [ReviewUserID], [TotalPrice], [TotalPrice1], [TotalPrice2]) VALUES (N'3', N'XSTH20231007171506', N'KH001', N'客户A', N'2023-10-07 00:00:00.000', N'0', N'', N'XS20231007171359', N'', N'', N'2', N'0.0000', N'0.0000', N'0.0000'), (N'4', N'XSTH20231007172053', N'KH001', N'客户A', N'2023-10-07 00:00:00.000', N'0', N'', N'XS20231007171359', N'', N'', N'0', N'0.0000', N'0.0000', N'0.0000'), (N'5', N'XSTH20231007172221', N'KH001', N'客户A', N'2023-10-07 00:00:00.000', N'8', N'', N'XS20231007171359', N'', N'', N'0', N'0.0000', N'0.0000', N'0.0000')
GO

SET IDENTITY_INSERT [dbo].[w_SaleReturn] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleReturnDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleReturnDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleReturnDetail]
GO

CREATE TABLE [dbo].[w_SaleReturnDetail] (
  [SaleReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleReturnDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [UnitPrice] money  NULL,
  [DiscountPrice] money  NULL,
  [TotalPrice] money  NULL,
  [TaxRate] decimal(18)  NULL,
  [TaxUnitPrice] money  NULL,
  [TaxPrice] money  NULL,
  [TotalTaxPrice] money  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SaleReturnDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'SaleReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'SaleReturnDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货单价',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'UnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'折扣金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'DiscountPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'退货金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'TotalPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税率',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'TaxRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税单价',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'TaxUnitPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'TaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'含税金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'TotalTaxPrice'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnDetail',
'COLUMN', N'SaleCode'
GO


-- ----------------------------
-- Records of w_SaleReturnDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_SaleReturnDetail] ([SaleReturnCode], [SaleReturnDetailCode], [GoodsCode], [GoodsName], [GoodsSpec], [GoodsUnit], [Number], [UnitPrice], [DiscountPrice], [TotalPrice], [TaxRate], [TaxUnitPrice], [TaxPrice], [TotalTaxPrice], [Remark], [SaleCode]) VALUES (N'XSTH20230915141101', N'XSTH20230915141101001', N'SP003', N'华硕电脑-8G+512G', N'', N'', N'5', N'3999.0000', N'0.0000', N'19995.0000', N'0', N'3999.0000', N'0.0000', N'19995.0000', N'', N'XS20230915123132'), (N'XSTH20231007171506', N'XSTH20231007171506001', N'SP001', N'联想电脑', N'16G+500G', N'台', N'0', N'0.0000', N'0.0000', N'0.0000', N'0', N'0.0000', N'0.0000', N'0.0000', N'', N'XS20231007171359'), (N'XSTH20231007172053', N'XSTH20231007172053001', N'SP001', N'联想电脑', N'16G+500G', N'台', N'0', N'0.0000', N'0.0000', N'0.0000', N'0', N'0.0000', N'0.0000', N'0.0000', N'', N'XS20231007171359'), (N'XSTH20231007172221', N'XSTH20231007172221001', N'SP001', N'联想电脑', N'16G+500G', N'台', N'0', N'0.0000', N'0.0000', N'0.0000', N'0', N'0.0000', N'0.0000', N'0.0000', N'', N'XS20231007171359')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleReturnInWarehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleReturnInWarehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleReturnInWarehouse]
GO

CREATE TABLE [dbo].[w_SaleReturnInWarehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [SaleReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [CustomerName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleReturnInWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [InvicesDate] datetime  NULL,
  [InvicesFile] text COLLATE Chinese_PRC_CI_AS  NULL,
  [MakeUserID] int  NULL,
  [ReviewUserID] int  NULL,
  [Status] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SaleReturnInWarehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'SaleReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'SaleCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'CustomerCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'客户名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'CustomerName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'入库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'SaleReturnInWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据日期',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'InvicesDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据附件',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'InvicesFile'
GO

EXEC sp_addextendedproperty
'MS_Description', N'制单人',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'MakeUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审核人',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'ReviewUserID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'单据状态',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouse',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_SaleReturnInWarehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_SaleReturnInWarehouse] ON
GO

SET IDENTITY_INSERT [dbo].[w_SaleReturnInWarehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SaleReturnInWarehouseDetail
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SaleReturnInWarehouseDetail]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SaleReturnInWarehouseDetail]
GO

CREATE TABLE [dbo].[w_SaleReturnInWarehouseDetail] (
  [SaleReturnInWarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleReturnInWarehouseDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleReturnCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SaleReturnDetailCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsSpec] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [GoodsUnit] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NULL,
  [Number] decimal(18)  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SaleReturnInWarehouseDetail] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货入库单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'SaleReturnInWarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货入库明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'SaleReturnInWarehouseDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'SaleReturnCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销售退货明细单号',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'SaleReturnDetailCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'GoodsCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'GoodsName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'商品规格',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'GoodsSpec'
GO

EXEC sp_addextendedproperty
'MS_Description', N'计量单位',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'GoodsUnit'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'Number'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SaleReturnInWarehouseDetail',
'COLUMN', N'Remark'
GO


-- ----------------------------
-- Records of w_SaleReturnInWarehouseDetail
-- ----------------------------
BEGIN TRANSACTION
GO

INSERT INTO [dbo].[w_SaleReturnInWarehouseDetail] ([SaleReturnInWarehouseCode], [SaleReturnInWarehouseDetailCode], [SaleReturnCode], [SaleReturnDetailCode], [WarehouseCode], [WarehouseName], [GoodsCode], [GoodsName], [GoodsSpec], [GoodsUnit], [Number], [Remark]) VALUES (N'XSTHRK20230916161821', N'XSTHRK20230916161821001', N'XSTH20230915141101', N'XSTH20230915141101001', N'CK001', N'成品仓', N'SP003', N'', N'', N'', N'5', N'')
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_SettlementAccount
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_SettlementAccount]') AND type IN ('U'))
	DROP TABLE [dbo].[w_SettlementAccount]
GO

CREATE TABLE [dbo].[w_SettlementAccount] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [Code] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Name] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BeginAmount] decimal(18,2)  NULL,
  [NowAmount] decimal(18,2)  NULL,
  [SortOrder] int  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_SettlementAccount] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'编号',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount',
'COLUMN', N'Code'
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount',
'COLUMN', N'Name'
GO

EXEC sp_addextendedproperty
'MS_Description', N'期初金额',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount',
'COLUMN', N'BeginAmount'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前余额',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount',
'COLUMN', N'NowAmount'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'结算账户',
'SCHEMA', N'dbo',
'TABLE', N'w_SettlementAccount'
GO


-- ----------------------------
-- Records of w_SettlementAccount
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_SettlementAccount] ON
GO

INSERT INTO [dbo].[w_SettlementAccount] ([ID], [Code], [Name], [BeginAmount], [NowAmount], [SortOrder], [Remark]) VALUES (N'3', N'JSZH001', N'账户A', N'0.00', N'3020.00', N'0', N'')
GO

SET IDENTITY_INSERT [dbo].[w_SettlementAccount] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Supplier
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Supplier]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Supplier]
GO

CREATE TABLE [dbo].[w_Supplier] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [SupplierName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [ContactPerson] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [PhoneNumber] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Email] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BeginPay] decimal(18,2)  NULL,
  [EndPay] decimal(18,2)  NULL,
  [TaxtRate] decimal(18,4)  NULL,
  [SortOrder] int  NULL,
  [Status] bit  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Faxing] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Address] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Bank] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [TaxpayerNumber] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [BankAccount] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [LandlinePhone] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [SupplierCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Supplier] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'SupplierName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'联系人',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'ContactPerson'
GO

EXEC sp_addextendedproperty
'MS_Description', N'手机号码',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'PhoneNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'邮箱',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'Email'
GO

EXEC sp_addextendedproperty
'MS_Description', N'期初应付',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'BeginPay'
GO

EXEC sp_addextendedproperty
'MS_Description', N'期末应付',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'EndPay'
GO

EXEC sp_addextendedproperty
'MS_Description', N'税率',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'TaxtRate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'传真',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'Faxing'
GO

EXEC sp_addextendedproperty
'MS_Description', N'地址',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'Address'
GO

EXEC sp_addextendedproperty
'MS_Description', N'开户行',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'Bank'
GO

EXEC sp_addextendedproperty
'MS_Description', N'纳税人识别号',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'TaxpayerNumber'
GO

EXEC sp_addextendedproperty
'MS_Description', N'银行账号',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'BankAccount'
GO

EXEC sp_addextendedproperty
'MS_Description', N'固定电话',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'LandlinePhone'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier',
'COLUMN', N'SupplierCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'供应商',
'SCHEMA', N'dbo',
'TABLE', N'w_Supplier'
GO


-- ----------------------------
-- Records of w_Supplier
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Supplier] ON
GO

INSERT INTO [dbo].[w_Supplier] ([ID], [SupplierName], [ContactPerson], [PhoneNumber], [Email], [BeginPay], [EndPay], [TaxtRate], [SortOrder], [Status], [Remark], [Faxing], [Address], [Bank], [TaxpayerNumber], [BankAccount], [LandlinePhone], [SupplierCode]) VALUES (N'3', N'供应商1', N'', N'', N'', N'0.00', N'0.00', N'0.0000', N'1', N'0', N'', N'', N'', N'', N'', N'', N'', N'')
GO

SET IDENTITY_INSERT [dbo].[w_Supplier] OFF
GO

COMMIT
GO


-- ----------------------------
-- Table structure for w_Warehouse
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[w_Warehouse]') AND type IN ('U'))
	DROP TABLE [dbo].[w_Warehouse]
GO

CREATE TABLE [dbo].[w_Warehouse] (
  [ID] int  IDENTITY(1,1) NOT NULL,
  [WarehouseName] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [Address] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [StorageFee] decimal(18,2)  NULL,
  [ChargePersonID] int  NULL,
  [SortOrder] int  NULL,
  [Remark] nvarchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [WarehouseCode] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[w_Warehouse] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库名称',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'WarehouseName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'地址',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'Address'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓储费',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'StorageFee'
GO

EXEC sp_addextendedproperty
'MS_Description', N'负责人ID',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'ChargePersonID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'备注',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'Remark'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库编码',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse',
'COLUMN', N'WarehouseCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'仓库',
'SCHEMA', N'dbo',
'TABLE', N'w_Warehouse'
GO


-- ----------------------------
-- Records of w_Warehouse
-- ----------------------------
BEGIN TRANSACTION
GO

SET IDENTITY_INSERT [dbo].[w_Warehouse] ON
GO

INSERT INTO [dbo].[w_Warehouse] ([ID], [WarehouseName], [Address], [StorageFee], [ChargePersonID], [SortOrder], [Remark], [WarehouseCode]) VALUES (N'2', N'成品仓', N'', N'0.00', N'8', N'0', N'', N'CK001')
GO

SET IDENTITY_INSERT [dbo].[w_Warehouse] OFF
GO

COMMIT
GO


-- ----------------------------
-- View structure for ViewSysMenu
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[ViewSysMenu]') AND type IN ('V'))
	DROP VIEW [dbo].[ViewSysMenu]
GO

CREATE VIEW [dbo].[ViewSysMenu] AS SELECT
	menu.*, 
	pmenu.Title AS PMenuName, 
	[user].Fullname AS CreateByName, 
	user1.Fullname AS UpdateByName
FROM
	dbo.sysMenu AS menu
	LEFT JOIN
	dbo.sysMenu AS pmenu
	ON 
		menu.ParentID = pmenu.ID
	LEFT JOIN
	dbo.sysUser AS [user]
	ON 
		menu.CreatedBy = [user].ID
	LEFT JOIN
	dbo.sysUser AS user1
	ON 
		menu.UpdatedBy = user1.ID
GO


-- ----------------------------
-- Auto increment value for sysDataSource
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysDataSource]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table sysDataSource
-- ----------------------------
ALTER TABLE [dbo].[sysDataSource] ADD CONSTRAINT [PK__sysDataS__3214EC278CD3C1D5] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysDept
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysDept]', RESEED, 1001)
GO


-- ----------------------------
-- Primary Key structure for table sysDept
-- ----------------------------
ALTER TABLE [dbo].[sysDept] ADD CONSTRAINT [PK__sysDept__3214EC2709C49029] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysDicData
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysDicData]', RESEED, 2018)
GO


-- ----------------------------
-- Primary Key structure for table sysDicData
-- ----------------------------
ALTER TABLE [dbo].[sysDicData] ADD CONSTRAINT [PK__sysDicDa__3214EC27BB670471] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysDicType
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysDicType]', RESEED, 2006)
GO


-- ----------------------------
-- Primary Key structure for table sysDicType
-- ----------------------------
ALTER TABLE [dbo].[sysDicType] ADD CONSTRAINT [PK__sysDicTy__3214EC272718E4DB] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysMenu
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysMenu]', RESEED, 5135)
GO


-- ----------------------------
-- Primary Key structure for table sysMenu
-- ----------------------------
ALTER TABLE [dbo].[sysMenu] ADD CONSTRAINT [PK__sysMenu__3214EC274164A6D3] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysMessage
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysMessage]', RESEED, 5009)
GO


-- ----------------------------
-- Primary Key structure for table sysMessage
-- ----------------------------
ALTER TABLE [dbo].[sysMessage] ADD CONSTRAINT [PK__sysMessa__3214EC27D3539EA3] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysRole
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysRole]', RESEED, 9)
GO


-- ----------------------------
-- Primary Key structure for table sysRole
-- ----------------------------
ALTER TABLE [dbo].[sysRole] ADD CONSTRAINT [PK__sysRole__3214EC27E20F2FDE] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for sysUser
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[sysUser]', RESEED, 1012)
GO


-- ----------------------------
-- Primary Key structure for table sysUser
-- ----------------------------
ALTER TABLE [dbo].[sysUser] ADD CONSTRAINT [PK__sysUser__3214EC275A91AA4C] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Buy
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Buy]', RESEED, 2020)
GO


-- ----------------------------
-- Primary Key structure for table w_Buy
-- ----------------------------
ALTER TABLE [dbo].[w_Buy] ADD CONSTRAINT [PK__W_Buy__3214EC27EA9B31A1] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_BuyDetail
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_BuyDetail]', RESEED, 2023)
GO


-- ----------------------------
-- Primary Key structure for table w_BuyDetail
-- ----------------------------
ALTER TABLE [dbo].[w_BuyDetail] ADD CONSTRAINT [PK__w_BuyDet__3214EC27B382A508] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_BuyInWarehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_BuyInWarehouse]', RESEED, 20)
GO


-- ----------------------------
-- Primary Key structure for table w_BuyInWarehouse
-- ----------------------------
ALTER TABLE [dbo].[w_BuyInWarehouse] ADD CONSTRAINT [PK__w_BuyInW__3214EC27A93E42F6] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_BuyReturn
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_BuyReturn]', RESEED, 3)
GO


-- ----------------------------
-- Primary Key structure for table w_BuyReturn
-- ----------------------------
ALTER TABLE [dbo].[w_BuyReturn] ADD CONSTRAINT [PK__w_BuyRet__3214EC273F229FAA] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_BuyReturnDetail
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_BuyReturnDetail]', RESEED, 4)
GO


-- ----------------------------
-- Primary Key structure for table w_BuyReturnDetail
-- ----------------------------
ALTER TABLE [dbo].[w_BuyReturnDetail] ADD CONSTRAINT [PK__w_BuyRet__3214EC2764360830] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_BuyReturnOutWarehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_BuyReturnOutWarehouse]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table w_BuyReturnOutWarehouse
-- ----------------------------
ALTER TABLE [dbo].[w_BuyReturnOutWarehouse] ADD CONSTRAINT [PK__w_BuyRet__3214EC2748573CE6] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Customer
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Customer]', RESEED, 5)
GO


-- ----------------------------
-- Primary Key structure for table w_Customer
-- ----------------------------
ALTER TABLE [dbo].[w_Customer] ADD CONSTRAINT [PK__w_Suppli__3214EC27CD6521CC] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for W_Goods
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[W_Goods]', RESEED, 1005)
GO


-- ----------------------------
-- Primary Key structure for table W_Goods
-- ----------------------------
ALTER TABLE [dbo].[W_Goods] ADD CONSTRAINT [PK__W_Goods__3214EC276358C51D] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_HandlerManger
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_HandlerManger]', RESEED, 1)
GO


-- ----------------------------
-- Primary Key structure for table w_HandlerManger
-- ----------------------------
ALTER TABLE [dbo].[w_HandlerManger] ADD CONSTRAINT [PK__w_Handle__3214EC275590B6DA] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_OtherIncome
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_OtherIncome]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table w_OtherIncome
-- ----------------------------
ALTER TABLE [dbo].[w_OtherIncome] ADD CONSTRAINT [PK__w_OtherI__3214EC2729EEFCAC] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_OtherInWarehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_OtherInWarehouse]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table w_OtherInWarehouse
-- ----------------------------
ALTER TABLE [dbo].[w_OtherInWarehouse] ADD CONSTRAINT [PK__w_BuyInW__3214EC2798F36142] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_OtherOutlay
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_OtherOutlay]', RESEED, 1)
GO


-- ----------------------------
-- Primary Key structure for table w_OtherOutlay
-- ----------------------------
ALTER TABLE [dbo].[w_OtherOutlay] ADD CONSTRAINT [PK__w_OtherI__3214EC27A55A37CE] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_OtherOutWarehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_OtherOutWarehouse]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table w_OtherOutWarehouse
-- ----------------------------
ALTER TABLE [dbo].[w_OtherOutWarehouse] ADD CONSTRAINT [PK__w_BuyRet__3214EC27495960BE] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Payment
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Payment]', RESEED, 18)
GO


-- ----------------------------
-- Primary Key structure for table w_Payment
-- ----------------------------
ALTER TABLE [dbo].[w_Payment] ADD CONSTRAINT [PK__w_Paymen__3214EC27CA1F3486] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Project
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Project]', RESEED, 1)
GO


-- ----------------------------
-- Primary Key structure for table w_Project
-- ----------------------------
ALTER TABLE [dbo].[w_Project] ADD CONSTRAINT [PK__w_Projec__3214EC27D8EEED0B] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Receivable
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Receivable]', RESEED, 6)
GO


-- ----------------------------
-- Primary Key structure for table w_Receivable
-- ----------------------------
ALTER TABLE [dbo].[w_Receivable] ADD CONSTRAINT [PK__w_Receiv__3214EC27A5AA652A] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Sale
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Sale]', RESEED, 13)
GO


-- ----------------------------
-- Primary Key structure for table w_Sale
-- ----------------------------
ALTER TABLE [dbo].[w_Sale] ADD CONSTRAINT [PK__w_Sale__3214EC27997AAB67] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_SaleDetail
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_SaleDetail]', RESEED, 13)
GO


-- ----------------------------
-- Primary Key structure for table w_SaleDetail
-- ----------------------------
ALTER TABLE [dbo].[w_SaleDetail] ADD CONSTRAINT [PK__w_SaleDe__3214EC27018F639E] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_SaleOutWarehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_SaleOutWarehouse]', RESEED, 5)
GO


-- ----------------------------
-- Primary Key structure for table w_SaleOutWarehouse
-- ----------------------------
ALTER TABLE [dbo].[w_SaleOutWarehouse] ADD CONSTRAINT [PK__w_SaleOu__3214EC278EC73A45] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_SaleReturn
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_SaleReturn]', RESEED, 5)
GO


-- ----------------------------
-- Primary Key structure for table w_SaleReturn
-- ----------------------------
ALTER TABLE [dbo].[w_SaleReturn] ADD CONSTRAINT [PK__w_SaleRe__3214EC276A1FA662] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_SaleReturnInWarehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_SaleReturnInWarehouse]', RESEED, 1)
GO


-- ----------------------------
-- Primary Key structure for table w_SaleReturnInWarehouse
-- ----------------------------
ALTER TABLE [dbo].[w_SaleReturnInWarehouse] ADD CONSTRAINT [PK__w_SaleRe__3214EC27CF8C98FE] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_SettlementAccount
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_SettlementAccount]', RESEED, 3)
GO


-- ----------------------------
-- Primary Key structure for table w_SettlementAccount
-- ----------------------------
ALTER TABLE [dbo].[w_SettlementAccount] ADD CONSTRAINT [PK__w_Settle__3214EC27E9E8C949] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Supplier
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Supplier]', RESEED, 3)
GO


-- ----------------------------
-- Primary Key structure for table w_Supplier
-- ----------------------------
ALTER TABLE [dbo].[w_Supplier] ADD CONSTRAINT [PK__w_Suppli__3214EC27E84A5418] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for w_Warehouse
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[w_Warehouse]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table w_Warehouse
-- ----------------------------
ALTER TABLE [dbo].[w_Warehouse] ADD CONSTRAINT [PK__w_Wareho__3214EC27CA6E2E4E] PRIMARY KEY CLUSTERED ([ID])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

