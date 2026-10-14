using System;
using System.Drawing;
using System.Windows.Forms;
using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms
{
    internal sealed class OrderForm : Form
    {
        private readonly TextBox orderIdBox = new TextBox();
        private readonly Label resultLabel = new Label();
        private readonly Button btn;

        public OrderForm()
        {
            Text = "Расчёт стоимости заказа";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(480, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            UiTheme.ApplyForm(this);

            Controls.Add(UiTheme.CreateHeader("Расчёт стоимости", "Калькуляция заказа по спецификации"));

            var card = new Panel
            {
                Location = new Point(32, 100),
                Size = new Size(416, 150),
                BackColor = UiTheme.Surface,
                Padding = new Padding(20)
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var lbl = new Label
            {
                Text = "Номер заказа",
                Location = new Point(24, 18),
                AutoSize = true,
                ForeColor = UiTheme.TextMuted
            };
            orderIdBox.Location = new Point(24, 42);
            orderIdBox.Width = 160;
            UiTheme.StyleTextBox(orderIdBox);

            btn = UiTheme.CreatePrimaryButton("Рассчитать", 140, 36);
            btn.Location = new Point(200, 38);
            btn.Click += Calculate;

            resultLabel.Location = new Point(24, 96);
            resultLabel.AutoSize = true;
            resultLabel.Font = new Font("Segoe UI Semibold", 12f);
            resultLabel.ForeColor = UiTheme.Success;

            card.Controls.AddRange(new Control[] { lbl, orderIdBox, btn, resultLabel });
            Controls.Add(card);
            AcceptButton = btn;
        }

        private async void Calculate(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(orderIdBox.Text.Trim(), out id) || id <= 0)
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
                resultLabel.Text = "Заказ №" + cost.OrderId + ":  " + cost.TotalCost.ToString("N2") + " ₽";
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
}
