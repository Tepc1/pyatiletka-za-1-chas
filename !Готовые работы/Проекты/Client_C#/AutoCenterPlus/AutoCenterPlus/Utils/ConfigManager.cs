using System;
using System.Configuration;
using System.Globalization;
using System.Threading;

namespace AutoCenterPlus.Utils
{
    internal static class ConfigManager
    {
        public static void ApplyCulture()
        {
            var language = ConfigurationManager.AppSettings["InterfaceLanguage"];
            if (string.IsNullOrWhiteSpace(language)) return;
            CultureInfo culture;
            try { culture = CultureInfo.GetCultureInfo(language); }
            catch (CultureNotFoundException exception) { throw new ConfigurationErrorsException("Недопустимое значение InterfaceLanguage.", exception); }
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
