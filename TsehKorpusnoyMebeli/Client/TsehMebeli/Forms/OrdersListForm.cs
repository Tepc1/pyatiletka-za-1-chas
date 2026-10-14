using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms;

internal sealed class OrdersListForm : Form
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

    public OrdersListForm()
    {
        Text = "Заказы";
        UiTheme.ApplyForm(this);
        UiTheme.StyleGrid(grid);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = UiTheme.Background
        };
        var btnRefresh = UiTheme.CreatePrimaryButton("Обновить", 120, 34);
        btnRefresh.Click += async (_, _) => await LoadData();
        var btnCost = UiTheme.CreateSecondaryButton("Стоимость", 120, 34);
        btnCost.Click += async (_, _) => await ShowCost();
        toolbar.Controls.AddRange([btnRefresh, btnCost]);

        var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 12, 12), BackColor = UiTheme.Background };
        host.Controls.Add(grid);

        Controls.Add(host);
        Controls.Add(toolbar);
        Load += async (_, _) => await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            grid.DataSource = await ApiClient.GetOrdersAsync();
        }
        catch (Exception ex)
        {
            Logger.Error("Загрузка заказов", ex);
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ShowCost()
    {
        if (grid.CurrentRow?.DataBoundItem is not ApiOrderSummary row) return;
        try
        {
            var cost = await ApiClient.CalculateOrderCostAsync(row.OrderId);
            MessageBox.Show(
                $"Заказ №{cost.OrderId}\nКлиент: {row.CustomerName}\nСтоимость: {cost.TotalCost:N2} ₽",
                "Расчёт стоимости", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Logger.Error("Стоимость заказа", ex);
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
