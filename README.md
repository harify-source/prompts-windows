# Prompts for Windows

Tray shell for [Prompts](https://prompts.harify.com) — open templates quickly from a global shortcut.

## Install

1. Install the [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) if Windows does not already include it.
2. Download the latest `Prompts-windows.zip` from [Releases](https://github.com/harify-source/prompts-windows/releases).
3. Unzip and run `Prompts.exe`.

## Usage

- Left-click the tray icon to show or hide Prompts.
- Right-click for Reload, Open at Login, Settings, and Quit.
- Default shortcut: Ctrl+Alt+P (change or clear it in Settings).

## Development build

Requires Windows with the .NET 8 SDK.

```powershell
pwsh -File scripts/build.ps1
# Output: dist/Prompts-windows.zip
```

Optional: point at a local site with `$env:PROMPTS_SITE_URL = "http://localhost:4310"`.

```powershell
dotnet run --project tests/SitePolicyChecks/SitePolicyChecks.csproj
```

## License

MIT — see [LICENSE](LICENSE).
