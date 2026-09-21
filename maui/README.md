# MAUI Blazor Hybrid host

`src/Clms.Maui` is the optional native host, built on top of the same `Clms.Ui`
Razor component library the web app uses. **Deliberately not in `Clms.sln`** — a
MAUI project can't restore without the MAUI workload installed, so including it
there would break `dotnet build Clms.sln` for anyone who hasn't installed it
(teammates, graders, CI).

If you have the workload, use `Clms.Windows.sln` (Windows) or `Clms.Mac.sln`
(macOS) instead — both include `Clms.Maui` alongside the rest of the solution.

## Installing the workload

```bash
# macOS (iOS + Mac Catalyst)
dotnet workload install maui

# Windows
dotnet workload install maui-windows
```

In Visual Studio 2026 (Windows only), add the **.NET Multi-platform App UI
development** workload via the Visual Studio Installer for full designer/debugger
support — the CLI install alone is enough to build and run, just without VS's
project-system integration.

## Building and running

Always run a plain `dotnet build` before `-t:Run` — the run target alone doesn't
refresh `wwwroot` in the app bundle, so you'll see stale CSS/HTML otherwise.

```bash
# macOS — Mac Catalyst
dotnet build src/Clms.Maui -f net10.0-maccatalyst
# folder is maccatalyst-arm64 on Apple Silicon, maccatalyst-x64 on Intel
open src/Clms.Maui/bin/Debug/net10.0-maccatalyst/<rid>/CLMS.app

# macOS — iOS simulator
dotnet build src/Clms.Maui -f net10.0-ios
dotnet build src/Clms.Maui -t:Run -f net10.0-ios
```

```powershell
# Windows
dotnet build src/Clms.Maui -f net10.0-windows10.0.19041.0
```

On Windows, `-t:Run` (and Visual Studio's F5) can fail with an unquoted-path error
if the checkout lives at a path containing a space. Use a space-free path, or launch
the built `.exe` directly from `bin\Debug\net10.0-windows10.0.19041.0\<rid>\`.

On macOS, `-t:Run` for Mac Catalyst fails with "not a tty" outside an interactive
terminal — use `open` on the `.app` instead.

## What changes vs. the web host

Nothing in `Clms.Ui`. That's the point — the same components, the same `IClmsApi`
interface, the same `Routes` root component. Only the host project and its DI
registration differ (each host points `HttpClient.BaseAddress` at its own API URL).

## Reality check for a semester project

- iOS device builds need a paid Apple Developer account and a Mac in the loop.
- Android is easier — build an APK and sideload it.
- Budget real time for workload/SDK version mismatches; they're the usual sink.

Ship the web app first. Treat this as a stretch goal.
