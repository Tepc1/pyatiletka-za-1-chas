using System;
using System.Drawing;
using System.Windows.Forms;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms
{
    internal sealed class MainForm : Form
    {
        private readonly AuthenticatedUser user;
        private bool endingSession;

        public MainForm(AuthenticatedUser user)
        {
            this.user = user;
            Text = "Цех корпусной мебели — информационная система";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            Size = new Size(780, 520);
            UiTheme.ApplyForm(this);

            Controls.Add(UiTheme.CreateHeader(
                "Цех корпусной мебели",
                "Здравствуйте, " + user.FullName + "  ·  роль: " + user.Role));

            var content = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32, 28, 32, 28),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = UiTheme.Background
            };

            content.Controls.Add(new Label
            {
                Text = "Главное меню",
                Font = new Font("Segoe UI Semibold", 12f),
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(4, 0, 4, 16)
            });

            if (CanManageCustomers(user.Role))
                AddMenuCard(content, "Справочник клиентов", "Добавление, изменение и удаление контрагентов",
                    delegate { using (var form = new CustomerForm()) form.ShowDialog(this); });

            if (CanCalculateOrders(user.Role))
                AddMenuCard(content, "Расчёт стоимости заказа", "Калькуляция по спецификации материалов и операций",
                    delegate { using (var form = new OrderForm()) form.ShowDialog(this); });

            if (CanViewAnalysis(user.Role))
                AddMenuCard(content, "Анализ заказов клиентов", "Сводка по изделиям, суммам и скидкам",
                    delegate { OpenReport(); });

            Controls.Add(content);
            FormClosing += CloseSession;
        }

        private static bool CanManageCustomers(string role) => role == "Admin" || role == "Manager";
        private static bool CanCalculateOrders(string role) => role == "Admin" || role == "Manager" || role == "Operator";
        private static bool CanViewAnalysis(string role) => role == "Admin" || role == "Manager";

        private static void AddMenuCard(Control parent, string title, string description, Action action)
        {
            var card = new Panel
            {
                Width = 680,
                Height = 88,
                BackColor = UiTheme.Surface,
                Margin = new Padding(4, 0, 4, 12),
                Cursor = Cursors.Hand
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var brush = new SolidBrush(UiTheme.Accent))
                    e.Graphics.FillRectangle(brush, 0, 0, 5, card.Height);
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 11.5f),
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 18)
            };
            var descLabel = new Label
            {
                Text = description,
                Font = UiTheme.SubtitleFont,
                ForeColor = UiTheme.TextMuted,
                AutoSize = true,
                Location = new Point(24, 48)
            };

            Action open = action;
            card.Click += (s, e) => open();
            titleLabel.Click += (s, e) => open();
            descLabel.Click += (s, e) => open();
            card.MouseEnter += (s, e) => card.BackColor = UiTheme.AccentSoft;
            card.MouseLeave += (s, e) => card.BackColor = UiTheme.Surface;

            card.Controls.Add(titleLabel);
            card.Controls.Add(descLabel);
            parent.Controls.Add(card);
        }

        private void OpenReport()
        {
            if (user.Role != "Manager" && user.Role != "Admin")
            {
                MessageBox.Show("У вас недостаточно прав для просмотра данного отчета. Обратитесь к администратору",
                    "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var form = new ClientOrderReportForm(user.Role)) form.ShowDialog(this);
        }

        private async void CloseSession(object sender, FormClosingEventArgs e)
        {
            if (endingSession) return;
            e.Cancel = true;
            Enabled = false;
            try { await ApiClient.LogoutAsync(); }
            catch (Exception exception) { Logger.Error("Завершение сеанса API", exception); }
            finally
            {
                endingSession = true;
                Close();
            }
        }
    }
}
