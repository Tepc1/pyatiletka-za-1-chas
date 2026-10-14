using System;
using System.Drawing;
using System.Windows.Forms;
using AutoCenterPlus.Services;
using AutoCenterPlus.Utils;

namespace AutoCenterPlus.Forms
{
    internal sealed class OrderForm : Form
    {
        private readonly TextBox orderId = new TextBox();
        private readonly TextBox total = new TextBox { ReadOnly = true };
        public OrderForm()
        {
            Text = "Расчёт стоимости заказа"; StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(480, 170);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Номер заказа", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0); layout.Controls.Add(orderId, 1, 0);
            layout.Controls.Add(new Label { Text = "Полная стоимость", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1); layout.Controls.Add(total, 1, 1);
            var button = new Button { Text = "Рассчитать", AutoSize = true, Anchor = AnchorStyles.Right }; button.Click += Calculate;
            layout.Controls.Add(button, 1, 2); Controls.Add(layout); AcceptButton = button;
        }
        private async void Calculate(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(orderId.Text, out id) || id <= 0)
            {
                MessageBox.Show("Введите положительный целый номер заказа.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            try
            {
                var result = await ApiClient.CalculateOrderCostAsync(id);
                total.Text = result.TotalCost.ToString("N2");
            }
            catch (Exception exception)
            {
                Logger.Error("Расчёт стоимости заказа", exception);
                MessageBox.Show("Не удалось рассчитать стоимость. Проверьте номер заказа, доступность API и журнал приложения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
