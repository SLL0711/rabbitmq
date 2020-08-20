using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Core.Core
{
    public class ConfigModel
    {
        public List<ConfigSection> Queue { get; set; }
    }

    public class ConfigSection
    {
        /// <summary>
        /// 类 fullname
        /// </summary>
        public string ClassName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string connectionName { get; set; }
    }
}
