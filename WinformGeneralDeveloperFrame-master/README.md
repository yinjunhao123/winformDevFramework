# WinformDevFramework

#### 介绍
winform通用开发框架是一个简单实用的二次开发框架。内置完整的权限架构，包括：菜单、角色、用户、字典、日志、代码生成等一系列系统常规模块。为了一般管理系统避免重复造轮子，不需要在关注权限 页面等，新增功能只需要新增form界面并完成自己的业务，在系统配置即可。

#### WINFORM原生控件版本
![输入图片说明](https://foruda.gitee.com/images/1692459908033654123/1ebf5632_2337895.png "屏幕截图")

#### WINFORM原生控件版本架构
数据库：Sqlserver

ORM框架：Sqlsugar

UI框架：无-原生winform控件

业务代码生成：支持

Form代码生成：不支持

Controller代码生成：不支持

自动更新：支持

软件架构：单机

权限管控：支持

API访问日志：不支持

在线用户管理：不支持

数据分页：不支持

通用查询：不支持

#### 关注公众号获取源码和C#相关学习资料
![输入图片说明](%E6%95%B0%E6%8D%AE%E5%BA%93%E8%84%9A%E6%9C%AC/qrcode_for_gh_4291c495bcc6_258.jpg)

#### Dev前后端分离版本
![输入图片说明](WinformDevFramework/Resources/image.png)

#### Dev前后端分离版本架构
数据库：Mysql

ORM框架：Sqlsugar

UI框架：Devexpress

业务代码生成：支持

Form代码生成：支持单表和主从表

Controller代码生成：支持

自动更新：支持

软件架构：前后端分离

权限管控：支持

API访问日志：支持

在线用户管理：支持

数据分页：支持

通用查询：支持

持续更新：支持

#### Dev前后端分离版本 试用
https://bosswang-1255687113.cos.ap-shanghai.myqcloud.com/winformuPDATE/%E6%A1%86%E6%9E%B6%E5%AE%A2%E6%88%B7%E7%AB%AF.zip



#### 个人主页  http://bosswang.site/
#### 进销存管理客户端源码 https://gitee.com/wkjerry_admin/jxc
#### QQ交流群 228243401  大家可以加我微信15029367414 拉大家进微信交流群
#### WINFORM原生控件版本功能说明

    登陆页面
![输入图片说明](https://foruda.gitee.com/images/1692462828029666251/d1635ac8_2337895.png "屏幕截图")

    自动更新
![输入图片说明](https://foruda.gitee.com/images/1692462850743171911/6d908038_2337895.png "屏幕截图")

    主页  可以用作报表展示
    菜单设置 
![输入图片说明](https://foruda.gitee.com/images/1692462113535017206/fef5d0dc_2337895.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1692462124858049899/e77f9ff1_2337895.png "屏幕截图")

    用户设置
![输入图片说明](https://foruda.gitee.com/images/1692462160173025024/d42be1c1_2337895.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1692462201395066393/072eb794_2337895.png "屏幕截图")

    角色管理
![输入图片说明](https://foruda.gitee.com/images/1692462237065379526/1ed93973_2337895.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1692462254426965953/73cf27e3_2337895.png "屏幕截图")

    数据源维护 （用于代码生成）
![输入图片说明](https://foruda.gitee.com/images/1692462278471666377/0df575e2_2337895.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1692462280863206777/6dcd39a5_2337895.png "屏幕截图")

    生成代码  生成Repository Services Entity
![输入图片说明](https://foruda.gitee.com/images/1692462322922061684/b00cfe9c_2337895.png "屏幕截图") 

    消息通知
![输入图片说明](https://foruda.gitee.com/images/1692462453384709380/cfd3e653_2337895.png "屏幕截图")

    字典类型
![输入图片说明](https://foruda.gitee.com/images/1692462552360872843/8839aacf_2337895.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1692462580076264279/cc711264_2337895.png "屏幕截图")

    字典内容
![输入图片说明](https://foruda.gitee.com/images/1692462609571316481/864dca1a_2337895.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1692462626255361563/1b029fa4_2337895.png "屏幕截图")

#### 安装教程

1.  先还原下数据库  怎么还原可以自行百度
![输入图片说明](https://foruda.gitee.com/images/1692462906323072310/ec2a2670_2337895.png "屏幕截图")
2.  运行程序时可能会报连接服务器异常（能连上外网的应该不会报），这是自动更新的原因 可以去配置文件将自动更新改为false。也可将更新服务器部署在本机，相关文件在项目可以找到![输入图片说明](https://foruda.gitee.com/images/1692463801206567902/ad5b4fff_2337895.png "屏幕截图")
，部署可以百度 autoupdater.net部署教程


