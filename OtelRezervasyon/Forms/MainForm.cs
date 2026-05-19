using OtelRezervasyon.Forms.Views;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms;

public class MainForm : Form
{
    private readonly Panel _sidebar = new() { Dock = DockStyle.Left, Width = 240, BackColor = Theme.Sidebar };
    private readonly Panel _topbar  = new() { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Theme.AppBg, Padding = new Padding(24) };

    private readonly Label _pageTitle = new() { Font = Theme.H2, ForeColor = Theme.TextDark, AutoSize = true };
    private readonly Label _pageSub   = new() { Font = Theme.Small, ForeColor = Theme.TextMuted, AutoSize = true };
    private readonly Label _saatLabel = new() { Font = Theme.Body, ForeColor = Theme.TextMuted, AutoSize = true };
    private readonly System.Windows.Forms.Timer _saatTimer = new() { Interval = 1000 };

    private readonly List<SidebarItem> _items = new();
    private UserControl? _aktifView;

    public MainForm()
    {
        Text = "Otel Rezervasyon Sistemi";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        BackColor = Theme.AppBg;
        Font = Theme.Body;

        Controls.Add(_content);
        Controls.Add(_topbar);
        Controls.Add(_sidebar);

        SidebarKur();
        TopbarKur();
        SaatiBaslat();

        Goster<DashboardView>("Dashboard");
    }

    private void SidebarKur()
    {
        var logoPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Theme.Sidebar };
        var logoLabel = new Label
        {
            Text = "  🏨  OtelPro",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0)
        };
        logoPanel.Controls.Add(logoLabel);

        var items = new (string icon, string text, string title, string sub, Type view)[]
        {
            ("📊", "Dashboard",      "Dashboard",           "Genel bakış",                       typeof(DashboardView)),
            ("📋", "Rezervasyonlar", "Rezervasyonlar",      "Tüm rezervasyon kayıtları",         typeof(RezervasyonView)),
            ("🔍", "Müsait Odalar",  "Müsait Odalar",       "Boş oda arama",                     typeof(MusaitOdaView)),
            ("🛏",  "Odalar",         "Odalar",              "Oda yönetimi",                      typeof(OdaView)),
            ("👥", "Müşteriler",     "Müşteriler",          "Müşteri kayıtları",                 typeof(MusteriView)),
            ("📈", "Raporlar",       "Raporlar",            "Doluluk ve gelir analizleri",       typeof(RaporView)),
        };

        // Reverse: Dock=Top eklendikçe önce eklenen üstte kalır
        foreach (var info in items.Reverse())
        {
            var item = new SidebarItem { Text = info.text, Icon = info.icon };
            item.Click += (_, _) => SecimDegistir(item, info.view, info.title, info.sub);
            _sidebar.Controls.Add(item);
            _items.Add(item);
        }
        _sidebar.Controls.Add(logoPanel);

        var aktifItem = _items.FirstOrDefault(i => i.Text == "Dashboard");
        if (aktifItem != null) aktifItem.Active = true;
    }

    private void SecimDegistir(SidebarItem item, Type viewType, string title, string sub)
    {
        foreach (var i in _items) i.Active = (i == item);
        var ctor = viewType.GetConstructor(Type.EmptyTypes);
        if (ctor == null) return;
        var view = (UserControl)ctor.Invoke(null);
        SetIcerik(view, title, sub);
    }

    private void Goster<T>(string title, string sub = "") where T : UserControl, new()
    {
        var v = new T();
        SetIcerik(v, title, sub);
    }

    private void SetIcerik(UserControl view, string title, string sub)
    {
        _pageTitle.Text = title;
        _pageSub.Text = sub;

        _aktifView?.Dispose();
        _aktifView = view;
        view.Dock = DockStyle.Fill;
        _content.Controls.Clear();
        _content.Controls.Add(view);
    }

    private void TopbarKur()
    {
        _topbar.Padding = new Padding(24, 0, 24, 0);

        var titlePanel = new Panel { Dock = DockStyle.Left, Width = 500 };
        _pageTitle.Location = new Point(0, 12);
        _pageSub.Location = new Point(2, 38);
        titlePanel.Controls.Add(_pageTitle);
        titlePanel.Controls.Add(_pageSub);

        var rightPanel = new Panel { Dock = DockStyle.Right, Width = 280 };
        _saatLabel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        rightPanel.Resize += (_, _) =>
        {
            _saatLabel.Location = new Point(rightPanel.Width - _saatLabel.Width - 4, (rightPanel.Height - _saatLabel.Height) / 2);
        };
        rightPanel.Controls.Add(_saatLabel);

        _topbar.Controls.Add(titlePanel);
        _topbar.Controls.Add(rightPanel);

        var alt = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.CardBorder };
        _topbar.Controls.Add(alt);
    }

    private void SaatiBaslat()
    {
        void Ts() => _saatLabel.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy  •  HH:mm",
            new System.Globalization.CultureInfo("tr-TR"));
        Ts();
        _saatTimer.Tick += (_, _) => Ts();
        _saatTimer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _saatTimer.Stop();
        base.OnFormClosing(e);
    }
}
