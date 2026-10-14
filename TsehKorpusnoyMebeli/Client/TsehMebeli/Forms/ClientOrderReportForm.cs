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
            Size = new Size(1000, 550);
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            var btnRefresh = new Button { Text = "Обновить", Width = 100 };
            btnRefresh.Click += async (s, e) => await LoadData();
            panel.Controls.Add(btnRefresh);
            Controls.Add(grid);
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
