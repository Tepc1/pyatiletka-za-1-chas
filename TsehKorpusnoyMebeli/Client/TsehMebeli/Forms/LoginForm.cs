using System.Configuration;
using System.Drawing.Drawing2D;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms;

internal sealed class LoginForm : Form
{
    private readonly TextBox login = new();
    private readonly TextBox password = new();
    private readonly Button button;

    public LoginForm()
    {
        Text = "Цех корпусной мебели — вход";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(480, 420);
        UiTheme.ApplyForm(this);

        var header = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = UiTheme.Header };
        header.Paint += (s, e) =>
        {
            UiTheme.DrawCabinetIcon(e.Graphics, new Rectangle(24, 22, 56, 56), Color.FromArgb(220, 184, 115));
            using var titleFont = new Font("Segoe UI Semibold", 16f, FontStyle.Bold);
            e.Graphics.DrawString("Цех корпусной мебели", titleFont, Brushes.White, 96, 28);
            using var subFont = new Font("Segoe UI", 9.5f);
            e.Graphics.DrawString("Информационная система  ·  вход", subFont,
                new SolidBrush(Color.FromArgb(200, 188, 174)), 96, 60);
        };
        Controls.Add(header);

        var card = new Panel
        {
            BackColor = UiTheme.Surface,
            Location = new Point(50, 130),
            Size = new Size(380, 250)
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var lblLogin = new Label { Text = "Логин", Location = new Point(28, 24), AutoSize = true, ForeColor = UiTheme.TextMuted };
        login.Location = new Point(28, 46);
        login.Width = 320;
        UiTheme.StyleTextBox(login);

        var lblPass = new Label { Text = "Пароль", Location = new Point(28, 88), AutoSize = true, ForeColor = UiTheme.TextMuted };
        password.Location = new Point(28, 110);
        password.Width = 320;
        password.UseSystemPasswordChar = true;
        UiTheme.StyleTextBox(password);

        button = UiTheme.CreatePrimaryButton("Войти", 320, 42);
        button.Location = new Point(28, 160);
        button.Click += SignIn;

        var hint = new Label
        {
            Text = "Проверка: admin / Admin123",
            Location = new Point(28, 214),
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Font = UiTheme.SubtitleFont
        };

        card.Controls.AddRange([lblLogin, login, lblPass, password, button, hint]);
        Controls.Add(card);
        AcceptButton = button;
    }

    private async void SignIn(object? sender, EventArgs e)
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
            AuthenticatedUser? user;
            try { user = await ApiClient.LoginAsync(enteredLogin, password.Text); }
            finally { password.Clear(); }

            if (user is null)
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hide();
            using (var main = new MainForm(user))
                main.ShowDialog();
            Close();
        }
        catch (Exception exception)
        {
            Logger.Error("Вход в систему", exception);
            var message = exception is ApiRequestException or HttpRequestException or ConfigurationErrorsException
                ? exception.Message
                : "Не удалось выполнить вход. Подробности в журнале.";
            MessageBox.Show(message, "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            button.Enabled = true;
            button.Text = "Войти";
        }
    }
}
