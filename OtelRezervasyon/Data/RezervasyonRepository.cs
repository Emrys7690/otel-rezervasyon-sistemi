using Microsoft.Data.Sqlite;
using OtelRezervasyon.Models;

namespace OtelRezervasyon.Data;

public static class RezervasyonRepository
{
    private const string Tarih = "yyyy-MM-dd";
    private const string TarihSaat = "yyyy-MM-dd HH:mm:ss";

    public static List<Rezervasyon> Listele(RezervasyonDurumu? durum = null,
        DateTime? baslangic = null, DateTime? bitis = null)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.*,
                   m.Ad AS M_Ad, m.Soyad AS M_Soyad, m.TcKimlikNo AS M_Tc,
                   m.Telefon AS M_Tel, m.Eposta AS M_Eposta, m.Adres AS M_Adres,
                   m.KayitTarihi AS M_KayitTarihi,
                   o.OdaNumarasi AS O_Numara, o.Tip AS O_Tip, o.Kapasite AS O_Kapasite,
                   o.GecelikUcret AS O_Ucret, o.Kat AS O_Kat, o.Aciklama AS O_Aciklama, o.Aktif AS O_Aktif
            FROM Rezervasyonlar r
            INNER JOIN Musteriler m ON m.Id = r.MusteriId
            INNER JOIN Odalar o ON o.Id = r.OdaId
            WHERE ($durum IS NULL OR r.Durum = $durum)
              AND ($baslangic IS NULL OR r.CikisTarihi > $baslangic)
              AND ($bitis IS NULL OR r.GirisTarihi < $bitis)
            ORDER BY r.GirisTarihi DESC, r.Id DESC;";

        cmd.Parameters.AddWithValue("$durum", (object?)(durum.HasValue ? (int)durum.Value : (int?)null) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$baslangic", (object?)baslangic?.ToString(Tarih) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$bitis", (object?)bitis?.ToString(Tarih) ?? DBNull.Value);

        var liste = new List<Rezervasyon>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) liste.Add(OkuTam(reader));
        return liste;
    }

    public static Rezervasyon? Getir(int id)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.*,
                   m.Ad AS M_Ad, m.Soyad AS M_Soyad, m.TcKimlikNo AS M_Tc,
                   m.Telefon AS M_Tel, m.Eposta AS M_Eposta, m.Adres AS M_Adres,
                   m.KayitTarihi AS M_KayitTarihi,
                   o.OdaNumarasi AS O_Numara, o.Tip AS O_Tip, o.Kapasite AS O_Kapasite,
                   o.GecelikUcret AS O_Ucret, o.Kat AS O_Kat, o.Aciklama AS O_Aciklama, o.Aktif AS O_Aktif
            FROM Rezervasyonlar r
            INNER JOIN Musteriler m ON m.Id = r.MusteriId
            INNER JOIN Odalar o ON o.Id = r.OdaId
            WHERE r.Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? OkuTam(reader) : null;
    }

    public static int Ekle(Rezervasyon r)
    {
        if (CakismaVarMi(r.OdaId, r.GirisTarihi, r.CikisTarihi))
            throw new InvalidOperationException("Bu oda seçili tarihlerde başka bir rezervasyona ait.");

        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Rezervasyonlar
                (MusteriId, OdaId, GirisTarihi, CikisTarihi, KisiSayisi, ToplamUcret, Durum, Notlar, OlusturmaTarihi)
            VALUES
                ($mid, $oid, $giris, $cikis, $kisi, $ucret, $durum, $notlar, $olusturma);
            SELECT last_insert_rowid();";
        Parametreleri(cmd, r);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        r.Id = id;
        return id;
    }

    public static void Guncelle(Rezervasyon r)
    {
        if (CakismaVarMi(r.OdaId, r.GirisTarihi, r.CikisTarihi, r.Id))
            throw new InvalidOperationException("Bu oda seçili tarihlerde başka bir rezervasyona ait.");

        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Rezervasyonlar
            SET MusteriId = $mid, OdaId = $oid, GirisTarihi = $giris, CikisTarihi = $cikis,
                KisiSayisi = $kisi, ToplamUcret = $ucret, Durum = $durum, Notlar = $notlar
            WHERE Id = $id;";
        Parametreleri(cmd, r);
        cmd.Parameters.AddWithValue("$id", r.Id);
        cmd.ExecuteNonQuery();
    }

    public static void DurumDegistir(int id, RezervasyonDurumu yeniDurum)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Rezervasyonlar SET Durum = $d WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$d", (int)yeniDurum);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static void Sil(int id)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Rezervasyonlar WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static bool CakismaVarMi(int odaId, DateTime giris, DateTime cikis, int? haricId = null)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Rezervasyonlar
            WHERE OdaId = $oid
              AND Durum <> 4
              AND GirisTarihi < $cikis
              AND CikisTarihi > $giris
              AND ($haric IS NULL OR Id <> $haric);";
        cmd.Parameters.AddWithValue("$oid", odaId);
        cmd.Parameters.AddWithValue("$giris", giris.ToString(Tarih));
        cmd.Parameters.AddWithValue("$cikis", cikis.ToString(Tarih));
        cmd.Parameters.AddWithValue("$haric", (object?)haricId ?? DBNull.Value);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public static int SuandaKonaklayan()
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rezervasyonlar WHERE Durum = 2;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static int BugunGirisSayisi()
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rezervasyonlar WHERE Durum <> 4 AND GirisTarihi = $bugun;";
        cmd.Parameters.AddWithValue("$bugun", DateTime.Today.ToString(Tarih));
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static int BugunCikisSayisi()
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rezervasyonlar WHERE Durum <> 4 AND CikisTarihi = $bugun;";
        cmd.Parameters.AddWithValue("$bugun", DateTime.Today.ToString(Tarih));
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static decimal ToplamGelir(DateTime baslangic, DateTime bitis)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COALESCE(SUM(ToplamUcret), 0) FROM Rezervasyonlar
            WHERE Durum <> 4
              AND GirisTarihi < $bitis AND CikisTarihi > $baslangic;";
        cmd.Parameters.AddWithValue("$baslangic", baslangic.ToString(Tarih));
        cmd.Parameters.AddWithValue("$bitis", bitis.ToString(Tarih));
        return (decimal)Convert.ToDouble(cmd.ExecuteScalar());
    }

    public static (int rezervasyonGeceleri, int toplamOdaGeceleri) DolulukVerisi(DateTime baslangic, DateTime bitis)
    {
        var gunSayisi = Math.Max(0, (bitis.Date - baslangic.Date).Days);

        using var conn = DatabaseManager.AcConnection();
        using var aktifOda = conn.CreateCommand();
        aktifOda.CommandText = "SELECT COUNT(*) FROM Odalar WHERE Aktif = 1;";
        var odaSayisi = Convert.ToInt32(aktifOda.ExecuteScalar());

        using var rezGece = conn.CreateCommand();
        rezGece.CommandText = @"
            SELECT COALESCE(SUM(
                MAX(0,
                    (julianday(MIN(CikisTarihi, $bitis)) - julianday(MAX(GirisTarihi, $baslangic)))
                )
            ), 0)
            FROM Rezervasyonlar
            WHERE Durum <> 4
              AND GirisTarihi < $bitis AND CikisTarihi > $baslangic;";
        rezGece.Parameters.AddWithValue("$baslangic", baslangic.ToString(Tarih));
        rezGece.Parameters.AddWithValue("$bitis", bitis.ToString(Tarih));
        var rezGeceleri = (int)Math.Round(Convert.ToDouble(rezGece.ExecuteScalar()));

        return (rezGeceleri, odaSayisi * gunSayisi);
    }

    private static void Parametreleri(SqliteCommand cmd, Rezervasyon r)
    {
        cmd.Parameters.AddWithValue("$mid", r.MusteriId);
        cmd.Parameters.AddWithValue("$oid", r.OdaId);
        cmd.Parameters.AddWithValue("$giris", r.GirisTarihi.ToString(Tarih));
        cmd.Parameters.AddWithValue("$cikis", r.CikisTarihi.ToString(Tarih));
        cmd.Parameters.AddWithValue("$kisi", r.KisiSayisi);
        cmd.Parameters.AddWithValue("$ucret", (double)r.ToplamUcret);
        cmd.Parameters.AddWithValue("$durum", (int)r.Durum);
        cmd.Parameters.AddWithValue("$notlar", r.Notlar ?? string.Empty);
        cmd.Parameters.AddWithValue("$olusturma", r.OlusturmaTarihi.ToString(TarihSaat));
    }

    private static Rezervasyon OkuTam(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        MusteriId = reader.GetInt32(reader.GetOrdinal("MusteriId")),
        OdaId = reader.GetInt32(reader.GetOrdinal("OdaId")),
        GirisTarihi = DateTime.Parse(reader.GetString(reader.GetOrdinal("GirisTarihi"))),
        CikisTarihi = DateTime.Parse(reader.GetString(reader.GetOrdinal("CikisTarihi"))),
        KisiSayisi = reader.GetInt32(reader.GetOrdinal("KisiSayisi")),
        ToplamUcret = (decimal)reader.GetDouble(reader.GetOrdinal("ToplamUcret")),
        Durum = (RezervasyonDurumu)reader.GetInt32(reader.GetOrdinal("Durum")),
        Notlar = reader.GetString(reader.GetOrdinal("Notlar")),
        OlusturmaTarihi = DateTime.Parse(reader.GetString(reader.GetOrdinal("OlusturmaTarihi"))),
        Musteri = new Musteri
        {
            Id = reader.GetInt32(reader.GetOrdinal("MusteriId")),
            Ad = reader.GetString(reader.GetOrdinal("M_Ad")),
            Soyad = reader.GetString(reader.GetOrdinal("M_Soyad")),
            TcKimlikNo = reader.GetString(reader.GetOrdinal("M_Tc")),
            Telefon = reader.GetString(reader.GetOrdinal("M_Tel")),
            Eposta = reader.GetString(reader.GetOrdinal("M_Eposta")),
            Adres = reader.GetString(reader.GetOrdinal("M_Adres")),
            KayitTarihi = DateTime.Parse(reader.GetString(reader.GetOrdinal("M_KayitTarihi")))
        },
        Oda = new Oda
        {
            Id = reader.GetInt32(reader.GetOrdinal("OdaId")),
            OdaNumarasi = reader.GetString(reader.GetOrdinal("O_Numara")),
            Tip = (OdaTipi)reader.GetInt32(reader.GetOrdinal("O_Tip")),
            Kapasite = reader.GetInt32(reader.GetOrdinal("O_Kapasite")),
            GecelikUcret = (decimal)reader.GetDouble(reader.GetOrdinal("O_Ucret")),
            Kat = reader.GetInt32(reader.GetOrdinal("O_Kat")),
            Aciklama = reader.GetString(reader.GetOrdinal("O_Aciklama")),
            Aktif = reader.GetInt32(reader.GetOrdinal("O_Aktif")) == 1
        }
    };
}
