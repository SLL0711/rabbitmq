using RabbitMq.Core.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace RabbitMq.Core.Core
{
    public class MessageManager<T> where T : MessageModel
    {
        private static Dictionary<string, IMessageQueue<T>> queueList = new Dictionary<string, IMessageQueue<T>>();
        private static ConfigModel _config;

        /// <summary>
        /// 获取消息队列实例 并建立远程连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queuename"></param>
        /// <returns></returns>
        public static IMessageQueue<T> CreateMessageQueue(string queuename)
        {
            return CreateMessageQueue(queuename, null);
        }

        public static IMessageQueue<T> CreateMessageQueue(string queuename, ConfigModel config)
        {
            lock (queueList)
            {
                if (queueList.ContainsKey(queuename))
                {
                    return queueList[queuename];
                }

                if (config == null)
                {
                    if (_config == null)
                    {
                        //加载配置文件
                        _config = ConfigManager.LoadItem();
                    }

                    config = _config;
                }

                if (config == null || config.Queue == null || config.Queue.Count < 1)
                {
                    throw new Exception("config queue is not exsit!");
                }

                foreach (var queueConfig in config.Queue)
                {
                    if (string.IsNullOrWhiteSpace(queueConfig.ClassName))
                    {
                        throw new Exception("config queue classname is null!");
                    }

                    try
                    {
                        Type type = Assembly.GetCallingAssembly().GetType(queueConfig.ClassName + "`1");

                        //Type type = assembly.GetType(queueConfig.ClassName, true, true);

                        type = type.MakeGenericType(typeof(T));

                        var instance = System.Activator.CreateInstance(type, queueConfig);

                        if (!(instance is IMessageQueue<T>))
                        {
                            throw new Exception("queue must implement IMessageQueue");
                        }

                        var mq = (IMessageQueue<T>)instance;
                        mq.queueName = queuename;
                        mq.Init();

                        if (!queueList.ContainsKey(queuename))
                        {
                            queueList.Add(queuename, mq);
                        }
                    }
                    catch (Exception ex)
                    {
                        //初始化失败
                        return null;
                    }
                }

                return queueList[queuename];
            }
        }
    }
}
