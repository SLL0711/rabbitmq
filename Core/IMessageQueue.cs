using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Core.Core
{
    public delegate void MessageHandler<T>(T message);

    public interface IMessageQueue<T>
    {
        event MessageHandler<T> OnMessageReceive;
        void Init();
        string queueName { get; set; }
        void SendMessage(T msg);
        void ConfirmMessage();
    }
}
