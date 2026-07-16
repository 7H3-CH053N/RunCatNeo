# RunCat Neo for Windows

**A cute running cat animation in your Windows system tray.**

This is the Windows port of [RunCat Neo](https://github.com/runcat-dev/RunCatNeo). The runner
animates in the system tray and its speed reflects the current CPU load — exactly like the
macOS original (speed factor = CPU % / 5, clamped to 1…20, at a base rate of 2 frames per second).

## Features

- All eight built-in runners from the macOS app (cat, dog, slime, drop, coffee,
  Newton's cradle, engine, mochi), using the identical frame images and frame orders.
- Animation speed follows CPU usage; optional "speed decreases under load" mode.
- Dashboard with CPU and memory line graphs plus storage, network, and battery info.
- Settings: runner picker with live preview, launch at login, horizontal flip,
  update interval (3/5/10 s), and per-metric monitoring toggles.
- Light/dark taskbar aware: template runners are tinted black or white automatically.
- Custom runners: drop PNG key frames into `%APPDATA%\RunCatNeo\Runners\<id>\frame-<n>.png`
  and describe them in `%APPDATA%\RunCatNeo\Runners\custom-runners.json` using the same JSON
  shape the macOS app persists (`{"id", "name", "isTemplate", "frameOrder": [0, 1, ...]}`).

## Requirements

- Windows 10 (1809) or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build

## Build & Run

```powershell
cd windows
dotnet run --project src/RunCatNeo.Windows
```

To create a self-contained executable:

```powershell
dotnet publish src/RunCatNeo.Windows -c Release -r win-x64 --self-contained false
```

## Tests

The platform-neutral core (frame orders, runner speed, settings persistence) lives in
`src/RunCatNeo.Core` and is covered by tests that run on any OS:

```sh
cd windows
dotnet test
```

## Project Layout

- `src/RunCatNeo.Core` — platform-neutral port of the macOS `DataSource` entities
  (`Runner`, `FrameOrder`, `RunnerKind`, `UpdateInterval`, `RingBuffer`, speed calculation,
  settings persistence).
- `src/RunCatNeo.Windows` — WinForms tray application (`TrayAppContext` plays the role of
  `RunnerBar` + the services; `SystemInfoObserver` is the counterpart of SystemInfoKit).
- `tests/RunCatNeo.Core.Tests` — xUnit tests for the core logic.

## Not (yet) ported

- Metrics bar (the secondary menu-bar item) — Windows tray icons cannot render wide
  multi-indicator items.
- Custom metrics JSON cards on the dashboard.
- In-app custom runner editor (custom runners are supported via the file format above).
- Donation settings (App Store specific).

## License

Apache License 2.0 — see the repository's [LICENSE](../LICENSE).
