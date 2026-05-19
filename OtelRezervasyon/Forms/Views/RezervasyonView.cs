using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms.Views;

public class RezervasyonView : UserControl
{
    private readonly DataGridView _grid;
    private readonly ComboBox _durumFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, Font = Theme.Body, Height = 32 };
    private readonly DateTimePicker _bas = new() { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today.AddMonths(-1), Font = Theme.Body };
    private readonly DateTimePicker _bit = new() { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today.AddMonths(3), Font = Theme.Body };

    private readonly FlatButton _yeni = new() { Text = "+ Yeni Rezervasyon", Width = 170 };
    private readonly SecondaryButton _duzenle = new() { Text = "Düzenle", Width = 100 };
    private readonly SuccessButton _girisYap = new() { Text = "Check-in", Width = 110 };
    private readonly FlatButton _cikisYap = new() { Text = "Check-out", Width = 110 };
    private readonly DangerButton _iptal = new() { Text = "İptal", Width = 90 };

    public RezervasyonView()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.AppBg;

        _cikisYap.BaseColor = Theme.Warning;
        _cikisYap.HoverColor = Color.FromArgb(217, 119, 6);

        var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 8)
        };
        toolbar.Controls.AddRange(new Control[] { _yeni, _duzenle, _girisYap, _cikisYap, _iptal });

        var filterRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 8)
        };
        filterRow.Controls.Add(Styler.FieldLabel("Durum"));
        filterRow.Controls.Add(_durumFilter);
        filterRow.Controls.Add(new Panel { Width = 16, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(Styler.FieldLabel("Başlangıç"));
        filterRow.Controls.Add(_bas);
        filterRow.Controls.Add(new Panel { Width = 16, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(Styler.FieldLabel("Bitiş"));
        filterRow.Controls.Add(_bit);

        _durumFilter.Items.Add(new DurumFiltre(null, "Tüm Durumlar"));
        foreach (RezervasyonDurumu d in Enum.GetValues<RezervasyonDurumu>())
            _durumFilter.Items.Add(new DurumFiltre(d, DashboardView.DurumAdi(d)));
        _durumFilter.SelectedIndex = 0;

        _grid = Styler.Grid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "#", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { HeaderText = "Müşteri", DataPropertyName = "MusteriAdi" },
            new DataGridViewTextBoxColumn { HeaderText = "Oda", DataPropertyName = "OdaNo", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Giriş", DataPropertyName = "Giris", Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd MMM yyyy", Padding = new Padding(12, 0, 0, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Çıkış", DataPropertyName = "Cikis", Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd MMM yyyy", Padding = new Padding(12, 0, 0, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Gece", DataPropertyName = "Gece", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Kişi", DataPropertyName = "Kisi", Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = "Tutar",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", DataPropertyName = "DurumAdi", Width = 120 }
        );
        _grid.CellFormatting += GridFormat;
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) DuzenleTiklandi(); };

        card.Controls.Add(_grid);
        card.Controls.Add(filterRow);
        card.Controls.Add(toolbar);

        Controls.Add(card);

        _yeni.Click += YeniTiklandi;
        _duzenle.Click += (_, _) => DuzenleTiklandi();
        _girisYap.Click += (_, _) => DurumGuncelle(RezervasyonDurumu.GirisYapildi);
        _cikisYap.Click += (_, _) => DurumGuncelle(RezervasyonDurumu.Tamamlandi);
        _iptal.Click += (_, _) => DurumGuncelle(RezervasyonDurumu.IptalEdildi);
        _durumFilter.SelectedIndexChanged += (_, _) => Yukle();
        _bas.ValueChanged += (_, _) => Yukle();
        _bit.ValueChanged += (_, _) => Yukle();

        Yukle();
    }

    private void Yukle()
    {
        var durum = ((DurumFiltre)_durumFilter.SelectedItem!).Durum;
        var rezler = RezervasyonRepository.Listele(durum, _bas.Value.Date, _bit.Value.Date.AddDays(1));
        _grid.DataSource = rezler.Select(r => new RezSatir(r)).ToList();
    }

    private void GridFormat(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Rows[e.RowIndex].DataBoundItem is not RezSatir r) return;
        if (_grid.Columns[e.ColumnIndex].DataPropertyName != "DurumAdi") return;
        e.CellStyle!.ForeColor = r.Durum switch
        {
            RezervasyonDurumu.IptalEdildi => Theme.TextMuted,
            RezervasyonDurumu.Tamamlandi => Theme.Info,
            RezervasyonDurumu.GirisYapildi => Theme.Success,
            RezervasyonDurumu.Onaylandi => Theme.Primary,
            _ => Theme.Warning
        };
        e.CellStyle.Font = Theme.BodyBold;
    }

    private RezSatir? Secili() => _grid.CurrentRow?.DataBoundItem as RezSatir;

    private void YeniTiklandi(object? sender, EventArgs e)
    {
        using var f = new YeniRezervasyonForm();
        if (f.ShowDialog(this) == DialogResult.OK) Yukle();
    }

    private void DuzenleTiklandi()
    {
        var s = Secili(); if (s == null) return;
        var r = RezervasyonRepository.Getir(s.Id); if (r == null) return;
        using var f = new YeniRezervasyonForm(r);
        if (f.ShowDialog(this) == DialogResult.OK) Yukle();
    }

    private void DurumGuncelle(RezervasyonDurumu yeni)
    {
        var s = Secili(); if (s == null) return;
        var onay = MessageBox.Show(
            $"Rezervasyon #{s.Id} \"{DashboardView.DurumAdi(yeni)}\" olarak güncellensin mi?",
            "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (onay != DialogResult.Yes) return;
        try { RezervasyonRepository.DurumDegistir(s.Id, yeni); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private sealed record DurumFiltre(RezervasyonDurumu? Durum, string Ad)
    {
        public override string ToString() => Ad;
    }

    public class RezSatir
    {
        public Rezervasyon Rezervasyon { get; }
        public RezSatir(Rezervasyon r) { Rezervasyon = r; }
        public int Id => Rezervasyon.Id;
        public string MusteriAdi => Rezervasyon.Musteri?.TamAd ?? "";
        public string OdaNo => Rezervasyon.Oda?.OdaNumarasi ?? "";
        public DateTime Giris => Rezervasyon.GirisTarihi;
        public DateTime Cikis => Rezervasyon.CikisTarihi;
        public int Gece => Rezervasyon.GeceSayisi;
        public int Kisi => Rezervasyon.KisiSayisi;
        public decimal Tutar => Rezervasyon.ToplamUcret;
        public RezervasyonDurumu Durum => Rezervasyon.Durum;
        public string DurumAdi => DashboardView.DurumAdi(Rezervasyon.Durum);
    }
}
