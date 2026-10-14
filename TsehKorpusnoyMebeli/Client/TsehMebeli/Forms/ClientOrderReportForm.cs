using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms;

internal sealed class ClientOrderReportForm : Form
{
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    public ClientOrderReportForm(string role)
    {
        Text = "Анализ заказов";
        UiTheme.ApplyForm(this);
        UiTheme.StyleGrid(grid);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = UiTheme.Background
        };
        var btnRefresh = UiTheme.CreatePrimaryButton("Обновить", 120, 34);
        btnRefresh.Click += async (_, _) => await LoadData();
        panel.Controls.Add(btnRefresh);

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
            grid.DataSource = await ApiClient.GetAnalysisAsync(null);
        }
        catch (Exception ex)
        {
            Logger.Error("Анализ заказов", ex);
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
