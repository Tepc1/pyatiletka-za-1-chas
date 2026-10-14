using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TsehMebeli.Utils
{
    /// <summary>Единая палитра и стили интерфейса (тёплый «мебельный» стиль).</summary>
    internal static class UiTheme
    {
        public static readonly Color Background = Color.FromArgb(245, 241, 235);
        public static readonly Color Surface = Color.FromArgb(255, 252, 248);
        public static readonly Color Header = Color.FromArgb(61, 43, 31);
        public static readonly Color Accent = Color.FromArgb(184, 115, 51);
        public static readonly Color AccentHover = Color.FromArgb(160, 96, 40);
        public static readonly Color AccentSoft = Color.FromArgb(232, 210, 188);
        public static readonly Color TextPrimary = Color.FromArgb(40, 32, 26);
        public static readonly Color TextMuted = Color.FromArgb(120, 108, 96);
        public static readonly Color Border = Color.FromArgb(210, 198, 184);
        public static readonly Color Success = Color.FromArgb(56, 142, 60);
        public static readonly Color Danger = Color.FromArgb(180, 60, 50);
        public static readonly Color GridHeader = Color.FromArgb(74, 55, 40);
        public static readonly Color GridAlt = Color.FromArgb(250, 246, 240);

        public static readonly Font TitleFont = new Font("Segoe UI Semibold", 16f, FontStyle.Bold);
        public static readonly Font SubtitleFont = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static readonly Font BodyFont = new Font("Segoe UI", 10f);
        public static readonly Font ButtonFont = new Font("Segoe UI Semibold", 10f);

        public static void ApplyForm(Form form)
        {
            form.BackColor = Background;
            form.Font = BodyFont;
            form.ForeColor = TextPrimary;
        }

        public static Panel CreateHeader(string title, string subtitle = null)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = string.IsNullOrEmpty(subtitle) ? 64 : 84,
                BackColor = Header,
                Padding = new Padding(20, 12, 20, 12)
            };

            var titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = TitleFont,
                AutoSize = true,
                Location = new Point(20, 14)
            };
            header.Controls.Add(titleLabel);

            if (!string.IsNullOrEmpty(subtitle))
            {
                header.Controls.Add(new Label
                {
                    Text = subtitle,
                    ForeColor = Color.FromArgb(210, 198, 184),
                    Font = SubtitleFont,
                    AutoSize = true,
                    Location = new Point(22, 48)
                });
            }

            return header;
        }

        public static Button CreatePrimaryButton(string text, int width = 160, int height = 40)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                Margin = new Padding(6)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AccentHover;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 80, 30);
            return btn;
        }

        public static Button CreateSecondaryButton(string text, int width = 120, int height = 36)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                BackColor = Surface,
                ForeColor = TextPrimary,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                Margin = new Padding(4)
            };
            btn.FlatAppearance.BorderColor = Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = AccentSoft;
            return btn;
        }

        public static Button CreateDangerButton(string text, int width = 120, int height = 36)
        {
            var btn = CreateSecondaryButton(text, width, height);
            btn.ForeColor = Danger;
            btn.FlatAppearance.BorderColor = Color.FromArgb(220, 180, 175);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 235, 232);
            return btn;
        }

        public static void StyleTextBox(TextBox box)
        {
            box.BorderStyle = BorderStyle.FixedSingle;
            box.BackColor = Surface;
            box.ForeColor = TextPrimary;
            box.Font = BodyFont;
            box.Height = 28;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridHeader,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                SelectionBackColor = GridHeader,
                SelectionForeColor = Color.White,
                Padding = new Padding(6, 4, 6, 4),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            grid.ColumnHeadersHeight = 36;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Surface,
                ForeColor = TextPrimary,
                SelectionBackColor = AccentSoft,
                SelectionForeColor = TextPrimary,
                Font = BodyFont,
                Padding = new Padding(4)
            };
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridAlt,
                ForeColor = TextPrimary,
                SelectionBackColor = AccentSoft,
                SelectionForeColor = TextPrimary
            };
            grid.RowTemplate.Height = 32;
            grid.RowHeadersVisible = false;
        }

        public static Label CreateMutedLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = TextMuted,
                Font = SubtitleFont,
                AutoSize = true
            };
        }
    }
}
