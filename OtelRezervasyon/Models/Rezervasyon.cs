namespace OtelRezervasyon.Models;

public class Rezervasyon
{
    public int Id { get; set; }
    public int MusteriId { get; set; }
    public int OdaId { get; set; }
    public DateTime GirisTarihi { get; set; }
    public DateTime CikisTarihi { get; set; }
    public int KisiSayisi { get; set; }
    public decimal ToplamUcret { get; set; }
    public RezervasyonDurumu Durum { get; set; } = RezervasyonDurumu.Beklemede;
    public string Notlar { get; set; } = string.Empty;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    public Musteri? Musteri { get; set; }
    public Oda? Oda { get; set; }

    public int GeceSayisi => Math.Max(0, (CikisTarihi.Date - GirisTarihi.Date).Days);
}
