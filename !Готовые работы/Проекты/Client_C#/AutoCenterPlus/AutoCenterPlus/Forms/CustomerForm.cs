using System;
using System.Drawing;
using System.Windows.Forms;
using AutoCenterPlus.Services;
using AutoCenterPlus.Utils;

namespace AutoCenterPlus.Forms
{
    internal sealed class CustomerForm : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly TextBox name = new TextBox();
        private readonly TextBox inn = new TextBox();
        private readonly TextBox address = new TextBox();
        private readonly TextBox phone = new TextBox();
        private readonly CheckBox buyer = new CheckBox { Text = "Покупатель" };
        private readonly CheckBox supplier = new CheckBox { Text = "Поставщик" };
        private int? selectedId;

        public CustomerForm()
        {
            Text = "Справочник клиентов";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(950, 540);
            grid.Dock = DockStyle.Top;
            grid.Height = 280;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionChanged += SelectCustomer;
            var fields = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), ColumnCount = 4, RowCount = 3 };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            AddField(fields, "Название", name, 0, 0); AddField(fields, "ИНН", inn, 2, 0);
            AddField(fields, "Адрес", address, 0, 1); AddField(fields, "Телефон", phone, 2, 1);
            fields.Controls.Add(buyer, 1, 2); fields.Controls.Add(supplier, 3, 2);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44 };
            AddButton(buttons, "Добавить", AddCustomer); AddButton(buttons, "Сохранить изменения", UpdateCustomer);
            AddButton(buttons, "Удалить", DeleteCustomer);
            AddButton(buttons, "Обновить", async delegate { await LoadCustomers(); });
            Controls.Add(fields); Controls.Add(buttons); Controls.Add(grid);
            Load += async delegate { await LoadCustomers(); };
        }

        private static void AddField(TableLayoutPanel panel, string label, Control control, int column, int row)
        {
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, column, row);
            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, column + 1, row);
        }

        private static void AddButton(Control parent, string text, Action action)
        {
            var button = new Button { Text = text, AutoSize = true };
            button.Click += delegate { action(); };
            parent.Controls.Add(button);
        }

        private void SelectCustomer(object sender, EventArgs e)
        {
            var customer = grid.CurrentRow == null ? null : grid.CurrentRow.DataBoundItem as ApiCustomer;
            if (customer == null) return;
            selectedId = customer.Id;
            name.Text = customer.Name;
            inn.Text = customer.INN;
            address.Text = customer.Address;
            phone.Text = customer.Phone;
            buyer.Checked = customer.IsBuyer;
            supplier.Checked = customer.IsSalesman;
        }

        private async System.Threading.Tasks.Task LoadCustomers()
        {
            try
            {
                selectedId = null;
                grid.DataSource = await ApiClient.GetCustomersAsync();
            }
            catch (Exception exception) { ShowError("Загрузка клиентов", exception); }
        }

        private async void AddCustomer()
        {
            if (!ValidateName()) return;
            try
            {
                await ApiClient.CreateCustomerAsync(CreateRequest());
                await LoadCustomers();
            }
            catch (Exception exception) { ShowError("Добавление клиента", exception); }
        }

        private async void UpdateCustomer()
        {
            if (!selectedId.HasValue) { MessageBox.Show("Сначала выберите клиента.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!ValidateName()) return;
            try
            {
                await ApiClient.UpdateCustomerAsync(selectedId.Value, CreateRequest());
                await LoadCustomers();
            }
            catch (Exception exception) { ShowError("Сохранение клиента", exception); }
        }

        private bool ValidateName()
        {
            if (string.IsNullOrWhiteSpace(name.Text)) { MessageBox.Show("Наименование клиента обязательно.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private ApiCustomerRequest CreateRequest()
        {
            return new ApiCustomerRequest
            {
                Name = name.Text.Trim(),
                INN = Optional(inn.Text),
                Address = Optional(address.Text),
                Phone = Optional(phone.Text),
                IsBuyer = buyer.Checked,
                IsSalesman = supplier.Checked
            };
        }

        private static string Optional(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }

        private async void DeleteCustomer()
        {
            if (!selectedId.HasValue) { MessageBox.Show("Сначала выберите клиента.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show("Удалить выбранного клиента?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await ApiClient.DeleteCustomerAsync(selectedId.Value);
                selectedId = null;
                await LoadCustomers();
            }
            catch (Exception exception) { ShowError("Удаление клиента (возможно, на него ссылаются заказы)", exception); }
        }

        private static void ShowError(string operation, Exception exception)
        {
            Logger.Error(operation, exception);
            MessageBox.Show("Операция не выполнена. Проверьте данные, доступность API и журнал приложения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
