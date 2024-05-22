using R = TrayToolbar.Resources;
using System.Linq;

namespace TrayToolbar
{
    public partial class SettingsForm : Form
    {
        internal TrayToolbarConfiguration Configuration = new();

        public MenuItemCollection MenuItems = [];

        public SettingsForm()
        {
            InitializeComponent();
            LoadResources();
            SetupMenu();
            PopulateConfig();
        }

        #region LoadResources

        private void LoadResources()
        {
            label1.Text = R.Folders;
            label2.Text = R.Exclude_files;
            RunOnLoginCheckbox.Text = R.Run_on_log_in;
            SaveButton.Text = R.Save;
            CancelBtn.Text = R.Cancel;
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
                new ToolStripMenuItem { Text = R.Options, CommandParameter = "Options" },
                new ToolStripMenuItem { Text = R.Exit, CommandParameter = "Exit" }
            });
        }

        private void ShowUpdateAvailable(string updateUri)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { ShowUpdateAvailable(updateUri); }));
                return;
            }
            NewVersionLabel.Visible = true;
            NewVersionLabel.Tag = "https://github.com" + updateUri;
        }

        #endregion

        private bool init = false;
        protected override void SetVisibleCore(bool value)
        {
            if (!init && File.Exists(ConfigHelper.ConfigurationFile))
            {
                init = true;
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
                foreach (var folder in Configuration.Folders)
                {
                    if (folder.Name.HasValue() && Directory.Exists(folder.Name.ToLocalPath()))
                    {
                        StartWatchingFolder(folder.Name);
                    }
                }
            }
        }

        private readonly Dictionary<string, FileSystemWatcher> watchers = [];
        private void StartWatchingFolder(string path)
        {
            var watcher = new FileSystemWatcher(path)
            {
                Filter = "*.*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.CreationTime
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size,
            };
            watcher.Created += (_, _) => RefreshMenu();
            watcher.Deleted += (_, _) => RefreshMenu();
            watcher.Renamed += (_, _) => RefreshMenu();
            watchers[path] = watcher;
        }

        private void RefreshMenu()
        {
            MenuItems.NeedsRefresh = true;
        }

        private void PopulateConfig()
        {
            var list = FolderControls().ToArray();
            foreach (var c in list) foldersLayout.Controls.Remove(c);
            var i = 0;
            if (Configuration.Folders.Count == 0)
            {
                Configuration.Folders.Add(new FolderConfig { Recursive = true });
            }
            Configuration.Folders.ForEach(f => AddFolder(f, i++));
            IgnoreFilesTextBox.Text = Configuration.IgnoreFiles.Join("; ");
            RunOnLoginCheckbox.Checked = ConfigHelper.GetStartupKey();
        }

        private IEnumerable<FolderControl> FolderControls()
        {
            foreach (var c in foldersLayout.Controls)
            {
                if (c is FolderControl control)
                {
                    yield return control;
                }
            }
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            var me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                TrayIcon.ContextMenuStrip = RightClickMenu;
            }
            else
            {
                if (MenuItems.NeedsRefresh)
                {
                    ReloadMenuItems();
                }
                TrayIcon.ContextMenuStrip = LeftClickMenu;

                if (LeftClickMenu.Items.Count == 0)
                {
                    ShowNormal();
                    return;
                }
            }
            TrayIcon.ShowContextMenu();
        }

        private void ReloadMenuItems()
        {
            MenuItems.Clear();
            if (!Configuration.Folder.HasValue() || !Directory.Exists(Configuration.Folder.ToLocalPath())) return;
            var files = Directory.GetFiles(Configuration.Folder.ToLocalPath());
            foreach (var file in files.Where(f =>
                !Configuration.IgnoreFiles.Any(i => i == f.FileExtension())
                ))
            {
                MenuItems.Add(new ToolStripMenuItem
                {
                    Text = Path.GetFileNameWithoutExtension(file),
                    CommandParameter = file,
                    Image = file.GetImage()
                });
            }
            LeftClickMenu.Items.Clear();
            LeftClickMenu.Items.AddRange(MenuItems.ToArray());
            MenuItems.NeedsRefresh = false;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowNormal();
        }

        private void ShowNormal()
        {
            Visible = true;
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        private bool quitting = false;
        private void Quit()
        {
            quitting = true;
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
                case "Options":
                    ShowNormal();
                    break;
                case "Refresh":
                    LoadConfiguration();
                    RefreshMenu();
                    break;
                case "Exit":
                    Quit();
                    Application.Exit();
                    break;
            }
        }

        private void LeftClickMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem?.CommandParameter != null)
            {
                Program.Launch($"{e.ClickedItem.CommandParameter}");
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
            if (!FolderControls().All(c => c.Config.Name.HasValue()))
            {
                MessageBox.Show(R.The_folder_value_must_be_set, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!FolderControls().All(c => Directory.Exists(c.Config.Name!.ToLocalPath())))
            {
                MessageBox.Show(R.The_folder_does_not_exist, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Configuration.Folders = FolderControls().Select(c => c.Config).ToList();
                Configuration.IgnoreFiles = IgnoreFilesTextBox.Text.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                LoadConfiguration();
                if (ConfigHelper.WriteConfiguration(Configuration))
                {
                    Close();
                }
                ConfigHelper.SetStartupKey(RunOnLoginCheckbox.Checked);
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            PopulateConfig();
            Close();
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
        }

        private void FolderControl_DeleteClicked(object? sender, EventArgs e)
        {
            var control = (FolderControl)sender!;
            control.BrowseFolder -= FolderControl_BrowseClicked;
            control.DeleteFolder -= FolderControl_DeleteClicked;
            Configuration.Folders.Remove(control.Config);
            foldersLayout.Controls.Remove(control);
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
                Height = 28,
                Width = 420,
                Margin = new Padding { All = 0 },
            };
            folderControl.BrowseFolder += FolderControl_BrowseClicked;
            folderControl.DeleteFolder += FolderControl_DeleteClicked;
            foldersLayout.Controls.Add(folderControl);
            FoldersUpdated();
        }

        private void FoldersUpdated()
        {
            var list = FolderControls();
            var count = list.Count();
            foreach(var c in list)
            {
                c.HideDeleteButton = count == 1;
            }
        }
    }
}
