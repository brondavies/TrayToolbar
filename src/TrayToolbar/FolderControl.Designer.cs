namespace TrayToolbar
{
    partial class FolderControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderControl));
            FolderComboBox = new ComboBox();
            BrowseFolderButton = new PictureBox();
            DeleteFolderButton = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)BrowseFolderButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DeleteFolderButton).BeginInit();
            SuspendLayout();
            // 
            // FolderComboBox
            // 
            FolderComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FolderComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            FolderComboBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            FolderComboBox.DropDownStyle = ComboBoxStyle.Simple;
            FolderComboBox.FormattingEnabled = true;
            FolderComboBox.Location = new Point(3, 3);
            FolderComboBox.MaxDropDownItems = 15;
            FolderComboBox.Name = "FolderComboBox";
            FolderComboBox.Size = new Size(346, 23);
            FolderComboBox.TabIndex = 6;
            FolderComboBox.TextUpdate += FolderComboBox_TextUpdate;
            // 
            // BrowseFolderButton
            // 
            BrowseFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BrowseFolderButton.BackgroundImage = (Image)resources.GetObject("BrowseFolderButton.BackgroundImage");
            BrowseFolderButton.BackgroundImageLayout = ImageLayout.Center;
            BrowseFolderButton.Location = new Point(387, 3);
            BrowseFolderButton.Name = "BrowseFolderButton";
            BrowseFolderButton.Size = new Size(30, 23);
            BrowseFolderButton.TabIndex = 7;
            BrowseFolderButton.TabStop = false;
            BrowseFolderButton.Click += BrowseFolderButton_Click;
            // 
            // DeleteFolderButton
            // 
            DeleteFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteFolderButton.BackgroundImage = (Image)resources.GetObject("DeleteFolderButton.BackgroundImage");
            DeleteFolderButton.BackgroundImageLayout = ImageLayout.Center;
            DeleteFolderButton.Location = new Point(355, 3);
            DeleteFolderButton.Name = "DeleteFolderButton";
            DeleteFolderButton.Size = new Size(30, 23);
            DeleteFolderButton.TabIndex = 8;
            DeleteFolderButton.TabStop = false;
            DeleteFolderButton.Click += DeleteFolderButton_Click;
            // 
            // FolderControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            Controls.Add(DeleteFolderButton);
            Controls.Add(BrowseFolderButton);
            Controls.Add(FolderComboBox);
            Name = "FolderControl";
            Size = new Size(420, 29);
            ((System.ComponentModel.ISupportInitialize)BrowseFolderButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)DeleteFolderButton).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox FolderComboBox;
        private PictureBox BrowseFolderButton;
        private PictureBox DeleteFolderButton;
    }
}
