using OtelRezervasyon.Data;
using OtelRezervasyon.Models;
using OtelRezervasyon.UI;

namespace OtelRezervasyon.Forms.Views;

public class OdaView : UserControl
{
    private readonly DataGridView _grid;
    private readonly FlatButton _yeni = new() { Text = "+ Yeni Oda", Width = 130 };
    private readonly SecondaryButton _duzenle = new() { Text = "Düzenle", Width = 100 };
    private readonly DangerButton _sil = new() { Text = "Sil", Width = 80 };

    public OdaView()
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

        _grid = Styler.Grid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Oda No", DataPropertyName = nameof(Oda.OdaNumarasi), Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Tip", DataPropertyName = "TipAdi", Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = "Kapasite", DataPropertyName = nameof(Oda.Kapasite), Width = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Kat", DataPropertyName = nameof(Oda.Kat), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Gecelik Ücret", DataPropertyName = nameof(Oda.GecelikUcret), Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) } },
            new DataGridViewTextBoxColumn { HeaderText = "Açıklama", DataPropertyName = nameof(Oda.Aciklama) },
            new DataGridViewTextBoxColumn { HeaderText = "Durum", DataPropertyName = "DurumAdi", Width = 90 }
        );
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Rows[e.RowIndex].DataBoundItem is not OdaSatir s) return;
            if (_grid.Columns[e.ColumnIndex].DataPropertyName == "DurumAdi")
            {
                e.CellStyle!.ForeColor = s.Aktif ? Theme.Success : Theme.TextMuted;
                e.CellStyle.Font = Theme.BodyBold;
            }
        };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) DuzenleTiklandi(); };

        card.Controls.Add(_grid);
        card.Controls.Add(toolbar);
        Controls.Add(card);

        _yeni.Click += YeniTiklandi;
        _duzenle.Click += (_, _) => DuzenleTiklandi();
        _sil.Click += SilTiklandi;

        Yukle();
    }

    private void Yukle()
    {
        _grid.DataSource = OdaRepository.Listele().Select(o => new OdaSatir(o)).ToList();
    }

    private OdaSatir? Secili() => _grid.CurrentRow?.DataBoundItem as OdaSatir;

    private void YeniTiklandi(object? sender, EventArgs e)
    {
        using var f = new OdaDuzenleForm();
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try { OdaRepository.Ekle(f.Oda); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DuzenleTiklandi()
    {
        var s = Secili(); if (s == null) return;
        var o = s.Oda;
        using var f = new OdaDuzenleForm(new Oda
        {
            Id = o.Id, OdaNumarasi = o.OdaNumarasi, Tip = o.Tip,
            Kapasite = o.Kapasite, GecelikUcret = o.GecelikUcret, Kat = o.Kat,
            Aciklama = o.Aciklama, Aktif = o.Aktif
        });
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try { OdaRepository.Guncelle(f.Oda); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void SilTiklandi(object? sender, EventArgs e)
    {
        var s = Secili(); if (s == null) return;
        if (OdaRepository.KullanimdaMi(s.Oda.Id))
        {
            MessageBox.Show($"\"{s.Oda.OdaNumarasi}\" odasının rezervasyonu var, silinemez.\nBunun yerine pasifleştirebilirsiniz.",
                "Silinemez", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var onay = MessageBox.Show($"\"{s.Oda.OdaNumarasi}\" silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (onay != DialogResult.Yes) return;
        try { OdaRepository.Sil(s.Oda.Id); Yukle(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    public class OdaSatir
    {
        public Oda Oda { get; }
        public OdaSatir(Oda o) { Oda = o; }
        public string OdaNumarasi => Oda.OdaNumarasi;
        public string TipAdi => Oda.Tip switch
        {
            OdaTipi.TekKisilik => "Tek Kişilik",
            OdaTipi.CiftKisilik => "Çift Kişilik",
            OdaTipi.UcKisilik => "Üç Kişilik",
            OdaTipi.Suit => "Suit",
            OdaTipi.Aile => "Aile",
            _ => Oda.Tip.ToString()
        };
        public int Kapasite => Oda.Kapasite;
        public int Kat => Oda.Kat;
        public decimal GecelikUcret => Oda.GecelikUcret;
        public string Aciklama => Oda.Aciklama;
        public bool Aktif => Oda.Aktif;
        public string DurumAdi => Oda.Aktif ? "Aktif" : "Pasif";
    }
}
