using System;
using System.Linq;

namespace ServiceFramework
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(String[] msg)
        {
            var position = msg.DetectAction();
            Order = position.Item1;
            Arguments = position.Item2;
        }

        public String[] Arguments { get; private set; }

        public Order Order { get; private set; }
    }

    public enum Order
    {
        start = 0,
        stop = 1,
        restart = 2,
        terminate = 3,
        noaction = 4,
        test = -1
    }

    public static class BasicServiceOrder
    {
        private static String[] orderBase = new[] { "-OpSrv", "" };

        public static String Flag { get { return orderBase[0]; } set { orderBase[0] = value; } }

        public static String[] Stop { get { return orderBase.Copy(Order.stop); } }
        public static String[] ReStart { get { return orderBase.Copy(Order.restart); } }
        public static String[] Start { get { return orderBase.Copy(Order.start); } }
        public static String[] NoAction { get { return orderBase.Copy(Order.noaction); } }
        public static String[] Test { get { return orderBase.Copy(Order.test); } }
        public static String[] Terminate { get { return orderBase.Copy(Order.terminate); } }

        private static String[] Copy(this String[] strArray, Order order)
        {
            strArray[1] = order.ToString();
            return strArray.Clone() as String[];
        }

        public static Tuple<Order, String[]> DetectAction(this String[] strArray)
        {
            try
            {
                if (strArray[0] == Flag)
                {
                    return new Tuple<Order, String[]>(strArray[1].ToEnum<Order>(), strArray.Skip(2).ToArray());
                }
                else
                {
                    return new Tuple<Order, String[]>(strArray[0].ToEnum<Order>(), strArray.Skip(1).ToArray());
                }
            }
            catch (Exception ex)
            {
                return new Tuple<Order, string[]>(Order.noaction, strArray);
            }
        }

        public static T ToEnum<T>(this String str)
        {
            return (T)Enum.Parse(typeof(T), str.ToLower());
        }
    }
}
