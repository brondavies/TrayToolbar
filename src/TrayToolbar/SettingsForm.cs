using System.Collections.Concurrent;
using TrayToolbar.Controls;
using TrayToolbar.Extensions;
using TrayToolbar.Models;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

public partial class SettingsForm : Form
{
    internal TrayToolbarConfiguration Configuration = new();

    public Dictionary<FolderConfig, MenuItemCollection> MenuItems = [];

    internal List<NotifyIcon> TrayIcons = [];

    internal bool RightMouseClicked;

    public SettingsForm()
    {
        InitializeComponent();
        LoadResources();
        SetupMenu();
        PopulateConfig();
        if (!ValidateFolderConfigurations())
        {
            ShowNormal();
        }
        SystemTheme.UseImmersiveDarkMode(0, UseDarkMode());
        HandleCreated += SettingsForm_HandleCreated;
        ThemeChangeMessageFilter.ThemeChanged += SettingsForm_SystemThemeChanged;
        HotKeys.HotKeyPressed += HotKey_Pressed;
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
    }

    #region LoadResources

    const string Command_Options = "Options";
    const string Command_Open = "Open";
    const string Command_Exit = "Exit";

    private void LoadResources()
    {
        FoldersLabel.Text = R.Folders;
        ExcludeFileTypesLabel.Text = R.Exclude_files;
        ThemeLabel.Text = R.Theme;
        ExcludeFoldersLabel.Text = R.Exclude_folders;
        MenuFontSizeLabel.Text = R.Menu_Font_Size;
        IconSizeLabel.Text = R.Icon_Size;
        IconSizeSmallCheckbox.Text = R.Small;
        IconSizeLargeCheckbox.Text = R.Large;
        RunOnLoginCheckbox.Text = R.Run_on_log_in;
        SaveButton.Text = R.Save;
        CancelBtn.Text = R.Cancel;
        AddFolderButton.Text = R.Add_Folder;
        Text = $"{R.TrayToolbar_Settings} ({ConfigHelper.ApplicationVersion})";
        NewVersionLabel.Text = R.A_new_version_is_available;

        List<ToolStripItem> itemsToAdd = [
           new ToolStripMenuItem { Text = R.Options, CommandParameter = Command_Options },
           new ToolStripMenuItem { Text = R.Open_Folder, CommandParameter = Command_Open },
           new ToolStripMenuItem { Text = R.Exit, CommandParameter = Command_Exit }
       ];


        RightClickMenu.Items.AddRange(itemsToAdd.ToArray());
    }

    private void ShowUpdateAvailable(string updateUri)
    {
        if (NewVersionLabel.InvokeRequired)
        {
            NewVersionLabel.Invoke(ShowUpdateAvailable, updateUri);
            return;
        }
        NewVersionLabel.Visible = true;
        NewVersionLabel.Tag = "https://github.com" + updateUri;
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
            foreach (var folder in Configuration.Folders)
            {
                StartWatchingFolder(folder);
                RefreshMenu(folder);
            }
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
            MenuItems[folder] = [];
            if (folder.Hotkey.HasValue())
            {
                HotKeys.Register(TrayIcons.Count, folder.Hotkey);
            }
            TrayIcons.Add(CreateTrayIcon(folder));

        }
    }

    private FileSystemEventHandler MenuItemChanged(FolderConfig folder)
    {
        return (_, changed) => {
            Invoke(() => {
                MenuItems[folder].DeleteMenu(changed.FullPath);
                CreateMenuItem(changed.FullPath, folder);
            });
        };
    }

    private FileSystemEventHandler MenuItemCreated(FolderConfig folder)
    {
        return (_, created) => {
            Invoke(() => {
                CreateMenuItem(created.FullPath, folder);
            });
        };
    }

    private FileSystemEventHandler MenuItemDeleted(FolderConfig folder)
    {
        return (_, deleted) => {
            Invoke(() => {
                MenuItems[folder].DeleteMenu(deleted.FullPath);
            });
        };
    }

    private RenamedEventHandler MenuItemRenamed(FolderConfig folder)
    {
        return (_, renamed) => {
            Invoke(() => {
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
                MenuItems[folder].CreateMenuItem(false, file, folder, Configuration, LeftClickMenu_ItemClicked, LeftClickMenuEntry_MouseDown);
            }
        }
        else if (File.Exists(fullPath) && Configuration.IncludesFile(fullPath))
        {
            MenuItems[folder].CreateMenuItem(false, fullPath, folder, Configuration, LeftClickMenu_ItemClicked, LeftClickMenuEntry_MouseDown);
        }
    }

    private NotifyIcon CreateTrayIcon(FolderConfig folder)
    {
        var text = Path.GetFileName(folder.Name!.ToLocalPath()).Or(folder.Name!.ToLocalPath());
        var icon = new NotifyIcon(components)
        {
            Icon = folder.Name!.ToLocalPath().GetIcon() ?? Icon,
            Text = text,
            Visible = true
        };
        icon.Click += TrayIcon_Click;
        icon.DoubleClick += TrayIcon_DoubleClick;
        icon.Tag = folder;
        return icon;
    }

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
            Configuration.Folders.Add(new FolderConfig { Recursive = true });
        }
        Configuration.Folders.ForEach(f => AddFolder(f, i++));
        FoldersUpdated();
        IgnoreFilesTextBox.Text = Configuration.IgnoreFiles.Join("; ");
        IgnoreFoldersTextBox.Text = Configuration.IgnoreFolders.Join("; ");
        FontSizeInput.Text = Configuration.FontSize.ToString();
        IconSizeLargeCheckbox.Checked = Configuration.LargeIcons;
        IconSizeSmallCheckbox.Checked = !Configuration.LargeIcons;
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
            ConfigHelper.CheckForUpdate().ContinueWith(r =>
            {
                if (r.Result?.Name != null && r.Result.Name != "v" + ConfigHelper.ApplicationVersion)
                {
                    ShowUpdateAvailable(r.Result.UpdateUrl);
                }
            });
        }
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
            if (MenuItems[folder].NeedsRefresh)
            {
                return;
            }
            var font = LeftClickMenu.Font;
            LeftClickMenu.Font = new Font(font.FontFamily, Configuration.FontSize, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
            LeftClickMenu.Items.Clear();
            List<ToolStripItem> itemsToAdd = [.. MenuItems[folder]];

            LeftClickMenu.Items.AddRange(itemsToAdd.ToArray());
            LeftClickMenu.Renderer = new MenuRenderer();
            trayIcon.ContextMenuStrip = LeftClickMenu;
            SystemTheme.SetThemeColors(LeftClickMenu, UseDarkMode());

            if (LeftClickMenu.Items.Count == 0)
            {
                ShowNormal();
                return;
            }
        }
        trayIcon.ShowContextMenu();
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
                //If path is not in the root folder, create a submenu to add it into
                menu.CreateMenuItem(true, file, folder, Configuration, LeftClickMenu_ItemClicked, LeftClickMenuEntry_MouseDown);
            }
            SetupLeftClickMenu(menu);
        }
    }

    private void LeftClickMenuEntry_MouseDown(object? sender, MouseEventArgs e)
    {
        RightMouseClicked = e.Button == MouseButtons.Right;
    }

    private void SetupLeftClickMenu(MenuItemCollection menu)
    {
        if (LeftClickMenu.InvokeRequired)
        {
            LeftClickMenu.Invoke(SetupLeftClickMenu, menu);
            return;
        }
        LeftClickMenu.Renderer = new MenuRenderer();
        LeftClickMenu.Items.Clear();
#pragma warning disable IDE0305 // Simplify collection initialization
        LeftClickMenu.Items.AddRange(menu.ToArray());
#pragma warning restore IDE0305
        menu.NeedsRefresh = false;
    }

    private IEnumerable<string> EnumerateFiles(string path, bool recursive)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            ReturnSpecialDirectories = false,
        };
        return Directory.EnumerateFiles(path, "*.*", options)
            .Where(Configuration.IncludesFile)
            .OrderBy(f => f.ToUpper());
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
            case Command_Exit:
                Quit();
                Application.Exit();
                break;
        }
    }

    private void LeftClickMenu_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem?.CommandParameter != null)
        {
            Visible = false;
            ShowInTaskbar = false;
            var filename = $"{e.ClickedItem.CommandParameter}";
            if (RightMouseClicked)
            {
                ShowContextMenu(filename);
            }
            else
            {
                try
                {
                    Program.Launch(filename);
                }
                catch { }
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
        Configuration.IgnoreFiles = IgnoreFilesTextBox.Text.SplitPaths();
        Configuration.IgnoreFolders = IgnoreFoldersTextBox.Text.SplitPaths();
        Configuration.Theme = (int)ThemeToggleButton.Theme;
        Configuration.LargeIcons = IconSizeLargeCheckbox.Checked;
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
            MessageBox.Show(R.The_folder_value_must_be_set, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        foreach (var c in FolderControls().Where(c => !c.Config.Name!.IsDirectory()))
        {
            c.Error = error = true;
        }
        if (error)
        {
            MessageBox.Show(R.The_folder_does_not_exist, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void CancelBtn_Click(object sender, EventArgs e)
    {
        Close();
        SetupMenu();
        PopulateConfig();
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
    }

    private void SettingsForm_SystemColorsChanged(object sender, EventArgs e)
    {
        SettingsForm_SystemThemeChanged(sender, e);
    }
}
