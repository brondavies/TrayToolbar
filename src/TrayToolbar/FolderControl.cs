using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrayToolbar
{
    public partial class FolderControl : UserControl
    {
        private FolderConfig config = new();
        private bool hideDeleteButton;

        public event EventHandler? BrowseFolder;
        public event EventHandler? DeleteFolder;

        public bool HideDeleteButton
        {
            get => hideDeleteButton;
            set
            {
                hideDeleteButton = value;
                DeleteFolderButton.Visible = !hideDeleteButton;
            }
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

        public FolderControl()
        {
            InitializeComponent();
            UpdateConfig();
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
                if (FolderComboBox != null) FolderComboBox.Text = config.Name;
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

        private void FolderComboBox_TextUpdate(object sender, EventArgs e)
        {
            Config.Name = FolderComboBox.Text;
        }
    }
}
