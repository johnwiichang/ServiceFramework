using System;

namespace ServiceFramework
{
    public static class Utils
    {
        public static Platform DetectPlatform()
        {
            Platform platform = Platform.Windows;
            var OS = Environment.GetEnvironmentVariable("OS") ?? ((Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "").Contains("Apple") ? "macOS" : "Linux");
            platform = OS.Contains("Windows") ? Platform.Windows :
                         OS == "macOS" ? Platform.macOS :
                         OS == "Linux" ? Platform.Linux :
                         Platform.Others;
            return platform;
        }

        /// <summary>
        /// 肮脏的写法，调用此方法将会忽略在调用过程中产生的一切错误。
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        internal static String InvokeSomethingWithoutWatching(Action act)
        {
            try
            {
                act();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    public enum Platform
    {
        Windows = 1,
        macOS = 2,
        Linux = 3,
        Others = 10
    }
}
