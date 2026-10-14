using System;
using System.Configuration;
using System.Windows.Forms;
using TsehMebeli.Forms;
using TsehMebeli.Utils;

namespace TsehMebeli
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                ConfigManager.ApplyCulture();
                Logger.ValidateConfiguration();
            }
            catch (ConfigurationErrorsException exception)
            {
                MessageBox.Show(exception.Message, "Ошибка конфигурации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
