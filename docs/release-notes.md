# Release notes

`CHANGELOG.md` is the canonical, Keep a Changelog-style release history for this repository.
This file stays as the supplemental narrative release summary for highlights, rollout notes, and packaging context that are easier to read in prose.

- Canonical history: [`../CHANGELOG.md`](../CHANGELOG.md)
- GitHub release assets: <https://github.com/brondavies/TrayToolbar/releases>
- Update and packaging trust boundary: [`update-security.md`](update-security.md)

## 1.8.1

## Highlights

- Automatic updates work again. The Authenticode check added in 1.8.1 rejected every update, signed or not, because of a `WinVerifyTrust` interop mistake. Applying `[MarshalAs(UnmanagedType.LPStruct)]` to the already-by-ref `in Guid actionId` parameter passed a pointer to a pointer, so Windows never resolved the verification action and returned `TRUST_E_PROVIDER_UNKNOWN` (`0x800B0001`) for every file it was handed.
- Clearer failure messages. A trust status meaning "the check could not be performed" now says so instead of reporting an unverifiable signature, and any status the app does not recognize now carries its `WinVerifyTrust` code in the message shown to you.
- Regression coverage. The update signature verifier now has tests that exercise the real Windows trust call rather than a stand-in, so a broken interop declaration fails the build instead of shipping.
- **Full changelog**: see [`../CHANGELOG.md`](../CHANGELOG.md).

## Upgrading from 1.8.1

1.8.1 checks the staged updater with its own broken verifier before launching it, so a 1.8.1 install cannot update itself to 1.8.2 automatically. Download the portable zip for your architecture from the release assets and replace `TrayToolbar.exe` once. Automatic updates work normally from 1.8.2 onward.

Installs on 1.7.1 or earlier hit the same wall from the other side. 1.7.1 shipped no verifier, but the staged 1.8.1 executable failed its own self-check before copying itself over the target, which is why the 1.7.1 to 1.8.1 upgrade also appeared to do nothing.

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