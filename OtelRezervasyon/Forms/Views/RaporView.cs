using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms.Views;

public class RaporView : UserControl
{
    private readonly DateTimePicker _bas = new() { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today.AddDays(-30), Font = Theme.Body };
    private readonly DateTimePicker _bit = new() { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today, Font = Theme.Body };
    private readonly FlatButton _hesapla = new() { Text = "Hesapla", Width = 110 };

    private readonly StatCard _gelirCard  = new() { Label = "TOPLAM GELİR",   Icon = "💰", Accent = Theme.Success };
    private readonly StatCard _dolulukCard = new() { Label = "DOLULUK ORANI", Icon = "📊", Accent = Theme.Primary };
    private readonly StatCard _rezSayiCard = new() { Label = "REZERVASYON",   Icon = "📋", Accent = Theme.Info };

    private readonly DataGridView _grid;

    public RaporView()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.AppBg;

        var filterCard = new Card { Dock = DockStyle.Top, Height = 90, Padding = new Padding(20, 16, 20, 16) };
        var filterRow = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        filterRow.Controls.Add(Styler.FieldLabel("Başlangıç"));
        filterRow.Controls.Add(_bas);
        filterRow.Controls.Add(new Panel { Width = 16, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(Styler.FieldLabel("Bitiş"));
        filterRow.Controls.Add(_bit);
        filterRow.Controls.Add(new Panel { Width = 16, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(_hesapla);
        filterCard.Controls.Add(filterRow);

        var bosluk1 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Theme.AppBg };

        var statRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top, Height = 130, ColumnCount = 3, RowCount = 1,
            BackColor = Theme.AppBg
        };
        for (int i = 0; i < 3; i++) statRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        _gelirCard.Margin = new Padding(0, 0, 12, 0);
        _dolulukCard.Margin = new Padding(0, 0, 12, 0);
        _rezSayiCard.Margin = new Padding(0);
        statRow.Controls.Add(_gelirCard);
        statRow.Controls.Add(_dolulukCard);
        statRow.Controls.Add(_rezSayiCard);

        var bosluk2 = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Theme.AppBg };

        var listeCard = new Card { Dock = DockStyle.Fill, Padding = new Padding(20) };
        var baslik = new Label
        {
            Text = "Dönem Rezervasyonları",
            Font = Theme.H3, ForeColor = Theme.TextDark,
            Dock = DockStyle.Top, Height = 36, TextAlign = ContentAlignment.MiddleLeft
        };

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
            new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = "Tutar",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", DataPropertyName = "DurumAdi", Width = 120 }
        );

        listeCard.Controls.Add(_grid);
        listeCard.Controls.Add(baslik);

        Controls.Add(listeCard);
        Controls.Add(bosluk2);
        Controls.Add(statRow);
        Controls.Add(bosluk1);
        Controls.Add(filterCard);

        _hesapla.Click += (_, _) => Hesapla();
        Hesapla();
    }

    private void Hesapla()
    {
        if (_bit.Value.Date <= _bas.Value.Date)
        {
            MessageBox.Show("Bitiş tarihi başlangıçtan sonra olmalı.", "Hatalı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var rezler = RezervasyonRepository.Listele(null, _bas.Value.Date, _bit.Value.Date.AddDays(1))
            .Where(r => r.Durum != RezervasyonDurumu.IptalEdildi)
            .ToList();

        var toplamGelir = rezler.Sum(r => r.ToplamUcret);
        _gelirCard.Value = $"₺{toplamGelir:N0}";
        _gelirCard.Sub = $"{_bas.Value:dd MMM} – {_bit.Value:dd MMM}";

        var (rezGece, toplamGece) = RezervasyonRepository.DolulukVerisi(_bas.Value.Date, _bit.Value.Date.AddDays(1));
        var oran = toplamGece == 0 ? 0 : (double)rezGece / toplamGece * 100;
        _dolulukCard.Value = $"%{oran:N1}";
        _dolulukCard.Sub = $"{rezGece}/{toplamGece} oda-gecesi";

        _rezSayiCard.Value = rezler.Count.ToString();
        _rezSayiCard.Sub = "iptal hariç";

        _grid.DataSource = rezler.Select(r => new RezervasyonView.RezSatir(r)).ToList();
    }
}
