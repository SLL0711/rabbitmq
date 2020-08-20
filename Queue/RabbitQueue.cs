using RabbitMq.Core.Core;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading;
using Lib;
using Lib.Log;

namespace RabbitMq.Core.Queue
{
    public class RabbitQueue<T> : IMessageQueue<T>
    {
        private MessageHandler<T> messageHandler;

        private ConnectionFactory connectionFactory;
        private IConnection conn;
        private IModel channel;
        private ConfigSection config;
        public string queueName { get; set; }

        public RabbitQueue(ConfigSection configSection)
        {
            config = configSection ?? null;
        }

        public event MessageHandler<T> OnMessageReceive
        {
            add
            {
                if (messageHandler == null)
                {
                    messageHandler += value;
                }

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += Consumer_Received;

                channel.BasicConsume(this.queueName, false, consumer);
            }
            remove
            {
                if (messageHandler != null)
                {
                    messageHandler -= value;
                }
            }
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            MessageModel m = null;
            try
            {
                var msg = Encoding.UTF8.GetString(e.Body.ToArray());

                if (string.IsNullOrWhiteSpace(msg))
                {
                    return;
                }

                if (this.messageHandler != null)
                {
                    var message = JsonConvert.DeserializeObject<T>(msg);

                    m = message as MessageModel;

                    this.messageHandler(message);

                    //消息接收成功主动应答
                    channel.BasicAck(e.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                //TODO:记录异常日志
                LogHelper.Error($"异常来源：{m?.MsgSource},Message:{ex.Message}");

                //休息5s 等待重发
                Thread.Sleep(5 * 1000);

                //消息应答失败 重新扔回queue 执行消息重发
                channel.BasicNack(e.DeliveryTag, false, true);
            }
        }

        /// <summary>
        /// 队列初始化
        /// </summary>
        public void Init()
        {
            connectionFactory = new ConnectionFactory();
            connectionFactory.UserName = config.UserName;
            connectionFactory.Password = config.Password;
            connectionFactory.HostName = config.HostName;
            connectionFactory.Port = config.Port;
            connectionFactory.ClientProvidedName = config.connectionName;

            connectionFactory.AutomaticRecoveryEnabled = true;//连接自动修复
            connectionFactory.TopologyRecoveryEnabled = true;

            connectionFactory.RequestedHeartbeat = new TimeSpan(0, 1, 0);//设置rabbit心跳机制超时时间
            connectionFactory.RequestedConnectionTimeout = new TimeSpan(0, 0, 15);//设置连接超时时间

            conn = connectionFactory.CreateConnection();

            channel = conn.CreateModel();

            channel.BasicQos(0, 1, false);

            channel.QueueDeclare(this.queueName, true, false, false, null);

            //当channel销毁时做处理
            channel.ModelShutdown += (object sender, ShutdownEventArgs e) =>
            {

            };
        }

        public void ConfirmMessage()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SendMessage(T msg)
        {
            if (msg == null)
            {
                return;
            }

            IBasicProperties prop = channel.CreateBasicProperties();
            prop.DeliveryMode = 2;
            prop.ContentType = "application/json";

            var jsonStr = JsonConvert.SerializeObject(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonStr);

            channel.BasicPublish("", queueName, prop, bytes);
        }
    }
}
