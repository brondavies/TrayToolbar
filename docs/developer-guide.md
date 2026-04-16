# TrayToolbar Developer Guide

This guide is the contributor-facing companion to `README.md` and `AGENTS.md`.

- `README.md` stays user-facing and short.
- `AGENTS.md` stays automation-oriented.
- This guide explains how TrayToolbar is built, configured, localized, and debugged.

## Project layout

Important paths:

- Solution: `src/TrayToolbar.sln`
- App project: `src/TrayToolbar/TrayToolbar.csproj`
- Test project: `src/TrayToolbar.Tests/TrayToolbar.Tests.csproj`
- Main runtime/config code:
  - `src/TrayToolbar/ConfigHelper.cs`
  - `src/TrayToolbar/Program.cs`
  - `src/TrayToolbar/SettingsForm.cs`
  - `src/TrayToolbar/Models/TrayToolbarConfiguration.cs`
  - `src/TrayToolbar/Services/ConfigurationStore.cs`
  - `src/TrayToolbar/Services/FolderScanner.cs`
  - `src/TrayToolbar/UpdateHelper.cs`
  - `src/TrayToolbar/UpdateLogic.cs`
- Localization resources: `src/TrayToolbar/Resources/`

## Build and validation

TrayToolbar is a Windows Forms app targeting `net8.0-windows`.
That target is intentional: the app relies on Windows desktop APIs and native WinRT interop, but the project avoids a version-specific Windows TFM to keep the portable release output lean.

### Prerequisites

For contributors:

- Windows 11
- .NET 8 SDK to build and test
- .NET Desktop Runtime 8 to run the published portable app

Microsoft's current Windows install guidance also confirms:

- desktop apps such as Windows Forms require the **.NET Desktop Runtime**
- `dotnet --list-sdks` shows installed SDKs
- `dotnet --list-runtimes` shows installed runtimes, including `Microsoft.WindowsDesktop.App`

If you're using Visual Studio, Microsoft documents Visual Studio 2022 17.8+ as the minimum version for .NET 8 SDK support.

### Verified commands

Run these from `src\` unless noted otherwise:

- `dotnet format .\TrayToolbar.sln`
- `dotnet test .\TrayToolbar.sln`
- `dotnet build .\TrayToolbar.sln`

Preferred release build from the repository root:

- `./build.ps1`

What `build.ps1` does today:

- restores `src/TrayToolbar/TrayToolbar.csproj`
- publishes **framework-dependent**, **single-file** portable builds for:
  - `win-arm64`
  - `win-x64`
- writes `publish/TrayToolbar.exe`
- creates zip artifacts at the repo root:
  - `TrayToolbar-win-arm64-portable-<version>.zip`
  - `TrayToolbar-win-x64-portable-<version>.zip`

### Recommended contributor loop

1. Confirm .NET 8 SDK and Desktop Runtime are installed.
2. Make the smallest focused change possible.
3. Run `dotnet test .\TrayToolbar.sln`.
4. Run `dotnet format .\TrayToolbar.sln` before sending changes out.
5. Use `./build.ps1` when you need to validate portable release packaging.

## Runtime expectations

### Supported environment

TrayToolbar is intended for Windows 11

The app is portable rather than MSI-installed, so it expects to run from a folder the user can read and write.
That matters for two reasons:

- unhandled startup/runtime exceptions are written next to the executable as `Error-*.txt`
- update flow launches a downloaded replacement executable and copies it over the existing app

A writable location such as `C:\tools\TrayToolbar` or `%LOCALAPPDATA%\TrayToolbar` is a good fit.

### Configuration file locations

Current config location:

- `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json`

Relevant symbols:

- `ConfigHelper.ProfileFolder`
- `ConfigHelper.ConfigurationFile`

Legacy migration path:

- old location: `<app folder>\TrayToolbar.json`
- symbol: `ConfigHelper.LegacyConfigurationFile`

On startup, `Program.Main()` calls `ConfigHelper.MigrateConfiguration()`. The migration moves the legacy file into `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json` **only if** the new config file does not already exist.

### Startup and launch behavior

TrayToolbar supports:

- launching existing files
- launching existing directories
- launching direct TrayToolbar GitHub Releases `https://` URLs

The launcher intentionally rejects other direct remote targets because `Program.Launch()` only proceeds for:

- `File.Exists(fileName)`
- `Directory.Exists(fileName)`
- `UpdateLogic.TryGetAllowedRemoteLaunchUri(fileName, out _)`

That means direct remote launches are currently restricted to:

- host: `github.com`
- path prefix: `/brondavies/TrayToolbar/releases`

Local files are still allowed if they exist on disk, including files the user placed in monitored folders.

Run-on-login is configured in the current user's registry hive:

- `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\TrayToolbar`

The app also writes a tray notification preference under:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\RunNotification`

## Localization and translations

Localization files live in `src/TrayToolbar/Resources/`.

Current resource set:

- `Resources.resx` (default strings)
- `Resources.de.resx`
- `Resources.es.resx`
- `Resources.fr.resx`
- `Resources.it.resx`
- `Resources.ja.resx`
- `Resources.ko.resx`
- `Resources.pt.resx`
- `Resources.ru.resx`
- `Resources.zh.resx`

### How localization works

- `Resources.resx` is the neutral/default resource file.
- Language-specific `Resources.<culture>.resx` files override those strings.
- `SettingsForm.LoadResources()` switches `CurrentCulture` and `CurrentUICulture` based on `Configuration.Language`.
- If no language is configured, TrayToolbar falls back to the current system culture captured when `SettingsForm` starts.

### Supported UI languages

`SettingsForm.SupportedLanguages` currently exposes these two-letter culture codes:

- `en`
- `es`
- `fr`
- `de`
- `pt`
- `it`
- `ja`
- `zh`
- `ru`
- `ko`

If you add a new translation file, update the supported language list in `SettingsForm.cs` so it can be selected in the UI.

### Translation workflow

When updating translations:

1. Add or edit keys in `Resources.resx` first.
2. Mirror those keys in the relevant `Resources.<culture>.resx` files.
3. Build and run the app.
4. Open Settings and switch languages.
5. Check common surfaces:
   - settings labels
   - update banner text
   - context menu items
   - folder control labels/buttons
6. Watch for truncated labels, placeholder regressions, or missing resource keys.

## Configuration schema reference

The configuration model is defined by:

- `TrayToolbarConfiguration`
- `FolderConfig`

Serialization uses `System.Text.Json` with indented output.

### Top-level configuration

| Property | Type | Default | Optional | Notes |
| --- | --- | --- | --- | --- |
| `HideFileExtensions` | `bool` | `false` | Yes | Hides file extensions in menu text. `.lnk` and `.url` names are already shown without extensions in menus. |
| `IgnoreAllDotFiles` | `bool` | `false` | Yes | Prevents items from dot-prefixed folders such as `.git` from appearing in nested menus. |
| `IgnoreFiles` | `string[]` | `['.bak', '.config', '.dll', '.ico', '.ini']` | Yes | Case-insensitive patterns filtered by regex-style matching. |
| `IncludeFiles` | `string[]` | `['.*']` | Yes | Allow-list patterns; the default effectively includes everything not ignored. |
| `IgnoreFolders` | `string[]` | `['.git', '.github']` | Yes | Folder names skipped during scanning. Matching is case-insensitive. |
| `MaxMenuPath` | `int` | `512` | Yes | Guards against extremely deep/looping menu paths. |
| `Theme` | `int` | `0` | Yes | `0` = system, `1` = light, `-1` = dark. |
| `FontSize` | `float` | `9` | Yes | Menu font size. |
| `LargeIcons` | `bool` | `false` | Yes | `false` means small icons. |
| `Language` | `string?` | `null` | Yes | Two-letter code such as `en` or `fr`. Null or empty behaves as “use system language.” |
| `CheckForUpdates` | `bool` | `true` | Yes | Enables release checks against GitHub. |
| `ShowFolderLinksAsSubMenus` | `bool` | `false` | Yes | If enabled, `.lnk` files that resolve to directories are expanded as submenus instead of launched as shortcuts. |
| `NotifyOnUpdateAvailable` | `bool` | `false` | Yes | Requires `CheckForUpdates = true`; also enables the periodic update timer. |
| `UpdateCheckInterval` | `double` | `1440` | Yes | Minutes between background update checks. Default is 1 day. |
| `ShowToolTips` | `bool` | `false` | Yes | Shows full file/folder paths as tooltips on menu items. |
| `Folders` | `FolderConfig[]` | `[]` | Yes | Folder list displayed as tray icons and menus. In practice the first-run UI seeds one default folder before save. |

### Folder entries

| Property | Type | Default | Optional | Notes |
| --- | --- | --- | --- | --- |
| `Icon` | `string?` | `null` | Yes | Path to a custom icon source. Environment variables and `file://` paths are expanded. |
| `IconIndex` | `int` | `0` | Yes | Index used when extracting an icon from `Icon`. Omitted from JSON when `0`. |
| `Name` | `string?` | `null` | No in practice | Folder path to scan. Must point to an existing directory before the UI will save successfully. |
| `Recursive` | `bool` | `false` | Yes | Whether subdirectories are scanned. Newly added folders in the UI default this to `true`, even though the model default is `false`. |
| `Hotkey` | `string?` | `null` | Yes | Global hotkey text such as `CTRL + ALT + T`. |

### Legacy and compatibility properties

These older JSON properties are still read for compatibility but should not be written in new configs:

| Legacy property | Current behavior |
| --- | --- |
| `Folder` | If present, it is converted into a single entry in `Folders`. |
| `IgnoreFileTypes` | If present, it is copied into `IgnoreFiles`. |
| `MaxRecursionDepth` | Obsolete and ignored. |

### Example configuration

```json
{
  "HideFileExtensions": false,
  "IgnoreAllDotFiles": false,
  "IgnoreFiles": [".bak", ".config", ".dll", ".ico", ".ini"],
  "IncludeFiles": [".*"],
  "IgnoreFolders": [".git", ".github"],
  "MaxMenuPath": 512,
  "Theme": 0,
  "FontSize": 9,
  "LargeIcons": false,
  "Language": "en",
  "CheckForUpdates": true,
  "ShowFolderLinksAsSubMenus": true,
  "NotifyOnUpdateAvailable": true,
  "UpdateCheckInterval": 1440,
  "ShowToolTips": false,
  "Folders": [
    {
      "Name": "%APPDATA%\\Microsoft\\Windows\\Start Menu",
      "Recursive": true,
      "Hotkey": "CTRL + ALT + T"
    },
    {
      "Name": "C:\\tools\\TrayToolbarShortcuts",
      "Icon": "C:\\Windows\\System32\\shell32.dll",
      "IconIndex": 3,
      "Recursive": false
    }
  ]
}
```

## FAQ and troubleshooting

### Where is the config file?

TrayToolbar reads and writes:

- `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json`

If you are upgrading from an older portable layout, the app will move `<app folder>\TrayToolbar.json` into the LocalAppData profile folder the first time it starts, as long as the new file does not already exist.

### Why is the tray icon missing?

A few common causes:

- Windows may have placed the icon in the hidden overflow area.
- Windows 11 may require enabling the icon under **Settings > Personalization > Taskbar > Other system tray icons**.
- The app only creates tray icons after folder configuration is valid.

If a configured folder path is empty or no longer exists, the settings form is shown instead of quietly creating icons.

### Why doesn’t a folder or file appear in the menu?

Check these in order:

- the folder path exists
- `Recursive` is enabled if the item lives in a subfolder
- the file is not filtered out by `IgnoreFiles`
- the parent folder is not filtered out by `IgnoreFolders`
- the item is not in a dot-prefixed path while `IgnoreAllDotFiles` is enabled
- the file matches at least one `IncludeFiles` pattern

Folder scanning is handled by `FolderScanner`, and menu creation has an extra `MaxMenuPath` guard to avoid runaway nesting.

### Why didn’t an update notification appear?

All of the following must line up:

- `CheckForUpdates` is `true`
- `NotifyOnUpdateAvailable` is `true`
- the machine supports Windows toast notifications (`Windows 10 10240+`)
- GitHub's REST `releases/latest` API returns a usable stable release payload
- the latest release version differs from the current app version
- the release page URL passes the direct launch policy
- the current build is not already newer than the published release

If the current build is newer than the published release, TrayToolbar treats it as a prerelease scenario and updates the label text without showing the “Update now” action toast.

For the full update asset and execution contract, see [`docs/update-security.md`](update-security.md).

### Why doesn’t clicking an item launch anything?

`Program.Launch()` only starts:

- existing files
- existing directories
- direct TrayToolbar GitHub Releases `https://` URLs

That means arbitrary raw URLs are intentionally ignored.
If a path is wrong or the target no longer exists, nothing launches.

### What should I check when startup or shortcut behavior fails?

For startup issues:

- confirm the **Run on log in** setting is enabled
- inspect `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\TrayToolbar`
- verify the app still exists at the stored executable path

For shortcut issues:

- TrayToolbar registers global hotkeys with `RegisterHotKey`
- hotkeys are stored as strings such as `CTRL + ALT + T`
- conflicting system-wide shortcuts can prevent the expected behavior
- each tray icon maps to a folder index, so deleting/reordering folders can change which menu a hotkey opens

### Where are crash logs and other diagnostics?

Unhandled exceptions caught in `Program.Main()` are written to the application directory as:

- `Error-yyyyMMddHHmmss.txt`

When triaging an issue, start there.

## Reproducing and diagnosing issues

When filing or reproducing a bug, gather:

- TrayToolbar version
- architecture (`x64` or `arm64`)
- Windows version/build
- whether the issue is in startup, updates, folder scanning, launching, hotkeys, or localization
- a sanitized config snippet from `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json`
- any `Error-*.txt` files from the app directory
- exact steps to reproduce the issue
- whether the problem happens every time or only intermittently

Helpful extra details:

- whether the app is running from a writable folder
- whether the affected menu item is a file, folder, `.lnk`, or `https://` URL
- whether the issue disappears after turning off custom filtering (`IncludeFiles`, `IgnoreFiles`, `IgnoreFolders`)

## Maintenance notes

When changing configuration behavior, runtime prerequisites, or localization support:

- update this guide
- update `README.md` if the user-facing installation story changes
- update `AGENTS.md` if validation steps or project structure changes
