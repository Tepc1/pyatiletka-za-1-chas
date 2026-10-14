using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace AutoCenterPlus.Utils
{
    internal static class Logger
    {
        public static void ValidateConfiguration() { GetMinimumSeverity(); }

        public static void Information(string operation, string details)
        {
            Write(1, "INFO", operation, details);
        }

        public static void Error(string operation, Exception exception)
        {
            Write(3, "ERROR", operation, exception.ToString());
        }

        private static void Write(int severity, string level, string operation, string details)
        {
            if (severity < GetMinimumSeverity()) return;
            var line = string.Format("{0:O} {1} [{2}] {3}{4}", DateTime.UtcNow, level, operation, details, Environment.NewLine);
            try
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoCenterPlus", "logs");
                Directory.CreateDirectory(directory);
                File.AppendAllText(Path.Combine(directory, "app.log"), line);
            }
            catch (IOException logError) { Trace.TraceError("Не удалось записать журнал: {0}", logError); }
            catch (UnauthorizedAccessException logError) { Trace.TraceError("Нет доступа к журналу: {0}", logError); }
        }

        private static int GetMinimumSeverity()
        {
            var value = ConfigurationManager.AppSettings["LogLevel"];
            if (string.IsNullOrWhiteSpace(value)) return 1;
            switch (value.Trim().ToLowerInvariant())
            {
                case "verbose":
                case "debug": return 0;
                case "information":
                case "info": return 1;
                case "warning":
                case "warn": return 2;
                case "error": return 3;
                case "off": return 4;
                default: throw new ConfigurationErrorsException("Недопустимое значение LogLevel.");
            }
        }
    }
}

