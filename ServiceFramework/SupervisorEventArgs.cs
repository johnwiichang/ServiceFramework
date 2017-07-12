using System;

namespace ServiceFramework
{
    public class SupervisorEventArgs
    {
        public Order Action { get; set; }

        public String Message { get; set; }

        public static SupervisorEventArgs Reply(String msg)
        {
            return new SupervisorEventArgs { Action = Order.noaction, Message = msg };
        }

        public static SupervisorEventArgs Answer(String msg)
        {
            return new SupervisorEventArgs { Action = Order.terminate, Message = msg };
        }
    }
}
