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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            CancelBtn = new Button();
            SaveButton = new Button();
            NewVersionLabel = new LinkLabel();
            tableLayout = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label4 = new Label();
            IgnoreFoldersTextBox = new TextBox();
            label3 = new Label();
            label5 = new Label();
            FoldersLayout = new FlowLayoutPanel();
            AddFolderButton = new Button();
            IgnoreFilesTextBox = new TextBox();
            ThemeToggleButton = new Controls.ThemeToggle();
            RunOnLoginCheckbox = new CheckBox();
            fontImageSizeTableLayout = new TableLayoutPanel();
            IconSizeLargeCheckbox = new RadioButton();
            label6 = new Label();
            FontSizeInput = new NumericUpDown();
            IconSizeSmallCheckbox = new RadioButton();
            RightClickMenu = new ContextMenuStrip(components);
            LeftClickMenu = new ContextMenuStrip(components);
            FolderDialog = new FolderBrowserDialog();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            tableLayout.SuspendLayout();
            fontImageSizeTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)FontSizeInput).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(CancelBtn);
            flowLayoutPanel1.Controls.Add(SaveButton);
            flowLayoutPanel1.Controls.Add(NewVersionLabel);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(10, 328);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(3);
            flowLayoutPanel1.Size = new Size(520, 39);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // CancelBtn
            // 
            CancelBtn.AutoSize = true;
            CancelBtn.DialogResult = DialogResult.Cancel;
            CancelBtn.Location = new Point(411, 6);
            CancelBtn.MinimumSize = new Size(100, 25);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(100, 27);
            CancelBtn.TabIndex = 2;
            CancelBtn.Text = "Cancel";
            CancelBtn.UseVisualStyleBackColor = true;
            CancelBtn.Click += CancelBtn_Click;
            // 
            // SaveButton
            // 
            SaveButton.AutoSize = true;
            SaveButton.DialogResult = DialogResult.OK;
            SaveButton.Location = new Point(305, 6);
            SaveButton.MinimumSize = new Size(100, 25);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(100, 27);
            SaveButton.TabIndex = 1;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // NewVersionLabel
            // 
            NewVersionLabel.ActiveLinkColor = Color.RoyalBlue;
            NewVersionLabel.AutoSize = true;
            NewVersionLabel.Dock = DockStyle.Left;
            NewVersionLabel.Image = (Image)resources.GetObject("NewVersionLabel.Image");
            NewVersionLabel.ImageAlign = ContentAlignment.MiddleLeft;
            NewVersionLabel.Location = new Point(104, 3);
            NewVersionLabel.Margin = new Padding(3, 0, 30, 0);
            NewVersionLabel.Name = "NewVersionLabel";
            NewVersionLabel.Padding = new Padding(24, 0, 0, 0);
            NewVersionLabel.Size = new Size(168, 33);
            NewVersionLabel.TabIndex = 10;
            NewVersionLabel.TabStop = true;
            NewVersionLabel.Text = "A new version is available!";
            NewVersionLabel.TextAlign = ContentAlignment.MiddleLeft;
            NewVersionLabel.Visible = false;
            NewVersionLabel.VisitedLinkColor = Color.Blue;
            NewVersionLabel.LinkClicked += NewVersionLabel_LinkClicked;
            // 
            // tableLayout
            // 
            tableLayout.AutoSize = true;
            tableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayout.ColumnCount = 2;
            tableLayout.ColumnStyles.Add(new ColumnStyle());
            tableLayout.ColumnStyles.Add(new ColumnStyle());
            tableLayout.Controls.Add(label1, 0, 0);
            tableLayout.Controls.Add(label2, 0, 2);
            tableLayout.Controls.Add(label4, 0, 3);
            tableLayout.Controls.Add(IgnoreFoldersTextBox, 1, 3);
            tableLayout.Controls.Add(label3, 0, 4);
            tableLayout.Controls.Add(label5, 0, 5);
            tableLayout.Controls.Add(FoldersLayout, 1, 0);
            tableLayout.Controls.Add(AddFolderButton, 1, 1);
            tableLayout.Controls.Add(IgnoreFilesTextBox, 1, 2);
            tableLayout.Controls.Add(ThemeToggleButton, 1, 4);
            tableLayout.Controls.Add(RunOnLoginCheckbox, 1, 6);
            tableLayout.Controls.Add(fontImageSizeTableLayout, 1, 5);
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.Location = new Point(10, 10);
            tableLayout.Margin = new Padding(10);
            tableLayout.Name = "tableLayout";
            tableLayout.RowCount = 7;
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.Size = new Size(520, 318);
            tableLayout.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(5);
            label1.Size = new Size(107, 25);
            label1.TabIndex = 0;
            label1.Text = "Folders";
            label1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Top;
            label2.Location = new Point(3, 75);
            label2.Name = "label2";
            label2.Padding = new Padding(5);
            label2.Size = new Size(107, 25);
            label2.TabIndex = 1;
            label2.Text = "Exclude file types";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Top;
            label4.Location = new Point(3, 125);
            label4.Name = "label4";
            label4.Padding = new Padding(5);
            label4.Size = new Size(107, 25);
            label4.TabIndex = 14;
            label4.Text = "Exclude folders";
            label4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // IgnoreFoldersTextBox
            // 
            IgnoreFoldersTextBox.BorderStyle = BorderStyle.FixedSingle;
            IgnoreFoldersTextBox.Dock = DockStyle.Top;
            IgnoreFoldersTextBox.Location = new Point(116, 128);
            IgnoreFoldersTextBox.Name = "IgnoreFoldersTextBox";
            IgnoreFoldersTextBox.PlaceholderText = ".git; .github";
            IgnoreFoldersTextBox.Size = new Size(414, 23);
            IgnoreFoldersTextBox.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Top;
            label3.Location = new Point(3, 175);
            label3.Name = "label3";
            label3.Padding = new Padding(5);
            label3.Size = new Size(107, 25);
            label3.TabIndex = 12;
            label3.Text = "Theme";
            label3.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Top;
            label5.Location = new Point(3, 225);
            label5.Name = "label5";
            label5.Padding = new Padding(5);
            label5.Size = new Size(107, 25);
            label5.TabIndex = 13;
            label5.Text = "Menu Font Size";
            label5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // FoldersLayout
            // 
            FoldersLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FoldersLayout.AutoSize = true;
            FoldersLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FoldersLayout.FlowDirection = FlowDirection.TopDown;
            FoldersLayout.Location = new Point(113, 3);
            FoldersLayout.Margin = new Padding(0, 3, 0, 0);
            FoldersLayout.MaximumSize = new Size(450, 270);
            FoldersLayout.MinimumSize = new Size(420, 0);
            FoldersLayout.Name = "FoldersLayout";
            FoldersLayout.Size = new Size(420, 0);
            FoldersLayout.TabIndex = 11;
            FoldersLayout.WrapContents = false;
            // 
            // AddFolderButton
            // 
            AddFolderButton.AutoSize = true;
            AddFolderButton.Image = (Image)resources.GetObject("AddFolderButton.Image");
            AddFolderButton.ImageAlign = ContentAlignment.MiddleLeft;
            AddFolderButton.Location = new Point(116, 28);
            AddFolderButton.Name = "AddFolderButton";
            AddFolderButton.Padding = new Padding(32, 0, 0, 0);
            AddFolderButton.Size = new Size(158, 29);
            AddFolderButton.TabIndex = 0;
            AddFolderButton.Text = "Add Folder";
            AddFolderButton.UseVisualStyleBackColor = true;
            AddFolderButton.Click += AddFolderButton_Click;
            // 
            // IgnoreFilesTextBox
            // 
            IgnoreFilesTextBox.BorderStyle = BorderStyle.FixedSingle;
            IgnoreFilesTextBox.Dock = DockStyle.Top;
            IgnoreFilesTextBox.Location = new Point(116, 78);
            IgnoreFilesTextBox.Name = "IgnoreFilesTextBox";
            IgnoreFilesTextBox.PlaceholderText = ".bak; .config; .dll; .ico; .ini";
            IgnoreFilesTextBox.Size = new Size(414, 23);
            IgnoreFilesTextBox.TabIndex = 3;
            // 
            // ThemeToggleButton
            // 
            ThemeToggleButton.AutoSize = true;
            ThemeToggleButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ThemeToggleButton.Location = new Point(116, 178);
            ThemeToggleButton.Name = "ThemeToggleButton";
            ThemeToggleButton.Size = new Size(182, 25);
            ThemeToggleButton.TabIndex = 5;
            ThemeToggleButton.Theme = Models.ThemeToggleEnum.SYSTEM_THEME;
            ThemeToggleButton.ThemeChanged += ThemeToggleButton_ThemeChanged;
            // 
            // RunOnLoginCheckbox
            // 
            RunOnLoginCheckbox.AutoSize = true;
            RunOnLoginCheckbox.Location = new Point(116, 278);
            RunOnLoginCheckbox.Name = "RunOnLoginCheckbox";
            RunOnLoginCheckbox.Size = new Size(97, 19);
            RunOnLoginCheckbox.TabIndex = 9;
            RunOnLoginCheckbox.Text = "Run on log in";
            RunOnLoginCheckbox.UseVisualStyleBackColor = true;
            // 
            // fontImageSizeTableLayout
            // 
            fontImageSizeTableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fontImageSizeTableLayout.ColumnCount = 4;
            fontImageSizeTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            fontImageSizeTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            fontImageSizeTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            fontImageSizeTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            fontImageSizeTableLayout.Controls.Add(IconSizeLargeCheckbox, 3, 0);
            fontImageSizeTableLayout.Controls.Add(label6, 1, 0);
            fontImageSizeTableLayout.Controls.Add(FontSizeInput, 0, 0);
            fontImageSizeTableLayout.Controls.Add(IconSizeSmallCheckbox, 2, 0);
            fontImageSizeTableLayout.Dock = DockStyle.Fill;
            fontImageSizeTableLayout.Location = new Point(116, 228);
            fontImageSizeTableLayout.Name = "fontImageSizeTableLayout";
            fontImageSizeTableLayout.RowCount = 1;
            fontImageSizeTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            fontImageSizeTableLayout.Size = new Size(414, 44);
            fontImageSizeTableLayout.TabIndex = 4;
            // 
            // IconSizeLargeCheckbox
            // 
            IconSizeLargeCheckbox.AutoSize = true;
            IconSizeLargeCheckbox.Location = new Point(312, 3);
            IconSizeLargeCheckbox.Name = "IconSizeLargeCheckbox";
            IconSizeLargeCheckbox.Size = new Size(54, 19);
            IconSizeLargeCheckbox.TabIndex = 8;
            IconSizeLargeCheckbox.Text = "Large";
            IconSizeLargeCheckbox.UseVisualStyleBackColor = true;
            IconSizeLargeCheckbox.CheckedChanged += IconSizeLargeCheckbox_CheckedChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Top;
            label6.Location = new Point(106, 0);
            label6.Name = "label6";
            label6.Padding = new Padding(5);
            label6.Size = new Size(97, 25);
            label6.TabIndex = 14;
            label6.Text = "Icon Size";
            label6.TextAlign = ContentAlignment.MiddleRight;
            // 
            // FontSizeInput
            // 
            FontSizeInput.Location = new Point(3, 3);
            FontSizeInput.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            FontSizeInput.Minimum = new decimal(new int[] { 9, 0, 0, 0 });
            FontSizeInput.Name = "FontSizeInput";
            FontSizeInput.Size = new Size(50, 23);
            FontSizeInput.TabIndex = 6;
            FontSizeInput.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // IconSizeSmallCheckbox
            // 
            IconSizeSmallCheckbox.AutoSize = true;
            IconSizeSmallCheckbox.Checked = true;
            IconSizeSmallCheckbox.Location = new Point(209, 3);
            IconSizeSmallCheckbox.Name = "IconSizeSmallCheckbox";
            IconSizeSmallCheckbox.Size = new Size(54, 19);
            IconSizeSmallCheckbox.TabIndex = 7;
            IconSizeSmallCheckbox.TabStop = true;
            IconSizeSmallCheckbox.Text = "Small";
            IconSizeSmallCheckbox.UseVisualStyleBackColor = true;
            IconSizeSmallCheckbox.CheckedChanged += IconSizeSmallCheckbox_CheckedChanged;
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
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(540, 377);
            Controls.Add(tableLayout);
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
            Shown += SettingsForm_Shown;
            Resize += SettingsForm_Resize;
            SystemColorsChanged += SettingsForm_SystemColorsChanged;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
            fontImageSizeTableLayout.ResumeLayout(false);
            fontImageSizeTableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)FontSizeInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ContextMenuStrip RightClickMenu;
        private ContextMenuStrip LeftClickMenu;
        private Label label1;
        private Label label2;
        private TextBox IgnoreFilesTextBox;
        private Button CancelBtn;
        private Button SaveButton;
        private FolderBrowserDialog FolderDialog;
        private LinkLabel NewVersionLabel;
        private TableLayoutPanel tableLayout;
        private Button AddFolderButton;
        private FlowLayoutPanel FoldersLayout;
        private Label label3;
        private Controls.ThemeToggle ThemeToggleButton;
        private Label label4;
        private Label label5;
        private TextBox IgnoreFoldersTextBox;
        private CheckBox RunOnLoginCheckbox;
        private TableLayoutPanel fontImageSizeTableLayout;
        private NumericUpDown FontSizeInput;
        private Label label6;
        private RadioButton IconSizeLargeCheckbox;
        private RadioButton IconSizeSmallCheckbox;
    }
}
