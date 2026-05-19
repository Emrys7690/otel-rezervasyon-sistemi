using System.Drawing.Drawing2D;

namespace OtelRezervasyon.UI;

public class FlatButton : Button
{
    private Color _baseColor;
    private Color _hoverColor;
    private bool _hover;
    private int _radius = 6;

    public int Radius { get => _radius; set { _radius = value; Invalidate(); } }
    public Color BaseColor
    {
        get => _baseColor;
        set { _baseColor = value; BackColor = value; Invalidate(); }
    }
    public Color HoverColor { get => _hoverColor; set { _hoverColor = value; Invalidate(); } }

    public FlatButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = Theme.Button;
        Cursor = Cursors.Hand;
        ForeColor = Theme.PrimaryText;
        Height = 36;
        BaseColor = Theme.Primary;
        HoverColor = Theme.PrimaryHover;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.AppBg);

        var fill = !Enabled ? Color.FromArgb(203, 213, 225) : (_hover ? _hoverColor : _baseColor);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedPath(rect, _radius);
        using var brush = new SolidBrush(fill);
        g.FillPath(brush, path);

        TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    public static GraphicsPath RoundedPath(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0) { path.AddRectangle(r); return path; }
        int d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class SecondaryButton : FlatButton
{
    public SecondaryButton()
    {
        BaseColor = Color.White;
        HoverColor = Color.FromArgb(248, 250, 252);
        ForeColor = Theme.TextBody;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedPath(rect, Radius);
        using var pen = new Pen(Theme.CardBorder, 1);
        g.DrawPath(pen, path);
    }
}

public class DangerButton : FlatButton
{
    public DangerButton()
    {
        BaseColor = Theme.Danger;
        HoverColor = Color.FromArgb(220, 38, 38);
    }
}

public class SuccessButton : FlatButton
{
    public SuccessButton()
    {
        BaseColor = Theme.Success;
        HoverColor = Color.FromArgb(5, 150, 105);
    }
}
