using System.Configuration;
using TsehMebeli.Forms;
using TsehMebeli.Utils;

namespace TsehMebeli;

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

        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }
}
