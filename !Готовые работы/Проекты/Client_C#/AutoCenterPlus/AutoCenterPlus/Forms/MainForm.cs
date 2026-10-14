using System;
using System.Drawing;
using System.Windows.Forms;
using AutoCenterPlus.Services;
using AutoCenterPlus.Utils;

namespace AutoCenterPlus.Forms
{
    internal sealed class MainForm : Form
    {
        private readonly AuthenticatedUser user;
        private bool endingSession;
        public MainForm(AuthenticatedUser user)
        {
            this.user = user;
            Text = "Автоцентр «Плюс» — информационная система";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(640, 360);
            var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            layout.Controls.Add(new Label { Text = "Здравствуйте, " + user.FullName + " (" + user.Role + ")", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(4, 4, 4, 20) });
            if (CanManageCustomers(user.Role))
                AddButton(layout, "Справочник клиентов", delegate { using (var form = new CustomerForm()) form.ShowDialog(this); });
            if (CanCalculateOrders(user.Role))
                AddButton(layout, "Расчёт стоимости заказа", delegate { using (var form = new OrderForm()) form.ShowDialog(this); });
            if (CanViewAnalysis(user.Role))
                AddButton(layout, "Анализ заказов клиентов", delegate { OpenReport(); });
            Controls.Add(layout);
            FormClosing += CloseSession;
        }

        private static bool CanManageCustomers(string role)
        {
            return role == "Admin" || role == "Manager";
        }

        private static bool CanCalculateOrders(string role)
        {
            return role == "Admin" || role == "Manager" || role == "Operator";
        }

        private static bool CanViewAnalysis(string role)
        {
            return role == "Admin" || role == "Manager";
        }

        private static Button AddButton(Control parent, string text, Action action)
        {
            var button = new Button { Text = text, Width = 290, Height = 42, Margin = new Padding(4, 5, 4, 5) };
            button.Click += delegate { action(); };
            parent.Controls.Add(button);
            return button;
        }

        private void OpenReport()
        {
            if (user.Role != "Manager" && user.Role != "Admin")
            {
                MessageBox.Show("У вас недостаточно прав для просмотра данного отчета. Обратитесь к администратору", "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
