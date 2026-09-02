using System.Collections.Concurrent;
using System.Globalization;

using TrayToolbar.Controls;
using TrayToolbar.Extensions;
using TrayToolbar.Models;
using TrayToolbar.Services;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

public partial class SettingsForm : Form
{
    internal TrayToolbarConfiguration Configuration = new();

    internal Dictionary<FolderConfig, MenuItemCollection> MenuItems = [];

    internal List<NotifyIcon> TrayIcons = [];

    internal bool RightMouseClicked;

    internal readonly CultureInfo DefaultCulture = CultureInfo.CurrentCulture;

    internal bool NewVersionMessage = false;

    private bool _firstTimeLoad = false;

    private System.Threading.Timer? _updateCheckTimer;

    internal readonly CultureInfo[] SupportedLanguages = [
        CultureInfo.GetCultureInfo("en"),
        CultureInfo.GetCultureInfo("es"),
        CultureInfo.GetCultureInfo("fr"),
        CultureInfo.GetCultureInfo("de"),
        CultureInfo.GetCultureInfo("pt"),
        CultureInfo.GetCultureInfo("it"),
        CultureInfo.GetCultureInfo("ja"),
        CultureInfo.GetCultureInfo("zh"),
        CultureInfo.GetCultureInfo("ru"),
        CultureInfo.GetCultureInfo("ko"),
    ];

    public SettingsForm()
    {
        InitializeComponent();
        LeftClickMenu.Closed += LeftClickMenu_Closed;
        SetupMenu();
        PopulateConfig();
        LoadResources(Configuration.Language);
        HandleCreated += SettingsForm_HandleCreated;
        if (ValidateFolderConfigurations() && !_firstTimeLoad)
        {
            CreateIcons();
        }
        else
        {
            _firstTimeLoad = false;
            ShowNormal();
        }
        SystemTheme.UseImmersiveDarkMode(0, UseDarkMode());
        ThemeChangeMessageFilter.ThemeChanged += SettingsForm_SystemThemeChanged;
        HotKeys.HotKeyPressed += HotKey_Pressed;
    }

    private void SetupUpdateCheckTimer()
    {
        var interval = TimeSpan.FromMinutes(Configuration.UpdateCheckInterval);
        _updateCheckTimer = new System.Threading.Timer(
            callback: _ => CheckForUpdateAsync(),
            state: null,
            dueTime: interval,
            period: interval
        );
    }

    private void CheckForUpdateAsync()
    {
        ConfigHelper.CheckForUpdate().ContinueWith(r =>
        {
            if (UpdateLogic.TryGetAvailableUpdate(r.Result, ConfigHelper.ApplicationVersion, out var version, out var updateUrl))
            {
                var prerelease = IsPrereleaseVersion(version);
                ShowUpdateAvailable(updateUrl, prerelease);
            }
        });
    }

    private void HotKey_Pressed(int value, EventArgs e)
    {
        if (TrayIcons.Count > value && !Visible)
        {
            var t = TrayIcons[value];
            TrayIcon_Click(t, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }
    }

    private void SettingsForm_HandleCreated(object? sender, EventArgs e)
    {
        var darkmode = UseDarkMode();
        SystemTheme.UseImmersiveDarkMode(Handle, darkmode);
        CreateIcons();
    }

    private void CreateIcons()
    {
        if (!IsHandleCreated) { return; }
        if (InvokeRequired)
        {
            Invoke(CreateIcons);
            return;
        }
        lock (this)
        {
            foreach (var folder in Configuration.Folders)
            {
                StartWatchingFolder(folder);
                RefreshMenu(folder);
            }
        }
    }

    #region LoadResources

    const string Command_Options = "Options";
    const string Command_Open = "Open";
    const string Command_Exit = "Exit";
    const string Command_Locate = "Locate";

    private void LoadResources(string? language)
    {
        if (language.HasValue())
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
        }
        else
        {
            Thread.CurrentThread.CurrentCulture = DefaultCulture;
            Thread.CurrentThread.CurrentUICulture = DefaultCulture;
        }
        FoldersLabel.Text = R.Folders;
        IncludeFileTypesLabel.Text = R.Include_files;
        ExcludeFileTypesLabel.Text = R.Exclude_files;
        ThemeLabel.Text = R.Theme;
        ExcludeFoldersLabel.Text = R.Exclude_folders;
        MenuFontSizeLabel.Text = R.Menu_Font_Size;
        IconSizeLabel.Text = R.Icon_Size;
        IconSizeSmallCheckbox.Text = R.Small;
        IconSizeLargeCheckbox.Text = R.Large;
        NotifyOnUpdateAvailableCheckbox.Text = R.Notify_me_when_a_new_version_is_available;
        RunOnLoginCheckbox.Text = R.Run_on_log_in;
        SaveButton.Text = R.Save;
        CancelBtn.Text = R.Cancel;
        AddFolderButton.Text = R.Add_Folder;
        Text = $"{R.TrayToolbar_Settings} ({ConfigHelper.ApplicationVersion})";
        NewVersionLabel.Text = isPrerelease ? R.You_are_using_a_prerelease_version : R.A_new_version_is_available;
        UpdateNowLabel.Text = R.Update_now;
        LanguageLabel.Text = R.Language;
        ShowFolderLinksAsSubMenusCheckbox.Text = R.Show_links_to_folders_as_submenus;


        RightClickMenu.Items.Clear();
        RightClickMenu.Items.AddRange([
            new ToolStripMenuItem { Text = R.Options, CommandParameter = Command_Options },
            new ToolStripMenuItem { Text = R.Open_Folder, CommandParameter = Command_Open },
            new ToolStripMenuItem { Text = R.TrayToolbar_Location, CommandParameter = Command_Locate },
            new ToolStripMenuItem { Text = R.Exit, CommandParameter = Command_Exit }
        ]);

        foreach (var control in FolderControls())
        {
            control.UpdateConfig();
        }
        ThemeToggleButton.UpdateConfig();
    }

    private bool isPrerelease;
    private void ShowUpdateAvailable(string updateUri, bool prerelease)
    {
        if (NewVersionLabel.InvokeRequired)
        {
            NewVersionLabel.Invoke(ShowUpdateAvailable, updateUri, prerelease);
            return;
        }
        isPrerelease = prerelease;
        NewVersionLabel.Text = prerelease
            ? R.You_are_using_a_prerelease_version
            : R.A_new_version_is_available;
        NewVersionLabel.Tag = updateUri;
        NewVersionLabel.Visible = true;
        UpdateNowLabel.Visible = !prerelease;
        if (!prerelease && Configuration.NotifyOnUpdateAvailable && ConfigHelper.SupportsToastNotifications)
        {
            NotificationsHelper.Notify(R.A_new_version_is_available, updateUri, R.Update_now, NotificationsHelper.UPDATE_ACTION);
            _updateCheckTimer?.Dispose();
            _updateCheckTimer = null;
        }
    }

    #endregion

    private bool initVisible = false;
    protected override void SetVisibleCore(bool value)
    {
        if (!initVisible && File.Exists(ConfigHelper.ConfigurationFile))
        {
            initVisible = true;
            return;
        }
        base.SetVisibleCore(value);
    }

    private void SetupMenu()
    {
        Configuration = ConfigHelper.ReadConfiguration();
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        lock (this)
        {
            foreach (var watcher in Watchers)
                watcher.Value.EnableRaisingEvents = false;
            Watchers.Clear();
            TrayIcons.ForEach(i => i.Visible = false);
            TrayIcons.Clear();
            HotKeys.UnregisterAll();
        }
    }

    private readonly Dictionary<string, FileSystemWatcher> Watchers = [];
    private void StartWatchingFolder(FolderConfig folder)
    {
        if (folder.Name.HasValue() && folder.Name.IsDirectory())
        {
            var watcher = new FileSystemWatcher(folder.Name.ToLocalPath())
            {
                Filter = "*.*",
                IncludeSubdirectories = folder.Recursive,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.CreationTime
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size
                             | NotifyFilters.DirectoryName,
            };
            watcher.Changed += MenuItemChanged(folder);
            watcher.Created += MenuItemCreated(folder);
            watcher.Deleted += MenuItemDeleted(folder);
            watcher.Renamed += MenuItemRenamed(folder);
            Watchers[folder.Name] = watcher;
            MenuItems[folder] = new MenuItemCollection(Configuration, LeftClickMenu_ItemClicked, LeftClickMenuEntry_MouseDown);
            var icon = TrayIcons.FirstOrDefault(i => i.Tag != null && ((FolderConfig)i.Tag).Name == folder.Name);
            var index = TrayIcons.Count;
            if (icon != null)
            {
                index = TrayIcons.IndexOf(icon);
                icon.Visible = false;
                TrayIcons[index] = CreateTrayIcon(folder);
            }
            else
            {
                TrayIcons.Add(CreateTrayIcon(folder));
            }
            if (folder.Hotkey.HasValue())
            {
                HotKeys.Register(index, folder.Hotkey);
            }
        }
    }

    private FileSystemEventHandler MenuItemChanged(FolderConfig folder)
    {
        return (_, changed) =>
        {
            Invoke(() =>
            {
                MenuItems[folder].DeleteMenu(changed.FullPath);
                CreateMenuItem(changed.FullPath, folder);
            });
        };
    }

    private FileSystemEventHandler MenuItemCreated(FolderConfig folder)
    {
        return (_, created) =>
        {
            Invoke(() =>
            {
                CreateMenuItem(created.FullPath, folder);
            });
        };
    }

    private FileSystemEventHandler MenuItemDeleted(FolderConfig folder)
    {
        return (_, deleted) =>
        {
            Invoke(() =>
            {
                MenuItems[folder].DeleteMenu(deleted.FullPath);
            });
        };
    }

    private RenamedEventHandler MenuItemRenamed(FolderConfig folder)
    {
        return (_, renamed) =>
        {
            Invoke(() =>
            {
                MenuItems[folder].DeleteMenu(renamed.OldFullPath);
                CreateMenuItem(renamed.FullPath, folder);
            });
        };
    }

    /// <summary>
    /// This is only used for real-time changes to the file system and is more efficient for those events
    /// ReloadMenuItems should be used for reloading entire menus
    /// </summary>
    private void CreateMenuItem(string fullPath, FolderConfig folder)
    {
        if (fullPath.IsDirectory())
        {
            foreach (var file in EnumerateFiles(fullPath, folder.Recursive))
            {
                MenuItems[folder].CreateMenuItem(file, folder);
            }
        }
        else if (File.Exists(fullPath) && Configuration.IncludesFile(fullPath))
        {
            MenuItems[folder].CreateMenuItem(fullPath, folder);
        }
    }

    private NotifyIcon CreateTrayIcon(FolderConfig folder)
    {
        var text = Path.GetFileName(folder.Name!.ToLocalPath());
        var customIcon = folder.GetIcon(true);
        var icon = new NotifyIcon(components)
        {
            Icon = (customIcon == null || customIcon.Size.IsEmpty) ? Icon : Icon.FromHandle(customIcon.GetHicon()),
            Tag = folder,
            Text = text,
            Visible = true,
        };
        icon.Click += TrayIcon_Click;
        icon.DoubleClick += TrayIcon_DoubleClick;
        return icon;
    }

    #region Tray icon loading spinner

    private System.Windows.Forms.Timer? _spinnerTimer;
    private int _spinnerFrame;
    private readonly Dictionary<NotifyIcon, Icon> _spinnerBaseIcons = [];
    private readonly Dictionary<NotifyIcon, Icon> _spinnerFrameIcons = [];

    /// <summary>
    /// Animates a spinner overlay on every tray icon whose menu is loading so it is
    /// clear the icon is not ready to be clicked yet
    /// </summary>
    private void StartTrayIconSpinner()
    {
        if (InvokeRequired)
        {
            BeginInvoke(StartTrayIconSpinner);
            return;
        }
        if (_spinnerTimer == null)
        {
            _spinnerTimer = new System.Windows.Forms.Timer(components) { Interval = 100 };
            _spinnerTimer.Tick += SpinnerTimer_Tick;
        }
        _spinnerTimer.Start();
    }

    private void SpinnerTimer_Tick(object? sender, EventArgs e)
    {
        _spinnerFrame = (_spinnerFrame + 1) % SpinnerIconRenderer.FrameCount;
        var anyLoading = false;
        foreach (var trayIcon in TrayIcons)
        {
            var loading = trayIcon.Tag is FolderConfig folder
                && MenuItems.TryGetValue(folder, out var menu)
                && menu.NeedsRefresh;
            if (loading && trayIcon.Icon != null)
            {
                anyLoading = true;
                if (!_spinnerBaseIcons.ContainsKey(trayIcon))
                {
                    _spinnerBaseIcons[trayIcon] = trayIcon.Icon;
                }
                var frame = SpinnerIconRenderer.RenderFrame(_spinnerBaseIcons[trayIcon], _spinnerFrame);
                trayIcon.Icon = frame;
                if (_spinnerFrameIcons.TryGetValue(trayIcon, out var previousFrame))
                {
                    previousFrame.Dispose();
                }
                _spinnerFrameIcons[trayIcon] = frame;
            }
            else
            {
                RestoreTrayIcon(trayIcon);
            }
        }
        // clean up icons that were replaced while spinning (e.g. settings were saved)
        foreach (var stale in _spinnerBaseIcons.Keys.Where(i => !TrayIcons.Contains(i)).ToArray())
        {
            RestoreTrayIcon(stale);
        }
        if (!anyLoading)
        {
            _spinnerTimer!.Stop();
        }
    }

    private void RestoreTrayIcon(NotifyIcon trayIcon)
    {
        if (_spinnerBaseIcons.TryGetValue(trayIcon, out var baseIcon))
        {
            trayIcon.Icon = baseIcon;
            _spinnerBaseIcons.Remove(trayIcon);
        }
        if (_spinnerFrameIcons.TryGetValue(trayIcon, out var frameIcon))
        {
            frameIcon.Dispose();
            _spinnerFrameIcons.Remove(trayIcon);
        }
    }

    #endregion

    readonly ConcurrentDictionary<FolderConfig, CancellationTokenSource> refreshCancellation = [];
    private void RefreshMenu(FolderConfig folder)
    {
        if (refreshCancellation.TryGetValue(folder, out var c))
        {
            c.Cancel();
        }
        if (MenuItems.TryGetValue(folder, out var menu))
        {
            menu.NeedsRefresh = true;
            StartTrayIconSpinner();
            var cancellation = new CancellationTokenSource();
            refreshCancellation[folder] = cancellation;
            Task.Run(() =>
            {
                Task.Delay(500, cancellation.Token);
                if (cancellation.IsCancellationRequested) { return; }
                ReloadMenuItems(folder, cancellation.Token);
                refreshCancellation.TryRemove(folder, out _);
            }, cancellation.Token);
        }
    }

    private void PopulateConfig()
    {
        var list = FolderControls().ToArray();
        foreach (var c in list) FoldersLayout.Controls.Remove(c);
        var i = 0;
        if (Configuration.Folders.Count == 0)
        {
            _firstTimeLoad = true;
            Configuration.Folders.Add(new FolderConfig { Recursive = true, Name = @"%APPDATA%\Microsoft\Windows\Start Menu" });
        }
        Configuration.Folders.ForEach(f => AddFolder(f, i++));
        FoldersUpdated();
        IncludeFilesTextBox.Text = Configuration.IncludeFiles.Join("; ");
        IgnoreFilesTextBox.Text = Configuration.IgnoreFiles.Join("; ");
        IgnoreFoldersTextBox.Text = Configuration.IgnoreFolders.Join("; ");
        ShowFolderLinksAsSubMenusCheckbox.Checked = Configuration.ShowFolderLinksAsSubMenus;
        FontSizeInput.Text = Configuration.FontSize.ToString();
        IconSizeLargeCheckbox.Checked = Configuration.LargeIcons;
        IconSizeSmallCheckbox.Checked = !Configuration.LargeIcons;
        LanguageSelectList.SelectedIndex = SupportedLanguages.IndexOf(l => l.TwoLetterISOLanguageName == Configuration.Language) + 1;
        NotifyOnUpdateAvailableCheckbox.Checked = Configuration.NotifyOnUpdateAvailable;
        RunOnLoginCheckbox.Checked = ConfigHelper.IsAutoStartupConfigured();
        if (SystemTheme.IsDarkModeSupported())
        {
            ThemeToggleButton.Theme = (ThemeToggleEnum)Configuration.Theme;
        }
        else
        {
            ThemeLabel.Visible = false;
            ThemeToggleButton.Visible = false;
            var row = tableLayout.GetRow(ThemeToggleButton);
            tableLayout.RowStyles[row].Height = 0;
        }
        if (Configuration.CheckForUpdates)
        {
            if (Configuration.NotifyOnUpdateAvailable)
            {
                SetupUpdateCheckTimer();
            }
            CheckForUpdateAsync();
        }
    }

    static bool IsPrereleaseVersion(string version)
    {
        return UpdateLogic.IsPrereleaseVersion(ConfigHelper.ApplicationVersion, version);
    }

    private IEnumerable<FolderControl> FolderControls()
    {
        foreach (var c in FoldersLayout.Controls)
            if (c is FolderControl control)
                yield return control;
    }

    private void TrayIcon_Click(object? sender, EventArgs e)
    {
        var trayIcon = (NotifyIcon)sender!;
        var folder = (FolderConfig)trayIcon.Tag!;
        SettingsForm_SystemThemeChanged(null, EventArgs.Empty);
        if (((MouseEventArgs)e).Button == MouseButtons.Right)
        {
            var font = RightClickMenu.Font;
            RightClickMenu.Font = new Font(font.FontFamily, Configuration.FontSize, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
            RightClickMenu.Tag = folder;
            RightClickMenu.Renderer = new MenuRenderer();
            trayIcon.ContextMenuStrip = RightClickMenu;
            SystemTheme.SetThemeColors(RightClickMenu, UseDarkMode());
        }
        else
        {
            if (MenuItems.TryGetValue(folder, out var menu) && menu.NeedsRefresh)
            {
                // The menu is still loading; pop it up automatically once it is
                // ready instead of ignoring the click
                _pendingTrayIconClick = trayIcon;
                _pendingTrayIconClickTime = DateTime.UtcNow;
                return;
            }
            _pendingTrayIconClick = null;
            if (!ShowLeftClickMenu(trayIcon, folder)) return;
        }
        trayIcon.ShowContextMenu();
    }

    private NotifyIcon? _pendingTrayIconClick;
    private DateTime _pendingTrayIconClickTime;
    private static readonly TimeSpan PendingTrayIconClickTimeout = TimeSpan.FromSeconds(10);

    private void ShowPendingTrayIconClick(MenuItemCollection menu)
    {
        var trayIcon = _pendingTrayIconClick;
        if (trayIcon?.Tag is not FolderConfig folder) return;
        _pendingTrayIconClick = null;
        if (DateTime.UtcNow - _pendingTrayIconClickTime > PendingTrayIconClickTimeout) return;
        if (MenuItems.TryGetValue(folder, out var pendingMenu)
            && pendingMenu == menu
            && ShowLeftClickMenu(trayIcon, folder))
        {
            trayIcon.ShowContextMenu();
        }
    }

    private bool ShowLeftClickMenu(NotifyIcon trayIcon, FolderConfig folder)
    {
        if (MenuItems[folder].NeedsRefresh)
        {
            return false;
        }
        if (InvokeRequired)
        {
            return Invoke(() =>
            {
                return ShowLeftClickMenu(trayIcon, folder);
            });
        }
        var font = LeftClickMenu.Font;
        font = new Font(font.FontFamily, Configuration.FontSize, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
        LeftClickMenu.Font = font;
        LeftClickMenu.Items.Clear();
        List<ToolStripItem> itemsToAdd = [.. MenuItems[folder]];

        LeftClickMenu.Items.AddRange(itemsToAdd.ToArray());
        LeftClickMenu.Renderer = new MenuRenderer();
        trayIcon.ContextMenuStrip = LeftClickMenu;
        SystemTheme.SetThemeColors(LeftClickMenu, UseDarkMode());

        if (LeftClickMenu.Items.Count == 0)
        {
            ShowNormal();
            return false;
        }
        return true;
    }

    private void ReloadMenuItems(FolderConfig folder, CancellationToken token)
    {
        lock (MenuItems[folder])
        {
            var menu = MenuItems[folder];
            menu.Clear();
            if (!folder.Name.HasValue() || !folder.Name.IsDirectory()) return;

            foreach (var file in EnumerateFiles(folder.Name.ToLocalPath(), folder.Recursive))
            {
                if (token.IsCancellationRequested == true) { return; }
                Invoke(() => menu.CreateMenuItem(file, folder));
            }
            SetupLeftClickMenu(menu);
        }
    }

    private readonly List<ToolStripDropDown> _suspendedDropDowns = [];

    private void LeftClickMenuEntry_MouseDown(object? sender, MouseEventArgs e)
    {
        RightMouseClicked = e.Button == MouseButtons.Right;
        if (RightMouseClicked && sender is ToolStripMenuItem menu)
        {
            RestoreSuspendedDropDowns(close: false);
            _suspendedDropDowns.AddRange(menu.DropDown.SetAutoClose(false));
        }
        LeftClickMenu.AutoClose = !RightMouseClicked;
    }

    /// <summary>
    /// Re-enables AutoClose on every dropdown that was suspended for a right-click, no matter
    /// which menu item ends up receiving the click; a left-opening submenu can overlap its
    /// parent so mouse-down and click may land on different items, which used to leave the
    /// deepest submenu stuck open
    /// </summary>
    private void RestoreSuspendedDropDowns(bool close)
    {
        if (_suspendedDropDowns.Count == 0) return;
        var suspended = _suspendedDropDowns.ToArray();
        _suspendedDropDowns.Clear();
        foreach (var dropDown in suspended)
        {
            dropDown.AutoClose = true;
            if (close && dropDown.Visible)
            {
                dropDown.Close();
            }
        }
    }

    private void LeftClickMenu_Closed(object? sender, ToolStripDropDownClosedEventArgs e)
    {
        RestoreSuspendedDropDowns(close: true);
    }

    private void SetupLeftClickMenu(MenuItemCollection menu)
    {
        if (InvokeRequired)
        {
            Invoke(SetupLeftClickMenu, menu);
            return;
        }
        LeftClickMenu.Renderer = new MenuRenderer();
        LeftClickMenu.Items.Clear();
#pragma warning disable IDE0305 // Simplify collection initialization
        LeftClickMenu.Items.AddRange(menu.ToArray());
#pragma warning restore IDE0305
        menu.NeedsRefresh = false;
        ShowPendingTrayIconClick(menu);
    }

    private IEnumerable<string> EnumerateFiles(string path, bool recursive)
    {
        return MenuItemCollection.EnumerateFiles(path, recursive, Configuration);
    }

    private void TrayIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowNormal();
    }

    private void ShowNormal()
    {
        if (InvokeRequired)
        {
            Invoke(ShowNormal, []);
            return;
        }
        SettingsForm_SystemThemeChanged(null, EventArgs.Empty);
        Visible = true;
        ShowInTaskbar = true;
        WindowState = FormWindowState.Normal;
    }

    private bool quitting = false;
    private void Quit()
    {
        quitting = true;
        foreach (var c in refreshCancellation.Values) { c.Cancel(); }
        Close();
    }

    private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!quitting)
        {
            Visible = false;
            ShowInTaskbar = false;
            e.Cancel = true;
        }
        else
        {
            _updateCheckTimer?.Dispose();
        }
    }

    private void SettingsForm_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            Visible = false;
            ShowInTaskbar = false;
        }
    }

    private void SettingsForm_Deactivate(object sender, EventArgs e)
    {
        LeftClickMenu.Hide();
        RightClickMenu.Hide();
        RestoreSuspendedDropDowns(close: true);
    }

    private void RightClickMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem?.CommandParameter == null) return;

        switch (e.ClickedItem.CommandParameter.ToString())
        {
            case Command_Options:
                ShowNormal();
                break;
            case Command_Open:
                var folder = (FolderConfig?)RightClickMenu.Tag;
                if (folder?.Name != null)
                    Program.Launch(folder.Name.ToLocalPath());
                break;
            case Command_Locate:
                Program.Launch(ConfigHelper.ApplicationRoot);
                break;
            case Command_Exit:
                Quit();
                break;
        }
    }

    private void LeftClickMenu_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem != null)
        {
            var filename = $"{e.ClickedItem.CommandParameter}";
            if (RightMouseClicked)
            {
                ShowContextMenu(filename);
                LeftClickMenu.AutoClose = true;
                RestoreSuspendedDropDowns(close: false);
                RightMouseClicked = false;
            }
            else if (e.ClickedItem.AccessibleRole != AccessibleRole.MenuPopup)
            {
                try
                {
                    Program.Launch(filename);
                }
                catch { }
                LaunchLogger.Log(Configuration, e.ClickedItem.Name.Or(Path.GetFileName(filename)) ?? "", filename);
                LeftClickMenu.Close(ToolStripDropDownCloseReason.ItemClicked);
                if (Visible)
                {
                    Visible = false;
                    ShowInTaskbar = false;
                }
            }
        }
    }

    private static void ShowContextMenu(string filename)
    {
        try
        {
            var menu = new ShellContextMenu();
            menu.ShowContextMenu([new FileInfo(filename)], Cursor.Position);
        }
        catch { }
    }

    private void FolderControl_BrowseClicked(object? sender, EventArgs e)
    {
        var control = (FolderControl?)sender;
        if (control != null)
        {
            var result = FolderDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                control.Config.Name = FolderDialog.SelectedPath;
                control.UpdateConfig();
            }
        }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        if (!ValidateFolderConfigurations())
        {
            return;
        }

        Configuration.Folders = FolderControls().Select(c => c.Config).ToList();
        Configuration.IncludeFiles = IncludeFilesTextBox.Text.SplitPaths();
        Configuration.IgnoreFiles = IgnoreFilesTextBox.Text.SplitPaths();
        Configuration.IgnoreFolders = IgnoreFoldersTextBox.Text.SplitPaths();
        Configuration.ShowFolderLinksAsSubMenus = ShowFolderLinksAsSubMenusCheckbox.Checked;
        Configuration.Theme = (int)ThemeToggleButton.Theme;
        Configuration.LargeIcons = IconSizeLargeCheckbox.Checked;
        Configuration.NotifyOnUpdateAvailable = NotifyOnUpdateAvailableCheckbox.Checked;
        if (FontSizeInput.Validate())
        {
            Configuration.FontSize = (float)FontSizeInput.Value;
        }
        LoadConfiguration();
        if (ConfigHelper.WriteConfiguration(Configuration))
        {
            Close();
        }
        ConfigHelper.SetStartupKey(RunOnLoginCheckbox.Checked);
    }

    private bool ValidateFolderConfigurations()
    {
        var error = false;
        foreach (var c in FolderControls()) c.Error = false;

        foreach (var c in FolderControls().Where(c => !c.Config.Name.HasValue()))
        {
            c.Error = error = true;
        }
        if (error)
        {
            MessageBox.Show(this,
                R.The_folder_value_must_be_set, R.Error,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        foreach (var c in FolderControls().Where(c => !c.Config.Name!.IsDirectory()))
        {
            c.Error = error = true;
        }
        if (error)
        {
            MessageBox.Show(this,
                R.The_folder_does_not_exist, R.Error,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void CancelBtn_Click(object sender, EventArgs e)
    {
        SetupMenu();
        PopulateConfig();
        if (ValidateFolderConfigurations())
        {
            Close();
            CreateIcons();
        }
    }

    private void NewVersionLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Program.Launch($"{NewVersionLabel.Tag}");
    }

    private void AddFolderButton_Click(object sender, EventArgs e)
    {
        var folderConfig = new FolderConfig { Recursive = true };
        Configuration.Folders.Add(folderConfig);
        AddFolder(folderConfig);
        FoldersUpdated();
    }

    private void FolderControl_DeleteClicked(object? sender, EventArgs e)
    {
        var control = (FolderControl)sender!;
        control.BrowseFolder -= FolderControl_BrowseClicked;
        control.DeleteFolder -= FolderControl_DeleteClicked;
        Configuration.Folders.Remove(control.Config);
        FoldersLayout.Controls.Remove(control);
        FoldersUpdated();
    }

    private void AddFolder(FolderConfig folderConfig, int i = -1)
    {
        if (i == -1)
        {
            i = FolderControls().Count();
        }
        var folderControl = new FolderControl
        {
            Config = folderConfig,
            Width = 400,
            Margin = new Padding { All = 0 },
        };
        folderControl.BrowseFolder += FolderControl_BrowseClicked;
        folderControl.DeleteFolder += FolderControl_DeleteClicked;
        FoldersLayout.Controls.Add(folderControl);
        FoldersLayout.AutoScroll = FoldersLayout.Controls.Count >= 4;
        if (FoldersLayout.AutoScroll)
        {
            FoldersLayout.ScrollControlIntoView(folderControl);
        }
        SystemTheme.SetThemeColors(folderControl, UseDarkMode());
    }

    private void FoldersUpdated()
    {
        var list = FolderControls();
        var count = list.Count();
        foreach (var c in list)
        {
            c.ShowRemoveButton = count > 1;
        }
    }

    private void ThemeToggleButton_ThemeChanged(object sender, EventArgs e)
    {
        var darkmode = UseDarkMode();
        SystemTheme.UseImmersiveDarkMode(Handle, darkmode);
    }

    private bool UseDarkMode()
    {
        return (ThemeToggleButton.Theme == ThemeToggleEnum.SYSTEM_THEME && SystemTheme.IsDarkModeEnabled())
            || ThemeToggleButton.Theme == ThemeToggleEnum.DARK_THEME;
    }

    private void SettingsForm_SystemThemeChanged(object? sender, EventArgs e)
    {
        if (ThemeToggleButton.Theme == ThemeToggleEnum.SYSTEM_THEME)
        {
            var darkmode = SystemTheme.IsDarkModeEnabled();
            SystemTheme.UseImmersiveDarkMode(Handle, darkmode);
        }
    }

    private void IconSizeSmallCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        IconSizeLargeCheckbox.Checked = !IconSizeSmallCheckbox.Checked;
    }

    private void IconSizeLargeCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        IconSizeSmallCheckbox.Checked = !IconSizeLargeCheckbox.Checked;
    }

    private void SettingsForm_Shown(object sender, EventArgs e)
    {
        SettingsForm_SystemThemeChanged(sender, e);
        if (NewVersionMessage)
        {
            NewVersionMessage = false;
            MessageBox.Show(this,
                string.Format(R.Updated_to_version, ConfigHelper.ApplicationVersion),
                R.Update_TrayToolbar, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SettingsForm_SystemColorsChanged(object sender, EventArgs e)
    {
        SettingsForm_SystemThemeChanged(sender, e);
    }

    private void LanguageSelectList_SelectedIndexChanged(object sender, EventArgs e)
    {
        var idx = LanguageSelectList.SelectedIndex;
        var code = "";
        if (idx > 0 && idx <= SupportedLanguages.Length)
        {
            code = SupportedLanguages[idx - 1].TwoLetterISOLanguageName;
        }
        Configuration.Language = code;
        LoadResources(code);
    }

    private void UpdateNowLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var result = MessageBox.Show(this,
            R.Are_you_sure_you_want_to_update_to_the_latest_version,
            R.Update_TrayToolbar,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            ConfigHelper.UpdateToLatestVersion();
        }
    }
}