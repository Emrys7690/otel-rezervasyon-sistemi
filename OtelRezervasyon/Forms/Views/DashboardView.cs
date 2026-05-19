using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms.Views;

public class DashboardView : UserControl
{
    private readonly StatCard _toplamOda = new() { Label = "TOPLAM ODA", Icon = "🛏", Accent = Theme.Info };
    private readonly StatCard _konaklayan = new() { Label = "ŞU AN KONAKLAYAN", Icon = "🏨", Accent = Theme.Primary };
    private readonly StatCard _bugunGiris = new() { Label = "BUGÜN CHECK-IN", Icon = "📥", Accent = Theme.Success };
    private readonly StatCard _bugunCikis = new() { Label = "BUGÜN CHECK-OUT", Icon = "📤", Accent = Theme.Warning };
    private readonly StatCard _ayDoluluk = new() { Label = "BU AY DOLULUK", Icon = "📊", Accent = Theme.Info };
    private readonly StatCard _ayGelir = new() { Label = "BU AY GELİR", Icon = "💰", Accent = Theme.Success };

    private readonly DataGridView _yaklasanRez;

    public DashboardView()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.AppBg;

        var statRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top, Height = 130, ColumnCount = 6, RowCount = 1,
            BackColor = Theme.AppBg
        };
        for (int i = 0; i < 6; i++) statRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
        foreach (var card in new[] { _toplamOda, _konaklayan, _bugunGiris, _bugunCikis, _ayDoluluk, _ayGelir })
        {
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 12, 0);
            statRow.Controls.Add(card);
        }
        // Son kart için sağ margin sıfırla
        _ayGelir.Margin = new Padding(0);

        var bosluk = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = Theme.AppBg };

        var listeCard = new Card { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 20) };
        var listeBaslik = new Label
        {
            Text = "Yaklaşan Rezervasyonlar",
            Font = Theme.H3,
            ForeColor = Theme.TextDark,
            Dock = DockStyle.Top,
            Height = 36,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _yaklasanRez = Styler.Grid();
        _yaklasanRez.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Müşteri", DataPropertyName = "MusteriAdi" },
            new DataGridViewTextBoxColumn { HeaderText = "Oda", DataPropertyName = "OdaNo", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Giriş", DataPropertyName = "Giris",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd MMM yyyy", Padding = new Padding(12, 0, 0, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Çıkış", DataPropertyName = "Cikis",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd MMM yyyy", Padding = new Padding(12, 0, 0, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Gece", DataPropertyName = "Gece", Width = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = "Tutar",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", DataPropertyName = "DurumAdi", Width = 120 }
        );

        listeCard.Controls.Add(_yaklasanRez);
        listeCard.Controls.Add(listeBaslik);

        Controls.Add(listeCard);
        Controls.Add(bosluk);
        Controls.Add(statRow);

        Yukle();
    }

    private void Yukle()
    {
        var odalar = OdaRepository.Listele();
        _toplamOda.Value = odalar.Count(o => o.Aktif).ToString();
        _toplamOda.Sub = $"{odalar.Count} kayıt";

        _konaklayan.Value = RezervasyonRepository.SuandaKonaklayan().ToString();
        _konaklayan.Sub = "şu anda otelde";

        _bugunGiris.Value = RezervasyonRepository.BugunGirisSayisi().ToString();
        _bugunGiris.Sub = "bugün giriş";

        _bugunCikis.Value = RezervasyonRepository.BugunCikisSayisi().ToString();
        _bugunCikis.Sub = "bugün çıkış";

        var ayBas = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var ayBit = ayBas.AddMonths(1);

        var (rezGece, toplamGece) = RezervasyonRepository.DolulukVerisi(ayBas, ayBit);
        var oran = toplamGece == 0 ? 0 : (double)rezGece / toplamGece * 100;
        _ayDoluluk.Value = $"%{oran:N0}";
        _ayDoluluk.Sub = $"{rezGece}/{toplamGece} oda-gecesi";

        var gelir = RezervasyonRepository.ToplamGelir(ayBas, ayBit);
        _ayGelir.Value = $"₺{gelir:N0}";
        _ayGelir.Sub = ayBas.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));

        var yaklasan = RezervasyonRepository.Listele(null, DateTime.Today, DateTime.Today.AddMonths(2))
            .Where(r => r.Durum != RezervasyonDurumu.IptalEdildi && r.Durum != RezervasyonDurumu.Tamamlandi)
            .OrderBy(r => r.GirisTarihi)
            .Take(10)
            .Select(r => new
            {
                MusteriAdi = r.Musteri?.TamAd ?? "",
                OdaNo = r.Oda?.OdaNumarasi ?? "",
                Giris = r.GirisTarihi,
                Cikis = r.CikisTarihi,
                Gece = r.GeceSayisi,
                Tutar = r.ToplamUcret,
                DurumAdi = DurumAdi(r.Durum)
            })
            .ToList();
        _yaklasanRez.DataSource = yaklasan;
    }

    public static string DurumAdi(RezervasyonDurumu d) => d switch
    {
        RezervasyonDurumu.Beklemede => "Beklemede",
        RezervasyonDurumu.Onaylandi => "Onaylandı",
        RezervasyonDurumu.GirisYapildi => "Giriş Yapıldı",
        RezervasyonDurumu.Tamamlandi => "Tamamlandı",
        RezervasyonDurumu.IptalEdildi => "İptal",
        _ => d.ToString()
    };
}
