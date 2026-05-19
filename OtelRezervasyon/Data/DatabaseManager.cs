using Microsoft.Data.Sqlite;

namespace OtelRezervasyon.Data;

public static class DatabaseManager
{
    private static readonly string DbDosyaYolu = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OtelRezervasyon",
        "otel.db");

    public static string DosyaYolu => DbDosyaYolu;
    public static string ConnectionString => $"Data Source={DbDosyaYolu}";

    public static SqliteConnection AcConnection()
    {
        var dir = Path.GetDirectoryName(DbDosyaYolu);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public static void VeritabaniniHazirla()
    {
        using var connection = AcConnection();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Odalar (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                OdaNumarasi     TEXT    NOT NULL UNIQUE,
                Tip             INTEGER NOT NULL,
                Kapasite        INTEGER NOT NULL,
                GecelikUcret    REAL    NOT NULL,
                Kat             INTEGER NOT NULL DEFAULT 0,
                Aciklama        TEXT    NOT NULL DEFAULT '',
                Aktif           INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Musteriler (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                Ad              TEXT    NOT NULL,
                Soyad           TEXT    NOT NULL,
                TcKimlikNo      TEXT    NOT NULL DEFAULT '',
                Telefon         TEXT    NOT NULL DEFAULT '',
                Eposta          TEXT    NOT NULL DEFAULT '',
                Adres           TEXT    NOT NULL DEFAULT '',
                KayitTarihi     TEXT    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Rezervasyonlar (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                MusteriId       INTEGER NOT NULL,
                OdaId           INTEGER NOT NULL,
                GirisTarihi     TEXT    NOT NULL,
                CikisTarihi     TEXT    NOT NULL,
                KisiSayisi      INTEGER NOT NULL,
                ToplamUcret     REAL    NOT NULL,
                Durum           INTEGER NOT NULL DEFAULT 0,
                Notlar          TEXT    NOT NULL DEFAULT '',
                OlusturmaTarihi TEXT    NOT NULL,
                FOREIGN KEY (MusteriId) REFERENCES Musteriler(Id) ON DELETE RESTRICT,
                FOREIGN KEY (OdaId)     REFERENCES Odalar(Id)     ON DELETE RESTRICT,
                CHECK (CikisTarihi > GirisTarihi)
            );

            CREATE INDEX IF NOT EXISTS IX_Rezervasyon_Oda
                ON Rezervasyonlar(OdaId, GirisTarihi, CikisTarihi);

            CREATE INDEX IF NOT EXISTS IX_Rezervasyon_Musteri
                ON Rezervasyonlar(MusteriId);

            CREATE UNIQUE INDEX IF NOT EXISTS UX_Musteri_TC
                ON Musteriler(TcKimlikNo) WHERE TcKimlikNo <> '';
        ";
        cmd.ExecuteNonQuery();

        OrnekVeriEkle(connection);
    }

    private static void OrnekVeriEkle(SqliteConnection connection)
    {
        using var sayim = connection.CreateCommand();
        sayim.CommandText = "SELECT COUNT(*) FROM Odalar;";
        var mevcut = Convert.ToInt64(sayim.ExecuteScalar());
        if (mevcut > 0) return;

        using var insert = connection.CreateCommand();
        insert.CommandText = @"
            INSERT INTO Odalar (OdaNumarasi, Tip, Kapasite, GecelikUcret, Kat, Aciklama, Aktif) VALUES
                ('101', 1, 1, 1500, 1, 'Tek kişilik standart oda', 1),
                ('102', 1, 1, 1500, 1, 'Tek kişilik standart oda', 1),
                ('201', 2, 2, 2500, 2, 'Çift kişilik deniz manzaralı', 1),
                ('202', 2, 2, 2400, 2, 'Çift kişilik şehir manzaralı', 1),
                ('203', 2, 2, 2400, 2, 'Çift kişilik şehir manzaralı', 1),
                ('301', 4, 2, 4500, 3, 'Suit oda - jakuzili', 1),
                ('401', 5, 4, 5000, 4, 'Aile odası - 2 yatak odalı', 1);
        ";
        insert.ExecuteNonQuery();
    }
}
