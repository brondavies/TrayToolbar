# Release notes

`CHANGELOG.md` is the canonical, Keep a Changelog-style release history for this repository.
This file stays as the supplemental narrative release summary for highlights, rollout notes, and packaging context that are easier to read in prose.

- Canonical history: [`../CHANGELOG.md`](../CHANGELOG.md)
- GitHub release assets: <https://github.com/brondavies/TrayToolbar/releases>
- Update and packaging trust boundary: [`update-security.md`](update-security.md)

## 1.7.2 Pre-release

## Highlights

- Signed portable release builds in GitHub CI. Tagged releases now publish the SignPath-signed `win-arm64` and `win-x64` zip assets built by GitHub Actions.
- Safer signing boundaries in CI. `pull_request` validation builds still package the app, but they intentionally skip SignPath signing so signing credentials are never exposed to untrusted pull-request code.
- Better Windows shortcut launching. `.lnk` app shortcuts now keep their saved arguments and working directory, while safe non-app targets open directly.
- Safer update and release links. Toast actions now go back through TrayToolbar, and only real GitHub Releases URLs are accepted.
- Simpler release packaging. Local builds now produce the same portable `win-arm64` and `win-x64` zip assets used for releases.
- More reliable local packaging parity. `build.ps1` now works on machines that rely on the .NET SDK's `dotnet msbuild` fallback instead of a standalone `msbuild.exe` on `PATH`.
- Also included a benchmark project plus contributor docs and templates to support future maintenance.
- **Full changelog**: see [`../CHANGELOG.md`](../CHANGELOG.md).

## Features

- Creates a menu with all your favorite shortcuts within easy reach just by putting them in a local folder.
- Replaces the feature removed from Windows 11 for custom toolbars on the taskbar.
- Choose the folder(s) and customize the file filter.
- Automatically updates the menu with changes in the folder.
- Launches links, files, and apps from this menu which exist in the chosen folder.
- Quick access to this menu with configurable global shortcut keys.
- Custom icons for individual folders.
- Windows 11 ["Dark mode"](https://support.microsoft.com/en-us/windows/change-colors-in-windows-d26ef4d6-819a-581c-1581-493cfcc005fe) support.
- Includes support for English, Spanish, French, German, Portuguese, Italian, Japanese, Korean, Chinese, and Russian.

## Requirements

- This application runs on .NET Desktop Runtime 8. Download and install the runtime here:
  [Download .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) | [Arm64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.18-windows-arm64-installer) | [x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.18-windows-x64-installer) | [winget instructions](https://learn.microsoft.com/dotnet/core/install/windows?WT.mc_id=dotnet-35129-website#install-with-windows-package-manager-winget)

## Installation

- Download and extract the zip file to a writable folder such as `C:\tools\TrayToolbar` or `C:\Users\%Username%\AppData\Local\TrayToolbar`.
- Run `TrayToolbar.exe` from this folder.
- Select a folder containing your shortcuts.
- Select **Run on log in** to start the application every time.
- Click **Save**.
- If you don't see the icon in your system tray, open **Settings** → **Personalization** → **Taskbar** → **Other system tray icons** and turn on TrayToolbar.