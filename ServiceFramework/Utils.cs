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
    }

    public enum Platform
    {
        Windows = 1,
        macOS = 2,
        Linux = 3,
        Others = 10
    }
}
