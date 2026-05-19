# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Turkish-language hotel reservation desktop app. **.NET 9 WinForms** (`net9.0-windows`), single project at `OtelRezervasyon/OtelRezervasyon.csproj`. Persistence is local **SQLite** via `Microsoft.Data.Sqlite` 9.0.0 — no ORM, no DI container, no async. All identifiers, namespaces, table/column names, and UI text are in Turkish; keep that convention when adding code.

## Commands

Run from the repo root (`C:\Users\Kerem\Desktop\Emir Efe Proje`):

```powershell
dotnet build OtelRezervasyon/OtelRezervasyon.csproj
dotnet run   --project OtelRezervasyon/OtelRezervasyon.csproj
```

No test project exists. There is no lint/format config beyond the SDK defaults.

The SQLite database is created on first run at:

```
%LOCALAPPDATA%\OtelRezervasyon\otel.db
```

`DatabaseManager.VeritabaniniHazirla()` (called from `Program.Main`) does `CREATE TABLE IF NOT EXISTS` for all three tables and seeds 7 sample rooms when `Odalar` is empty. To reset, delete that file — there are no migrations, schema changes go directly into the `CREATE TABLE` strings in `Data/DatabaseManager.cs`.

## Architecture

Four-layer layout under `OtelRezervasyon/`. Dependencies only flow downward: `Forms` → `UI` + `Data` → `Models`.

- **`Models/`** — Plain POCOs (`Oda`, `Musteri`, `Rezervasyon`) and int-backed enums (`OdaTipi`, `RezervasyonDurumu`). `Rezervasyon` carries optional `Musteri`/`Oda` navigation refs populated by repository joins.
- **`Data/`** — `DatabaseManager` owns the connection string and schema. Each operation calls `DatabaseManager.AcConnection()`, which opens a fresh `SqliteConnection`, turns on `PRAGMA foreign_keys`, and is wrapped in `using`. Three **static** repositories (`OdaRepository`, `MusteriRepository`, `RezervasyonRepository`) — no interfaces, no DI. Repositories use named SQL parameters (`$name`) and hand-mapped readers (`Oku(...)`).
- **`UI/`** — Custom flat controls and design tokens (see "UI shell" below).
- **`Forms/`** — `MainForm` is the application shell. **Dialogs** (`YeniRezervasyonForm`, `OdaDuzenleForm`, `MusteriDuzenleForm`) are `Form` subclasses opened with `ShowDialog`. **Pages** live in `Forms/Views/` as `UserControl` subclasses and are swapped into the `MainForm` content area — they are NOT separate windows.

### UI shell (load-bearing convention)

`MainForm` is a sidebar + topbar + content layout that swaps a single `UserControl` into the content `Panel` when a `SidebarItem` is clicked. Adding a new page = new `UserControl` in `Forms/Views/` + one row in the `items` array inside `MainForm.SidebarKur()` (icon, text, title, subtitle, view type). The reflection-based `ctor.Invoke(null)` requires a public parameterless constructor on each view.

**Do not switch this to MDI or a MenuStrip-based shell.** That style was explicitly rejected by the user — see `feedback_ui_tercihi.md` in auto-memory. New WinForms work in this repo must use the same modern flat pattern: dark `Theme.Sidebar`, white topbar, `Theme.AppBg` content area, rounded `Card`/`StatCard` containers, `FlatButton`/`SecondaryButton`/`SuccessButton`/`DangerButton`, grids styled via `Styler.Grid()`.

All colors and fonts live in `UI/Theme.cs` — reuse those tokens instead of hardcoding `Color.FromArgb(...)`. Custom owner-drawn controls (`FlatButton`, `Card`, `SidebarItem`) trigger analyzer warning `WFO1000`, suppressed project-wide via `<NoWarn>$(NoWarn);WFO1000</NoWarn>` in the csproj — keep that suppression in place.

### Reservation domain rules (non-obvious)

- **Overlap detection** uses the half-open interval test `GirisTarihi < $cikis AND CikisTarihi > $giris`. This is the canonical pattern in `RezervasyonRepository.CakismaVarMi`, `OdaRepository.MusaitOdalar`, `ToplamGelir`, and `DolulukVerisi`. Reuse it; do not write your own date-range logic.
- **Cancelled reservations are excluded from availability and stats** by `Durum <> 4`, where `4 = RezervasyonDurumu.IptalEdildi`. The magic number `4` appears in raw SQL by design — if you change the enum value, audit every repository.
- The DB enforces `CHECK (CikisTarihi > GirisTarihi)` at insert time, so a same-day checkout will throw from SQLite. Validate in the form first and show a friendly Turkish message (see `YeniRezervasyonForm.KaydetTiklandi`).
- `Ekle`/`Guncelle` on `RezervasyonRepository` call `CakismaVarMi` first and throw `InvalidOperationException` with a Turkish message — surface those messages in `MessageBox` rather than swallowing.
- Foreign keys are `ON DELETE RESTRICT`, so `MusteriRepository.Sil` / `OdaRepository.Sil` will fail if reservations reference the row. Use `RezervasyonuVarMi` / `KullanimdaMi` to check before offering delete.
- Dates round-trip as `yyyy-MM-dd` strings in SQLite (timestamps as `yyyy-MM-dd HH:mm:ss`). When writing new queries that compare dates, format with the same constants the repositories use.

## Auto-memory note

User preferences and project context are stored under `~/.claude/projects/C--Users-Kerem-Desktop-Emir-Efe-Proje/memory/` (indexed by `MEMORY.md`). The WinForms UI preference described above is the load-bearing one — read it before proposing UI changes.
