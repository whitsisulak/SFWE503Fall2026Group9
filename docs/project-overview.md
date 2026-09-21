# CLMS — Clinical Laboratory Management System

Scaffold for the SFWE semester project. One vertical slice built end to end
(auth + RBAC, reagent inventory, test orders, instrument result ingestion) so the
architecture is demonstrated and the remaining ConOps requirements have an obvious
place to go.

> **New to the project? Start with [development.md](development.md)** —
> step-by-step dev environment setup for macOS and Windows (prerequisites, running
> the app, IDE debugging, troubleshooting).

## Architecture

```
┌──────────────┐     ┌──────────────┐
│  Clms.Web    │     │  Clms.Maui   │   ← hosts (swappable)
│ Blazor WASM  │     │ MAUI Hybrid  │
│    (PWA)     │     │  (optional)  │
└──────┬───────┘     └──────┬───────┘
       │                    │
       └────────┬───────────┘
                ▼
        ┌───────────────┐
        │    Clms.Ui    │   ← Razor Class Library: ALL UI lives here
        │  components   │      + IClmsApi (the host-agnostic seam)
        └───────┬───────┘
                │ HTTP
                ▼
        ┌───────────────┐      ┌──────────────┐
        │   Clms.Api    │─────▶│  PostgreSQL  │
        │  + instrument │      │  (container) │
        │    watcher    │      └──────────────┘
        └───────┬───────┘
                │ polls
                ▼
        instrument-drop/*.txt
```

`Clms.Shared` holds the DTOs, referenced by both the API and the clients — no
OpenAPI codegen step, no duplicated models.

## Requirements

- .NET SDK 10 (retarget in `Directory.Build.props` if yours is older)
- Docker Desktop
- An IDE: **VS Code** with the C# Dev Kit extension (macOS or Windows), or
  **Visual Studio 2026** (Windows only)

## Run it

Quick reference below. For first-time setup, IDE configuration, and troubleshooting,
see [development.md](development.md).

### Everything in Docker

```bash
docker compose up --build
```

- API: http://localhost:5090
- Interactive API docs: http://localhost:5090/scalar/v1
- Health: http://localhost:5090/health

### Database in Docker, API + Web from the IDE

Usually nicer for day-to-day work — you get breakpoints.

```bash
docker compose up -d db          # Postgres only

dotnet run --project src/Clms.Api    # http://localhost:5090
dotnet run --project src/Clms.Web    # http://localhost:5080
```

In VS Code, the **API + Web** compound launch config does both at once.

### Sign in

Demo accounts are seeded on first boot. Password for all: `Passw0rd!`

| Username | Role | Can do |
|---|---|---|
| `labManager` | LabManager | Everything, including stock adjustments and unlocking accounts |
| `specimenCollector` | SpecimenCollector | Mark samples collected |
| `labTechnician` | LabTechnician | View orders and results |
| `cashier` | Cashier | (Billing screens — not built yet) |

Signing in as different users is the fastest way to see RBAC working: the
inventory adjust buttons and the "Mark collected" button appear and disappear.

### Watch the instrument pipeline work

```bash
./scripts/simulate-instrument.sh        # well-formed file
./scripts/simulate-instrument.sh --bad  # malformed lines + unknown barcode
```

Within ~5 seconds the worker picks the file up, attaches results to orders `1234`
and `5678`, and moves it to `instrument-drop/_processed/`. Bad files land in
`_failed/` with a `.error.txt` explaining each rejected line. Refresh
**Test Orders** to see results appear.

### Tests

```bash
dotnet test
```

The parser has the coverage that matters — both spec examples, malformed field
counts, bad timestamps, Windows line endings, and mixed good/bad files.

## What's built vs. what's left

Built: JWT auth with role claims, account lockout after 5 failed attempts
(6.2.6), role-gated endpoints and UI, reagent inventory with low-stock (6.4.2)
and 30-day expiration warnings (6.4.6), test orders with barcode generation,
sample collection (6.3.4), instrument file ingestion (6.3.5), audit logging
(6.3.6 / 6.5.1).

Left to build: POS and billing (6.6), reporting (6.5.2–6.5.4), patient CRUD
screens, purchase orders (6.4.3), the lockout notification email (6.2.6 says
email the lab manager — there's a comment marking the spot), and result
validation workflow.

## Deliberate shortcuts

Worth knowing before you build on this — each is a reasonable scaffold choice
that you'd change for anything real:

- **`EnsureCreated()` instead of migrations.** Zero setup steps, but it can't
  evolve a schema. Switch before your second model change:
  `dotnet ef migrations add Initial -p src/Clms.Api`
- **Dev JWT signing key sits in `appsettings.json`.** Move it to user-secrets or
  environment variables before this touches a real network.
- **Passwords use PBKDF2 via a hand-rolled helper.** Fine here; ASP.NET Core
  Identity is what you'd actually use.
- **CORS allows the two dev origins.** Update `AllowedClientOrigins` when you deploy.
- **The instrument watcher polls.** `FileSystemWatcher` is more elegant but
  notoriously flaky across platforms and network shares; polling is boring and works.

## Going native

`src/Clms.Maui` is an optional native host (Windows + macOS via Mac Catalyst/iOS)
built on the same `Clms.Ui` Razor component library as the web app — see
[maui/README.md](../maui/README.md) for build/run instructions per platform and workload setup.

## Platform notes

`Clms.sln` (API, Web, Ui, Shared, Tests) builds and runs identically on macOS and
Windows with no MAUI tooling required. The optional native host (`Clms.Maui`) needs
the MAUI workload, which is where the two platforms differ slightly:

### macOS

- Build/run the native host with the .NET CLI:
  ```bash
  dotnet build src/Clms.Maui -f net10.0-maccatalyst
  # folder is maccatalyst-arm64 on Apple Silicon, maccatalyst-x64 on Intel
  open src/Clms.Maui/bin/Debug/net10.0-maccatalyst/<rid>/CLMS.app
  ```
  (or `-f net10.0-ios` for the simulator). Always run a plain `dotnet build` before
  `-t:Run` — the run target alone doesn't refresh `wwwroot` in the app bundle.
- `Clms.Mac.sln` includes `Clms.Maui` alongside the rest, for opening everything in
  one VS Code/Rider window if you have the MAUI workload installed.

### Windows

- Install the MAUI workload once: `dotnet workload install maui-windows` (CLI), and/or
  add **.NET Multi-platform App UI development** via the Visual Studio Installer if
  you're using Visual Studio 2026.
- `Clms.Windows.sln` includes `Clms.Maui` alongside the rest — open this one in
  Visual Studio for the full solution with native debugging support.
- Build/run the native host directly:
  ```powershell
  dotnet build src/Clms.Maui -f net10.0-windows10.0.19041.0
  ```
  `-t:Run` (and Visual Studio's own F5) can fail with an unquoted-path error if your
  checkout lives at a path containing a space — clone/copy to a space-free path instead
  (e.g. `C:\src\clms`, not `C:\Users\you\My Projects\CLMS`), or launch the built
  `.exe` directly from `bin\Debug\net10.0-windows10.0.19041.0\<rid>\`.
- Use `scripts/simulate-instrument.ps1` instead of the bash script — same behavior,
  no Git Bash/WSL required.
- Don't build from a network share (e.g. a VM's shared folder to its host) — two
  OSes writing into the same `bin/`/`obj/` over SMB causes stale or corrupt builds.
  Work from a local disk path instead.
- If Docker Desktop can't run Postgres in your environment (e.g. no nested
  virtualization in a VM), point `ConnectionStrings__Clms` at any reachable Postgres
  instance instead — as a `dotnet user-secrets set` entry for `Clms.Api` (recommended;
  local-only, doesn't touch checked-in config) or an environment variable override.

Neither platform needs changes to the checked-in `appsettings.json` for this — its
`Host=localhost` default is correct once Postgres is reachable at that address.

## Known unknowns

1. **Package versions** use floating ranges (`10.0.*`, `2.*`). If restore fails,
   pin them to what's actually on your machine: `dotnet list package`.
2. **TFM is `net10.0`.** If `dotnet --version` shows 9.x, change the one line in
   `Directory.Build.props` — and the path in `.vscode/launch.json`.
3. **`Scalar.AspNetCore`** provides the API docs UI. If you'd rather not take the
   dependency, delete the package reference and the `MapScalarApiReference()` line;
   `MapOpenApi()` alone still serves the raw spec.
4. **`chmod +x scripts/simulate-instrument.sh`** if it won't run on macOS/Linux.
