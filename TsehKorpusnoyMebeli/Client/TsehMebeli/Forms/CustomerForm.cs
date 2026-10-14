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
            Size = new Size(960, 560);
            UiTheme.ApplyForm(this);
            UiTheme.StyleGrid(grid);

            Controls.Add(UiTheme.CreateHeader("Справочник клиентов", "Управление контрагентами цеха"));

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = UiTheme.Background,
                FlowDirection = FlowDirection.LeftToRight
            };

            var btnRefresh = UiTheme.CreateSecondaryButton("Обновить");
            btnRefresh.Click += async (s, e) => await LoadData();
            var btnAdd = UiTheme.CreatePrimaryButton("Добавить", 130, 36);
            btnAdd.Click += async (s, e) => await AddCustomer();
            var btnEdit = UiTheme.CreateSecondaryButton("Изменить");
            btnEdit.Click += async (s, e) => await EditCustomer();
            var btnDelete = UiTheme.CreateDangerButton("Удалить");
            btnDelete.Click += async (s, e) => await DeleteCustomer();

            panel.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });

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
            if (MessageBox.Show("Удалить клиента «" + row.Name + "»?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
        private readonly CheckBox isBuyer = new CheckBox { Text = "Покупатель", Checked = true, AutoSize = true };
        private readonly CheckBox isSalesman = new CheckBox { Text = "Поставщик", AutoSize = true };

        public ApiCustomerRequest Request { get; private set; }

        public CustomerEditDialog(ApiCustomer existing = null)
        {
            Text = existing == null ? "Новый клиент" : "Редактирование клиента";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 340);
            UiTheme.ApplyForm(this);

            Controls.Add(UiTheme.CreateHeader(existing == null ? "Новый клиент" : "Редактирование", "Карточка контрагента"));

            var layout = new TableLayoutPanel
            {
                Location = new Point(24, 90),
                Size = new Size(410, 230),
                ColumnCount = 2,
                RowCount = 7,
                BackColor = UiTheme.Background
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            UiTheme.StyleTextBox(name);
            UiTheme.StyleTextBox(inn);
            UiTheme.StyleTextBox(address);
            UiTheme.StyleTextBox(phone);
            name.Width = inn.Width = address.Width = phone.Width = 280;

            layout.Controls.Add(new Label { Text = "Название", AutoSize = true, ForeColor = UiTheme.TextMuted }, 0, 0);
            layout.Controls.Add(name, 1, 0);
            layout.Controls.Add(new Label { Text = "ИНН", AutoSize = true, ForeColor = UiTheme.TextMuted }, 0, 1);
            layout.Controls.Add(inn, 1, 1);
            layout.Controls.Add(new Label { Text = "Адрес", AutoSize = true, ForeColor = UiTheme.TextMuted }, 0, 2);
            layout.Controls.Add(address, 1, 2);
            layout.Controls.Add(new Label { Text = "Телефон", AutoSize = true, ForeColor = UiTheme.TextMuted }, 0, 3);
            layout.Controls.Add(phone, 1, 3);
            layout.Controls.Add(isBuyer, 1, 4);
            layout.Controls.Add(isSalesman, 1, 5);

            var ok = UiTheme.CreatePrimaryButton("Сохранить", 120, 36);
            ok.DialogResult = DialogResult.OK;
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
            var cancel = UiTheme.CreateSecondaryButton("Отмена");
            cancel.DialogResult = DialogResult.Cancel;

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
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
