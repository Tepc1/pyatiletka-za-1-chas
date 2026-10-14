using TsehMebeli.Services;
using TsehMebeli.Utils;

namespace TsehMebeli.Forms;

internal sealed class MainForm : Form
{
    private readonly AuthenticatedUser user;
    private readonly Panel contentHost = new() { Dock = DockStyle.Fill, BackColor = UiTheme.Background, Padding = new Padding(0) };
    private readonly Label pathLabel = new();
    private readonly Button btnBack;
    private readonly Button btnForward;
    private readonly Button btnHome;
    private readonly Stack<string> backStack = new();
    private readonly Stack<string> forwardStack = new();
    private string currentPage = "home";
    private bool endingSession;

    public MainForm(AuthenticatedUser user)
    {
        this.user = user;
        Text = "Цех корпусной мебели — информационная система";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1020, 680);
        UiTheme.ApplyForm(this);

        // —— верхняя панель навигации ——
        var topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = UiTheme.Header, Padding = new Padding(8, 10, 8, 10) };

        btnBack = UiTheme.CreateNavButton("◀");
        btnBack.Location = new Point(12, 12);
        btnBack.Click += (_, _) => GoBack();

        btnForward = UiTheme.CreateNavButton("▶");
        btnForward.Location = new Point(56, 12);
        btnForward.Click += (_, _) => GoForward();

        btnHome = UiTheme.CreateNavButton("⌂");
        btnHome.Location = new Point(100, 12);
        btnHome.Click += (_, _) => Navigate("home", pushHistory: true);

        pathLabel.AutoSize = false;
        pathLabel.Location = new Point(150, 16);
        pathLabel.Size = new Size(500, 28);
        pathLabel.ForeColor = UiTheme.TextOnDark;
        pathLabel.Font = new Font("Segoe UI Semibold", 11f);
        pathLabel.Text = "Главное меню";

        var userBadge = new Label
        {
            AutoSize = true,
            Text = user.FullName + "  ·  " + user.Role,
            ForeColor = Color.FromArgb(210, 198, 184),
            Font = UiTheme.SubtitleFont,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        userBadge.Location = new Point(Width - 220, 18);

        var btnLogout = UiTheme.CreateSecondaryButton("Выход", 90, 32);
        btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLogout.Location = new Point(Width - 120, 12);
        btnLogout.Click += (_, _) => Close();

        topBar.Controls.AddRange([btnBack, btnForward, btnHome, pathLabel]);
        topBar.Resize += (_, _) =>
        {
            userBadge.Location = new Point(topBar.Width - 280, 18);
            btnLogout.Location = new Point(topBar.Width - 120, 12);
        };
        topBar.Controls.Add(userBadge);
        topBar.Controls.Add(btnLogout);

        // —— боковое меню ——
        var side = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = UiTheme.Sidebar };
        side.Paint += (s, e) =>
        {
            UiTheme.DrawCabinetIcon(e.Graphics, new Rectangle(70, 16, 72, 72), Color.FromArgb(200, 160, 100));
        };

        var sideContent = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12, 100, 12, 12),
            BackColor = Color.Transparent
        };

        sideContent.Controls.Add(MakeSideButton("Главная", "home"));
        if (CanManageCustomers(user.Role))
            sideContent.Controls.Add(MakeSideButton("Клиенты", "customers"));
        if (CanCalculateOrders(user.Role))
        {
            sideContent.Controls.Add(MakeSideButton("Заказы", "orders"));
            sideContent.Controls.Add(MakeSideButton("Расчёт стоимости", "cost"));
        }
        if (CanViewAnalysis(user.Role))
            sideContent.Controls.Add(MakeSideButton("Анализ", "analysis"));

        side.Controls.Add(sideContent);

        Controls.Add(contentHost);
        Controls.Add(side);
        Controls.Add(topBar);

        FormClosing += CloseSession;
        Load += (_, _) => Navigate("home", pushHistory: false);
        UpdateNavButtons();
    }

    private Button MakeSideButton(string text, string page)
    {
        var btn = new Button
        {
            Text = "  " + text,
            Width = 190,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(74, 55, 40),
            ForeColor = UiTheme.TextOnDark,
            Font = UiTheme.ButtonFont,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 8),
            Tag = page
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = UiTheme.Accent;
        btn.Click += (_, _) => Navigate(page, pushHistory: true);
        return btn;
    }

    private void Navigate(string page, bool pushHistory)
    {
        if (pushHistory && page != currentPage)
        {
            backStack.Push(currentPage);
            forwardStack.Clear();
        }

        currentPage = page;
        contentHost.Controls.Clear();

        Control view = page switch
        {
            "customers" => new CustomerForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill },
            "orders" => new OrdersListForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill },
            "cost" => new OrderForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill },
            "analysis" => new ClientOrderReportForm(user.Role) { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill },
            _ => CreateHomeView()
        };

        pathLabel.Text = page switch
        {
            "customers" => "Справочник клиентов",
            "orders" => "Список заказов",
            "cost" => "Расчёт стоимости заказа",
            "analysis" => "Анализ заказов клиентов",
            _ => "Главное меню"
        };

        if (view is Form f)
        {
            f.Show();
            contentHost.Controls.Add(f);
        }
        else
            contentHost.Controls.Add(view);

        UpdateNavButtons();
    }

    private Control CreateHomeView()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background, Padding = new Padding(28) };
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        flow.Controls.Add(new Label
        {
            Text = "Добро пожаловать, " + user.FullName,
            Font = new Font("Segoe UI Semibold", 14f),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
            Margin = new Padding(4, 4, 4, 8)
        });
        flow.Controls.Add(new Label
        {
            Text = "Выберите раздел в меню слева или карточку ниже.",
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Margin = new Padding(4, 0, 4, 20)
        });

        if (CanManageCustomers(user.Role))
            AddHomeCard(flow, "Справочник клиентов", "Добавление, изменение и удаление контрагентов", "customers");
        if (CanCalculateOrders(user.Role))
        {
            AddHomeCard(flow, "Список заказов", "Просмотр заказов и сумм по спецификации", "orders");
            AddHomeCard(flow, "Расчёт стоимости", "Калькуляция материалов и операций по номеру заказа", "cost");
        }
        if (CanViewAnalysis(user.Role))
            AddHomeCard(flow, "Анализ заказов", "Сводка по клиентам, изделиям и скидкам", "analysis");

        panel.Controls.Add(flow);
        return panel;
    }

    private void AddHomeCard(Control parent, string title, string description, string page)
    {
        var card = new Panel
        {
            Width = 640,
            Height = 84,
            BackColor = UiTheme.Surface,
            Margin = new Padding(4, 0, 4, 12),
            Cursor = Cursors.Hand
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(UiTheme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var brush = new SolidBrush(UiTheme.Accent);
            e.Graphics.FillRectangle(brush, 0, 0, 5, card.Height);
        };

        var t = new Label { Text = title, Font = new Font("Segoe UI Semibold", 11.5f), ForeColor = UiTheme.TextPrimary, AutoSize = true, Location = new Point(24, 16) };
        var d = new Label { Text = description, Font = UiTheme.SubtitleFont, ForeColor = UiTheme.TextMuted, AutoSize = true, Location = new Point(24, 46) };
        void open(object? s, EventArgs e) => Navigate(page, true);
        card.Click += open;
        t.Click += open;
        d.Click += open;
        card.MouseEnter += (_, _) => card.BackColor = UiTheme.AccentSoft;
        card.MouseLeave += (_, _) => card.BackColor = UiTheme.Surface;
        card.Controls.Add(t);
        card.Controls.Add(d);
        parent.Controls.Add(card);
    }

    private void GoBack()
    {
        if (backStack.Count == 0) return;
        forwardStack.Push(currentPage);
        Navigate(backStack.Pop(), pushHistory: false);
    }

    private void GoForward()
    {
        if (forwardStack.Count == 0) return;
        backStack.Push(currentPage);
        Navigate(forwardStack.Pop(), pushHistory: false);
    }

    private void UpdateNavButtons()
    {
        btnBack.Enabled = backStack.Count > 0;
        btnForward.Enabled = forwardStack.Count > 0;
        btnBack.BackColor = btnBack.Enabled ? Color.FromArgb(80, 58, 42) : Color.FromArgb(55, 42, 32);
        btnForward.BackColor = btnForward.Enabled ? Color.FromArgb(80, 58, 42) : Color.FromArgb(55, 42, 32);
    }

    private static bool CanManageCustomers(string role) => role is "Admin" or "Manager";
    private static bool CanCalculateOrders(string role) => role is "Admin" or "Manager" or "Operator";
    private static bool CanViewAnalysis(string role) => role is "Admin" or "Manager";

    private async void CloseSession(object? sender, FormClosingEventArgs e)
    {
        if (endingSession) return;
        e.Cancel = true;
        Enabled = false;
        try { await ApiClient.LogoutAsync(); }
        catch (Exception ex) { Logger.Error("Завершение сеанса API", ex); }
        finally
        {
            endingSession = true;
            Close();
        }
    }
}
