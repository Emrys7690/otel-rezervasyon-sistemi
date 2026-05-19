using System.ComponentModel;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms;

public class OdaDuzenleForm : Form
{
    private readonly TextBox _numaraBox = Styler.Input(null, 200);
    private readonly ComboBox _tipBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Body, Height = 32 };
    private readonly NumericUpDown _kapasiteBox = new() { Minimum = 1, Maximum = 10, Value = 1, Font = Theme.Body };
    private readonly NumericUpDown _ucretBox = new() { Minimum = 0, Maximum = 1_000_000, DecimalPlaces = 2, Increment = 50, Value = 1000, Font = Theme.Body };
    private readonly NumericUpDown _katBox = new() { Minimum = 0, Maximum = 50, Value = 1, Font = Theme.Body };
    private readonly TextBox _aciklamaBox = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.Body, BorderStyle = BorderStyle.FixedSingle };
    private readonly CheckBox _aktifBox = new() { Text = "Aktif (rezervasyona açık)", Checked = true, Font = Theme.Body, ForeColor = Theme.TextBody, AutoSize = true };

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Oda Oda { get; private set; }

    public OdaDuzenleForm(Oda? mevcut = null)
    {
        Oda = mevcut ?? new Oda { Aktif = true, Kapasite = 1, Kat = 1, GecelikUcret = 1000 };

        Text = mevcut == null ? "Yeni Oda" : $"Oda Düzenle  •  {mevcut.OdaNumarasi}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 480);
        BackColor = Theme.CardBg;
        Font = Theme.Body;

        foreach (OdaTipi t in Enum.GetValues<OdaTipi>())
            _tipBox.Items.Add(new TipItem(t));
        _tipBox.SelectedIndex = 0;

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 20, 28, 8), BackColor = Theme.CardBg };
        var altPanel = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.FromArgb(248, 250, 252) };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2, RowCount = 8,
            BackColor = Theme.CardBg
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Satir(int r, string lbl, Control c, int h = 36)
        {
            grid.Controls.Add(Styler.FieldLabel(lbl), 0, r);
            c.Dock = DockStyle.Fill;
            grid.Controls.Add(c, 1, r);
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
        }

        Satir(0, "Oda No",       _numaraBox);
        Satir(1, "Tip",          _tipBox);
        Satir(2, "Kapasite",     _kapasiteBox);
        Satir(3, "Gecelik Ücret",_ucretBox);
        Satir(4, "Kat",          _katBox);

        grid.Controls.Add(Styler.FieldLabel("Açıklama"), 0, 5);
        _aciklamaBox.Dock = DockStyle.Fill;
        grid.Controls.Add(_aciklamaBox, 1, 5);
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        grid.Controls.Add(new Panel(), 0, 6);
        grid.Controls.Add(_aktifBox, 1, 6);
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        content.Controls.Add(grid);

        var iptal = new SecondaryButton { Text = "İptal", Width = 100, DialogResult = DialogResult.Cancel };
        var kaydet = new FlatButton { Text = "Kaydet", Width = 110 };
        kaydet.Click += KaydetTiklandi;

        kaydet.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        iptal.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        kaydet.Location = new Point(altPanel.Width - 130, 14);
        iptal.Location = new Point(altPanel.Width - 244, 14);
        altPanel.Controls.Add(kaydet);
        altPanel.Controls.Add(iptal);

        var ust = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Theme.CardBorder };
        altPanel.Controls.Add(ust);

        Controls.Add(content);
        Controls.Add(altPanel);
        AcceptButton = kaydet;
        CancelButton = iptal;

        FormuDoldur();
    }

    private void FormuDoldur()
    {
        _numaraBox.Text = Oda.OdaNumarasi;
        for (int i = 0; i < _tipBox.Items.Count; i++)
            if (((TipItem)_tipBox.Items[i]!).Tip == Oda.Tip) { _tipBox.SelectedIndex = i; break; }
        _kapasiteBox.Value = Math.Clamp(Oda.Kapasite, (int)_kapasiteBox.Minimum, (int)_kapasiteBox.Maximum);
        _ucretBox.Value = Math.Clamp(Oda.GecelikUcret, _ucretBox.Minimum, _ucretBox.Maximum);
        _katBox.Value = Math.Clamp(Oda.Kat, (int)_katBox.Minimum, (int)_katBox.Maximum);
        _aciklamaBox.Text = Oda.Aciklama;
        _aktifBox.Checked = Oda.Aktif;
    }

    private void KaydetTiklandi(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_numaraBox.Text))
        {
            MessageBox.Show("Oda numarası zorunlu.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Oda.OdaNumarasi = _numaraBox.Text.Trim();
        Oda.Tip = ((TipItem)_tipBox.SelectedItem!).Tip;
        Oda.Kapasite = (int)_kapasiteBox.Value;
        Oda.GecelikUcret = _ucretBox.Value;
        Oda.Kat = (int)_katBox.Value;
        Oda.Aciklama = _aciklamaBox.Text.Trim();
        Oda.Aktif = _aktifBox.Checked;
        DialogResult = DialogResult.OK;
        Close();
    }

    private sealed record TipItem(OdaTipi Tip)
    {
        public override string ToString() => Tip switch
        {
            OdaTipi.TekKisilik => "Tek Kişilik",
            OdaTipi.CiftKisilik => "Çift Kişilik",
            OdaTipi.UcKisilik => "Üç Kişilik",
            OdaTipi.Suit => "Suit",
            OdaTipi.Aile => "Aile",
            _ => Tip.ToString()
        };
    }
}
