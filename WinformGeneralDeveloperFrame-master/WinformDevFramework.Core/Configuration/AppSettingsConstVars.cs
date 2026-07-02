using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Core.Configuration
{
    /// <summary>
    /// 配置文件格式化
    /// </summary>
    public static class AppSettingsConstVars
    {
        #region 数据库================================================================================
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static readonly string DbSqlConnection = AppSettingsHelper.GetContent("ConnectionStrings", "SqlConnection");
        /// <summary>
        /// 获取数据库类型
        /// </summary>
        public static readonly string DbDbType = AppSettingsHelper.GetContent("ConnectionStrings", "DbType");

        ///// <summary>
        ///// 自动更新开关
        ///// </summary>
        //public static readonly bool AtuoUpdate =
        //    bool.Parse(AppSettingsHelper.GetContent("Update", "AtuoUpdate").ToLower().Equals("true")
        //        ? "true"
        //        : "false");

        ///// <summary>
        ///// 版本号
        ///// </summary>
        //public static readonly string Version = AppSettingsHelper.GetContent("Update", "Version");

        ///// <summary>
        ///// 更新服务器地址
        ///// </summary>

        //public static readonly string Url = AppSettingsHelper.GetContent("Update", "Url");

        /// <summary>
        /// 跳动IP
        /// </summary>
        public static readonly string BounceIp = AppSettingsHelper.GetContent("Bounce", "BounceIp");

        /// <summary>
        /// 跳动端口
        /// </summary>
        public static readonly int BouncePort = Convert.ToInt32(AppSettingsHelper.GetContent("Bounce", "BouncePort"));

        #endregion
    }
}
