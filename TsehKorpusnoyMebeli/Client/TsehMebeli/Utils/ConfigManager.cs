using System.Configuration;
using System.Globalization;
using System.Threading;

namespace TsehMebeli.Utils
{
    internal static class ConfigManager
    {
        public static void ApplyCulture()
        {
            var culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static string GetApiBaseUrl()
        {
            var url = ConfigurationManager.AppSettings["ApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(url))
                throw new ConfigurationErrorsException("В App.config не задан ApiBaseUrl.");
            return url.Trim();
        }

        public static string GetLogPath()
        {
            return ConfigurationManager.AppSettings["LogPath"] ?? "logs\\tsehmebeli.log";
        }
    }
}
