using System.Drawing.Drawing2D;

namespace OtelRezervasyon.UI;

public class Card : Panel
{
    private int _radius = 10;
    public int Radius { get => _radius; set { _radius = value; Invalidate(); } }

    public Card()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Padding = new Padding(16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = FlatButton.RoundedPath(rect, _radius);
        using var fill = new SolidBrush(Theme.CardBg);
        using var pen = new Pen(Theme.CardBorder, 1);
        g.FillPath(fill, path);
        g.DrawPath(pen, path);
        base.OnPaint(e);
    }
}

public class StatCard : Card
{
    private string _label = "";
    private string _value = "0";
    private string _sub = "";
    private Color _accent = Theme.Primary;
    private string _icon = "📊";

    public string Label { get => _label; set { _label = value; Invalidate(); } }
    public string Value { get => _value; set { _value = value; Invalidate(); } }
    public string Sub { get => _sub; set { _sub = value; Invalidate(); } }
    public Color Accent { get => _accent; set { _accent = value; Invalidate(); } }
    public string Icon { get => _icon; set { _icon = value; Invalidate(); } }

    public StatCard()
    {
        Height = 110;
        Padding = new Padding(20, 16, 20, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var iconRect = new Rectangle(Width - 60, 14, 40, 40);
        using (var iconBg = new SolidBrush(Color.FromArgb(20, _accent)))
        using (var path = FlatButton.RoundedPath(iconRect, 8))
            g.FillPath(iconBg, path);
        using var iconFont = new Font("Segoe UI Emoji", 14f);
        TextRenderer.DrawText(g, _icon, iconFont, iconRect, _accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(g, _label, Theme.Small,
            new Point(Padding.Left, Padding.Top), Theme.TextMuted);

        TextRenderer.DrawText(g, _value, Theme.Stat,
            new Point(Padding.Left - 2, Padding.Top + 18), Theme.TextDark);

        if (!string.IsNullOrEmpty(_sub))
            TextRenderer.DrawText(g, _sub, Theme.Small,
                new Point(Padding.Left, Height - Padding.Bottom - 16), _accent);
    }
}
