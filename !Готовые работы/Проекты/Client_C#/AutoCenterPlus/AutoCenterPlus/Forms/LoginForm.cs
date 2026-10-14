using System;
using System.Configuration;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using AutoCenterPlus.Services;
using AutoCenterPlus.Utils;

namespace AutoCenterPlus.Forms
{
    internal sealed class LoginForm : Form
    {
        private readonly TextBox login = new TextBox();
        private readonly TextBox password = new TextBox();

        public LoginForm()
        {
            Text = "Автоцентр «Плюс» — вход";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(380, 190);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 4 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Логин", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(login, 1, 0);
            layout.Controls.Add(new Label { Text = "Пароль", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            password.UseSystemPasswordChar = true;
            layout.Controls.Add(password, 1, 1);
            var button = new Button { Text = "Войти", AutoSize = true, Anchor = AnchorStyles.Right };
            button.Click += SignIn;
            layout.Controls.Add(button, 1, 2);
            layout.Controls.Add(new Label { Text = "Регистрация", AutoSize = true, ForeColor = Color.DimGray }, 0, 3);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 3), 2);
            Controls.Add(layout);
            AcceptButton = button;
        }

        private async void SignIn(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(login.Text) || string.IsNullOrEmpty(password.Text))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var enteredLogin = login.Text;
                AutoCenterPlus.Services.AuthenticatedUser user;
                try { user = await ApiClient.LoginAsync(enteredLogin, password.Text); }
                finally { password.Clear(); }
                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Hide();
                using (var main = new MainForm(user)) main.ShowDialog();
                Close();
            }
            catch (Exception exception)
            {
                Logger.Error("Вход в систему", exception);
                var message = exception is ApiRequestException || exception is HttpRequestException || exception is ConfigurationErrorsException
                    ? exception.Message
                    : "Не удалось выполнить вход. Подробности записаны в журнал приложения.";
                MessageBox.Show(message, "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
