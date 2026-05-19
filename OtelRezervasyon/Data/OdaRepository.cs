using Microsoft.Data.Sqlite;
using OtelRezervasyon.Models;

namespace OtelRezervasyon.Data;

public static class OdaRepository
{
    private const string TarihFormati = "yyyy-MM-dd";

    public static List<Oda> Listele(bool sadeceAktif = false)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sadeceAktif
            ? "SELECT * FROM Odalar WHERE Aktif = 1 ORDER BY OdaNumarasi;"
            : "SELECT * FROM Odalar ORDER BY OdaNumarasi;";

        var liste = new List<Oda>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) liste.Add(Oku(reader));
        return liste;
    }

    public static Oda? Getir(int id)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Odalar WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Oku(reader) : null;
    }

    public static int Ekle(Oda oda)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Odalar (OdaNumarasi, Tip, Kapasite, GecelikUcret, Kat, Aciklama, Aktif)
            VALUES ($numara, $tip, $kapasite, $ucret, $kat, $aciklama, $aktif);
            SELECT last_insert_rowid();";
        Parametreleri(cmd, oda);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        oda.Id = id;
        return id;
    }

    public static void Guncelle(Oda oda)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Odalar
            SET OdaNumarasi = $numara, Tip = $tip, Kapasite = $kapasite,
                GecelikUcret = $ucret, Kat = $kat, Aciklama = $aciklama, Aktif = $aktif
            WHERE Id = $id;";
        Parametreleri(cmd, oda);
        cmd.Parameters.AddWithValue("$id", oda.Id);
        cmd.ExecuteNonQuery();
    }

    public static void Sil(int id)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Odalar WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static List<Oda> MusaitOdalar(DateTime giris, DateTime cikis, int? minKapasite = null)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.* FROM Odalar o
            WHERE o.Aktif = 1
              AND ($minKapasite IS NULL OR o.Kapasite >= $minKapasite)
              AND NOT EXISTS (
                SELECT 1 FROM Rezervasyonlar r
                WHERE r.OdaId = o.Id
                  AND r.Durum <> 4
                  AND r.GirisTarihi < $cikis
                  AND r.CikisTarihi > $giris
              )
            ORDER BY o.OdaNumarasi;";
        cmd.Parameters.AddWithValue("$giris", giris.ToString(TarihFormati));
        cmd.Parameters.AddWithValue("$cikis", cikis.ToString(TarihFormati));
        cmd.Parameters.AddWithValue("$minKapasite", (object?)minKapasite ?? DBNull.Value);

        var liste = new List<Oda>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) liste.Add(Oku(reader));
        return liste;
    }

    public static bool KullanimdaMi(int odaId)
    {
        using var conn = DatabaseManager.AcConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rezervasyonlar WHERE OdaId = $id;";
        cmd.Parameters.AddWithValue("$id", odaId);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private static void Parametreleri(SqliteCommand cmd, Oda oda)
    {
        cmd.Parameters.AddWithValue("$numara", oda.OdaNumarasi);
        cmd.Parameters.AddWithValue("$tip", (int)oda.Tip);
        cmd.Parameters.AddWithValue("$kapasite", oda.Kapasite);
        cmd.Parameters.AddWithValue("$ucret", (double)oda.GecelikUcret);
        cmd.Parameters.AddWithValue("$kat", oda.Kat);
        cmd.Parameters.AddWithValue("$aciklama", oda.Aciklama ?? string.Empty);
        cmd.Parameters.AddWithValue("$aktif", oda.Aktif ? 1 : 0);
    }

    private static Oda Oku(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        OdaNumarasi = reader.GetString(reader.GetOrdinal("OdaNumarasi")),
        Tip = (OdaTipi)reader.GetInt32(reader.GetOrdinal("Tip")),
        Kapasite = reader.GetInt32(reader.GetOrdinal("Kapasite")),
        GecelikUcret = (decimal)reader.GetDouble(reader.GetOrdinal("GecelikUcret")),
        Kat = reader.GetInt32(reader.GetOrdinal("Kat")),
        Aciklama = reader.GetString(reader.GetOrdinal("Aciklama")),
        Aktif = reader.GetInt32(reader.GetOrdinal("Aktif")) == 1
    };
}
