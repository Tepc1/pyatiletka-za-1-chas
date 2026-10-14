using System;
using System.Drawing;
using System.Windows.Forms;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms
{
    internal sealed class CustomerForm : Form
    {
        private readonly DataGridView grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        public CustomerForm()
        {
            Text = "Справочник клиентов";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 500);
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            var btnRefresh = new Button { Text = "Обновить", Width = 100 };
            btnRefresh.Click += async (s, e) => await LoadData();
            var btnAdd = new Button { Text = "Добавить", Width = 100 };
            btnAdd.Click += async (s, e) => await AddCustomer();
            var btnEdit = new Button { Text = "Изменить", Width = 100 };
            btnEdit.Click += async (s, e) => await EditCustomer();
            var btnDelete = new Button { Text = "Удалить", Width = 100 };
            btnDelete.Click += async (s, e) => await DeleteCustomer();
            panel.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });
            Controls.Add(grid);
            Controls.Add(panel);
            Load += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var list = await ApiClient.GetCustomersAsync();
                grid.DataSource = list;
            }
            catch (Exception ex)
            {
                Logger.Error("Загрузка клиентов", ex);
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task AddCustomer()
        {
            using (var dlg = new CustomerEditDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    await ApiClient.CreateCustomerAsync(dlg.Request);
                    await LoadData();
                }
                catch (Exception ex)
                {
                    Logger.Error("Создание клиента", ex);
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async System.Threading.Tasks.Task EditCustomer()
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow.DataBoundItem as ApiCustomer;
            if (row == null) return;
            using (var dlg = new CustomerEditDialog(row))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    await ApiClient.UpdateCustomerAsync(row.Id, dlg.Request);
                    await LoadData();
                }
                catch (Exception ex)
                {
                    Logger.Error("Изменение клиента", ex);
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteCustomer()
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow.DataBoundItem as ApiCustomer;
            if (row == null) return;
            if (MessageBox.Show("Удалить клиента «" + row.Name + "»?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                await ApiClient.DeleteCustomerAsync(row.Id);
                await LoadData();
            }
            catch (Exception ex)
            {
                Logger.Error("Удаление клиента", ex);
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal sealed class CustomerEditDialog : Form
    {
        private readonly TextBox name = new TextBox();
        private readonly TextBox inn = new TextBox();
        private readonly TextBox address = new TextBox();
        private readonly TextBox phone = new TextBox();
        private readonly CheckBox isBuyer = new CheckBox { Text = "Покупатель", Checked = true };
        private readonly CheckBox isSalesman = new CheckBox { Text = "Поставщик" };

        public ApiCustomerRequest Request { get; private set; }

        public CustomerEditDialog(ApiCustomer existing = null)
        {
            Text = existing == null ? "Новый клиент" : "Редактирование клиента";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            ClientSize = new Size(420, 260);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Название", AutoSize = true }, 0, 0);
            layout.Controls.Add(name, 1, 0);
            layout.Controls.Add(new Label { Text = "ИНН", AutoSize = true }, 0, 1);
            layout.Controls.Add(inn, 1, 1);
            layout.Controls.Add(new Label { Text = "Адрес", AutoSize = true }, 0, 2);
            layout.Controls.Add(address, 1, 2);
            layout.Controls.Add(new Label { Text = "Телефон", AutoSize = true }, 0, 3);
            layout.Controls.Add(phone, 1, 3);
            layout.Controls.Add(isBuyer, 1, 4);
            layout.Controls.Add(isSalesman, 1, 5);
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text))
                {
                    MessageBox.Show("Название обязательно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
                Request = new ApiCustomerRequest
                {
                    Name = name.Text.Trim(),
                    INN = string.IsNullOrWhiteSpace(inn.Text) ? null : inn.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(address.Text) ? null : address.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(phone.Text) ? null : phone.Text.Trim(),
                    IsBuyer = isBuyer.Checked,
                    IsSalesman = isSalesman.Checked
                };
            };
            var cancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 1, 6);
            Controls.Add(layout);
            AcceptButton = ok;
            CancelButton = cancel;

            if (existing != null)
            {
                name.Text = existing.Name;
                inn.Text = existing.INN;
                address.Text = existing.Address;
                phone.Text = existing.Phone;
                isBuyer.Checked = existing.IsBuyer;
                isSalesman.Checked = existing.IsSalesman;
            }
        }
    }
}
