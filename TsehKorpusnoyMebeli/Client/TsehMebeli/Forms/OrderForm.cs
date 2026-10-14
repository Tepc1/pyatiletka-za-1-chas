using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms;

internal sealed class OrderForm : Form
{
    private readonly TextBox orderIdBox = new();
    private readonly Label resultLabel = new();
    private readonly Button btn;

    public OrderForm()
    {
        Text = "Расчёт стоимости заказа";
        UiTheme.ApplyForm(this);

        var card = new Panel
        {
            Location = new Point(32, 32),
            Size = new Size(480, 180),
            BackColor = UiTheme.Surface,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var brush = new SolidBrush(UiTheme.Accent);
            e.Graphics.FillRectangle(brush, 0, 0, 5, card.Height);
        };

        var lbl = new Label
        {
            Text = "Номер заказа",
            Location = new Point(28, 24),
            AutoSize = true,
            ForeColor = UiTheme.TextMuted
        };
        orderIdBox.Location = new Point(28, 50);
        orderIdBox.Width = 160;
        UiTheme.StyleTextBox(orderIdBox);

        btn = UiTheme.CreatePrimaryButton("Рассчитать", 140, 34);
        btn.Location = new Point(210, 46);
        btn.Click += Calculate;

        resultLabel.Location = new Point(28, 110);
        resultLabel.AutoSize = true;
        resultLabel.Font = new Font("Segoe UI Semibold", 13f);
        resultLabel.ForeColor = UiTheme.Success;

        card.Controls.AddRange([lbl, orderIdBox, btn, resultLabel]);
        Controls.Add(card);
        AcceptButton = btn;
    }

    private async void Calculate(object? sender, EventArgs e)
    {
        if (!int.TryParse(orderIdBox.Text.Trim(), out var id) || id <= 0)
        {
            MessageBox.Show("Введите положительный номер заказа.", "Ошибка ввода",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btn.Enabled = false;
        btn.Text = "…";
        try
        {
            var cost = await ApiClient.CalculateOrderCostAsync(id);
            resultLabel.ForeColor = UiTheme.Success;
            resultLabel.Text = $"Заказ №{cost.OrderId}:  {cost.TotalCost:N2} ₽";
        }
        catch (Exception ex)
        {
            Logger.Error("Расчёт стоимости", ex);
            resultLabel.Text = "";
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btn.Enabled = true;
            btn.Text = "Рассчитать";
        }
    }
}
