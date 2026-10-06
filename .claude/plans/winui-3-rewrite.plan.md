# TrayToolbar 2.0.0 — WinUI 3 rewrite plan

Written 2026-10-06 from the state of `master` at v1.8.4 (plus PR #114). Covers every open
issue in the **2.0.0** milestone and every line of `TODO.md`.

## 1. Goals and decisions

Key goals (from the maintainer):

1. Keep every existing behaviour and configuration option working.
2. Rewrite the UI in WinUI 3.
3. Load menus on demand without visible lag: the level below any visible menu is always
   loaded before it can be shown.
4. Load icons late and asynchronously behind a placeholder, to save memory.
5. Move update and app/version information into an About dialog; keep the "update
   available" notice on the Settings window. The About dialog ships first in 1.9.0 (Phase 0),
   together with a **Check for updates** checkbox, so anyone who wants to stay on 1.x can opt
   out of 2.x updates from the UI.

Decisions already made:

| Question | Decision |
| --- | --- |
| Minimum Windows | **Windows 11 only** (build 22000+). 1.x stays available for Windows 10. |
| .NET | **.NET 10 LTS, framework-dependent.** .NET 8 and 9 leave support on 2026-11-10. |
| Scope | Everything in the milestone **except** #72 (toolbar inside the taskbar; no supported API on stock Windows 11) and the archive-browsing and clipboard parts of #8. Both move to a 2.1 milestone. |
| Packaging | **Single exe if at all possible**, folder zip as the fallback. See §3.1: a framework-dependent single file is not supported by WinUI 3, so this decision needs a spike and a follow-up answer. |
| 1.9.0 bridge | **Yes.** Ship 1.9.0 on the WinForms app first (Phase 0). |
| LaunchPolicy | **Dropped.** The `LaunchPolicy` the docs described was never implemented. 1.9.0 corrects the docs instead; 2.0 keeps today's launch rules (shell-execute what the configured folders contain). |
| JSON-only settings | **Stay JSON-only** (`HideFileExtensions`, `ShowToolTips`, `UpdateCheckInterval`, launch logging). Exception: `CheckForUpdates` gets a checkbox in 1.9.0 and keeps it in 2.0. |
| About dialog in 1.9.0 | **Yes.** Built in WinForms for 1.9.0 with the content in §4.7; 2.0 re-creates it in WinUI. |
| Sort order | **Explorer order (`StrCmpLogicalW`) becomes the default**; called out in the release notes. |

## 2. Scope

### 2.1 Issues and TODO lines

| Item | Summary | Phase | Notes |
| --- | --- | --- | --- |
| Key goal 2 | WinUI 3 rewrite | 3 | |
| Key goal 3 | On-demand menu loading, one level ahead | 2–3 | §4.3 |
| Key goal 4 | Late icon loading with placeholder | 2–3 | §4.4; also closes #52 |
| Key goal 5 | About dialog; update notice stays in Settings | 0, 3 | Built in 1.9.0 (WinForms), re-created in 2.0; §4.7 |
| Opt-out | "Check for updates" checkbox so 1.x users can decline 2.x | 0 | Bound to the existing `CheckForUpdates` setting |
| #52 | Retry icons that failed to load | 2 | Failure cache with retry interval |
| #76 | Tray icon gone after sleep/hibernate | 3 | `TaskbarCreated` + resume re-add, §4.2 |
| #87 | Folder order differs from Explorer (`-#@=` vs `#-=@`) | 2 | `StrCmpLogicalW` comparer, default on |
| #42 | Optional strict alphabetical sort (folders not first) | 2 | `SortFoldersFirst` setting |
| #69 | Manual item ordering | 4 | `ItemOrder` per folder + "Arrange items" dialog |
| #88 | Direct file/app entries, not only folders | 4 | Tray entry kind `Item` |
| #40 | Shell folders by CLSID (This PC, Network, …) | 4 | Shell-item source, §4.5 |
| #60 | Drop files/folders/URLs onto TrayToolbar | 4 | Settings window + open menu as drop targets |
| #79 | Drag items out of the menu to Explorer/other apps | 4 | OLE `CF_HDROP` drag source |
| #70 | Rounded corners, darker background, rounded hover | 3 | Defaults in WinUI; two appearance settings |
| #8 (parts) | Hover-only cascading, right-click everywhere, double-click folder opens it | 3–4 | Cut/copy/delete/rename already come from the shell context menu |
| #8 (parts) | Browse archives; paste into the tree | **2.1** | Deferred |
| #72 | Host the toolbar in the taskbar | **2.1** | Deferred; investigate only |
| TODO "LaunchPolicy" | Documented but never implemented | 0 | Dropped; docs and TODO corrected in 1.9.0 |

### 2.2 Out of scope

Windows 10, MSIX packaging, Microsoft Store, Native AOT (blocked for unpackaged apps until
the `Microsoft.Windows.SDK.BuildTools.MSIX` fix reaches a stable SDK), archive browsing,
in-menu paste, taskbar hosting.

## 3. Findings that shape the design

Research notes with sources are in the session scratchpad; the facts that matter:

### 3.1 Packaging

- WinUI 3 single-file publishing is supported **only** for unpackaged + **self-contained**
  (`WindowsAppSDKSelfContained=true`, `SelfContained=true`, `IncludeAllContentForSelfExtract=true`).
  It is a self-extracting bundle: ~70–120 MB exe that unpacks ~150 MB into `%TEMP%` on first
  run. Framework-dependent single-file is explicitly unsupported, and three open bugs track
  single-file crashes on 1.8–2.3 (microsoft-ui-xaml #10173, #10876).
- Framework-dependent + `WindowsAppSDKSelfContained=true` gives a **folder** of about 40 MB
  (≈9 MB zipped) and needs the .NET 10 Desktop Runtime installed, as today.
- Trimming must stay off (`PublishTrimmed=false`), or the app crashes with 0xc000027b.
- So the two packaging answers ("framework-dependent" and "single exe") conflict. The spike in
  Phase 1 produces both and measures size, cold start and update time; the maintainer picks.
  **Recommendation: framework-dependent folder zip.**

### 3.2 The 1.8.x updater cannot install a folder

`UpdateHelper.DownloadAndUpdateAsync` in 1.8.4 accepts a zip with 1–32 entries, exactly one
of them `TrayToolbar.exe`, extracts **only that exe**, verifies its Authenticode signature and
runs it with `--update <installed exe>`. A WinUI 3 folder has more than 32 files (ten satellite
resource folders alone) and the lone exe cannot start without its runtime DLLs. Either:

- ship a **1.9.0 bridge release** first (Phase 0) whose updater understands folder zips, or
- ship 2.0 as a single-file self-contained exe (one zip entry; the old updater path works).

The bridge release is recommended regardless, because it also moves 1.x to .NET 10 before the
.NET 8 end of support.

### 3.3 Menus

- `MenuFlyout` needs a `XamlRoot`; every WinUI tray implementation shows it from a hidden,
  zero-size WinUI window placed at the cursor and closes it on focus loss.
- `MenuFlyoutSubItem` has **no Opening event**, no virtualization, and no first-letter
  type-ahead. Items added to an already open submenu don't render. Lazy population has to
  happen on `PointerEntered`/`GotFocus` before the hover timer fires.
- Nobody ships a hundreds-of-items cascading `MenuFlyout` from a tray icon. The alternative is
  one borderless window per menu level with a virtualized `ItemsRepeater`, which also gives
  type-ahead, drag sources and full control of hover timing.
- Acrylic, rounded corners and shadow are the Windows 11 defaults for flyouts (#70 is mostly
  free). Hover highlight is a theme resource.

### 3.4 Other platform facts

- No `NotifyIcon`. `Shell_NotifyIcon` via CsWin32 on a **hidden top-level window** (message-only
  windows never receive `TaskbarCreated`). WinUIEx 2.9.3 `TrayIcon` is the reference code;
  H.NotifyIcon's WinUI flyout mode has open blocking bugs.
- `IContextMenu` works unchanged from WinUI 3 when the hidden HWND owns `TrackPopupMenuEx`.
  The flyout must be closed first (the popup menu takes capture).
- Drag-out through `DataPackage.SetStorageItems` needs `StorageFile`, which Microsoft now lists
  as "requires package identity". OLE `DoDragDrop` with `CF_HDROP` from the hidden HWND is
  the safe route. Drops onto a WinUI window work unpackaged.
- `AppNotificationManager` replaces the hand-rolled toast/COM activator and works unpackaged,
  but `Register()` throws in self-contained unpackaged builds on 2.4.0–2.5.1 (WindowsAppSDK
  #6774) and toasts are silently dropped on 25H2 (#6821). Keep the old activator as fallback.
- `RegisterHotKey`, `WM_SETTINGCHANGE`, `WM_POWERBROADCAST` all land on the hidden HWND; no
  XAML window subclassing.
- Memory: expect **60–120 MB private bytes idle** (3–6× today) once XAML is initialized, and
  there is no supported way to start XAML lazily. Cold start ~0.5–1.4 s.
- Tests: logic stays in a plain class library; `VSTest` cannot host an unpackaged WinUI exe.
  MSTest.Sdk 4.5 supports self-hosted unpackaged WinUI test apps (`[UITestMethod]`).
- CI: `windows-2025` runners have VS 2022 17.14, .NET 10, Windows SDK 26100 and the WinAppSDK
  C# component, but no Windows App Runtime. Build with `/p:Platform=x64|arm64`, never AnyCPU.
- Local machine: VS 2026 Community lacks the Windows App SDK / WinUI component and the Windows
  SDK kit; install the "Windows application development" workload before Phase 1.

## 4. Architecture

### 4.1 Projects

```
src/TrayToolbar.sln
├─ TrayToolbar.Core/        net10.0-windows            no UI; everything testable
├─ TrayToolbar/             net10.0-windows10.0.22621  WinUI 3 app (WindowsPackageType=None)
├─ TrayToolbar.Tests/       MSTest, references Core    existing tests move here unchanged
├─ TrayToolbar.UITests/     MSTest.Sdk UITestMethod    self-hosted unpackaged WinUI test app
└─ TrayToolbar.Benchmarks/  BenchmarkDotNet            tree loading, icon cache, sorting
```

Package set: `Microsoft.WindowsAppSDK.WinUI` 2.5.x (not the metapackage, which pulls AI/ML
payload), `Microsoft.Windows.CsWin32`, `CommunityToolkit.Mvvm` 8.4,
`CommunityToolkit.WinUI.Controls.SettingsControls`. No H.NotifyIcon dependency.

### 4.2 Core library (`TrayToolbar.Core`)

Moves from the WinForms project with namespaces kept:

- `Models`: `TrayToolbarConfiguration`, `FolderConfig`, `FilePattern`, `Release`.
- `Services`: `ConfigurationStore`, `FolderScanner`, `ShortcutTargetResolver`, `LaunchLogger`,
  `GitHubReleaseClient`, `AuthenticodeUpdateSignatureVerifier`, `UpdateSignerPolicy`,
  `UpdateLogic`, `UpdateHelper`, all `I*` seams and their fakes.
- `Resources`: the `.resx` files and `Resources.Designer.cs`. Runtime language switching
  stays `ResourceManager`-based, not MRT, so the current behaviour (switch without restart) is
  kept.
- `Extensions`: string/path helpers; `ShellContextMenu` (third-party, 1,581 lines) kept as-is
  with its owner HWND made injectable.

New in Core:

- **`Shell/`** — `IShellItemSource` abstraction over "a thing that can be listed, launched,
  iconed and context-menued": `FileSystemItemSource` (today's behaviour, uses `IFileSystem`)
  and `ShellFolderItemSource` (`IShellItem`/`IEnumShellItems`, `SHParseDisplayName`, for
  `::{CLSID}` and `shell:` names; #40). Both yield `ShellEntry { DisplayName, Path or PIDL,
  IsContainer, IsShortcutToFolder, SortKey }`.
- **`Menu/`** — `MenuTree`, `MenuNode`, `MenuLoader` (§4.3), `MenuSorter` (§4.6),
  `ItemOrderStore` (#69).
- **`Icons/`** — `IconService`, `IconCache`, `IconFailureCache` (§4.4). Produces
  `IconHandle`s (HICON wrappers) and, for the UI, 16/32 px BGRA pixel buffers; no
  `System.Drawing` in the UI path.
- **`Tray/`** — `TrayIconHost`: hidden top-level HWND (CsWin32) owning `Shell_NotifyIcon`
  (`NOTIFYICON_VERSION_4`, one `uID` per entry), `TaskbarCreated` re-registration with
  `NIM_ADD` retry (#76), `WM_POWERBROADCAST` resume check via `Shell_NotifyIconGetRect`,
  `WM_HOTKEY` dispatch, `WM_SETTINGCHANGE` (theme + environment), the single-instance
  broadcast messages, owner window for `IContextMenu` and `DoDragDrop`.
- **`Launch/`** — `Launcher` (today's `Program.Launch` rules, unchanged: `.lnk` metadata
  honored, everything else shell-executed) plus `ShellExecuteEx` with a PIDL for shell items.
- **`Notifications/`** — `INotificationService` with `AppNotificationManager` implementation
  and the existing COM activator as fallback.
- **`Updates/`** — updater v2: whole-zip extraction, folder staging, atomic swap on next
  launch (§4.9).

### 4.3 Menu model and on-demand loading (key goal 3)

```
MenuTree (per tray entry)
  Root node = the folder            level 0
   ├─ children                      level 1  ← loaded before the icon is clickable
   │   ├─ grandchildren             level 2  ← loaded while level 1 is visible
```

Rules:

- A node has `State ∈ {Unloaded, Loading, Loaded, Failed}`, `Children`, `LoadedAtUtc`,
  `Version`.
- Startup loads levels 0 and 1 for every entry (spinner on the tray icon until then, as
  now). Recursive entries no longer enumerate the whole tree at startup.
- `EnsureLoaded(node, depth: 2)` runs when a menu level is shown or a submenu header gets
  pointer focus; it completes the next level synchronously if cached, otherwise awaits it
  (target: < 50 ms for ≤ 500 entries on SSD) and prefetches the level after that in the
  background with a bounded parallelism of 2 and a cancellation token tied to "menu closed".
- Memory cap: nodes two levels below any visible level are unloaded after the menu closes
  (children dropped, icons released to the cache). `MaxMenuPath` still guards depth.
- The `FileSystemWatcher` (now created through `IFileSystemWatcherFactory`, which 1.x never
  used in production) invalidates the affected node and its parent instead of rebuilding
  menus; the 500 ms debounce is implemented for real (1.x never awaited it).
  `ShellFolderItemSource` uses `SHChangeNotifyRegister` for drive insert/remove.
- Folder links as submenus (`ShowFolderLinksAsSubMenus`) and the cycle guard stay as node
  behaviour; link targets are separate trees sharing the cache.
- Dedup, hide-extension, `IgnoreAllDotFiles`, `IgnoreFolders` and pattern rules move into
  `MenuLoader` unchanged, with the two 1.x case-sensitivity inconsistencies fixed to
  case-insensitive (`IgnoreFolders` in `MenuItemCollection`, name dedup).

### 4.4 Icon pipeline (key goal 4, #52)

- Every item starts with a placeholder from a small set rendered once per DPI: folder, file,
  executable, shortcut, drive, shell folder.
- `IconService.GetAsync(entry, size, ct)` resolves on the thread pool:
  - per-extension key for ordinary files (`SHGetFileInfo` + `SHGFI_USEFILEATTRIBUTES`, no
    file access), per-file key for `.exe .lnk .url .ico .cur`, folders with custom icons,
    and shell items (`IShellItemImageFactory`, which also serves #40).
  - `.url` parsing and the `ms-settings:` special case move over unchanged.
- `IconCache`: LRU per size, 512 entries default, pixel buffers not HICONs, evicted on
  memory pressure and when a tree level is unloaded.
- `IconFailureCache`: a failed or empty icon is remembered with a timestamp; retried after
  `IconRetryInterval` (new setting, default 5 min) or when the menu is reopened after that
  interval, so icons for shortcuts to drives that appear later fill in (#52).
- Resolution is cancelled when the menu closes; results already produced are kept.

### 4.5 Shell items (#40, #88, part of #8)

- `FolderConfig.Name` accepts `::{CLSID}`, `shell:Name` and ordinary paths. Settings offers a
  "Shell folder…" picker listing This PC, Desktop, Network, Control Panel, Libraries,
  Recycle Bin, OneDrive, user profile folders.
- A tray entry whose `Name` is a file, or with `Kind: "Item"`, is a **direct item** (#88):
  left click launches it, right click shows the tray menu plus the shell context menu
  entries for the file; icon from the file. Settings gets "Add file or shortcut".
- Right click on any item, including submenu headers and shell items, shows the shell
  context menu (owner = hidden HWND; the flyout is closed first). Double-click on a submenu
  header opens that folder in Explorer. Cut, copy, delete and rename come from the shell
  menu; paste into the tree is deferred.

### 4.6 Sorting (#87, #42, #69)

- Default comparer becomes `StrCmpLogicalW` (Explorer order, numeric-aware), replacing
  `OrdinalIgnoreCase`; fixes #87.
- `SortFoldersFirst` (bool, default `true`) keeps today's grouping; `false` gives strict
  alphabetical (#42).
- `FolderConfig.ItemOrder: string[]` of relative paths; listed items come first in that
  order, the rest follow in the default sort. Settings "Arrange items…" dialog per folder:
  tree with drag-to-reorder and move up/down buttons; "Reset to automatic". Order survives
  file renames only by path, which is documented.

### 4.7 Windows and dialogs

- **Tray menu** (left click): spike decides between (a) `MenuFlyout` hosted in a hidden
  window and (b) a custom `CascadingMenuWindow` per level over a virtualized
  `ItemsRepeater`. Either way the view binds to `MenuNode` view-models, so the choice is
  confined to `TrayToolbar/Menu/`. Required behaviours: hover-open submenus with Explorer-like
  delay, open direction flips at the screen edge, mouse-wheel scrolling following the Windows
  lines-to-scroll setting, first-letter navigation that scrolls the selection into view,
  Escape/arrows/Enter, menu font size, small/large icons, tooltips showing the full path when
  `ShowToolTips` is on, dark/light/system theme, acrylic + rounded corners, rounded hover
  highlight, right-click shell menu, drag-out, keyboard-accessible.
- **Tray right-click menu**: Options, Open Folder, TrayToolbar Location, About, Exit. The
  "TrayToolbar on GitHub" item from 1.8.4 (#84) moves into About as the releases link.
- **Settings window**: Mica, `SettingsCard` groups: Folders (rows with path combo + folder
  autocomplete, browse, shell-folder picker, add file/shortcut, remove, include subfolders,
  shortcut key set/clear, custom icon via `PickIconDlg`, clear icon, arrange items), Filters
  (include/exclude files with `/regex/` validation, exclude folders, folder links as
  submenus), Appearance (theme, menu font size 9–72, icon size, menu backdrop
  Acrylic/Mica/None, black background in dark mode — #70), Sorting (folders first), Language
  (System + 10), Startup (run on log in), Updates (check for updates, notify when available). Settings that are
  JSON-only in 1.x stay JSON-only. Keeps the first-run
  default folder (`%APPDATA%\Microsoft\Windows\Start Menu`, recursive), validation message
  boxes (as `ContentDialog`), Save/Cancel semantics, hide-to-tray on close/minimize, the
  `--show` / `--newversion` arguments, and the "Updated to version {0}!" message.
- **Update notice** stays on Settings as an `InfoBar`: "A new version is available" with
  *Update now* and *Details* (opens About), or "You are using a prerelease version".
- **About dialog** (`ContentDialog` from Settings and from the tray menu): app icon, name,
  version and architecture, .NET and Windows App SDK versions, copyright, links (releases,
  changelog, report an issue, SignPath sponsor line), *Check for updates*, *Update now*
  (with the existing confirmation), last check time, and the post-update "Updated to
  version" state. Same content as the 1.9.0 WinForms dialog plus the Windows App SDK
  version, so the strings and the view-model carry over from Phase 0.
- Accessibility: every control named for Narrator; menu items expose `AutomationProperties`.

### 4.8 Startup, instance and lifetime

- Custom `Main` (`DISABLE_XAML_GENERATED_MAIN`): `UpdateHelper.ProcessUpdate()` first and
  pure-.NET (no WinUI type touched, so a staged exe can run the updater before its runtime
  files exist), then the per-user mutex and `--show` broadcast exactly as today, then
  `Application.Start` with `DispatcherShutdownMode.OnExplicitShutdown` and no window.
- `TrayIconHost` is created on the UI thread; Settings and About are created lazily.
- Unhandled exceptions still write `Error-yyyyMMddHHmmss.txt` next to the exe.
- `SetShowInTray`, `MigrateConfiguration`, hotkey registration, launch-log flush on exit:
  unchanged.

### 4.9 Updates and packaging (Phase 5)

- Release assets keep their names: `TrayToolbar-win-{x64|arm64}-portable-<version>.zip`.
  Contents depend on the packaging decision (folder, or a single exe).
- Updater v2 (shared with the 1.9.0 bridge): download, SHA-256 digest from the release
  API, extract the **whole** zip to `%TEMP%\TrayToolbar\Updates\<id>\extract`, verify
  `TrayToolbar.exe` Authenticode against `UpdateSignerPolicy`, verify every other `.dll`
  carries a Microsoft or SignPath Foundation signature, run
  `extract\TrayToolbar.exe --update "<installed exe>"`. The staged exe broadcasts exit,
  waits, copies the folder over the install directory (new files first, exe last, retries as
  now), leaves files the new version does not ship alone (the install folder may hold the
  user's own scripts), cleans stale temp folders on the next start, relaunches with
  `--show --newversion`.
- `docs/update-security.md`, `UpdateHelperTests` and `UpdateLogicTests` updated for the
  folder contract and the entry-count limits (raise 32 → 512, 512 MiB cap kept).
- `build.ps1` publishes per RID with `-p:Platform=` set, `WindowsAppSDKSelfContained=true`,
  `PublishTrimmed=false`, zips the publish folder. The SignPath artifact configuration keeps
  signing the root `TrayToolbar.exe`.
- `.github/workflows/dotnet-desktop.yml`: `/p:Platform=x64` and `arm64` builds, no AnyCPU;
  Windows App Runtime not needed for the framework-dependent-with-self-contained-SDK layout;
  `codeql.yml` manual build updated to the new project paths.
- Version bump to 2.0.0 in `TrayToolbar.csproj` and `app.manifest`; manifest declares
  `PerMonitorV2` DPI and Windows 11 `supportedOS`.

## 5. Compatibility contract (must hold in 2.0.0)

- `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json` read and written with the same
  property names; every 1.x property keeps its meaning and default (table in
  `docs/developer-guide.md`); obsolete `IgnoreFileTypes`, `Folder`, `MaxRecursionDepth`
  still migrate; legacy `TrayToolbar.json` next to the exe still moves.
- New properties are additive: `SortFoldersFirst`, `IconRetryInterval`, `MenuBackdrop`,
  `BlackBackgroundInDarkMode`, `FolderConfig.Kind`, `FolderConfig.ItemOrder`.
- Command line: `--show`, `--newversion`, `--update <path>`; toast activation argument.
- Registry: `HKCU\...\Run\TrayToolbar`, `RunNotification\StartupTNotiTrayToolbar`, the
  AUMID `Brontech.TrayToolbar` and its activator CLSID.
- Mutex `Local\TrayToolbar_<SID>` and the two broadcast window messages (same GUIDs).
- Launch rules (`.lnk` handling, advertised shortcuts, runas, arguments only for
  exe/com/bat/cmd, working directory, window style), launch logging formats, hotkey string
  format (`⊞ + CTRL + ALT + SHIFT + Key`), 10 languages with runtime switching, update
  eligibility rules and signer policy, release URL allow-list.
- Behaviours: spinner while loading, click-while-loading opens when ready (10 s window),
  empty menu opens Settings, submenu direction at screen edge, wheel scroll speed, letter
  navigation, right-click shell menu, folder links as submenus with cycle guard, Start-menu
  shortcut cleanup from 1.8.4.

## 6. Phases

Effort is for one developer, rough.

### Phase 0 — 1.9.0 bridge release on the WinForms app (≈1.5 weeks)

- Retarget to `net10.0-windows`; CI and docs updated for the .NET 10 Desktop Runtime.
- Updater v2 (§4.9) so 1.9 can install a folder-shaped 2.0; tests for both zip shapes.
- **About dialog** (WinForms `Form`, dark-mode aware like `SettingsForm`): app icon, name,
  version and architecture, .NET runtime version, copyright, links (releases, changelog,
  report an issue, SignPath sponsor line), *Check for updates*, *Update now* with the
  existing confirmation, last check time, and the prerelease / "Updated to version {0}!"
  states. Opened from a new **About** item in the tray right-click menu and from the
  Settings footer. *Update now* moves from the Settings footer into About; the
  "A new version is available!" footer link stays and opens About. The "TrayToolbar on
  GitHub" menu item added in 1.8.4 is removed from the tray menu; its link lives in About
  (the `TrayToolbar on GitHub` resource string is reused there). The tray menu becomes
  Options, Open Folder, TrayToolbar Location, About, Exit.
- **Check for updates** checkbox in Settings next to "Notify me when a new version is
  available", bound to `CheckForUpdates`. Off means no release-API call at startup, no
  update timer, no toast, and the notify checkbox is disabled. Default stays `true`.
- New resource strings for About and the checkbox in all 10 cultures; `ResourcesTests`
  guards them.
- Extract `TrayToolbar.Core` from the WinForms project (models, services, resources, update,
  launch logging) with the existing tests moved across. The WinForms exe references Core.
  This is the only part of the rewrite that ships to 1.x users, and it de-risks Phase 2.
- Branch `release/1.x` cut after 1.9.0 for hotfixes; `master` becomes 2.0 development.
- Exit: 1.9.0 published through the normal SignPath flow; a 1.8.4 install auto-updates to
  it; `dotnet test` green, including: `CheckForUpdates=false` round-trips through the
  Settings checkbox and JSON, and with it off the fake release client is never called at
  startup; About shows the right version, architecture and runtime.

### Phase 1 — Spikes (≈1–2 weeks, throwaway code)

1. **Packaging**: build an empty WinUI 3 app in both shapes (framework-dependent folder;
   self-contained single-file bundle) on Windows App SDK 2.5.1 and .NET 10, x64 and arm64.
   Record zip size, extracted size, cold-start time, memory, whether single-file launches
   at all, and whether `AppNotificationManager.Register()` works in each. **Decision gate
   with the maintainer** (§8 item 1).
2. **Menu**: hidden host window + `MenuFlyout` with 500 items, 3 levels, runtime icons, on a
   mixed-DPI dual-monitor setup. Measure first-open latency, hover-open delay, scrolling,
   keyboard. Accept if first open < 100 ms and type-ahead can be added; otherwise build the
   custom per-level window (`ItemsRepeater`) and measure the same. **Decision gate.**
3. **Shell menu + drag**: `IContextMenu` from the hidden HWND while a menu is open;
   `DoDragDrop` with `CF_HDROP` from the chosen menu host into Explorer.
- Exit: written decisions on packaging and menu host, with numbers.

### Phase 2 — Core 2.0 (≈2–3 weeks)

- `Shell/`, `Menu/`, `Icons/`, `Tray/`, `Launch/`, `Notifications/` as in §4.2–4.6.
- `StrCmpLogicalW` sorting, `SortFoldersFirst`, `ItemOrderStore`, `IconFailureCache`.
- Real debounce, watcher-driven invalidation, shell change notifications.
- Benchmarks: level load for 100/500/2,000 entries, icon cache hit/miss, sort.
- Exit: unit tests for every rule in §4.3–4.6 (in-memory `IFileSystem`, fake shell source,
  fake clock); benchmarks meet the §9 targets; no WinUI reference in Core.

### Phase 3 — WinUI 3 shell (≈3–4 weeks)

- App bootstrap (§4.8), `TrayIconHost` wiring, menu host from the Phase 1 decision, tray
  right-click menu, Settings window with all existing controls, About dialog, update
  `InfoBar`, theme handling, hotkeys, notifications, single instance.
- `TrayToolbar.UITests` smoke tests: menu opens from a simulated tray click, submenu loads
  one level ahead, placeholder is replaced, settings round-trip.
- Exit: feature parity checklist (§5) passes by hand on Windows 11 x64 and arm64; existing
  config files from 1.8.4 load unchanged; memory and startup measured and recorded.

### Phase 4 — Milestone features (≈3–4 weeks)

- #88 direct items, #40 shell folders and picker, #69 arrange dialog, #42 setting,
  #60 drop targets (Settings folder rows and open menu root: dropped folder → new tray entry;
  dropped file/shortcut/URL → copied or written as `.url` into that folder, with a confirm),
  #79 drag-out, #70 appearance settings, #8 double-click and right-click parity, #76 resume
  handling verified with a sleep/wake test.
- Exit: each issue has a unit or UI test and a manual verification note; issues linked in
  the changelog.

### Phase 5 — Packaging, docs, release (≈1–2 weeks)

- `build.ps1`, workflows, SignPath configuration, `docs/update-security.md`,
  `docs/developer-guide.md` (new schema rows, project layout),
  `README.md` (requirements: Windows 11, .NET 10 Desktop Runtime), `CONTRIBUTING.md`,
  `AGENTS.md`, `TODO.md` (drop shipped lines, keep 2.1 items), `CHANGELOG.md` 2.0.0,
  `docs/release-notes.md` with an "Upgrading from 1.x" section that calls out the
  Explorer-style sort order, the .NET 10 Desktop Runtime and the Windows 11 requirement.
- Publish 2.0.0 as a **prerelease** first (the workflow already does), dogfood for a week,
  then mark it the latest release so 1.9 installs pick it up.
- Exit: a 1.9.0 install auto-updates to 2.0.0; a fresh install runs first-run flow; both
  architectures signed and verified.

## 7. Testing strategy

- **Unit (Core)**: all current tests kept; new tests for `MenuLoader` level/prefetch rules,
  cancellation, cache eviction, failure retry with a fake clock, sorting with `StrCmpLogicalW`
  samples including the `-#@=` case, `ItemOrder` merge, shell-name parsing, updater v2 folder
  swap with a fake file system, config round-trip with the new properties, direct-item kind
  inference, `ResourcesTests` extended for the new strings.
- **UI (`TrayToolbar.UITests`)**: self-hosted MSTest app; menu host open/close, lazy load
  visibility, icon placeholder swap, settings binding, About content.
- **Benchmarks**: run on demand; thresholds in §9 checked manually before release.
- **Manual matrix**: Windows 11 x64 and arm64; 100 % and 150 % DPI, dual monitor; light and
  dark; folder with 1,000 shortcuts; network folder that is unavailable at startup (#52);
  sleep/wake and `taskkill explorer` (#76); each language once.
- **CI**: `dotnet test` on `windows-2025` for Core; UI tests self-contained; CodeQL manual
  build on the new projects.

## 8. Open decisions for the maintainer

1. **Packaging** — framework-dependent folder zip (recommended) or self-contained
   single-file bundle (~100 MB, slower first start). Decide after the Phase 1 numbers.
2. **Memory budget** — accept 60–120 MB idle for the WinUI process (today ≈ 30 MB), or
   require the §9 target of ≤ 100 MB as a release blocker.

Decided on 2026-10-06 (see §1): 1.9.0 bridge yes; LaunchPolicy dropped; JSON-only
settings stay JSON-only; Explorer sort order is the default.

## 9. Acceptance targets

| Measure | Target |
| --- | --- |
| Tray click → menu visible (level loaded) | < 100 ms |
| Submenu hover → open (next level pre-loaded) | < 50 ms after the hover delay |
| Startup to tray icons usable (levels 0–1, 500 entries) | < 1.5 s cold, < 0.7 s warm |
| Placeholder → real icon | < 200 ms for cached extensions, < 1 s per uncached file |
| Idle private bytes, 1,000 entries loaded | ≤ 100 MB |
| Menu open with 1,000 entries | no dropped frames while scrolling |
| Icon failure retry | fills in within `IconRetryInterval` without restart |
| Tray icon after explorer restart or wake | re-added within 5 s |

## 10. Risks

| Risk | Mitigation |
| --- | --- |
| Single-file packaging is the least-supported WinUI configuration | Phase 1 spike; folder zip fallback; both updater shapes tested |
| `MenuFlyout` cannot meet the lazy-load and type-ahead requirements | View-model boundary lets the custom per-level window replace it; spike decides early |
| Memory and startup regress 3–6× | Targets in §9, measured in Phase 3; AOT when unpackaged AOT is fixed upstream |
| 1.8.x installs cannot auto-update to a folder | 1.9.0 bridge release |
| `AppNotificationManager` bugs (#6774, #6821) | Old activator kept behind `INotificationService` |
| Windows App SDK majors every ~6 months | Pin the version; upgrade only at phase boundaries |
| Drag-out needs identity-listed APIs | OLE `CF_HDROP` path |
| Third-party `ShellContextMenu.cs` is large and untested | Keep as-is behind an interface; cover with one UI smoke test |
| Feature creep from #8 | 2.1 milestone created up front; archive/paste explicitly out |

## 11. First steps when work starts

1. Install the VS 2026 "Windows application development" workload (WinUI C# component and
   Windows SDK 10.0.26100).
2. Create milestone 2.1 and move #72 plus the archive/paste parts of #8 (comment on #8).
3. Branch `release/1.x` plan noted; Phase 0 is on `feature/1.9.0-bridge` (started 2026-10-06).
4. Open a tracking issue "2.0.0 WinUI 3 rewrite" linking this plan and the decision gates.
