# TrayToolbar
 Creates a menu with all your favorite shortcuts within easy reach just by putting them in a local folder.

[![CodeQL](https://github.com/brondavies/TrayToolbar/actions/workflows/codeql.yml/badge.svg?branch=master&event=push)](https://github.com/brondavies/TrayToolbar/actions/workflows/codeql.yml) [![Download](https://badge.fury.io/gh/brondavies%2FTrayToolbar.svg)](https://github.com/brondavies/TrayToolbar/releases)

- Replaces the Windows 7-10 feature that no longer exists in Windows 11 for custom toolbars on the taskbar
- Choose the folder and customize the file filter
- Launches links, files and apps from this menu which exist in the chosen folder
- Automatically updates the menu with changes in the folder
- Includes support for English, Spanish, French, German, Portuguese, Italian, Japanese, Chinese, Russian, and Korean

<p style="background:#f9f9f9">💡 Can also be used in Windows 8 and 10!</p>

## Download
[Check the releases](https://github.com/brondavies/TrayToolbar/releases) or compile the source code in any modern version of Visual Studio.

## Requirements
This application runs on .NET Desktop Runtime 8.  Download and install the runtime here:

[Download .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)


[Arm64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.3-windows-arm64-installer) | [x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.3-windows-x64-installer) | [x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.3-windows-x86-installer) | [winget instructions](https://learn.microsoft.com/dotnet/core/install/windows?WT.mc_id=dotnet-35129-website#install-with-windows-package-manager-winget)

## Installation
This application is "portable" so there is no setup package, just extract and go!
- Download and extract the zip file to a writable folder such as C:\tools\TrayToolbar or C:\Users\\%Username%\AppData\Local\TrayToolbar
- Run TrayToolbar.exe from this folder
- Select a folder containing your shortcuts
- Select **Run on log in** to start the application every time
- Click **Save**
- If you don't see the icon in your system tray, open **Settings** → **Personalization** → **Taskbar** → **Other system tray icons** and turn on TrayToolbar
