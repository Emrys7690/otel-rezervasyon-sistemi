using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms.Views;

public class MusaitOdaView : UserControl
{
    private readonly DateTimePicker _giris = new() { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today, Font = Theme.Body };
    private readonly DateTimePicker _cikis = new() { Format = DateTimePickerFormat.Short, Width = 120, Value = DateTime.Today.AddDays(1), Font = Theme.Body };
    private readonly NumericUpDown _kisi = new() { Minimum = 1, Maximum = 10, Value = 1, Width = 70, Font = Theme.Body };
    private readonly FlatButton _ara = new() { Text = "Ara", Width = 90 };
    private readonly SuccessButton _rezervasyonYap = new() { Text = "Bu Odaya Rezervasyon", Width = 200 };
    private readonly DataGridView _grid;

    public MusaitOdaView()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.AppBg;

        var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var filterRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 12)
        };
        filterRow.Controls.Add(Styler.FieldLabel("Giriş"));
        filterRow.Controls.Add(_giris);
        filterRow.Controls.Add(new Panel { Width = 16, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(Styler.FieldLabel("Çıkış"));
        filterRow.Controls.Add(_cikis);
        filterRow.Controls.Add(new Panel { Width = 16, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(Styler.FieldLabel("Kişi"));
        filterRow.Controls.Add(_kisi);
        filterRow.Controls.Add(new Panel { Width = 12, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(_ara);
        filterRow.Controls.Add(new Panel { Width = 12, Height = 1, BackColor = Color.Transparent });
        filterRow.Controls.Add(_rezervasyonYap);

        _grid = Styler.Grid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Oda No", DataPropertyName = nameof(Oda.OdaNumarasi), Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Tip", DataPropertyName = "TipAdi", Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = "Kapasite", DataPropertyName = nameof(Oda.Kapasite), Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Kat", DataPropertyName = nameof(Oda.Kat), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Ücret/Gece", DataPropertyName = nameof(Oda.GecelikUcret), Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Açıklama", DataPropertyName = nameof(Oda.Aciklama) }
        );
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) RezervasyonYap(); };

        card.Controls.Add(_grid);
        card.Controls.Add(filterRow);
        Controls.Add(card);

        _ara.Click += (_, _) => Ara();
        _rezervasyonYap.Click += (_, _) => RezervasyonYap();

        Ara();
    }

    private void Ara()
    {
        if (_cikis.Value.Date <= _giris.Value.Date)
        {
            MessageBox.Show("Çıkış tarihi girişten sonra olmalı.", "Hatalı tarih", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var liste = OdaRepository.MusaitOdalar(_giris.Value.Date, _cikis.Value.Date, (int)_kisi.Value);
        _grid.DataSource = liste.Select(o => new OdaView.OdaSatir(o)).ToList();
    }

    private void RezervasyonYap()
    {
        if (_grid.CurrentRow?.DataBoundItem is not OdaView.OdaSatir satir) return;
        var oda = satir.Oda;
        var gece = (_cikis.Value.Date - _giris.Value.Date).Days;
        var taslak = new Rezervasyon
        {
            OdaId = oda.Id,
            GirisTarihi = _giris.Value.Date,
            CikisTarihi = _cikis.Value.Date,
            KisiSayisi = (int)_kisi.Value,
            ToplamUcret = oda.GecelikUcret * gece,
            Durum = RezervasyonDurumu.Onaylandi,
            OlusturmaTarihi = DateTime.Now
        };
        using var f = new YeniRezervasyonForm(taslak);
        if (f.ShowDialog(this) == DialogResult.OK) Ara();
    }
}
