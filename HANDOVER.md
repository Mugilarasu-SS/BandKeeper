# BandKeeper Handover Guide

This document helps you take over the app from the current Milestone 1 implementation state.

## 1) Current Status (What is done)

The repository currently includes:
- Multi-project solution scaffold (`BandKeeper.sln`):
  - `BandKeeper.Core`
  - `BandKeeper.Infrastructure`
  - `BandKeeper.Widget`
- Live adapter counter sampling pipeline.
- SQLite storage initialization and sample persistence.
- Widget startup with persisted settings load/save.
- Basic usage totals query contract and repository implementation.
- Transparent always-on-top widget shell with live DL/UL text updates.

## 2) Build and Run Locally (Windows 11 / Server)

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 (recommended) with `.NET desktop development` workload

### CLI
```powershell
cd <repo-root>
dotnet --info
dotnet restore BandKeeper.sln
dotnet build BandKeeper.sln
dotnet run --project src/BandKeeper.Widget/BandKeeper.Widget.csproj
```

### Visual Studio
1. Open `BandKeeper.sln`.
2. Set startup project to `BandKeeper.Widget`.
3. Build solution.
4. Run (F5 / Ctrl+F5).

## 3) Where Data Lives

SQLite DB path currently defaults to:
- `%LOCALAPPDATA%\BandKeeper\bandkeeper.db`

Schema file source:
- `src/BandKeeper.Infrastructure/Data/schema.sql`

## 4) What to verify immediately

1. App launches with transparent widget.
2. DL/UL values update every ~1 second.
3. DB file is created in LocalAppData.
4. `samples` table rows are being inserted.
5. App closes cleanly without hanging.

## 5) Known Gaps / Risks

1. Adapter selection UI is not yet exposed (currently uses persisted/default settings).
2. Usage totals are computed via min/max counters in a range; adapter reset/restart edge cases need hardening.
3. No tray integration yet.
4. No start-with-Windows toggle wiring yet.
5. No explicit offline indicator UX yet.
6. No automated tests currently.

## 6) Immediate Next Implementation Steps (Recommended order)

### Step A — Settings UI + validation
- Add a small settings window (or panel) for:
  - Adapter selection
  - Poll interval
  - Include VPN/tunnel adapters toggle
- Persist via `SaveWidgetSettingsAsync`.

### Step B — Timeframe usage API and display
- Add query methods for presets:
  - Hour / Day / Week / Month / Year / Custom
- Show totals in a simple panel or diagnostics window.

### Step C — Robustness hardening
- Handle adapter disconnect/reconnect gracefully.
- Handle counter resets/rollovers for usage calculations.
- Add logging around collector and DB failures.

### Step D — Product UX finish for Milestone 1
- Tray icon + context menu (open settings, exit).
- Start-with-Windows toggle and wiring.
- Minimal-mode formatting alignment (`DL: xx unit/s  UL: xx unit/s`).

## 7) Suggested Ownership Checklist (for handover completion)

- [ ] Confirm local build on Windows machine.
- [ ] Confirm widget sampling and persistence works.
- [ ] Confirm settings read/write works from DB.
- [ ] Implement and verify settings UI.
- [ ] Implement timeframe totals UI.
- [ ] Add tray + startup integration.
- [ ] Add at least smoke tests for rate math + persistence methods.

## 8) Useful Files

- `PRODUCT_REQUIREMENTS.md` — approved requirements baseline.
- `src/BandKeeper.Infrastructure/Monitoring/AdapterCounterCollector.cs`
- `src/BandKeeper.Infrastructure/Data/SqliteSampleRepository.cs`
- `src/BandKeeper.Widget/Views/MainWindow.xaml`
- `src/BandKeeper.Widget/Views/MainWindow.xaml.cs`

---

If you want, the next coding pass can implement **Settings UI + Tray integration** in one focused PR.
