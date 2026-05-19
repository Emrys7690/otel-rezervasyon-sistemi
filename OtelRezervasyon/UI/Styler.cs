namespace OtelRezervasyon.UI;

public static class Styler
{
    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Theme.CardBg,
            BorderStyle = BorderStyle.None,
            GridColor = Theme.GridLine,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeight = 44,
            RowTemplate = { Height = 40 },
            Font = Theme.Body
        };
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Theme.TextHeader,
            Font = Theme.BodyBold,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            SelectionBackColor = Color.White,
            SelectionForeColor = Theme.TextHeader,
            WrapMode = DataGridViewTriState.False
        };
        g.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Theme.TextBody,
            Font = Theme.Body,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            SelectionBackColor = Theme.RowSelected,
            SelectionForeColor = Theme.TextDark,
            WrapMode = DataGridViewTriState.False
        };
        g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Theme.RowAlternate,
            ForeColor = Theme.TextBody,
            SelectionBackColor = Theme.RowSelected,
            SelectionForeColor = Theme.TextDark,
            Padding = new Padding(12, 0, 0, 0)
        };
        return g;
    }

    public static TextBox Input(string? placeholder = null, int width = 240)
    {
        var t = new TextBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            Font = Theme.Body,
            Width = width,
            Height = 32,
            BackColor = Color.White,
            ForeColor = Theme.TextDark
        };
        if (placeholder != null) t.PlaceholderText = placeholder;
        return t;
    }

    public static Label SectionTitle(string text) => new()
    {
        Text = text,
        Font = Theme.H2,
        ForeColor = Theme.TextDark,
        AutoSize = true
    };

    public static Label FieldLabel(string text) => new()
    {
        Text = text,
        Font = Theme.Body,
        ForeColor = Theme.TextMuted,
        AutoSize = true,
        Padding = new Padding(0, 8, 0, 4)
    };

    public static Label MutedText(string text) => new()
    {
        Text = text,
        Font = Theme.Small,
        ForeColor = Theme.TextMuted,
        AutoSize = true
    };
}
