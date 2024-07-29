using System.Collections.Concurrent;
using TrayToolbar.Controls;
using TrayToolbar.Extensions;
using TrayToolbar.Models;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar
{
    public partial class SettingsForm : Form
    {
        internal TrayToolbarConfiguration Configuration = new();

        public Dictionary<FolderConfig, MenuItemCollection> MenuItems = [];

        internal List<NotifyIcon> TrayIcons = [];

        public SettingsForm()
        {
            InitializeComponent();
            LoadResources();
            SetupMenu();
            PopulateConfig();
            SystemTheme.UseImmersiveDarkMode(0, UseDarkMode());
            this.HandleCreated += SettingsForm_HandleCreated;
            ThemeChangeMessageFilter.ThemeChanged += SettingsForm_SystemThemeChanged;
        }

        private void SettingsForm_HandleCreated(object? sender, EventArgs e)
        {
            var darkmode = UseDarkMode();
            SystemTheme.UseImmersiveDarkMode(this.Handle, darkmode);
        }

        #region LoadResources

        const string Command_Options = "Options";
        const string Command_Open = "Open";
        const string Command_Exit = "Exit";

        private void LoadResources()
        {
            label1.Text = R.Folders;
            label2.Text = R.Exclude_files;
            label3.Text = R.Theme;
            label4.Text = R.Exclude_folders;
            RunOnLoginCheckbox.Text = R.Run_on_log_in;
            SaveButton.Text = R.Save;
            CancelBtn.Text = R.Cancel;
            AddFolderButton.Text = R.Add_Folder;
            Text = R.TrayToolbar_Settings + " (" + ConfigHelper.ApplicationVersion + ")";
            NewVersionLabel.Text = R.A_new_version_is_available;
            ConfigHelper.CheckForUpdate().ContinueWith(r =>
            {
                if (r.Result?.Name != null)
                {
                    if (r.Result.Name != "v" + ConfigHelper.ApplicationVersion)
                    {
                        ShowUpdateAvailable(r.Result.UpdateUrl);
                    }
                }
            });

            RightClickMenu.Items.AddRange(new[] {
                new ToolStripMenuItem { Text = R.Options, CommandParameter = Command_Options },
                new ToolStripMenuItem { Text = R.Open_Folder, CommandParameter = Command_Open },
                new ToolStripMenuItem { Text = R.Exit, CommandParameter = Command_Exit }
            });
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
            if (folder.Name.HasValue() && Directory.Exists(folder.Name.ToLocalPath()))
            {
                var watcher = new FileSystemWatcher(folder.Name)
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
                //TODO: Handle each event atomically
                watcher.Created += (_, _) => RefreshMenu(folder);
                watcher.Deleted += (_, _) => RefreshMenu(folder);
                watcher.Renamed += (_, _) => RefreshMenu(folder);
                Watchers[folder.Name] = watcher;
                MenuItems[folder] = [];
                TrayIcons.Add(CreateTrayIcon(folder));
            }
        }

        private NotifyIcon CreateTrayIcon(FolderConfig folder)
        {
            var text = Path.GetFileName(folder.Name).Or(folder.Name!);
            var icon = new NotifyIcon(components)
            {
                Icon = folder.Name!.GetIcon() ?? this.Icon,
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
            RunOnLoginCheckbox.Checked = ConfigHelper.GetStartupKey();
            if (SystemTheme.IsDarkModeSupported())
            {
                ThemeToggleButton.Theme = (ThemeToggleEnum)Configuration.Theme;
            }
            else
            {
                label3.Visible = false;
                ThemeToggleButton.Visible = false;
                var row = tableLayout.GetRow(ThemeToggleButton);
                tableLayout.RowStyles[row].Height = 0;
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
                LeftClickMenu.Items.Clear();
                LeftClickMenu.Items.AddRange(MenuItems[folder].ToArray());
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
                if (!folder.Name.HasValue() || !Directory.Exists(folder.Name.ToLocalPath())) return;

                foreach (var file in EnumerateFiles(folder.Name.ToLocalPath(), folder.Recursive))
                {
                    if (token.IsCancellationRequested == true) { return; }
                    //If path is not in the root folder, create a submenu to add it into
                    ToolStripMenuItem? submenu = null;
                    var parentPath = Path.GetDirectoryName(file);
                    if (parentPath.HasValue() && !parentPath.Is(folder.Name))
                    {
                        if (Configuration.IgnoreAllDotFiles && parentPath.Contains(@"\."))
                            continue; //it's in a dot folder like .git or it's a dot file
                        if (Configuration.IgnoreFolders.Contains(Path.GetFileName(parentPath)))
                            continue; //it's in an ignored folder name
                        submenu = menu.CreateFolder(Path.GetRelativePath(folder.Name, parentPath), LeftClickMenu_ItemClicked);
                    }
                    var menuText = Path.GetFileName(file);
                    if (Configuration.HideFileExtensions || file.FileExtension().Is(".lnk"))
                    {
                        menuText = Path.GetFileNameWithoutExtension(file);
                    }
                    var entry = new ToolStripMenuItem
                    {
                        Text = menuText.ToMenuName(),
                        CommandParameter = file,
                        Image = file.GetImage()
                    };
                    if (submenu != null)
                    {
                        submenu.DropDownItems.Add(entry);
                    }
                    else
                    {
                        menu.Add(entry);
                    }
                }
                SetupLeftClickMenu(menu);
            }
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
            LeftClickMenu.Items.AddRange(menu.ToArray());
            menu.NeedsRefresh = false;
        }

        private IEnumerable<string> EnumerateFiles(string path, bool recursive)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = recursive,
                ReturnSpecialDirectories = false,
                MaxRecursionDepth = Configuration.MaxRecursionDepth > 0 ? Configuration.MaxRecursionDepth : int.MaxValue,
            };
            return Directory.EnumerateFiles(path, "*.*", options)
                .Where(f => !Configuration.IgnoreFiles.Any(i => f.IsMatch("." + i)))
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
                        Program.Launch(folder.Name);
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
                try
                {
                    Program.Launch($"{e.ClickedItem.CommandParameter}");
                }
                catch { }
            }
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
            var error = false;
            foreach (var c in FolderControls()) c.Error = false;

            foreach (var c in FolderControls().Where(c => !c.Config.Name.HasValue()))
            {
                c.Error = error = true;
            }
            if (error)
            {
                MessageBox.Show(R.The_folder_value_must_be_set, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var c in FolderControls().Where(c => !Directory.Exists(c.Config.Name!.ToLocalPath())))
            {
                c.Error = error = true;
            }
            if (error)
            {
                MessageBox.Show(R.The_folder_does_not_exist, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Configuration.Folders = FolderControls().Select(c => c.Config).ToList();
            Configuration.IgnoreFiles = IgnoreFilesTextBox.Text.SplitPaths();
            Configuration.IgnoreFolders = IgnoreFoldersTextBox.Text.SplitPaths();
            Configuration.Theme = (int)ThemeToggleButton.Theme;
            LoadConfiguration();
            if (ConfigHelper.WriteConfiguration(Configuration))
            {
                Close();
            }
            ConfigHelper.SetStartupKey(RunOnLoginCheckbox.Checked);
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
                Width = 412,
                Margin = new Padding { All = 0 },
            };
            folderControl.BrowseFolder += FolderControl_BrowseClicked;
            folderControl.DeleteFolder += FolderControl_DeleteClicked;
            FoldersLayout.Controls.Add(folderControl);
            FoldersLayout.AutoScroll = FoldersLayout.Controls.Count > 5;
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
                c.HideDeleteButton = count == 1;
            }
        }

        private void ThemeToggleButton_ThemeChanged(object sender, EventArgs e)
        {
            var darkmode = UseDarkMode();
            SystemTheme.UseImmersiveDarkMode(this.Handle, darkmode);
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
                SystemTheme.UseImmersiveDarkMode(this.Handle, darkmode);
            }
        }
    }
}
