# TrayToolbar
 Creates a menu with all your favorite shortcuts within easy reach just by putting them in a local folder.

[![CodeQL](https://github.com/brondavies/TrayToolbar/actions/workflows/codeql.yml/badge.svg?branch=master&event=push)](https://github.com/brondavies/TrayToolbar/actions/workflows/codeql.yml) [![Download](https://img.shields.io/github/v/release/brondavies/TrayToolbar?label=Version&labelColor=%23222&color=%233a4)](https://github.com/brondavies/TrayToolbar/releases)

- Replaces the feature removed from Windows 11 for custom toolbars on the taskbar
- Choose the folder(s) and customize the file filter
- Launches links, files and apps from this menu which exist in the chosen folders and subfolders
- Automatically updates the menu with changes in the folder
- Quick access to this menu with configurable global shortcut keys
- Custom icons for individual folders
- Choose your preference for icon and font sizes
- Windows 10/11 ["Dark mode"](https://support.microsoft.com/en-us/windows/change-colors-in-windows-d26ef4d6-819a-581c-1581-493cfcc005fe) support
- Includes support for English, Spanish, French, German, Portuguese, Italian, Japanese, Chinese, Russian, and Korean

<p style="background:#f9f9f9">💡 Can also be used in Windows 8 and 10!</p>

## Download
[Check the releases](https://github.com/brondavies/TrayToolbar/releases) or compile the source code in any modern version of Visual Studio.

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
