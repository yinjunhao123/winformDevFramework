using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Collections.Specialized.BitVector32;

namespace WinformDevFramework.Core.Configuration
{
    /// <summary>
    /// 获取Appsettings配置信息
    /// </summary>
    public class AppSettingsHelper
    {
        static IConfiguration Configuration { get; set; }

        public AppSettingsHelper(string contentPath)
        {
           
        }

        /// <summary>
        /// 封装要操作的字符
        /// AppSettingsHelper.GetContent(new string[] { "JwtConfig", "SecretKey" });
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string GetContent(params string[] sections)
        {
            try
            {
                string contentPath = AppDomain.CurrentDomain.BaseDirectory;
                string Path = "appsettings.json";
                Configuration = new ConfigurationBuilder().SetBasePath(contentPath).Add(new JsonConfigurationSource { Path = Path, Optional = false, ReloadOnChange = true }).Build();
                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";
        }
        public static bool SetContent(string value, params string[] sections)
        {
            try
            {
                string contentPath = AppDomain.CurrentDomain.BaseDirectory;
                string Path = contentPath+"appsettings.json";
                JObject jsonObject;
                using (StreamReader file = new StreamReader(Path))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    jsonObject = (JObject)JToken.ReadFrom(reader);

                    jsonObject[sections[0]][sections[1]]= value;
                }

                using (var writer = new StreamWriter(Path))
                using (JsonTextWriter jsonwriter = new JsonTextWriter(writer))
                {
                    jsonObject.WriteTo(jsonwriter);
                }
            }
            catch (Exception)
            {
                return false;

            }

            return true;
        }

    }
}
