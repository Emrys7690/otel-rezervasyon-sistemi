namespace OtelRezervasyon.Models;

public class Oda
{
    public int Id { get; set; }
    public string OdaNumarasi { get; set; } = string.Empty;
    public OdaTipi Tip { get; set; }
    public int Kapasite { get; set; }
    public decimal GecelikUcret { get; set; }
    public int Kat { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public bool Aktif { get; set; } = true;
}
