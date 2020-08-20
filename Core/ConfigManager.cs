using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RabbitMq.Core.Core
{
    /// <summary>
    /// 用于读取配置节点
    /// </summary>
    public class ConfigManager
    {
        private static string filename = "appsetting.json";
        private static IConfiguration _config;

        static ConfigManager()
        {
            var basePath = System.Environment.CurrentDirectory;
            string path = Path.Combine(basePath, filename);

            var builder = new ConfigurationBuilder()
                .AddJsonFile(path);

            _config = builder.Build();
        }

        public static ConfigModel LoadItem()
        {
            var c = _config.Get<ConfigModel>();

            return c;
        }
    }
}
