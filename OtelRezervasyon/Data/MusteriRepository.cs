using Microsoft.Data.Sqlite;
using OtelRezervasyon.Models;

namespace OtelRezervasyon.Data;

public static class MusteriRepository
{
    private const string TarihFormati = "yyyy-MM-dd HH:mm:ss";

    public static List<Musteri> Listele(string arama = "")
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();

        if (string.IsNullOrWhiteSpace(arama))
        {
            cmd.CommandText = "SELECT * FROM Musteriler ORDER BY Ad, Soyad;";
        }
        else
        {
            cmd.CommandText = @"
                SELECT * FROM Musteriler
                WHERE Ad LIKE $q OR Soyad LIKE $q OR TcKimlikNo LIKE $q
                   OR Telefon LIKE $q OR Eposta LIKE $q
                ORDER BY Ad, Soyad;";
            cmd.Parameters.AddWithValue("$q", $"%{arama}%");
        }

        var liste = new List<Musteri>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) liste.Add(Oku(reader));
        return liste;
    }

    public static Musteri? Getir(int id)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Musteriler WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Oku(reader) : null;
    }

    public static int Ekle(Musteri musteri)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Musteriler (Ad, Soyad, TcKimlikNo, Telefon, Eposta, Adres, KayitTarihi)
            VALUES ($ad, $soyad, $tc, $tel, $eposta, $adres, $tarih);
            SELECT last_insert_rowid();";
        Parametreleri(cmd, musteri);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        musteri.Id = id;
        return id;
    }

    public static void Guncelle(Musteri musteri)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Musteriler
            SET Ad = $ad, Soyad = $soyad, TcKimlikNo = $tc,
                Telefon = $tel, Eposta = $eposta, Adres = $adres
            WHERE Id = $id;";
        Parametreleri(cmd, musteri);
        cmd.Parameters.AddWithValue("$id", musteri.Id);
        cmd.ExecuteNonQuery();
    }

    public static void Sil(int id)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Musteriler WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static bool RezervasyonuVarMi(int musteriId)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rezervasyonlar WHERE MusteriId = $id;";
        cmd.Parameters.AddWithValue("$id", musteriId);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private static void Parametreleri(SqliteCommand cmd, Musteri m)
    {
        cmd.Parameters.AddWithValue("$ad", m.Ad);
        cmd.Parameters.AddWithValue("$soyad", m.Soyad);
        cmd.Parameters.AddWithValue("$tc", m.TcKimlikNo ?? string.Empty);
        cmd.Parameters.AddWithValue("$tel", m.Telefon ?? string.Empty);
        cmd.Parameters.AddWithValue("$eposta", m.Eposta ?? string.Empty);
        cmd.Parameters.AddWithValue("$adres", m.Adres ?? string.Empty);
        cmd.Parameters.AddWithValue("$tarih", m.KayitTarihi.ToString(TarihFormati));
    }

    private static Musteri Oku(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Ad = reader.GetString(reader.GetOrdinal("Ad")),
        Soyad = reader.GetString(reader.GetOrdinal("Soyad")),
        TcKimlikNo = reader.GetString(reader.GetOrdinal("TcKimlikNo")),
        Telefon = reader.GetString(reader.GetOrdinal("Telefon")),
        Eposta = reader.GetString(reader.GetOrdinal("Eposta")),
        Adres = reader.GetString(reader.GetOrdinal("Adres")),
        KayitTarihi = DateTime.Parse(reader.GetString(reader.GetOrdinal("KayitTarihi")))
    };
}
