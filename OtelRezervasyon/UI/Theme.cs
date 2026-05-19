namespace OtelRezervasyon.UI;

public static class Theme
{
    public static readonly Color Sidebar       = Color.FromArgb(15, 23, 42);     // slate-900
    public static readonly Color SidebarHover  = Color.FromArgb(30, 41, 59);     // slate-800
    public static readonly Color SidebarActive = Color.FromArgb(37, 99, 235);    // blue-600
    public static readonly Color SidebarText   = Color.FromArgb(148, 163, 184);  // slate-400
    public static readonly Color SidebarTextActive = Color.White;

    public static readonly Color AppBg         = Color.FromArgb(241, 245, 249);  // slate-100
    public static readonly Color CardBg        = Color.White;
    public static readonly Color CardBorder    = Color.FromArgb(226, 232, 240);  // slate-200

    public static readonly Color Primary       = Color.FromArgb(37, 99, 235);    // blue-600
    public static readonly Color PrimaryHover  = Color.FromArgb(29, 78, 216);    // blue-700
    public static readonly Color PrimaryText   = Color.White;

    public static readonly Color Success       = Color.FromArgb(16, 185, 129);   // emerald-500
    public static readonly Color Warning       = Color.FromArgb(245, 158, 11);   // amber-500
    public static readonly Color Danger        = Color.FromArgb(239, 68, 68);    // red-500
    public static readonly Color Info          = Color.FromArgb(14, 165, 233);   // sky-500

    public static readonly Color TextDark      = Color.FromArgb(15, 23, 42);     // slate-900
    public static readonly Color TextBody      = Color.FromArgb(51, 65, 85);     // slate-700
    public static readonly Color TextMuted     = Color.FromArgb(100, 116, 139);  // slate-500
    public static readonly Color TextHeader    = Color.FromArgb(71, 85, 105);    // slate-600

    public static readonly Color RowAlternate  = Color.FromArgb(248, 250, 252);  // slate-50
    public static readonly Color RowSelected   = Color.FromArgb(219, 234, 254);  // blue-100
    public static readonly Color GridLine      = Color.FromArgb(241, 245, 249);  // slate-100

    public static readonly Font H1     = new("Segoe UI", 22, FontStyle.Bold);
    public static readonly Font H2     = new("Segoe UI", 16, FontStyle.Bold);
    public static readonly Font H3     = new("Segoe UI", 12, FontStyle.Bold);
    public static readonly Font Body   = new("Segoe UI", 10, FontStyle.Regular);
    public static readonly Font BodyBold = new("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font Small  = new("Segoe UI", 9, FontStyle.Regular);
    public static readonly Font Stat   = new("Segoe UI", 24, FontStyle.Bold);
    public static readonly Font Button = new("Segoe UI", 10, FontStyle.Regular);
}
