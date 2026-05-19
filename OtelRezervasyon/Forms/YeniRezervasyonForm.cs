using System.ComponentModel;
using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms;

public class YeniRezervasyonForm : Form
{
    private readonly ComboBox _musteri = new() { DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems, Font = Theme.Body, Height = 32 };
    private readonly FlatButton _musteriEkle = new() { Text = "+", Width = 36, Height = 32 };
    private readonly DateTimePicker _giris = new() { Format = DateTimePickerFormat.Short, Font = Theme.Body };
    private readonly DateTimePicker _cikis = new() { Format = DateTimePickerFormat.Short, Font = Theme.Body };
    private readonly ComboBox _oda = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Body, Height = 32 };
    private readonly NumericUpDown _kisi = new() { Minimum = 1, Maximum = 10, Value = 1, Font = Theme.Body };
    private readonly ComboBox _durum = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Body, Height = 32 };
    private readonly TextBox _notlar = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Font = Theme.Body, BorderStyle = BorderStyle.FixedSingle };

    private readonly Label _toplam = new() { Text = "Toplam: ₺0,00", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Theme.Primary, AutoSize = true };
    private readonly Label _bilgi = new() { Text = "", Font = Theme.Small, ForeColor = Theme.TextMuted, AutoSize = true };

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Rezervasyon Rezervasyon { get; private set; }
    private bool _duzenleme;

    public YeniRezervasyonForm(Rezervasyon? mevcut = null)
    {
        _duzenleme = mevcut is { Id: > 0 };
        Rezervasyon = mevcut ?? new Rezervasyon
        {
            GirisTarihi = DateTime.Today,
            CikisTarihi = DateTime.Today.AddDays(1),
            KisiSayisi = 1,
            OlusturmaTarihi = DateTime.Now,
            Durum = RezervasyonDurumu.Onaylandi
        };

        Text = _duzenleme ? $"Rezervasyon Düzenle  •  #{Rezervasyon.Id}" : "Yeni Rezervasyon";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(620, 580);
        BackColor = Theme.CardBg;
        Font = Theme.Body;

        _giris.Value = Rezervasyon.GirisTarihi == default ? DateTime.Today : Rezervasyon.GirisTarihi;
        _cikis.Value = Rezervasyon.CikisTarihi == default ? DateTime.Today.AddDays(1) : Rezervasyon.CikisTarihi;
        _kisi.Value = Math.Max(1, Rezervasyon.KisiSayisi);

        foreach (RezervasyonDurumu d in Enum.GetValues<RezervasyonDurumu>())
            _durum.Items.Add(new DurumItem(d));
        DurumSec(Rezervasyon.Durum);

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 20, 28, 8), BackColor = Theme.CardBg };
        var altPanel = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Color.FromArgb(248, 250, 252) };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 3, BackColor = Theme.CardBg
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));

        void Satir(int r, string lbl, Control c, Control? sag = null, int h = 38)
        {
            grid.Controls.Add(Styler.FieldLabel(lbl), 0, r);
            c.Dock = DockStyle.Fill;
            grid.Controls.Add(c, 1, r);
            if (sag != null) { grid.Controls.Add(sag, 2, r); }
            else { grid.SetColumnSpan(c, 2); }
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
        }

        Satir(0, "Müşteri",      _musteri, _musteriEkle);
        Satir(1, "Giriş Tarihi", _giris);
        Satir(2, "Çıkış Tarihi", _cikis);
        Satir(3, "Kişi Sayısı",  _kisi);
        Satir(4, "Oda",          _oda);
        Satir(5, "Durum",        _durum);

        grid.Controls.Add(Styler.FieldLabel("Notlar"), 0, 6);
        _notlar.Dock = DockStyle.Fill;
        grid.SetColumnSpan(_notlar, 2);
        grid.Controls.Add(_notlar, 1, 6);
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        content.Controls.Add(grid);

        _toplam.Location = new Point(28, 16);
        _bilgi.Location = new Point(28, 50);

        var iptal = new SecondaryButton { Text = "İptal", Width = 100, DialogResult = DialogResult.Cancel };
        var kaydet = new FlatButton { Text = _duzenleme ? "Güncelle" : "Rezervasyonu Oluştur", Width = 180 };
        kaydet.Click += KaydetTiklandi;
        kaydet.Anchor = iptal.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        kaydet.Location = new Point(altPanel.Width - 200, 26);
        iptal.Location = new Point(altPanel.Width - 312, 26);

        altPanel.Controls.Add(_toplam);
        altPanel.Controls.Add(_bilgi);
        altPanel.Controls.Add(kaydet);
        altPanel.Controls.Add(iptal);
        altPanel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Theme.CardBorder });

        Controls.Add(content);
        Controls.Add(altPanel);
        AcceptButton = kaydet;
        CancelButton = iptal;

        MusteriListesiniYukle();
        _musteri.SelectedIndexChanged += (_, _) => GuncellemeHesapla();
        _giris.ValueChanged += (_, _) => { TarihKontrol(); OdalariYenile(); };
        _cikis.ValueChanged += (_, _) => { TarihKontrol(); OdalariYenile(); };
        _kisi.ValueChanged += (_, _) => OdalariYenile();
        _oda.SelectedIndexChanged += (_, _) => GuncellemeHesapla();
        _musteriEkle.Click += MusteriEkleTiklandi;

        OdalariYenile();
    }

    private void MusteriListesiniYukle()
    {
        var liste = MusteriRepository.Listele();
        _musteri.Items.Clear();
        foreach (var m in liste) _musteri.Items.Add(new MusteriItem(m));
        if (Rezervasyon.MusteriId > 0)
            for (int i = 0; i < _musteri.Items.Count; i++)
                if (((MusteriItem)_musteri.Items[i]!).Musteri.Id == Rezervasyon.MusteriId)
                { _musteri.SelectedIndex = i; break; }
    }

    private void OdalariYenile()
    {
        var seciliOdaId = (_oda.SelectedItem as OdaItem)?.Oda.Id ?? Rezervasyon.OdaId;
        var liste = OdaRepository.MusaitOdalar(_giris.Value.Date, _cikis.Value.Date, (int)_kisi.Value);
        if (_duzenleme && Rezervasyon.OdaId > 0 && liste.All(o => o.Id != Rezervasyon.OdaId))
        {
            var orijinal = OdaRepository.Getir(Rezervasyon.OdaId);
            if (orijinal != null) liste.Insert(0, orijinal);
        }
        _oda.Items.Clear();
        foreach (var o in liste) _oda.Items.Add(new OdaItem(o));
        for (int i = 0; i < _oda.Items.Count; i++)
            if (((OdaItem)_oda.Items[i]!).Oda.Id == seciliOdaId) { _oda.SelectedIndex = i; break; }
        if (_oda.SelectedIndex < 0 && _oda.Items.Count > 0) _oda.SelectedIndex = 0;
        GuncellemeHesapla();
    }

    private void TarihKontrol()
    {
        if (_cikis.Value.Date <= _giris.Value.Date) _cikis.Value = _giris.Value.AddDays(1);
    }

    private void GuncellemeHesapla()
    {
        var oda = (_oda.SelectedItem as OdaItem)?.Oda;
        var gece = Math.Max(0, (_cikis.Value.Date - _giris.Value.Date).Days);
        var toplam = oda == null ? 0m : oda.GecelikUcret * gece;
        _toplam.Text = $"Toplam: ₺{toplam:N2}";
        _bilgi.Text = oda == null
            ? "Seçili tarihlerde uygun oda yok."
            : $"{oda.OdaNumarasi}  •  {gece} gece  •  ₺{oda.GecelikUcret:N2} / gece";
    }

    private void DurumSec(RezervasyonDurumu d)
    {
        for (int i = 0; i < _durum.Items.Count; i++)
            if (((DurumItem)_durum.Items[i]!).Durum == d) { _durum.SelectedIndex = i; return; }
        _durum.SelectedIndex = 0;
    }

    private void MusteriEkleTiklandi(object? sender, EventArgs e)
    {
        using var f = new MusteriDuzenleForm();
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            MusteriRepository.Ekle(f.Musteri);
            Rezervasyon.MusteriId = f.Musteri.Id;
            MusteriListesiniYukle();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void KaydetTiklandi(object? sender, EventArgs e)
    {
        var musteri = (_musteri.SelectedItem as MusteriItem)?.Musteri;
        var oda = (_oda.SelectedItem as OdaItem)?.Oda;
        if (musteri == null) { Uyari("Müşteri seçin."); return; }
        if (oda == null) { Uyari("Oda seçin."); return; }
        if (_cikis.Value.Date <= _giris.Value.Date) { Uyari("Çıkış tarihi girişten sonra olmalı."); return; }
        if (_kisi.Value > oda.Kapasite) { Uyari($"Bu odanın kapasitesi {oda.Kapasite}."); return; }

        var gece = (_cikis.Value.Date - _giris.Value.Date).Days;
        Rezervasyon.MusteriId = musteri.Id;
        Rezervasyon.OdaId = oda.Id;
        Rezervasyon.GirisTarihi = _giris.Value.Date;
        Rezervasyon.CikisTarihi = _cikis.Value.Date;
        Rezervasyon.KisiSayisi = (int)_kisi.Value;
        Rezervasyon.ToplamUcret = oda.GecelikUcret * gece;
        Rezervasyon.Durum = ((DurumItem)_durum.SelectedItem!).Durum;
        Rezervasyon.Notlar = _notlar.Text.Trim();
        if (!_duzenleme) Rezervasyon.OlusturmaTarihi = DateTime.Now;

        try
        {
            if (_duzenleme) RezervasyonRepository.Guncelle(Rezervasyon);
            else RezervasyonRepository.Ekle(Rezervasyon);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Kaydedilemedi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Uyari(string mesaj) =>
        MessageBox.Show(mesaj, "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private sealed record MusteriItem(Musteri Musteri)
    {
        public override string ToString() =>
            string.IsNullOrEmpty(Musteri.TcKimlikNo) ? Musteri.TamAd : $"{Musteri.TamAd}  ({Musteri.TcKimlikNo})";
    }

    private sealed record OdaItem(Oda Oda)
    {
        public override string ToString() =>
            $"{Oda.OdaNumarasi}  •  {Oda.Tip}  •  {Oda.Kapasite} kişi  •  ₺{Oda.GecelikUcret:N0}";
    }

    private sealed record DurumItem(RezervasyonDurumu Durum)
    {
        public override string ToString() => Durum switch
        {
            RezervasyonDurumu.Beklemede => "Beklemede",
            RezervasyonDurumu.Onaylandi => "Onaylandı",
            RezervasyonDurumu.GirisYapildi => "Giriş Yapıldı",
            RezervasyonDurumu.Tamamlandi => "Tamamlandı",
            RezervasyonDurumu.IptalEdildi => "İptal Edildi",
            _ => Durum.ToString()
        };
    }
}
