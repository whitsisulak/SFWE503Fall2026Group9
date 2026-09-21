# Development setup (macOS and Windows)

How to get CLMS running on your machine for development. Follow this from top to
bottom on a new machine. Commands are the same on both platforms unless a step says
**macOS** or **Windows**.

You'll run three things locally:

| Piece | What it is | Runs where | URL |
|---|---|---|---|
| Database | PostgreSQL 17 | Docker container `clms-db` | `localhost:5432` |
| API | `src/Clms.Api` (ASP.NET Core) | `dotnet run` / your IDE | http://localhost:5090 |
| Web app | `src/Clms.Web` (Blazor WebAssembly) | `dotnet run` / your IDE | http://localhost:5080 |

The web app talks to the API, and the API talks to the database. Start them in that
order: database, then API, then web app.

---

## 1. Install the prerequisites

| Tool | macOS | Windows |
|---|---|---|
| **.NET 10 SDK** | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) (pick Arm64 on Apple Silicon) | Same link (pick Arm64 on ARM PCs/VMs, x64 otherwise) |
| **Docker Desktop** | [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop/) | Same link (uses WSL 2, which the installer sets up) |
| **Git** | Included with Xcode Command Line Tools: `xcode-select --install` | [git-scm.com](https://git-scm.com/download/win) |
| **Editor** | VS Code + C# Dev Kit, or JetBrains Rider | VS Code + C# Dev Kit, Visual Studio 2026, or Rider |

Check that the tools are installed:

```bash
dotnet --version    # 10.x
docker --version
git --version
```

> If `dotnet --version` shows 9.x, install the 10 SDK. Retargeting the projects works
> (see *Known unknowns* in the [project overview](project-overview.md#known-unknowns)), but everyone on the team should use the same
> SDK version.

## 2. Get the code

```bash
git clone <repo-url> clms
cd clms
```

**Windows:**
- **Clone to a local path with no spaces,** such as `C:\src\clms`. Some MSBuild run
  targets fail on paths that contain a space.
- **Don't build from a network share or a VM shared folder.** Clone to a local disk.

## 3. Start the database

Start **Docker Desktop** first and wait until it says it's running. Then run:

```bash
docker compose up -d db
```

The first run downloads the Postgres image. The database, user, and password are all
`clms`, and the data lives in a Docker volume, so it survives restarts.

Check that the database is healthy:

```bash
docker compose ps        # clms-db should show "healthy"
```

## 4. Build and test

```bash
dotnet build Clms.sln
dotnet test Clms.sln
```

All tests should pass. `Clms.sln` contains everything except the optional native
app, so it builds without any MAUI workloads installed.

## 5. Run the app

Use two terminals, one for each process:

```bash
# Terminal 1: API
dotnet run --project src/Clms.Api
```

```bash
# Terminal 2: web app
dotnet run --project src/Clms.Web
```

Wait for `Now listening on:` in both terminals, then open **http://localhost:5080**.

On first startup the API creates the schema and seeds demo data. The password for
every demo account is `Passw0rd!`:

| Username | Role |
|---|---|
| `labManager` | LabManager (can do everything) |
| `specimenCollector` | SpecimenCollector |
| `labTechnician` | LabTechnician |
| `cashier` | Cashier |

Interactive API docs are at http://localhost:5090/scalar/v1.

Stop the API and web app with **Ctrl+C**. Stop the database with `docker compose stop db`.

### Hot reload

To have the app rebuild automatically when you save a file, use `dotnet watch` instead
of `dotnet run`:

```bash
dotnet watch --project src/Clms.Web
```

## 6. Run and debug from your IDE

### VS Code (macOS and Windows)

1. Open the repo folder (**File → Open Folder**).
2. Accept the prompt to install the recommended extensions (C# Dev Kit, Docker).
3. Start the database (step 3). You can also run it from VS Code: **Terminal → Run
   Task → db: up**.
4. **Run and Debug** panel → choose **API + Web** → **F5**.

This starts both projects with breakpoints enabled and opens the API docs.

Other tasks under **Terminal → Run Task**: `build`, `test`, `simulate instrument`,
`stack: up` / `stack: down`.

### Visual Studio 2026 (Windows only)

Visual Studio runs only on Windows. Visual Studio for Mac was retired in 2024, so Mac
developers should use VS Code or Rider.

**One-time setup**

1. In the **Visual Studio Installer**, make sure the **ASP.NET and web development**
   workload is installed. It includes the .NET SDK and Blazor WebAssembly debugging.
   Add **.NET Multi-platform App UI development** only if you'll work on the native app.
2. Open `Clms.sln` (**File → Open → Project/Solution**). Open `Clms.Windows.sln`
   instead if you also installed the MAUI workload and want the native app in the same
   window.
3. Set up both projects to start together:
   1. Right-click the solution in **Solution Explorer** → **Configure Startup Projects**.
   2. Select **Multiple startup projects**.
   3. Set the **Action** for `Clms.Api` and `Clms.Web` to **Start**, and leave the
      other projects set to **None**.
   4. Use the arrows to move `Clms.Api` above `Clms.Web`, so the API starts first.
   5. Click **OK**.

   Visual Studio saves this in the `.vs/` folder, which isn't committed, so each
   developer does this once per clone.

**Every time**

1. Start Docker Desktop and the database (step 3). The API won't start without it.
2. Press **F5** to debug, or **Ctrl+F5** to run without the debugger.
3. The web app opens in your browser at http://localhost:5080. Breakpoints work in the
   API code and in the Razor components under `Clms.Ui`.
4. Stop both with **Shift+F5**.

Run the tests from **Test → Test Explorer → Run All**.

### Rider (macOS and Windows)

Open `Clms.sln`, then create a **Compound** run configuration that includes the
`Clms.Api` and `Clms.Web` launch profiles.

## 7. Everyday tasks

### Simulate a lab instrument

The API watches the `instrument-drop/` folder for result files. To drop in a test file
while the API is running:

```bash
# macOS
./scripts/simulate-instrument.sh          # well-formed file
./scripts/simulate-instrument.sh --bad    # includes malformed lines
```

```powershell
# Windows (PowerShell)
.\scripts\simulate-instrument.ps1         # well-formed file
.\scripts\simulate-instrument.ps1 -Bad    # includes malformed lines
```

Within about 5 seconds the file moves to `instrument-drop/_processed/`. A bad file
moves to `_failed/` with a `.error.txt` file. Refresh **Test Orders** to see the
results.

### Reset the database

The API uses `EnsureCreated()`, not EF migrations. **It creates the schema only when
the database is empty and never updates it.** If you pull a change that adds or
modifies an entity, and the API then fails with errors like `column ... does not
exist`, delete the database and let it rebuild:

```bash
docker compose down -v        # -v deletes the data volume
docker compose up -d db
```

The next API start recreates the schema and the seed data. This deletes all local data.

### Run everything in Docker

Run the database and the API in containers, for example to check the production build:

```bash
docker compose up --build
```

The web app still runs with `dotnet run --project src/Clms.Web`, because it isn't in
Compose. Stop the containers with `docker compose down`.

## 8. Configuration

All development settings work without changes. You only need this section if your
setup is different.

| Setting | Default | Where |
|---|---|---|
| Database connection | `Host=localhost;Port=5432;Database=clms;Username=clms;Password=clms` | `src/Clms.Api/appsettings.json` → `ConnectionStrings:Clms` |
| API URL used by the web app | `http://localhost:5090/` | `src/Clms.Web/wwwroot/appsettings.json` → `ApiBaseUrl` |
| API port / web port | 5090 / 5080 | each project's `Properties/launchSettings.json` |
| Instrument drop folder | `../../instrument-drop` (relative to `src/Clms.Api`) | `src/Clms.Api/appsettings.Development.json` |
| Allowed web origins (CORS) | `http://localhost:5080`, `https://localhost:7080` | `src/Clms.Api/appsettings.json` → `AllowedClientOrigins` |

**Don't commit machine-specific values** to `appsettings.json`. Override them locally
with user secrets, which are stored outside the repo:

```bash
dotnet user-secrets set "ConnectionStrings:Clms" "Host=...;Port=5432;Database=clms;Username=clms;Password=clms" --project src/Clms.Api
```

You can also use an environment variable with a double underscore in place of `:`,
for example `ConnectionStrings__Clms`.

## 9. Troubleshooting

| Symptom | Fix |
|---|---|
| `failed to connect to the docker API` / `Cannot connect to the Docker daemon` | Docker Desktop isn't running. Start it, wait until it's ready, then retry. |
| `Conflict. The container name "/clms-db" is already in use` | There's an old `clms-db` container from an earlier checkout. Either start it with `docker start clms-db`, or remove it with `docker rm -f clms-db` and run `docker compose up -d db` again. |
| `port is already allocated` on 5432 | Postgres is already installed and running on your machine. Stop that service, or change the host port in `docker-compose.yml` (for example `"5433:5432"`) and set `Port=5433` in your user-secrets connection string. |
| `address already in use` on 5090 or 5080 | Another copy of the API or web app is still running. Stop it, or find it with `lsof -i :5090` (macOS) or `netstat -ano \| findstr 5090` (Windows). |
| API crashes on start with a Npgsql connection error | The database isn't running or isn't healthy yet. Run `docker compose ps`. |
| API errors like `relation/column ... does not exist` after a `git pull` | The schema is out of date. See [Reset the database](#reset-the-database). |
| Web app loads but login fails with a network/CORS error | Make sure the API is running on port 5090 and the web app is on port 5080 (the only origins CORS allows). |
| `permission denied: ./scripts/simulate-instrument.sh` (macOS) | Run `chmod +x scripts/simulate-instrument.sh`. |
| `...ps1 cannot be loaded because running scripts is disabled` (Windows) | Run `powershell -ExecutionPolicy Bypass -File scripts\simulate-instrument.ps1`, or allow scripts for your user once: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`. |
| Instrument files stay in `instrument-drop/` and aren't picked up | The API must run with `src/Clms.Api` as its working directory (the default for `dotnet run --project` and the VS Code launch config), because the drop path is relative to it. |
| `The current .NET SDK does not support targeting .NET 10.0` | Install the .NET 10 SDK. |
| Web app shows an old version after changes | Hard-refresh the page (**Cmd+Shift+R** / **Ctrl+Shift+R**). Blazor WebAssembly caches files in the browser. |

## Optional: native app (MAUI)

`src/Clms.Maui` wraps the same UI in a native app (iOS/Mac Catalyst on macOS, Windows
app on Windows). You don't need it for web development. To build it, see *Going
native* and *Platform notes* in the [project overview](project-overview.md#going-native).
