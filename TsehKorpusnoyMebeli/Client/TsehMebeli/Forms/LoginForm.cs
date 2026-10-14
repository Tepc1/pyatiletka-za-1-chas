using System;
using System.Configuration;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms
{
    internal sealed class LoginForm : Form
    {
        private readonly TextBox login = new TextBox();
        private readonly TextBox password = new TextBox();
        private readonly Button button;

        public LoginForm()
        {
            Text = "Цех корпусной мебели — вход";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ClientSize = new Size(440, 360);
            UiTheme.ApplyForm(this);

            Controls.Add(UiTheme.CreateHeader("Цех корпусной мебели", "Информационная система · вход в учётную запись"));

            var card = new Panel
            {
                BackColor = UiTheme.Surface,
                Location = new Point(40, 110),
                Size = new Size(360, 220),
                Padding = new Padding(24)
            };
            // лёгкая «карточка»
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lblLogin = new Label { Text = "Логин", Location = new Point(24, 20), AutoSize = true, ForeColor = UiTheme.TextMuted };
            login.Location = new Point(24, 42);
            login.Width = 310;
            UiTheme.StyleTextBox(login);

            var lblPass = new Label { Text = "Пароль", Location = new Point(24, 80), AutoSize = true, ForeColor = UiTheme.TextMuted };
            password.Location = new Point(24, 102);
            password.Width = 310;
            password.UseSystemPasswordChar = true;
            UiTheme.StyleTextBox(password);

            button = UiTheme.CreatePrimaryButton("Войти", 310, 42);
            button.Location = new Point(24, 148);
            button.Click += SignIn;

            var hint = UiTheme.CreateMutedLabel("Учётка для проверки: admin / Admin123");
            hint.Location = new Point(24, 198);

            card.Controls.AddRange(new Control[] { lblLogin, login, lblPass, password, button, hint });
            Controls.Add(card);
            AcceptButton = button;
        }

        private async void SignIn(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(login.Text) || string.IsNullOrEmpty(password.Text))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            button.Enabled = false;
            button.Text = "Вход…";
            try
            {
                var enteredLogin = login.Text;
                AuthenticatedUser user;
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
            finally
            {
                button.Enabled = true;
                button.Text = "Войти";
            }
        }
    }
}
