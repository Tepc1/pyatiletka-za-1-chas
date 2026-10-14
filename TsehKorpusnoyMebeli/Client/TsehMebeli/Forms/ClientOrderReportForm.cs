using System;
using System.Drawing;
using System.Windows.Forms;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms
{
    internal sealed class ClientOrderReportForm : Form
    {
        private readonly DataGridView grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        private readonly string role;

        public ClientOrderReportForm(string role)
        {
            this.role = role;
            Text = "Анализ заказов клиентов";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1080, 600);
            UiTheme.ApplyForm(this);
            UiTheme.StyleGrid(grid);

            Controls.Add(UiTheme.CreateHeader("Анализ заказов", "Сводка по клиентам, изделиям и скидкам"));

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = UiTheme.Background
            };
            var btnRefresh = UiTheme.CreatePrimaryButton("Обновить", 130, 36);
            btnRefresh.Click += async (s, e) => await LoadData();
            panel.Controls.Add(btnRefresh);

            var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16), BackColor = UiTheme.Background };
            gridHost.Controls.Add(grid);

            Controls.Add(gridHost);
            Controls.Add(panel);
            Load += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var list = await ApiClient.GetAnalysisAsync(null);
                grid.DataSource = list;
            }
            catch (Exception ex)
            {
                Logger.Error("Анализ заказов", ex);
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
