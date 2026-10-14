using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoCenterPlus.Services;
using AutoCenterPlus.Utils;

namespace AutoCenterPlus.Forms
{
    internal sealed class ClientOrderReportForm : Form
    {
        private readonly string role;
        private readonly ComboBox customer = new ComboBox();
        private readonly DataGridView grid = new DataGridView();

        public ClientOrderReportForm(string role)
        {
            this.role = role;
            Text = "Анализ заказов клиентов";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 620);
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10), WrapContents = false };
            top.Controls.Add(new Label { Text = "Клиент:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 7, 0, 0) });
            customer.DropDownStyle = ComboBoxStyle.DropDownList;
            customer.Width = 320;
            top.Controls.Add(customer);
            var generate = new Button { Text = "Сформировать", AutoSize = true };
            generate.Click += async delegate { await LoadReport(); };
            top.Controls.Add(generate);
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Columns.Add(CreateColumn("ClientName", "Клиент"));
            grid.Columns.Add(CreateColumn("ProductName", "Товары (работы, услуги)"));
            grid.Columns.Add(CreateColumn("ProductCode", "Код"));
            grid.Columns.Add(CreateColumn("Quantity", "Кол-во", "N2"));
            grid.Columns.Add(CreateColumn("Unit", "Ед."));
            grid.Columns.Add(CreateColumn("Price", "Цена", "N2"));
            grid.Columns.Add(CreateColumn("Discount", "Скидка", "N2"));
            grid.Columns.Add(CreateColumn("TotalSum", "Сумма", "N2"));
            grid.CellFormatting += FormatDiscount;
            Controls.Add(grid);
            Controls.Add(top);
            Load += async delegate { await LoadCustomers(); };
        }

        private static DataGridViewTextBoxColumn CreateColumn(string property, string title, string format = null)
        {
            var column = new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = title, Name = property };
            if (format != null) column.DefaultCellStyle.Format = format;
            return column;
        }

        private async Task LoadCustomers()
        {
            try
            {
                var options = new List<CustomerOption> { new CustomerOption { Id = 0, Name = "Все клиенты" } };
                foreach (var item in await ApiClient.GetCustomersAsync())
                    options.Add(new CustomerOption { Id = item.Id, Name = item.Name });
                customer.DataSource = options;
                customer.DisplayMember = "Name";
                customer.ValueMember = "Id";
                customer.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                Logger.Error("Загрузка клиентов для отчёта", exception);
                MessageBox.Show("Не удалось загрузить список клиентов. Проверьте доступность API.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadReport()
        {
            if (role != "Manager" && role != "Admin")
            {
                MessageBox.Show("У вас недостаточно прав для просмотра данного отчета. Обратитесь к администратору", "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selected = customer.SelectedValue;
                var customerId = selected == null ? (int?)null : Convert.ToInt32(selected) == 0 ? (int?)null : Convert.ToInt32(selected);
                var source = await ApiClient.GetAnalysisAsync(customerId);
                var rows = new List<ReportGridRow>();
                decimal discountTotal = 0m;
                decimal amountTotal = 0m;
                foreach (var row in source)
                {
                    discountTotal += row.Discount;
                    amountTotal += row.TotalSum;
                    rows.Add(new ReportGridRow
                    {
                        ClientName = row.ClientName,
                        ProductName = row.ProductName,
                        ProductCode = row.ProductCode,
                        Quantity = row.Quantity,
                        Unit = row.Unit,
                        Price = row.Price,
                        Discount = row.Discount,
                        TotalSum = row.TotalSum,
                        DiscountPercent = row.DiscountPercent
                    });
                }
                if (rows.Count == 0)
                {
                    grid.DataSource = rows;
                    MessageBox.Show("Данные за выбранный период или по выбранному клиенту отсутствуют", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                rows.Add(new ReportGridRow
                {
                    ClientName = "ИТОГО",
                    Discount = discountTotal,
                    TotalSum = amountTotal,
                    IsTotal = true
                });
                grid.DataSource = rows;
                grid.Refresh();
            }
            catch (Exception exception)
            {
                Logger.Error("Формирование отчёта по заказам клиентов", exception);
                MessageBox.Show("Не удалось сформировать отчёт. Проверьте полномочия, доступность API и журнал приложения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDiscount(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = grid.Rows[e.RowIndex].DataBoundItem as ReportGridRow;
            if (row == null) return;
            if (row.IsTotal)
            {
                grid.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                return;
            }
            if (grid.Columns[e.ColumnIndex].Name == "Discount" && row.DiscountPercent > 20m)
                e.CellStyle.BackColor = Color.LightGreen;
        }

        private sealed class CustomerOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private sealed class ReportGridRow
        {
            public string ClientName { get; set; }
            public string ProductName { get; set; }
            public string ProductCode { get; set; }
            public decimal? Quantity { get; set; }
            public string Unit { get; set; }
            public decimal? Price { get; set; }
            public decimal? Discount { get; set; }
            public decimal? TotalSum { get; set; }
            public decimal DiscountPercent { get; set; }
            public bool IsTotal { get; set; }
        }
    }
}
