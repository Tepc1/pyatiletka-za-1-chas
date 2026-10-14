using System;
using System.Drawing;
using System.Windows.Forms;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms
{
    internal sealed class OrderForm : Form
    {
        private readonly TextBox orderIdBox = new TextBox { Width = 120 };
        private readonly Label resultLabel = new Label { AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };

        public OrderForm()
        {
            Text = "Расчёт стоимости заказа";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Номер заказа", AutoSize = true }, 0, 0);
            layout.Controls.Add(orderIdBox, 1, 0);
            var btn = new Button { Text = "Рассчитать", AutoSize = true };
            btn.Click += Calculate;
            layout.Controls.Add(btn, 1, 1);
            layout.Controls.Add(resultLabel, 1, 2);
            Controls.Add(layout);
            AcceptButton = btn;
        }

        private async void Calculate(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(orderIdBox.Text.Trim(), out id) || id <= 0)
            {
                MessageBox.Show("Введите положительный номер заказа.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var cost = await ApiClient.CalculateOrderCostAsync(id);
                resultLabel.Text = "Стоимость заказа №" + cost.OrderId + ": " + cost.TotalCost.ToString("N2") + " руб.";
            }
            catch (Exception ex)
            {
                Logger.Error("Расчёт стоимости", ex);
                resultLabel.Text = "";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
