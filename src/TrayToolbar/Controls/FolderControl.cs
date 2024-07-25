﻿using System.Diagnostics.Eventing.Reader;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar
{
    public partial class FolderControl : UserControl
    {
        private FolderConfig config = new();

        public event EventHandler? BrowseFolder;
        public event EventHandler? DeleteFolder;

        public FolderControl()
        {
            InitializeComponent();
            UpdateConfig();
            RecursiveCheckbox.Text = R.Include_Subfolders;
            toolTips.SetToolTip(BrowseFolderButton, R.Browse_Folder);
            toolTips.SetToolTip(DeleteFolderButton, R.Remove_Folder);
        }

        public bool HideDeleteButton
        {
            get => !DeleteFolderButton.Visible;
            set => DeleteFolderButton.Visible = !value;
        }

        public FolderConfig Config
        {
            get => config;
            set
            {
                config = value;
                UpdateConfig();
            }
        }

        public bool Error
        {
            get => ErrorIcon.Visible;
            set {
                ErrorIcon.BackColor = FolderTextBox.BackColor;
                ErrorIcon.Visible = value; 
            }
        }

        public void OnThemeChanged(bool darkMode)
        {
            ErrorIcon.BackColor = FolderTextBox.BackColor;
        }

        public void UpdateConfig()
        {
            if (InvokeRequired)
            {
                Invoke(UpdateConfig);
                return;
            }
            if (config != null)
            {
                if (FolderTextBox != null) FolderTextBox.Text = config.Name;
                if (RecursiveCheckbox != null) RecursiveCheckbox.Checked = config.Recursive;
            }
        }

        private void BrowseFolderButton_Click(object sender, EventArgs e)
        {
            BrowseFolder?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteFolderButton_Click(object sender, EventArgs e)
        {
            DeleteFolder?.Invoke(this, EventArgs.Empty);
        }

        private void FolderTextBox_TextUpdate(object sender, EventArgs e)
        {
            Config.Name = FolderTextBox.Text;
        }

        private void RecursiveCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Config.Recursive = RecursiveCheckbox.Checked;
        }
    }
}
