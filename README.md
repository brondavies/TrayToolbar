# TrayToolbar
 Creates a menu with all your favorite shortcuts within easy reach just by putting them in a local folder.

[![CodeQL](https://github.com/brondavies/TrayToolbar/actions/workflows/codeql.yml/badge.svg?branch=master&event=push)](https://github.com/brondavies/TrayToolbar/actions/workflows/codeql.yml) [![GitRated](https://gitrated.com/brondavies/TrayToolbar/badge?theme=dark)](https://gitrated.com/brondavies/TrayToolbar) [![Download](https://img.shields.io/github/v/release/brondavies/TrayToolbar?label=Version&labelColor=%23222&color=%233a4)](https://github.com/brondavies/TrayToolbar/releases)

- Replaces the feature removed from Windows 11 for custom toolbars on the taskbar
- Choose the folder(s) and customize the file filter
- Launches links, files and apps from this menu which exist in the chosen folders and subfolders
- Automatically updates the menu with changes in the folder
- Quick access to this menu with configurable global shortcut keys
- Custom icons for individual folders
- Choose your preference for icon and font sizes
- Windows 11 ["Dark mode"](https://support.microsoft.com/en-us/windows/change-colors-in-windows-d26ef4d6-819a-581c-1581-493cfcc005fe) support
- Includes support for English, Spanish, French, German, Portuguese, Italian, Japanese, Chinese, Russian, and Korean

## Download

[Check the releases](https://github.com/brondavies/TrayToolbar/releases) or compile the source code in any modern version of Visual Studio.

For the canonical release history, see [CHANGELOG.md](CHANGELOG.md).
For supplemental narrative release notes, packaging callouts, and rollout context, see [docs/release-notes.md](docs/release-notes.md).

## Requirements
This application runs on .NET Desktop Runtime 8.  Download and install the runtime here:

[Download .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

[Arm64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.18-windows-arm64-installer) | [x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.18-windows-x64-installer) | [winget instructions](https://learn.microsoft.com/dotnet/core/install/windows?WT.mc_id=dotnet-35129-website#install-with-windows-package-manager-winget)

## Installation
This application is "portable" so there is no setup package, just extract and go!

- Download and extract the zip file to a writable folder such as C:\tools\TrayToolbar or C:\Users\\%Username%\AppData\Local\TrayToolbar
- Run TrayToolbar.exe from this folder
- Select a folder containing your shortcuts
- Select **Run on log in** to start the application every time
- Click **Save**
- If you don't see the icon in your system tray, open **Settings** → **Personalization** → **Taskbar** → **Other system tray icons** and turn on TrayToolbar

## Configuration and launch policy

TrayToolbar stores its current configuration in `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json`.

The default launch policy trusts items that originate from your configured folders and always allows TrayToolbar GitHub Releases URLs for update flows.

For the full configuration schema, command-line reference, and launch-policy details, see [docs/developer-guide.md](docs/developer-guide.md).

## More docs

- Contributor workflow, release tagging, labels, and PR guidance: [CONTRIBUTING.md](CONTRIBUTING.md)
- Build, packaging, CLI, configuration, localization, and troubleshooting details: [docs/developer-guide.md](docs/developer-guide.md)
- Release history: [CHANGELOG.md](CHANGELOG.md)
- Supplemental release notes: [docs/release-notes.md](docs/release-notes.md)
- Update and launch trust boundaries: [docs/update-security.md](docs/update-security.md)
- Private vulnerability reporting: [SECURITY.md](SECURITY.md)

