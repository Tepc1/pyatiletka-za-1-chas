using System;
using System.Configuration;
using System.IO;

namespace TsehMebeli.Utils
{
    internal static class Logger
    {
        private static readonly object Sync = new object();

        public static void ValidateConfiguration()
        {
            var path = ConfigManager.GetLogPath();
            var dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static void Error(string context, Exception exception)
        {
            try
            {
                var path = ConfigManager.GetLogPath();
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + context + "] " +
                           exception.GetType().Name + ": " + exception.Message + Environment.NewLine +
                           exception.StackTrace + Environment.NewLine;
                lock (Sync)
                    File.AppendAllText(path, line);
            }
            catch
            {
            }
        }

        public static void Info(string message)
        {
            try
            {
                var path = ConfigManager.GetLogPath();
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [INFO] " + message + Environment.NewLine;
                lock (Sync)
                    File.AppendAllText(path, line);
            }
            catch
            {
            }
        }
    }
}
