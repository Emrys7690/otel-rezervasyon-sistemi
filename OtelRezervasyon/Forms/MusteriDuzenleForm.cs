using System.ComponentModel;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms;

public class MusteriDuzenleForm : Form
{
    private readonly TextBox _ad = Styler.Input(null, 280);
    private readonly TextBox _soyad = Styler.Input(null, 280);
    private readonly TextBox _tc = Styler.Input(null, 280);
    private readonly TextBox _telefon = Styler.Input(null, 280);
    private readonly TextBox _eposta = Styler.Input(null, 280);
    private readonly TextBox _adres = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.Body, BorderStyle = BorderStyle.FixedSingle };

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Musteri Musteri { get; private set; }

    public MusteriDuzenleForm(Musteri? mevcut = null)
    {
        Musteri = mevcut ?? new Musteri { KayitTarihi = DateTime.Now };

        Text = mevcut == null ? "Yeni Müşteri" : $"Müşteri Düzenle  •  {mevcut.TamAd}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(500, 460);
        BackColor = Theme.CardBg;
        Font = Theme.Body;

        _tc.MaxLength = 20;

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 20, 28, 8), BackColor = Theme.CardBg };
        var altPanel = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.FromArgb(248, 250, 252) };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, BackColor = Theme.CardBg
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

        Satir(0, "Ad",            _ad);
        Satir(1, "Soyad",         _soyad);
        Satir(2, "TC / Pasaport", _tc);
        Satir(3, "Telefon",       _telefon);
        Satir(4, "E-posta",       _eposta);

        grid.Controls.Add(Styler.FieldLabel("Adres"), 0, 5);
        _adres.Dock = DockStyle.Fill;
        grid.Controls.Add(_adres, 1, 5);
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        content.Controls.Add(grid);

        var iptal = new SecondaryButton { Text = "İptal", Width = 100, DialogResult = DialogResult.Cancel };
        var kaydet = new FlatButton { Text = "Kaydet", Width = 110 };
        kaydet.Click += KaydetTiklandi;
        kaydet.Anchor = iptal.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        kaydet.Location = new Point(altPanel.Width - 130, 14);
        iptal.Location = new Point(altPanel.Width - 244, 14);
        altPanel.Controls.Add(kaydet);
        altPanel.Controls.Add(iptal);
        altPanel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Theme.CardBorder });

        Controls.Add(content);
        Controls.Add(altPanel);
        AcceptButton = kaydet;
        CancelButton = iptal;

        _ad.Text = Musteri.Ad;
        _soyad.Text = Musteri.Soyad;
        _tc.Text = Musteri.TcKimlikNo;
        _telefon.Text = Musteri.Telefon;
        _eposta.Text = Musteri.Eposta;
        _adres.Text = Musteri.Adres;
    }

    private void KaydetTiklandi(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_ad.Text) || string.IsNullOrWhiteSpace(_soyad.Text))
        {
            MessageBox.Show("Ad ve soyad zorunlu.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Musteri.Ad = _ad.Text.Trim();
        Musteri.Soyad = _soyad.Text.Trim();
        Musteri.TcKimlikNo = _tc.Text.Trim();
        Musteri.Telefon = _telefon.Text.Trim();
        Musteri.Eposta = _eposta.Text.Trim();
        Musteri.Adres = _adres.Text.Trim();
        DialogResult = DialogResult.OK;
        Close();
    }
}
