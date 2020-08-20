using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMq.Core.Core
{
    public class MessageModel
    {
        /// <summary>
        /// 消息来源
        /// </summary>
        public string MsgSource { get; set; }

        /// <summary>
        /// 消息推送时间
        /// </summary>

        public DateTime PushTime
        {
            get => DateTime.Now;
        }
    }
}
