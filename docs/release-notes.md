# 1.7.0 Pre-release

## Highlights

- Added a built-in Windows toast notification implementation for update alerts without the previous notification dependency for a smaller bundle.
- Added one-click updating from update notifications and the **Update now** action in Settings.
- Hardened the self-update flow by validating release metadata, asset names, download URLs, content types, and SHA-256 digests before launching the staged updater.
- Added automated test coverage for configuration handling, folder scanning, startup behavior, and update logic.
- Added contributor-facing documentation, security guidance, and GitHub issue templates to support future development.

- **Full Changelog**: https://github.com/brondavies/TrayToolbar/compare/v1.6.2...v1.7.0

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