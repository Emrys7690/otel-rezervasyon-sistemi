using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms.Views;

public class MusteriView : UserControl
{
    private readonly DataGridView _grid;
    private readonly TextBox _arama = Styler.Input("Ad, soyad, TC, telefon...", 280);
    private readonly FlatButton _yeni = new() { Text = "+ Yeni Müşteri", Width = 150 };
    private readonly SecondaryButton _duzenle = new() { Text = "Düzenle", Width = 100 };
    private readonly DangerButton _sil = new() { Text = "Sil", Width = 80 };

    public MusteriView()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.AppBg;

        var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 12)
        };
        toolbar.Controls.AddRange(new Control[] { _yeni, _duzenle, _sil });

        var aramaPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 12)
        };
        aramaPanel.Controls.Add(Styler.FieldLabel("Ara"));
        aramaPanel.Controls.Add(_arama);

        _grid = Styler.Grid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Ad", DataPropertyName = nameof(Musteri.Ad), Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = "Soyad", DataPropertyName = nameof(Musteri.Soyad), Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = "TC / Pasaport", DataPropertyName = nameof(Musteri.TcKimlikNo), Width = 160 },
            new DataGridViewTextBoxColumn { HeaderText = "Telefon", DataPropertyName = nameof(Musteri.Telefon), Width = 140 },
            new DataGridViewTextBoxColumn { HeaderText = "E-posta", DataPropertyName = nameof(Musteri.Eposta) },
            new DataGridViewTextBoxColumn { HeaderText = "Kayıt", DataPropertyName = nameof(Musteri.KayitTarihi), Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy", Padding = new Padding(12, 0, 0, 0) } }
        );
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) DuzenleTiklandi(); };

        card.Controls.Add(_grid);
        card.Controls.Add(aramaPanel);
        card.Controls.Add(toolbar);
        Controls.Add(card);

        _yeni.Click += YeniTiklandi;
        _duzenle.Click += (_, _) => DuzenleTiklandi();
        _sil.Click += SilTiklandi;
        _arama.TextChanged += (_, _) => Yukle();

        Yukle();
    }

    private void Yukle()
    {
        _grid.DataSource = MusteriRepository.Listele(_arama.Text.Trim());
    }

    private Musteri? Secili() => _grid.CurrentRow?.DataBoundItem as Musteri;

    private void YeniTiklandi(object? sender, EventArgs e)
    {
        using var f = new MusteriDuzenleForm();
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try { MusteriRepository.Ekle(f.Musteri); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DuzenleTiklandi()
    {
        var m = Secili(); if (m == null) return;
        var kopya = new Musteri
        {
            Id = m.Id, Ad = m.Ad, Soyad = m.Soyad, TcKimlikNo = m.TcKimlikNo,
            Telefon = m.Telefon, Eposta = m.Eposta, Adres = m.Adres, KayitTarihi = m.KayitTarihi
        };
        using var f = new MusteriDuzenleForm(kopya);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try { MusteriRepository.Guncelle(f.Musteri); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void SilTiklandi(object? sender, EventArgs e)
    {
        var m = Secili(); if (m == null) return;
        if (MusteriRepository.RezervasyonuVarMi(m.Id))
        {
            MessageBox.Show($"\"{m.TamAd}\" adlı müşterinin rezervasyonu var, silinemez.",
                "Silinemez", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var onay = MessageBox.Show($"\"{m.TamAd}\" silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (onay != DialogResult.Yes) return;
        try { MusteriRepository.Sil(m.Id); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
