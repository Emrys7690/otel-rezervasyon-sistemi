using System.Drawing.Drawing2D;

namespace OtelRezervasyon.UI;

public class SidebarItem : Control
{
    private bool _active;
    private bool _hover;
    private string _icon = "•";

    public string Icon { get => _icon; set { _icon = value; Invalidate(); } }
    public bool Active { get => _active; set { _active = value; Invalidate(); } }

    public SidebarItem()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Dock = DockStyle.Top;
        Height = 46;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bg = _active ? Theme.SidebarActive : (_hover ? Theme.SidebarHover : Theme.Sidebar);
        using (var b = new SolidBrush(bg)) g.FillRectangle(b, ClientRectangle);

        if (_active)
        {
            using var accent = new SolidBrush(Color.White);
            g.FillRectangle(accent, 0, 0, 3, Height);
        }

        var color = _active ? Theme.SidebarTextActive : Theme.SidebarText;
        using var iconFont = new Font("Segoe UI Emoji", 13f);
        TextRenderer.DrawText(g, _icon, iconFont,
            new Rectangle(14, 0, 28, Height), color,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

        TextRenderer.DrawText(g, Text, Font,
            new Rectangle(50, 0, Width - 56, Height), color,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }
}
