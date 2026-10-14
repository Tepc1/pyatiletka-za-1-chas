using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms;

internal sealed class CustomerForm : Form
{
    private readonly DataGridView grid = new()
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
        UiTheme.ApplyForm(this);
        UiTheme.StyleGrid(grid);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = UiTheme.Background
        };

        var btnRefresh = UiTheme.CreateSecondaryButton("Обновить");
        btnRefresh.Click += async (_, _) => await LoadData();
        var btnAdd = UiTheme.CreatePrimaryButton("Добавить", 120, 34);
        btnAdd.Click += async (_, _) => await AddCustomer();
        var btnEdit = UiTheme.CreateSecondaryButton("Изменить");
        btnEdit.Click += async (_, _) => await EditCustomer();
        var btnDelete = UiTheme.CreateDangerButton("Удалить");
        btnDelete.Click += async (_, _) => await DeleteCustomer();

        panel.Controls.AddRange([btnRefresh, btnAdd, btnEdit, btnDelete]);

        var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 12, 12), BackColor = UiTheme.Background };
        host.Controls.Add(grid);

        Controls.Add(host);
        Controls.Add(panel);
        Load += async (_, _) => await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            grid.DataSource = await ApiClient.GetCustomersAsync();
        }
        catch (Exception ex)
        {
            Logger.Error("Загрузка клиентов", ex);
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task AddCustomer()
    {
        using var dlg = new CustomerEditDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            await ApiClient.CreateCustomerAsync(dlg.Request!);
            await LoadData();
        }
        catch (Exception ex)
        {
            Logger.Error("Создание клиента", ex);
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task EditCustomer()
    {
        if (grid.CurrentRow?.DataBoundItem is not ApiCustomer row) return;
        using var dlg = new CustomerEditDialog(row);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        try
        {
            await ApiClient.UpdateCustomerAsync(row.Id, dlg.Request!);
            await LoadData();
        }
        catch (Exception ex)
        {
            Logger.Error("Изменение клиента", ex);
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteCustomer()
    {
        if (grid.CurrentRow?.DataBoundItem is not ApiCustomer row) return;
        if (MessageBox.Show($"Удалить клиента «{row.Name}»?", "Подтверждение",
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
    private readonly TextBox name = new();
    private readonly TextBox inn = new();
    private readonly TextBox address = new();
    private readonly TextBox phone = new();
    private readonly CheckBox isBuyer = new() { Text = "Покупатель", Checked = true, AutoSize = true };
    private readonly CheckBox isSalesman = new() { Text = "Поставщик", AutoSize = true };

    public ApiCustomerRequest? Request { get; private set; }

    public CustomerEditDialog(ApiCustomer? existing = null)
    {
        Text = existing is null ? "Новый клиент" : "Редактирование клиента";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 300);
        UiTheme.ApplyForm(this);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 7
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        foreach (var tb in new[] { name, inn, address, phone })
        {
            UiTheme.StyleTextBox(tb);
            tb.Dock = DockStyle.Fill;
        }

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

        var ok = UiTheme.CreatePrimaryButton("Сохранить", 120, 34);
        ok.DialogResult = DialogResult.None;
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                MessageBox.Show("Название обязательно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Request = new ApiCustomerRequest
            {
                Name = name.Text.Trim(),
                Inn = string.IsNullOrWhiteSpace(inn.Text) ? null : inn.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(address.Text) ? null : address.Text.Trim(),
                Phone = string.IsNullOrWhiteSpace(phone.Text) ? null : phone.Text.Trim(),
                IsBuyer = isBuyer.Checked,
                IsSalesman = isSalesman.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
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

        if (existing is not null)
        {
            name.Text = existing.Name;
            inn.Text = existing.Inn;
            address.Text = existing.Address;
            phone.Text = existing.Phone;
            isBuyer.Checked = existing.IsBuyer;
            isSalesman.Checked = existing.IsSalesman;
        }
    }
}
