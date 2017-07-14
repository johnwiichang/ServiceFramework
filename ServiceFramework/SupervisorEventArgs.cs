using System;

namespace ServiceFramework
{
    public class SupervisorEventArgs
    {
        /// <summary>
        /// 操作
        /// </summary>
        public Order Action { get; set; }

        /// <summary>
        /// 消息内容。
        /// </summary>
        public String Message { get; set; }
    }

    public static class SupervisorEventArgsExtender
    {
        /// <summary>
        /// 转换为回复给客户端的信息包并保持连接。
        /// </summary>
        /// <returns>数据包</returns>
        /// <param name="msg">Message.</param>
        public static SupervisorEventArgs ToSupervisorReply(this String msg)
        {
            return new SupervisorEventArgs { Action = Order.noaction, Message = msg };
        }

        /// <summary>
        /// 转换为回复给客户端的信息包并关闭连接。
        /// </summary>
        /// <returns>数据包</returns>
        /// <param name="msg">Message.</param>
        public static SupervisorEventArgs ToSupervisorAnswer(this String msg)
        {
            return new SupervisorEventArgs { Action = Order.terminate, Message = msg };
        }
    }
}
