# 🏨 Otel Rezervasyon Sistemi

Modern arayüzlü, masaüstü tabanlı bir **otel rezervasyon yönetim sistemi**. .NET 9 WinForms ve SQLite ile geliştirilmiş, kurulum gerektirmeyen tek dosyalık bir uygulama. Bitirme projesi olarak hazırlanmıştır.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![WinForms](https://img.shields.io/badge/UI-WinForms-0078D6?style=flat-square&logo=windows&logoColor=white)
![SQLite](https://img.shields.io/badge/DB-SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=flat-square)

---

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Ekran Görüntüleri](#-ekran-görüntüleri)
- [Teknolojiler](#️-teknolojiler)
- [Kurulum & Çalıştırma](#-kurulum--çalıştırma)
- [Proje Yapısı](#-proje-yapısı)
- [Veritabanı Şeması](#️-veritabanı-şeması)
- [Mimari Notlar](#-mimari-notlar)
- [Geliştirme](#️-geliştirme)

---

## ✨ Özellikler

### 📊 Dashboard
- Toplam oda, şu an konaklayan misafir, günlük giriş/çıkış sayıları
- Aylık doluluk oranı ve toplam gelir kartları
- Yaklaşan rezervasyonlar listesi

### 📋 Rezervasyon Yönetimi
- Yeni rezervasyon oluşturma, düzenleme, iptal etme
- **Otomatik çakışma kontrolü** — aynı oda, aynı tarihte birden fazla rezervasyona izin verilmez
- Durum takibi: Beklemede → Onaylandı → Giriş Yapıldı → Tamamlandı (veya İptal)
- Tarih ve gece sayısına göre **otomatik ücret hesaplama**

### 🔍 Müsait Oda Arama
- Tarih aralığı + minimum kapasite ile filtreleme
- Anlık müsaitlik sorgusu (yarı-açık tarih aralığı algoritması)

### 🛏 Oda Yönetimi
- 5 farklı oda tipi: Tek Kişilik, Çift Kişilik, Üç Kişilik, Suit, Aile
- Kat, kapasite, gecelik ücret, açıklama
- Aktif/pasif durumu

### 👥 Müşteri Yönetimi
- Ad, soyad, TC kimlik, telefon, e-posta, adres
- TC kimlik üzerinde **benzersizlik kısıtı** (boş bırakılabilir)
- Arama: ad / soyad / TC / telefon / e-posta üzerinden

### 📈 Raporlar
- Belirli tarih aralığı için doluluk ve gelir analizleri

---

## 🖼 Ekran Görüntüleri

> _Ekran görüntüleri eklenecek_

---

## 🛠️ Teknolojiler

| Katman | Teknoloji |
|---|---|
| **Dil** | C# 12 |
| **Runtime** | .NET 9 (`net9.0-windows`) |
| **UI** | Windows Forms (özel flat tasarım) |
| **Veritabanı** | SQLite (Microsoft.Data.Sqlite 9.0.0) |
| **Mimari** | Katmanlı (Models / Data / UI / Forms) |
| **Bağımlılık Yönetimi** | Yok — saf statik repository deseni |

**Sıfır harici bağımlılık:** Sadece tek bir NuGet paketi (`Microsoft.Data.Sqlite`). ORM yok, DI yok, framework şişkinliği yok.

---

## 🚀 Kurulum & Çalıştırma

### Gereksinimler
- Windows 10 / 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Adımlar

```powershell
# 1) Depoyu klonla
git clone https://github.com/Emrys7690/otel-rezervasyon-sistemi.git
cd otel-rezervasyon-sistemi

# 2) Derle
dotnet build OtelRezervasyon/OtelRezervasyon.csproj

# 3) Çalıştır
dotnet run --project OtelRezervasyon/OtelRezervasyon.csproj
```

Uygulama ilk çalıştırmada veritabanını otomatik oluşturur ve 7 örnek odayla doldurur:

```
%LOCALAPPDATA%\OtelRezervasyon\otel.db
```

**Veritabanını sıfırlamak için** bu dosyayı silmen yeterli — bir sonraki çalıştırmada yeniden oluşturulur.

---

## 📁 Proje Yapısı

```
OtelRezervasyon/
├── Models/                  POCO sınıflar ve enum'lar
│   ├── Oda.cs               Oda bilgi modeli
│   ├── Musteri.cs           Müşteri modeli
│   ├── Rezervasyon.cs       Rezervasyon (Musteri + Oda navigasyon ref)
│   ├── OdaTipi.cs           Enum: 1=Tek, 2=Çift, 3=Üç, 4=Suit, 5=Aile
│   └── RezervasyonDurumu.cs Enum: 0=Beklemede ... 4=İptal
│
├── Data/                    Veritabanı erişim katmanı
│   ├── DatabaseManager.cs   Bağlantı + şema oluşturma + seed
│   ├── OdaRepository.cs     Oda CRUD + müsaitlik sorgusu
│   ├── MusteriRepository.cs Müşteri CRUD + arama
│   └── RezervasyonRepository.cs  Rezervasyon CRUD + çakışma + istatistikler
│
├── UI/                      Yeniden kullanılabilir özel kontroller
│   ├── Theme.cs             Renk ve font tokenları (tek kaynak)
│   ├── FlatButton.cs        FlatButton + Secondary/Danger/Success varyantları
│   ├── Card.cs              Yuvarlatılmış Card + StatCard
│   ├── SidebarItem.cs       Sol menü navigasyon öğesi
│   └── Styler.cs            DataGridView, TextBox, Label stil yardımcıları
│
├── Forms/                   Pencereler
│   ├── MainForm.cs          Ana kabuk (sidebar + topbar + içerik)
│   ├── YeniRezervasyonForm.cs   Rezervasyon ekleme/düzenleme dialog
│   ├── OdaDuzenleForm.cs        Oda ekleme/düzenleme dialog
│   ├── MusteriDuzenleForm.cs    Müşteri ekleme/düzenleme dialog
│   └── Views/               MainForm içine yüklenen sayfa UserControl'leri
│       ├── DashboardView.cs
│       ├── RezervasyonView.cs
│       ├── MusaitOdaView.cs
│       ├── OdaView.cs
│       ├── MusteriView.cs
│       └── RaporView.cs
│
├── Program.cs               Uygulama giriş noktası
└── OtelRezervasyon.csproj   Proje dosyası (.NET 9, WinForms)
```

---

## 🗃️ Veritabanı Şeması

Üç tablo, iki indeks, bir benzersizlik kısıtı:

```sql
Odalar
  Id, OdaNumarasi (UNIQUE), Tip, Kapasite,
  GecelikUcret, Kat, Aciklama, Aktif

Musteriler
  Id, Ad, Soyad, TcKimlikNo, Telefon, Eposta, Adres, KayitTarihi
  UX_Musteri_TC (TcKimlikNo benzersiz, boş hariç)

Rezervasyonlar
  Id, MusteriId (FK), OdaId (FK),
  GirisTarihi, CikisTarihi (CHECK CikisTarihi > GirisTarihi),
  KisiSayisi, ToplamUcret, Durum, Notlar, OlusturmaTarihi
  IX_Rezervasyon_Oda     (OdaId, GirisTarihi, CikisTarihi)
  IX_Rezervasyon_Musteri (MusteriId)
```

İlişkiler `ON DELETE RESTRICT` — rezervasyonu olan bir müşteri veya oda silinemez.

---

## 🧠 Mimari Notlar

### Modern flat UI kabuk
`MainForm` klasik MDI yerine **sidebar + topbar + içerik swap** mimarisi kullanır. Yeni bir sayfa eklemek için:

1. `Forms/Views/` altına yeni bir `UserControl` ekle
2. `MainForm.SidebarKur()` içindeki `items` dizisine bir satır ekle

Tüm renkler `UI/Theme.cs` içinde — kodun hiçbir yerinde sabit `Color.FromArgb(...)` kullanılmaz.

### Çakışma algoritması
Tarih aralığı çakışması **yarı-açık aralık** testi ile yapılır:

```sql
GirisTarihi < $cikis AND CikisTarihi > $giris
```

Bu, `13–15` ile `15–17` aralıklarının çakışmamasını sağlar — `15` günü çıkış ve yeni giriş aynı odada mümkündür.

### İptal edilen rezervasyonlar
`Durum = 4` (İptal Edildi) rezervasyonlar müsaitlik, gelir ve doluluk hesaplarından SQL seviyesinde **dışlanır** (`Durum <> 4`).

### Parametreli sorgular
Tüm SQL sorgularında `$name` formatında **named parameters** kullanılır — SQL injection riski yok.

---

## 🛠️ Geliştirme

Proje ek yapı kuralları için `CLAUDE.md` dosyasına bakın (AI yardımcıları ve gelecekteki geliştiriciler için kısa rehber).

### Build uyarıları
Özel owner-drawn kontroller (`FlatButton`, `Card`, `SidebarItem`) `WFO1000` designer uyarısı tetikler — bu uyarı `csproj` içinde proje genelinde susturulmuştur.

---

## 📝 Lisans

Bu proje bitirme projesi olarak hazırlanmıştır. Eğitim amaçlı serbestçe incelenebilir.
