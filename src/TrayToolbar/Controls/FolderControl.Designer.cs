using TrayToolbar.Controls;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderControl));
            FolderComboBox = new CustomComboBox();
            BrowseFolderButton = new PictureBox();
            DeleteFolderButton = new PictureBox();
            RecursiveCheckbox = new CheckBox();
            toolTips = new ToolTip(components);
            ErrorIcon = new PictureBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            HotkeyLabel = new Label();
            HotkeyValue = new Label();
            SetHotKey = new Label();
            BottomLine = new Label();
            SelectIconButton = new PictureBox();
            ClearIconButton = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)BrowseFolderButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DeleteFolderButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ErrorIcon).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SelectIconButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ClearIconButton).BeginInit();
            SuspendLayout();
            // 
            // FolderComboBox
            // 
            FolderComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FolderComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            FolderComboBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            FolderComboBox.BorderColor = SystemColors.WindowFrame;
            FolderComboBox.DropDownStyle = ComboBoxStyle.Simple;
            FolderComboBox.FormattingEnabled = true;
            FolderComboBox.Location = new Point(44, 3);
            FolderComboBox.MaxDropDownItems = 15;
            FolderComboBox.Name = "FolderComboBox";
            FolderComboBox.Size = new Size(289, 23);
            FolderComboBox.Sorted = true;
            FolderComboBox.TabIndex = 0;
            FolderComboBox.TextUpdate += FolderComboBox_TextUpdate;
            // 
            // BrowseFolderButton
            // 
            BrowseFolderButton.AccessibleDescription = "Browse for folder";
            BrowseFolderButton.AccessibleName = "Browse";
            BrowseFolderButton.AccessibleRole = AccessibleRole.PushButton;
            BrowseFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BrowseFolderButton.BackgroundImage = Resources.Resources.TrayIcon;
            BrowseFolderButton.BackgroundImageLayout = ImageLayout.Zoom;
            BrowseFolderButton.Cursor = Cursors.Hand;
            BrowseFolderButton.Location = new Point(335, 3);
            BrowseFolderButton.Name = "BrowseFolderButton";
            BrowseFolderButton.Size = new Size(30, 23);
            BrowseFolderButton.SizeMode = PictureBoxSizeMode.AutoSize;
            BrowseFolderButton.TabIndex = 7;
            BrowseFolderButton.TabStop = false;
            BrowseFolderButton.Click += BrowseFolderButton_Click;
            // 
            // DeleteFolderButton
            // 
            DeleteFolderButton.AccessibleDescription = "Remove folder";
            DeleteFolderButton.AccessibleName = "Remove";
            DeleteFolderButton.AccessibleRole = AccessibleRole.PushButton;
            DeleteFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteFolderButton.BackgroundImage = Resources.Resources.delete;
            DeleteFolderButton.BackgroundImageLayout = ImageLayout.Zoom;
            DeleteFolderButton.Cursor = Cursors.Hand;
            DeleteFolderButton.Location = new Point(367, 3);
            DeleteFolderButton.Name = "DeleteFolderButton";
            DeleteFolderButton.Size = new Size(30, 23);
            DeleteFolderButton.SizeMode = PictureBoxSizeMode.AutoSize;
            DeleteFolderButton.TabIndex = 8;
            DeleteFolderButton.TabStop = false;
            DeleteFolderButton.Click += DeleteFolderButton_Click;
            // 
            // RecursiveCheckbox
            // 
            RecursiveCheckbox.AutoSize = true;
            RecursiveCheckbox.Location = new Point(44, 32);
            RecursiveCheckbox.Name = "RecursiveCheckbox";
            RecursiveCheckbox.Size = new Size(124, 19);
            RecursiveCheckbox.TabIndex = 1;
            RecursiveCheckbox.Text = "Include Subfolders";
            RecursiveCheckbox.UseVisualStyleBackColor = true;
            RecursiveCheckbox.CheckedChanged += RecursiveCheckbox_CheckedChanged;
            // 
            // ErrorIcon
            // 
            ErrorIcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ErrorIcon.BackColor = Color.Transparent;
            ErrorIcon.BackgroundImage = Resources.Resources.warning;
            ErrorIcon.BackgroundImageLayout = ImageLayout.Zoom;
            ErrorIcon.Location = new Point(313, 6);
            ErrorIcon.Margin = new Padding(0);
            ErrorIcon.Name = "ErrorIcon";
            ErrorIcon.Size = new Size(16, 16);
            ErrorIcon.SizeMode = PictureBoxSizeMode.AutoSize;
            ErrorIcon.TabIndex = 10;
            ErrorIcon.TabStop = false;
            ErrorIcon.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(HotkeyLabel);
            flowLayoutPanel1.Controls.Add(HotkeyValue);
            flowLayoutPanel1.Controls.Add(SetHotKey);
            flowLayoutPanel1.Location = new Point(3, 57);
            flowLayoutPanel1.Margin = new Padding(0, 0, 0, 5);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(394, 19);
            flowLayoutPanel1.TabIndex = 11;
            // 
            // HotkeyLabel
            // 
            HotkeyLabel.AutoSize = true;
            HotkeyLabel.Location = new Point(3, 0);
            HotkeyLabel.Name = "HotkeyLabel";
            HotkeyLabel.Padding = new Padding(0, 2, 0, 2);
            HotkeyLabel.Size = new Size(74, 19);
            HotkeyLabel.TabIndex = 0;
            HotkeyLabel.Text = "Shortcut Key";
            HotkeyLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // HotkeyValue
            // 
            HotkeyValue.AutoSize = true;
            HotkeyValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            HotkeyValue.Location = new Point(83, 0);
            HotkeyValue.Name = "HotkeyValue";
            HotkeyValue.Padding = new Padding(0, 2, 0, 2);
            HotkeyValue.Size = new Size(0, 19);
            HotkeyValue.TabIndex = 1;
            HotkeyValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SetHotKey
            // 
            SetHotKey.AutoSize = true;
            SetHotKey.BorderStyle = BorderStyle.FixedSingle;
            SetHotKey.Cursor = Cursors.Hand;
            SetHotKey.Location = new Point(89, 0);
            SetHotKey.MinimumSize = new Size(50, 0);
            SetHotKey.Name = "SetHotKey";
            SetHotKey.Padding = new Padding(0, 1, 0, 1);
            SetHotKey.Size = new Size(50, 19);
            SetHotKey.TabIndex = 2;
            SetHotKey.Text = "Set";
            SetHotKey.TextAlign = ContentAlignment.MiddleCenter;
            SetHotKey.Click += SetHotKey_Click;
            // 
            // BottomLine
            // 
            BottomLine.AccessibleRole = AccessibleRole.Caret;
            BottomLine.BackColor = Color.DarkGray;
            BottomLine.Dock = DockStyle.Bottom;
            BottomLine.Location = new Point(0, 84);
            BottomLine.Margin = new Padding(0, 5, 0, 0);
            BottomLine.Name = "BottomLine";
            BottomLine.Size = new Size(400, 1);
            BottomLine.TabIndex = 12;
            // 
            // SelectIconButton
            // 
            SelectIconButton.AccessibleDescription = "Browse for folder";
            SelectIconButton.AccessibleName = "Browse";
            SelectIconButton.AccessibleRole = AccessibleRole.PushButton;
            SelectIconButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SelectIconButton.BackgroundImageLayout = ImageLayout.Zoom;
            SelectIconButton.Cursor = Cursors.Hand;
            SelectIconButton.Location = new Point(3, 3);
            SelectIconButton.Name = "SelectIconButton";
            SelectIconButton.Size = new Size(32, 32);
            SelectIconButton.SizeMode = PictureBoxSizeMode.AutoSize;
            SelectIconButton.TabIndex = 13;
            SelectIconButton.TabStop = false;
            SelectIconButton.Click += SelectIconButton_Click;
            // 
            // ClearIconButton
            // 
            ClearIconButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ClearIconButton.BackColor = Color.Transparent;
            ClearIconButton.BackgroundImage = (Image)resources.GetObject("ClearIconButton.BackgroundImage");
            ClearIconButton.BackgroundImageLayout = ImageLayout.None;
            ClearIconButton.Location = new Point(24, 3);
            ClearIconButton.Margin = new Padding(0);
            ClearIconButton.Name = "ClearIconButton";
            ClearIconButton.Size = new Size(12, 12);
            ClearIconButton.TabIndex = 14;
            ClearIconButton.TabStop = false;
            ClearIconButton.Visible = false;
            ClearIconButton.Click += ClearIconButton_Click;
            // 
            // FolderControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(ClearIconButton);
            Controls.Add(SelectIconButton);
            Controls.Add(BottomLine);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(ErrorIcon);
            Controls.Add(RecursiveCheckbox);
            Controls.Add(DeleteFolderButton);
            Controls.Add(BrowseFolderButton);
            Controls.Add(FolderComboBox);
            Name = "FolderControl";
            Size = new Size(400, 85);
            ((System.ComponentModel.ISupportInitialize)BrowseFolderButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)DeleteFolderButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)ErrorIcon).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)SelectIconButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)ClearIconButton).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomComboBox FolderComboBox;
        private PictureBox BrowseFolderButton;
        private PictureBox DeleteFolderButton;
        private CheckBox RecursiveCheckbox;
        private ToolTip toolTips;
        private PictureBox ErrorIcon;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label HotkeyLabel;
        private Label HotkeyValue;
        private Label SetHotKey;
        private Label BottomLine;
        private PictureBox SelectIconButton;
        private PictureBox ClearIconButton;
    }
}
