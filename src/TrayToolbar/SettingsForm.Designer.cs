namespace TrayToolbar
{
    partial class SettingsForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            FlowLayoutPanel flowLayoutPanel1;
            TableLayoutPanel tableLayoutPanel1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            CancelBtn = new Button();
            SaveButton = new Button();
            label1 = new Label();
            label2 = new Label();
            IgnoreFilesTextBox = new TextBox();
            FolderComboBox = new ComboBox();
            BrowseFolderButton = new PictureBox();
            RunOnLoginCheckbox = new CheckBox();
            TrayIcon = new NotifyIcon(components);
            RightClickMenu = new ContextMenuStrip(components);
            LeftClickMenu = new ContextMenuStrip(components);
            FolderDialog = new FolderBrowserDialog();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BrowseFolderButton).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(CancelBtn);
            flowLayoutPanel1.Controls.Add(SaveButton);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(10, 172);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(3);
            flowLayoutPanel1.Size = new Size(472, 37);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // CancelBtn
            // 
            CancelBtn.AutoSize = true;
            CancelBtn.DialogResult = DialogResult.Cancel;
            CancelBtn.Location = new Point(363, 6);
            CancelBtn.MinimumSize = new Size(100, 25);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(100, 25);
            CancelBtn.TabIndex = 0;
            CancelBtn.Text = "Cancel";
            CancelBtn.UseVisualStyleBackColor = true;
            CancelBtn.Click += CancelBtn_Click;
            // 
            // SaveButton
            // 
            SaveButton.AutoSize = true;
            SaveButton.DialogResult = DialogResult.OK;
            SaveButton.Location = new Point(257, 6);
            SaveButton.MinimumSize = new Size(100, 25);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(100, 25);
            SaveButton.TabIndex = 1;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(IgnoreFilesTextBox, 1, 1);
            tableLayoutPanel1.Controls.Add(FolderComboBox, 1, 0);
            tableLayoutPanel1.Controls.Add(BrowseFolderButton, 2, 0);
            tableLayoutPanel1.Controls.Add(RunOnLoginCheckbox, 1, 2);
            tableLayoutPanel1.Location = new Point(10, 10);
            tableLayoutPanel1.Margin = new Padding(10);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.Size = new Size(472, 150);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(5);
            label1.Size = new Size(84, 25);
            label1.TabIndex = 0;
            label1.Text = "Folder";
            label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Top;
            label2.Location = new Point(3, 50);
            label2.Name = "label2";
            label2.Padding = new Padding(5);
            label2.Size = new Size(84, 25);
            label2.TabIndex = 1;
            label2.Text = "Exclude Files";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // IgnoreFilesTextBox
            // 
            IgnoreFilesTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            IgnoreFilesTextBox.Location = new Point(93, 53);
            IgnoreFilesTextBox.Name = "IgnoreFilesTextBox";
            IgnoreFilesTextBox.PlaceholderText = ".dll; .bak; .ini";
            IgnoreFilesTextBox.Size = new Size(340, 23);
            IgnoreFilesTextBox.TabIndex = 3;
            // 
            // FolderComboBox
            // 
            FolderComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FolderComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            FolderComboBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            FolderComboBox.DropDownStyle = ComboBoxStyle.Simple;
            FolderComboBox.FormattingEnabled = true;
            FolderComboBox.Location = new Point(93, 3);
            FolderComboBox.MaxDropDownItems = 15;
            FolderComboBox.Name = "FolderComboBox";
            FolderComboBox.Size = new Size(340, 23);
            FolderComboBox.TabIndex = 5;
            // 
            // BrowseFolderButton
            // 
            BrowseFolderButton.BackgroundImage = (Image)resources.GetObject("BrowseFolderButton.BackgroundImage");
            BrowseFolderButton.BackgroundImageLayout = ImageLayout.Center;
            BrowseFolderButton.Location = new Point(439, 3);
            BrowseFolderButton.Name = "BrowseFolderButton";
            BrowseFolderButton.Size = new Size(30, 23);
            BrowseFolderButton.TabIndex = 6;
            BrowseFolderButton.TabStop = false;
            BrowseFolderButton.Click += BrowseFolderButton_Click;
            // 
            // RunOnLoginCheckbox
            // 
            RunOnLoginCheckbox.AutoSize = true;
            RunOnLoginCheckbox.Location = new Point(93, 103);
            RunOnLoginCheckbox.Name = "RunOnLoginCheckbox";
            RunOnLoginCheckbox.Size = new Size(97, 19);
            RunOnLoginCheckbox.TabIndex = 8;
            RunOnLoginCheckbox.Text = "Run on log in";
            RunOnLoginCheckbox.UseVisualStyleBackColor = true;
            // 
            // TrayIcon
            // 
            TrayIcon.Icon = (Icon)resources.GetObject("TrayIcon.Icon");
            TrayIcon.Text = "TrayToolbar";
            TrayIcon.Visible = true;
            TrayIcon.Click += TrayIcon_Click;
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;
            // 
            // RightClickMenu
            // 
            RightClickMenu.Name = "contextMenuStrip1";
            RightClickMenu.Size = new Size(61, 4);
            RightClickMenu.ItemClicked += RightClickMenu_ItemClicked;
            // 
            // LeftClickMenu
            // 
            LeftClickMenu.Name = "leftClickMenu";
            LeftClickMenu.Size = new Size(61, 4);
            LeftClickMenu.ItemClicked += LeftClickMenu_ItemClicked;
            // 
            // SettingsForm
            // 
            AcceptButton = SaveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(492, 219);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(flowLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(480, 240);
            Name = "SettingsForm";
            Padding = new Padding(10);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TrayToolbar Settings";
            Deactivate += SettingsForm_Deactivate;
            FormClosing += SettingsForm_FormClosing;
            Resize += SettingsForm_Resize;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BrowseFolderButton).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon TrayIcon;
        private ContextMenuStrip RightClickMenu;
        private ContextMenuStrip LeftClickMenu;
        private Label label1;
        private Label label2;
        private TextBox IgnoreFilesTextBox;
        private Button CancelBtn;
        private Button SaveButton;
        private FolderBrowserDialog FolderDialog;
        private ComboBox FolderComboBox;
        private PictureBox BrowseFolderButton;
        private CheckBox RunOnLoginCheckbox;
    }
}
