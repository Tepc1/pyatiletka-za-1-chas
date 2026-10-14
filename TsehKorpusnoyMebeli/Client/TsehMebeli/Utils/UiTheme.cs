using System.Drawing.Drawing2D;

namespace TsehMebeli.Utils;

internal static class UiTheme
{
    public static readonly Color Background = Color.FromArgb(246, 242, 236);
    public static readonly Color Surface = Color.FromArgb(255, 252, 248);
    public static readonly Color Header = Color.FromArgb(52, 36, 26);
    public static readonly Color Sidebar = Color.FromArgb(61, 43, 31);
    public static readonly Color Accent = Color.FromArgb(184, 115, 51);
    public static readonly Color AccentHover = Color.FromArgb(160, 96, 40);
    public static readonly Color AccentSoft = Color.FromArgb(236, 216, 196);
    public static readonly Color TextPrimary = Color.FromArgb(40, 32, 26);
    public static readonly Color TextMuted = Color.FromArgb(120, 108, 96);
    public static readonly Color TextOnDark = Color.FromArgb(245, 238, 230);
    public static readonly Color Border = Color.FromArgb(210, 198, 184);
    public static readonly Color Success = Color.FromArgb(56, 142, 60);
    public static readonly Color Danger = Color.FromArgb(180, 60, 50);
    public static readonly Color GridHeader = Color.FromArgb(74, 55, 40);
    public static readonly Color GridAlt = Color.FromArgb(250, 246, 240);

    public static readonly Font TitleFont = new("Segoe UI Semibold", 15f, FontStyle.Bold);
    public static readonly Font SubtitleFont = new("Segoe UI", 9.5f);
    public static readonly Font BodyFont = new("Segoe UI", 10f);
    public static readonly Font ButtonFont = new("Segoe UI Semibold", 9.5f);

    public static void ApplyForm(Form form)
    {
        form.BackColor = Background;
        form.Font = BodyFont;
        form.ForeColor = TextPrimary;
    }

    public static Button CreatePrimaryButton(string text, int width = 140, int height = 36)
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
            Margin = new Padding(4)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = AccentHover;
        return btn;
    }

    public static Button CreateSecondaryButton(string text, int width = 110, int height = 34)
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

    public static Button CreateNavButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            Width = 40,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 58, 42),
            ForeColor = TextOnDark,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(2)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Accent;
        return btn;
    }

    public static Button CreateDangerButton(string text, int width = 110, int height = 34)
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
            Padding = new Padding(6, 4, 6, 4)
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

    /// <summary>Простая иконка шкафа (рисуется, без внешних файлов).</summary>
    public static void DrawCabinetIcon(Graphics g, Rectangle bounds, Color color)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(color, 2f);
        using var brush = new SolidBrush(Color.FromArgb(40, color));
        var r = Rectangle.Inflate(bounds, -4, -4);
        g.FillRectangle(brush, r);
        g.DrawRectangle(pen, r);
        var mid = r.Left + r.Width / 2;
        g.DrawLine(pen, mid, r.Top, mid, r.Bottom);
        g.FillEllipse(new SolidBrush(color), mid - 10, r.Top + r.Height / 2 - 3, 6, 6);
        g.FillEllipse(new SolidBrush(color), mid + 4, r.Top + r.Height / 2 - 3, 6, 6);
    }
}
