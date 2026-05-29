# TrayToolbar Developer Guide

This guide is the contributor-facing companion to `README.md` and `AGENTS.md`.

- `README.md` stays user-facing and short.
- `AGENTS.md` stays automation-oriented.
- This guide explains how TrayToolbar is built, packaged, configured, localized, and debugged.

## Project layout

Important paths:

- Solution: `src/TrayToolbar.sln`
- App project: `src/TrayToolbar/TrayToolbar.csproj`
- Test project: `src/TrayToolbar.Tests/TrayToolbar.Tests.csproj`
- Benchmark project: `src/TrayToolbar.Benchmarks/TrayToolbar.Benchmarks.csproj`
- Main runtime/config code:
  - `src/TrayToolbar/ConfigHelper.cs`
  - `src/TrayToolbar/Program.cs`
  - `src/TrayToolbar/SettingsForm.cs`
  - `src/TrayToolbar/Models/TrayToolbarConfiguration.cs`
  - `src/TrayToolbar/Services/ConfigurationStore.cs`
  - `src/TrayToolbar/Services/FolderScanner.cs`
  - `src/TrayToolbar/Services/LaunchPolicyEvaluator.cs`
  - `src/TrayToolbar/Services/ShortcutTargetResolver.cs`
  - `src/TrayToolbar/UpdateHelper.cs`
  - `src/TrayToolbar/UpdateLogic.cs`
- Test seams and infrastructure:
  - `src/TrayToolbar/Services/IProcessLauncher.cs`
  - `src/TrayToolbar/Services/ITrayToolbarFileSystemWatcher.cs`
  - `src/TrayToolbar.Tests/TestInfrastructure/`
- Localization resources: `src/TrayToolbar/Resources/`

## Build, validation, and packaging

TrayToolbar is a Windows Forms app targeting `net8.0-windows`.
That target is intentional: the app relies on Windows desktop APIs and native WinRT interop, but the project avoids a version-specific Windows TFM to keep the portable release output lean.

### Prerequisites

For contributors:

- Windows 11
- .NET 8 SDK to build and test
- .NET Desktop Runtime 8 to run the published portable app

If you are using Visual Studio, Visual Studio 2022 17.8+ is the minimum practical baseline for .NET 8 SDK support.

### Command matrix

Use these commands as the authoritative developer workflow.

| Purpose | Run from | Command | Notes |
| --- | --- | --- | --- |
| Restore | `src\` | `dotnet restore .\TrayToolbar.sln` | Restores app, tests, and benchmark dependencies. |
| Test | `src\` | `dotnet test .\TrayToolbar.sln` | Required before merging. |
| Format | `src\` | `dotnet format .\TrayToolbar.sln` | Use before submitting a PR. |
| Build | `src\` | `dotnet build .\TrayToolbar.sln` | Standard validation build. |
| Package | repo root | `./build.ps1` | Portable release parity script. |
| Benchmark (optional) | `src\` | `dotnet run -c Release --project .\TrayToolbar.Benchmarks\TrayToolbar.Benchmarks.csproj --filter *` | Run only when you intentionally want BenchmarkDotNet output. |

### Portable release outputs

`build.ps1` and CI are expected to produce these assets:

- `TrayToolbar-win-arm64-portable-<version>.zip`
- `TrayToolbar-win-x64-portable-<version>.zip`

`build.ps1` also writes `publish/TrayToolbar.exe` as the final local publish output.

### Release workflow and versioning

- The version source of truth is `<Version>` in `src/TrayToolbar/TrayToolbar.csproj`.
- Release tags must use `v<Version>` and match the project version exactly.
- `CHANGELOG.md` is the canonical release history.
- `docs/release-notes.md` is the supplemental narrative release summary.
- `.github/workflows/dotnet-desktop.yml` is the canonical CI packaging path.
- `build.ps1` is the local parity and troubleshooting path.

Workflow behavior:

- `push` on `master`: restore, test, format verification, build, publish workflow artifacts, and SignPath-sign them when the repository SignPath configuration is present
- `pull_request` on `master`: the same validation and packaging flow, but signing is intentionally skipped so signing credentials are not exposed to untrusted pull-request code
- `workflow_dispatch`: same validation and packaging flow for manual dry runs, with signing when SignPath is configured and the run is not a pull request
- `push` on `v*.*.*` tags: same validation and packaging flow plus GitHub Release asset upload from the SignPath-signed outputs; tag builds require SignPath configuration

### SignPath artifact configuration

- the workflow uploads each portable zip with `archive: false`, so SignPath receives the real file name such as `TrayToolbar-win-arm64-portable-<version>.zip` or `TrayToolbar-win-x64-portable-<version>.zip`
- the uploaded artifact is treated as a `<zip-file>` in SignPath
- the root `TrayToolbar.exe` inside the zip is Authenticode-signed
- runtime update installation also validates that staged `TrayToolbar.exe` with `WinVerifyTrust` and requires the signer identity to match `UpdateSignerPolicy.Default` in `src/TrayToolbar/Services/AuthenticodeUpdateSignatureVerifier.cs`
- if the signing certificate subject changes, or if you add or rotate pinned thumbprints, update `UpdateSignerPolicy.Default` before the next release so the new signer is update-valid
- the release-signing GitHub policy requires GitHub-hosted runners, rejects workflow reruns, and expects a GitHub branch ruleset that blocks force pushes and requires reviewed pull requests on the default branch
- in the SignPath `release-signing` policy itself, enable **Verify origin** and set **Allowed branch names** to `master`

Unsigned PR workflow artifacts and local `build.ps1` outputs are useful for testing, but they are not valid automatic-update artifacts unless they are signed with the allowed TrayToolbar publisher identity.

### Reproducible-build note

The repo currently aims for **functional parity** between local and CI builds rather than byte-identical reproducibility.
Use Windows, the .NET 8 SDK, Release configuration, and the commands above when comparing artifacts.

## Runtime expectations

### Supported environment

TrayToolbar is intended for Windows 11.

The app is portable rather than MSI-installed, so it expects to run from a folder the user can read and write.
That matters because:

- unhandled startup/runtime exceptions are written next to the executable as `Error-*.txt`
- the update flow launches a downloaded replacement executable and copies it over the existing app

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

On startup, `Program.Main()` calls `ConfigHelper.MigrateConfiguration()`. The migration moves the legacy file into `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json` only if the new config file does not already exist.

### Command-line reference

TrayToolbar does not currently expose a public end-user CLI, but these internal arguments are part of the runtime contract and should stay documented.

| Switch | Consumed by | Purpose | Publicly supported? |
| --- | --- | --- | --- |
| `--show` | `Program.Main()` | Forces the settings form to appear in the running instance or after restart. | Internal / stable enough for repo tooling |
| `--newversion` | `Program.Main()` | Pairs with `--show` to highlight the “new version installed” state after update restart. | Internal |
| `--update <path to installed TrayToolbar.exe>` | `UpdateHelper.ProcessUpdate()` | Runs the staged updater flow and copies the extracted updater over the installed executable. | Internal |

Treat these switches as implementation contract, not polished product CLI surface.

### Startup and launch behavior

`Program.Launch(...)` delegates launch decisions to `LaunchPolicyEvaluator`.

The evaluator builds an approved-root set from:

- the application root
- configured folder paths from `TrayToolbarConfiguration.Folders`

Launch evaluation then allows or rejects targets based on both origin and target type.

Direct raw values can launch only when they resolve to:

- an existing directory under an approved root
- an existing file under an approved root
- a trusted TrayToolbar GitHub Releases URL

Shortcut behavior is more nuanced:

- `.url` files are resolved and the resolved target must satisfy the active `LaunchPolicy`
- `.lnk` files are resolved when possible; in stricter policies, unresolved shortcuts are rejected
- if a `.lnk` target is allowed, the shortcut file itself is launched so normal shell behavior is preserved

Run-on-login is configured in the current user's registry hive:

- `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\TrayToolbar`

The app also writes a tray-notification preference under:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\RunNotification`

## Configuration schema reference

The configuration model is defined by:

- `TrayToolbarConfiguration`
- `FolderConfig`

Serialization uses `System.Text.Json` with indented output.
`LaunchPolicy` is serialized as a string enum value.

### Top-level configuration

| Property | Type | Default | Optional | Notes |
| --- | --- | --- | --- | --- |
| `HideFileExtensions` | `bool` | `false` | Yes | Hides file extensions in menu text. `.lnk` and `.url` names are already shown without extensions in menus. |
| `IgnoreAllDotFiles` | `bool` | `false` | Yes | Prevents items from dot-prefixed folders such as `.git` from appearing in nested menus. |
| `IgnoreFiles` | `string[]` | `['.bak', '.config', '.dll', '.ico', '.ini']` | Yes | Case-insensitive patterns filtered by regex-style matching. |
| `IncludeFiles` | `string[]` | `['.*']` | Yes | Allow-list patterns; the default effectively includes everything not ignored. |
| `IgnoreFolders` | `string[]` | `['.git', '.github']` | Yes | Folder names skipped during scanning. Matching is case-insensitive. |
| `MaxMenuPath` | `int` | `512` | Yes | Guards against extremely deep or looping menu paths. |
| `Theme` | `int` | `0` | Yes | `0` = system, `1` = light, `-1` = dark. |
| `FontSize` | `float` | `9` | Yes | Menu font size. |
| `LargeIcons` | `bool` | `false` | Yes | `false` means small icons. |
| `Language` | `string?` | `null` | Yes | Two-letter code such as `en` or `fr`. Null or empty behaves as “use system language.” |
| `CheckForUpdates` | `bool` | `true` | Yes | Enables release checks against GitHub. |
| `LaunchPolicy` | `LaunchPolicyMode` | `ConfiguredSources` | Yes | Advanced trust-boundary control for files, shortcuts, URLs, and network paths. Currently JSON-config only; not exposed in the settings UI. |
| `ShowFolderLinksAsSubMenus` | `bool` | `false` | Yes | If enabled, `.lnk` files that resolve to directories are expanded as submenus instead of launched as shortcuts. |
| `NotifyOnUpdateAvailable` | `bool` | `false` | Yes | Requires `CheckForUpdates = true`; also enables the periodic update timer. |
| `UpdateCheckInterval` | `double` | `1440` | Yes | Minutes between background update checks. Default is 1 day. |
| `ShowToolTips` | `bool` | `false` | Yes | Shows full file or folder paths as tooltips on menu items. |
| `Folders` | `FolderConfig[]` | `[]` | Yes | Folder list displayed as tray icons and menus. In practice the first-run UI seeds one default folder before save. |

### Launch policy modes

| Mode | What it allows | What it blocks |
| --- | --- | --- |
| `ConfiguredSources` | Local files and folders under approved roots, plus `.url` or `.lnk` shortcuts whose resolved targets are acceptable; TrayToolbar GitHub Releases URLs are always allowed. | Direct arbitrary raw URLs and missing targets. |
| `NetworkBlocked` | Local files and folders under approved roots, plus trusted TrayToolbar GitHub Releases URLs. | UNC paths, non-release remote URLs, and `.url` or `.lnk` targets that resolve to network or other remote locations. |
| `LocalOnly` | Local files and folders whose resolved targets stay within approved roots, plus trusted TrayToolbar GitHub Releases URLs. | UNC paths, non-release remote URLs, and local shortcut targets that resolve outside approved roots. |

Approved roots are the application root plus configured folder paths.

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
  "LaunchPolicy": "ConfiguredSources",
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
   - folder control labels and buttons
6. Watch for truncated labels, placeholder regressions, or missing resource keys.

## FAQ and troubleshooting

### Where is the config file?

TrayToolbar reads and writes `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json`.

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
- the release page URL passes the trusted-release validation
- the current build is not already newer than the published release

If the current build is newer than the published release, TrayToolbar treats it as a prerelease scenario and updates the label text without showing the **Update now** action toast.

For the full update asset and execution contract, see [`update-security.md`](update-security.md).

### Why doesn’t clicking an item launch anything?

Launches are filtered through `LaunchPolicyEvaluator`.
Common reasons a click does nothing:

- the file or folder no longer exists
- the item is outside the configured roots
- a shortcut resolves to a blocked target under `NetworkBlocked` or `LocalOnly`
- the target is a raw arbitrary URL rather than a trusted release URL or a permitted shortcut target

### What should I check when startup or shortcut behavior fails?

For startup issues:

- confirm the **Run on log in** setting is enabled
- inspect `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\TrayToolbar`
- verify the app still exists at the stored executable path

For shortcut issues:

- TrayToolbar registers global hotkeys with `RegisterHotKey`
- hotkeys are stored as strings such as `CTRL + ALT + T`
- conflicting system-wide shortcuts can prevent the expected behavior
- each tray icon maps to a folder index, so deleting or reordering folders can change which menu a hotkey opens
- `.url` and `.lnk` files now pass through launch-policy evaluation instead of launching blindly

### Where are crash logs and other diagnostics?

Unhandled exceptions caught in `Program.Main()` are written to the application directory as:

- `Error-yyyyMMddHHmmss.txt`

When triaging an issue, start there.

## Reproducing and diagnosing issues

When filing or reproducing a bug, gather:

- TrayToolbar version
- architecture (`x64` or `arm64`)
- Windows version or build
- whether the issue is in startup, updates, folder scanning, launching, hotkeys, or localization
- a sanitized config snippet from `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json`
- any `Error-*.txt` files from the app directory
- exact steps to reproduce the issue
- whether the problem happens every time or only intermittently

Helpful extra details:

- whether the app is running from a writable folder
- whether the affected menu item is a file, folder, `.lnk`, `.url`, or trusted release URL
- whether the issue disappears after turning off custom filtering (`IncludeFiles`, `IgnoreFiles`, `IgnoreFolders`)

## Maintenance notes

When changing configuration behavior, runtime prerequisites, release packaging, or localization support:

- update this guide
- update `README.md` if the user-facing installation story changes
- update `CHANGELOG.md` and `docs/release-notes.md` when release-facing behavior changes
- update `docs/update-security.md` and `SECURITY.md` when trust boundaries or update execution rules change
- update `AGENTS.md` if validation steps or project structure change
