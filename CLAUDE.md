# CLMS: notes for Claude Code sessions

Clinical Laboratory Management System (SFWE 503 Fall 2026, Group 9).
Architecture, demo accounts, and run instructions are in `docs/project-overview.md`;
dev environment setup is in `docs/development.md`. Read those first.
This file records what those docs don't: decisions and gotchas found while getting
the scaffold running on macOS and Windows.

Personal, machine-specific notes go in `CLAUDE.local.md` (gitignored), not here.

## Commands

```bash
dotnet build Clms.sln && dotnet test Clms.sln   # API, Web, Ui, Shared, Tests (18 tests)
docker compose up -d db                         # Postgres only
dotnet run --project src/Clms.Api               # http://localhost:5090 (docs: /scalar/v1)
dotnet run --project src/Clms.Web               # http://localhost:5080
bash scripts/simulate-instrument.sh [--bad]     # drop a result file into instrument-drop/
.\scripts\simulate-instrument.ps1 [-Bad]        # Windows equivalent (native PowerShell)
```

MAUI app (`src/Clms.Maui`). Always build before running:

```bash
dotnet build src/Clms.Maui -f net10.0-ios && dotnet build src/Clms.Maui -t:Run -f net10.0-ios
dotnet build src/Clms.Maui -f net10.0-maccatalyst
open src/Clms.Maui/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/CLMS.app
dotnet build src/Clms.Maui -f net10.0-windows10.0.19041.0      # Windows (needs maui-windows workload)
```

## Things that will bite you

- **Entities assign their own Guid keys** (`Id = Guid.NewGuid()`). `ClmsDbContext` sets
  `ValueGenerated.Never` on all keys. Without it, children added through a tracked
  navigation (`order.Results.Add(...)`) get UPDATEd instead of INSERTed, which throws
  `DbUpdateConcurrencyException`. That was the instrument-ingestion bug. Keep it for new entities.
- **Schema uses `EnsureCreated()`, not migrations.** Entity changes don't reach an existing
  database. Reset with `docker compose down -v && docker compose up -d db`.
- **Two copies of the stylesheet.** `src/Clms.Maui/wwwroot/css/app.css` is a verbatim copy of
  `src/Clms.Web/wwwroot/css/app.css`. Edit the Web one, then re-copy. Native-only tweaks
  (iOS safe areas) go in `src/Clms.Maui/wwwroot/css/maui.css`.
- **`AuthorizeRouteView` already wraps `<NotAuthorized>` in `DefaultLayout`.** Don't add a
  `LayoutView` around it in `Routes.razor` (it doubled the header).
- **Buttons need an explicit `color`.** iOS WebKit's default was invisible on white.
- **Instrument watcher**: `DropPath` in `appsettings.Development.json` is relative to the
  process working directory (`src/Clms.Api` under `dotnet run`). It polls on purpose, since
  file events don't cross Docker bind mounts reliably.
- **Keep `simulate-instrument.sh` and `.ps1` in sync** if the message format changes.
  The `.sh` must stay executable (`git update-index --chmod=+x`) and LF (`.gitattributes`).
- **`Directory.Build.props` must keep excluding Clms.Maui.** Its singular
  `TargetFramework` would override the MAUI project's `TargetFrameworks` and build it as plain net10.0.
- **Clms.Maui is deliberately NOT in `Clms.sln`**, so solution build/test works without
  MAUI workloads. `maui/README.md` still says `dotnet sln add`. That's outdated, don't.
  `Clms.Mac.sln` and `Clms.Windows.sln` are the same 5 projects plus `Clms.Maui`, for
  machines with the matching workload. Its OS-conditional `TargetFrameworks` resolves
  per host: iOS + Mac Catalyst on macOS, the Windows app on Windows.
- **MAUI `-t:Run` alone doesn't refresh `wwwroot`.** It rebuilds C# but leaves stale
  CSS/HTML in the app bundle. Run a plain `dotnet build` first.
- **Mac Catalyst `-t:Run` fails with "not a tty"** in a non-interactive shell. `open` the `.app`.
- **Windows: MAUI `-t:Run` fails if the project path contains a space.** MSBuild's generated
  run command isn't quoted (error 9009). Use a space-free path such as `C:\src\clms`, or launch
  `src\Clms.Maui\bin\Debug\net10.0-windows10.0.19041.0\<rid>\Clms.Maui.exe` directly.
- **Windows: don't build from a network share / VM shared folder.** Two OSes writing the
  same `bin/`/`obj/` over SMB gives stale or corrupt builds and file locks. Clone locally.
- **Don't commit machine-specific connection strings.** `appsettings.json` defaults to
  `Host=localhost`; override with `dotnet user-secrets` or `ConnectionStrings__Clms`.
- **Background `dotnet run` on Windows:** a PowerShell `Start-Job` dies with its parent
  shell, so launch the API as a properly detached process.

## Open items

- Result idempotency matches on (TestCode, ResultedAtUtc) only, so a corrected value in the
  same minute is silently dropped. Revisit with the result-validation workflow.
- Inventory and Test Orders pages are unverified at phone width (tables scroll inside cards).
- Consider moving `app.css` into `Clms.Ui` (served as `_content/Clms.Ui/...`) to end the copy.
- Replace `EnsureCreated()` with EF migrations before the schema starts changing regularly.
