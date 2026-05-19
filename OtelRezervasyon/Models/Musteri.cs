namespace OtelRezervasyon.Models;

public class Musteri
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    public string TamAd => $"{Ad} {Soyad}".Trim();
}
