using R = TrayToolbar.Resources;

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
            label1.Text = R.Folder;
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
                if (Configuration.Folder.HasValue())
                {
                    if (Directory.Exists(Configuration.Folder.ToLocalPath()))
                    {
                        StartWatchingFolder(Configuration.Folder);
                    }
                    else
                    {
                        watcher = null;
                    }
                }
                else
                {
                    watcher = null;
                }
            }
        }

        private FileSystemWatcher? watcher;
        private void StartWatchingFolder(string path)
        {
            watcher = new FileSystemWatcher(path)
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
        }

        private void RefreshMenu()
        {
            MenuItems.NeedsRefresh = true;
        }

        private void PopulateConfig()
        {
            FolderComboBox.Text = Configuration.Folder;
            IgnoreFilesTextBox.Text = Configuration.IgnoreFiles.Join("; ");
            RunOnLoginCheckbox.Checked = ConfigHelper.GetStartupKey();
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

        private void BrowseFolderButton_Click(object sender, EventArgs e)
        {
            var result = FolderDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                FolderComboBox.Text = FolderDialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!FolderComboBox.Text.HasValue())
            {
                MessageBox.Show(R.The_folder_value_must_be_set, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!Directory.Exists(FolderComboBox.Text.ToLocalPath()))
            {
                MessageBox.Show(R.The_folder_does_not_exist, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Configuration.Folder = FolderComboBox.Text;
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
    }
}
