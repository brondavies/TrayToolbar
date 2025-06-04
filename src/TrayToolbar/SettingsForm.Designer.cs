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
            LanguageLabel = new Label();
            row8col1placeholder = new Label();
            IncludeFilesTextBox = new TextBox();
            IncludeFileTypesLabel = new Label();
            FoldersLabel = new Label();
            ExcludeFileTypesLabel = new Label();
            ExcludeFoldersLabel = new Label();
            IgnoreFoldersTextBox = new TextBox();
            ThemeLabel = new Label();
            MenuFontSizeLabel = new Label();
            FoldersLayout = new FlowLayoutPanel();
            AddFolderButton = new Button();
            IgnoreFilesTextBox = new TextBox();
            ThemeToggleButton = new TrayToolbar.Controls.ThemeToggle();
            RunOnLoginCheckbox = new CheckBox();
            fontImageSizeTableLayout = new TableLayoutPanel();
            IconSizeLargeCheckbox = new RadioButton();
            IconSizeLabel = new Label();
            FontSizeInput = new NumericUpDown();
            IconSizeSmallCheckbox = new RadioButton();
            LanguageSelectList = new TrayToolbar.Controls.CustomComboBox();
            row2col1placeholder = new Label();
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
            flowLayoutPanel1.Location = new Point(10, 433);
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
            CancelBtn.TabIndex = 1;
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
            SaveButton.TabIndex = 0;
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
            NewVersionLabel.TabIndex = 0;
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
            tableLayout.Controls.Add(LanguageLabel, 0, 7);
            tableLayout.Controls.Add(row8col1placeholder, 0, 8);
            tableLayout.Controls.Add(IncludeFilesTextBox, 1, 2);
            tableLayout.Controls.Add(IncludeFileTypesLabel, 0, 2);
            tableLayout.Controls.Add(FoldersLabel, 0, 0);
            tableLayout.Controls.Add(ExcludeFileTypesLabel, 0, 3);
            tableLayout.Controls.Add(ExcludeFoldersLabel, 0, 4);
            tableLayout.Controls.Add(IgnoreFoldersTextBox, 1, 4);
            tableLayout.Controls.Add(ThemeLabel, 0, 5);
            tableLayout.Controls.Add(MenuFontSizeLabel, 0, 6);
            tableLayout.Controls.Add(FoldersLayout, 1, 0);
            tableLayout.Controls.Add(AddFolderButton, 1, 1);
            tableLayout.Controls.Add(IgnoreFilesTextBox, 1, 3);
            tableLayout.Controls.Add(ThemeToggleButton, 1, 5);
            tableLayout.Controls.Add(RunOnLoginCheckbox, 1, 8);
            tableLayout.Controls.Add(fontImageSizeTableLayout, 1, 6);
            tableLayout.Controls.Add(LanguageSelectList, 1, 7);
            tableLayout.Controls.Add(row2col1placeholder, 0, 1);
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.Location = new Point(10, 10);
            tableLayout.Margin = new Padding(10);
            tableLayout.Name = "tableLayout";
            tableLayout.RowCount = 9;
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.Size = new Size(520, 423);
            tableLayout.TabIndex = 3;
            // 
            // LanguageLabel
            // 
            LanguageLabel.AutoSize = true;
            LanguageLabel.Dock = DockStyle.Top;
            LanguageLabel.Location = new Point(3, 325);
            LanguageLabel.Name = "LanguageLabel";
            LanguageLabel.Padding = new Padding(5);
            LanguageLabel.Size = new Size(107, 25);
            LanguageLabel.TabIndex = 18;
            LanguageLabel.Text = "Language";
            LanguageLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // row8col1placeholder
            // 
            row8col1placeholder.AutoSize = true;
            row8col1placeholder.Dock = DockStyle.Top;
            row8col1placeholder.Location = new Point(3, 375);
            row8col1placeholder.Name = "row8col1placeholder";
            row8col1placeholder.Padding = new Padding(5);
            row8col1placeholder.Size = new Size(107, 25);
            row8col1placeholder.TabIndex = 17;
            row8col1placeholder.TextAlign = ContentAlignment.MiddleRight;
            // 
            // IncludeFilesTextBox
            // 
            IncludeFilesTextBox.BorderStyle = BorderStyle.FixedSingle;
            IncludeFilesTextBox.Dock = DockStyle.Top;
            IncludeFilesTextBox.Location = new Point(116, 78);
            IncludeFilesTextBox.Name = "IncludeFilesTextBox";
            IncludeFilesTextBox.PlaceholderText = ".*";
            IncludeFilesTextBox.Size = new Size(414, 23);
            IncludeFilesTextBox.TabIndex = 2;
            // 
            // IncludeFileTypesLabel
            // 
            IncludeFileTypesLabel.AutoSize = true;
            IncludeFileTypesLabel.Dock = DockStyle.Top;
            IncludeFileTypesLabel.Location = new Point(3, 75);
            IncludeFileTypesLabel.Name = "IncludeFileTypesLabel";
            IncludeFileTypesLabel.Padding = new Padding(5);
            IncludeFileTypesLabel.Size = new Size(107, 25);
            IncludeFileTypesLabel.TabIndex = 16;
            IncludeFileTypesLabel.Text = "Include file types";
            IncludeFileTypesLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // FoldersLabel
            // 
            FoldersLabel.AutoSize = true;
            FoldersLabel.Dock = DockStyle.Top;
            FoldersLabel.Location = new Point(3, 0);
            FoldersLabel.Name = "FoldersLabel";
            FoldersLabel.Padding = new Padding(5);
            FoldersLabel.Size = new Size(107, 25);
            FoldersLabel.TabIndex = 0;
            FoldersLabel.Text = "Folders";
            FoldersLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ExcludeFileTypesLabel
            // 
            ExcludeFileTypesLabel.AutoSize = true;
            ExcludeFileTypesLabel.Dock = DockStyle.Top;
            ExcludeFileTypesLabel.Location = new Point(3, 125);
            ExcludeFileTypesLabel.Name = "ExcludeFileTypesLabel";
            ExcludeFileTypesLabel.Padding = new Padding(5);
            ExcludeFileTypesLabel.Size = new Size(107, 25);
            ExcludeFileTypesLabel.TabIndex = 1;
            ExcludeFileTypesLabel.Text = "Exclude file types";
            ExcludeFileTypesLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ExcludeFoldersLabel
            // 
            ExcludeFoldersLabel.AutoSize = true;
            ExcludeFoldersLabel.Dock = DockStyle.Top;
            ExcludeFoldersLabel.Location = new Point(3, 175);
            ExcludeFoldersLabel.Name = "ExcludeFoldersLabel";
            ExcludeFoldersLabel.Padding = new Padding(5);
            ExcludeFoldersLabel.Size = new Size(107, 25);
            ExcludeFoldersLabel.TabIndex = 14;
            ExcludeFoldersLabel.Text = "Exclude folders";
            ExcludeFoldersLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // IgnoreFoldersTextBox
            // 
            IgnoreFoldersTextBox.BorderStyle = BorderStyle.FixedSingle;
            IgnoreFoldersTextBox.Dock = DockStyle.Top;
            IgnoreFoldersTextBox.Location = new Point(116, 178);
            IgnoreFoldersTextBox.Name = "IgnoreFoldersTextBox";
            IgnoreFoldersTextBox.PlaceholderText = ".git; .github";
            IgnoreFoldersTextBox.Size = new Size(414, 23);
            IgnoreFoldersTextBox.TabIndex = 4;
            // 
            // ThemeLabel
            // 
            ThemeLabel.AutoSize = true;
            ThemeLabel.Dock = DockStyle.Top;
            ThemeLabel.Location = new Point(3, 225);
            ThemeLabel.Name = "ThemeLabel";
            ThemeLabel.Padding = new Padding(5);
            ThemeLabel.Size = new Size(107, 25);
            ThemeLabel.TabIndex = 12;
            ThemeLabel.Text = "Theme";
            ThemeLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // MenuFontSizeLabel
            // 
            MenuFontSizeLabel.AutoSize = true;
            MenuFontSizeLabel.Dock = DockStyle.Top;
            MenuFontSizeLabel.Location = new Point(3, 275);
            MenuFontSizeLabel.Name = "MenuFontSizeLabel";
            MenuFontSizeLabel.Padding = new Padding(5);
            MenuFontSizeLabel.Size = new Size(107, 25);
            MenuFontSizeLabel.TabIndex = 13;
            MenuFontSizeLabel.Text = "Menu Font Size";
            MenuFontSizeLabel.TextAlign = ContentAlignment.MiddleRight;
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
            FoldersLayout.TabIndex = 0;
            FoldersLayout.WrapContents = false;
            // 
            // AddFolderButton
            // 
            AddFolderButton.AutoSize = true;
            AddFolderButton.Image = Resources.Resources.add_small;
            AddFolderButton.ImageAlign = ContentAlignment.MiddleLeft;
            AddFolderButton.Location = new Point(116, 28);
            AddFolderButton.Name = "AddFolderButton";
            AddFolderButton.Padding = new Padding(32, 0, 0, 0);
            AddFolderButton.Size = new Size(158, 30);
            AddFolderButton.TabIndex = 1;
            AddFolderButton.Text = "Add Folder";
            AddFolderButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            AddFolderButton.UseVisualStyleBackColor = true;
            AddFolderButton.Click += AddFolderButton_Click;
            // 
            // IgnoreFilesTextBox
            // 
            IgnoreFilesTextBox.BorderStyle = BorderStyle.FixedSingle;
            IgnoreFilesTextBox.Dock = DockStyle.Top;
            IgnoreFilesTextBox.Location = new Point(116, 128);
            IgnoreFilesTextBox.Name = "IgnoreFilesTextBox";
            IgnoreFilesTextBox.PlaceholderText = ".bak; .config; .dll; .ico; .ini";
            IgnoreFilesTextBox.Size = new Size(414, 23);
            IgnoreFilesTextBox.TabIndex = 3;
            // 
            // ThemeToggleButton
            // 
            ThemeToggleButton.AutoSize = true;
            ThemeToggleButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ThemeToggleButton.Location = new Point(116, 228);
            ThemeToggleButton.Name = "ThemeToggleButton";
            ThemeToggleButton.Size = new Size(182, 25);
            ThemeToggleButton.TabIndex = 5;
            ThemeToggleButton.Theme = Models.ThemeToggleEnum.SYSTEM_THEME;
            ThemeToggleButton.ThemeChanged += ThemeToggleButton_ThemeChanged;
            // 
            // RunOnLoginCheckbox
            // 
            RunOnLoginCheckbox.AutoSize = true;
            RunOnLoginCheckbox.Location = new Point(116, 378);
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
            fontImageSizeTableLayout.Controls.Add(IconSizeLabel, 1, 0);
            fontImageSizeTableLayout.Controls.Add(FontSizeInput, 0, 0);
            fontImageSizeTableLayout.Controls.Add(IconSizeSmallCheckbox, 2, 0);
            fontImageSizeTableLayout.Dock = DockStyle.Fill;
            fontImageSizeTableLayout.Location = new Point(116, 278);
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
            IconSizeLargeCheckbox.TabIndex = 3;
            IconSizeLargeCheckbox.Text = "Large";
            IconSizeLargeCheckbox.UseVisualStyleBackColor = true;
            IconSizeLargeCheckbox.CheckedChanged += IconSizeLargeCheckbox_CheckedChanged;
            // 
            // IconSizeLabel
            // 
            IconSizeLabel.AutoSize = true;
            IconSizeLabel.Dock = DockStyle.Top;
            IconSizeLabel.Location = new Point(106, 0);
            IconSizeLabel.Name = "IconSizeLabel";
            IconSizeLabel.Padding = new Padding(5);
            IconSizeLabel.Size = new Size(97, 25);
            IconSizeLabel.TabIndex = 1;
            IconSizeLabel.Text = "Icon Size";
            IconSizeLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // FontSizeInput
            // 
            FontSizeInput.Location = new Point(3, 3);
            FontSizeInput.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            FontSizeInput.Minimum = new decimal(new int[] { 9, 0, 0, 0 });
            FontSizeInput.Name = "FontSizeInput";
            FontSizeInput.Size = new Size(50, 23);
            FontSizeInput.TabIndex = 0;
            FontSizeInput.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // IconSizeSmallCheckbox
            // 
            IconSizeSmallCheckbox.AutoSize = true;
            IconSizeSmallCheckbox.Checked = true;
            IconSizeSmallCheckbox.Location = new Point(209, 3);
            IconSizeSmallCheckbox.Name = "IconSizeSmallCheckbox";
            IconSizeSmallCheckbox.Size = new Size(54, 19);
            IconSizeSmallCheckbox.TabIndex = 2;
            IconSizeSmallCheckbox.TabStop = true;
            IconSizeSmallCheckbox.Text = "Small";
            IconSizeSmallCheckbox.UseVisualStyleBackColor = true;
            IconSizeSmallCheckbox.CheckedChanged += IconSizeSmallCheckbox_CheckedChanged;
            // 
            // LanguageSelectList
            // 
            LanguageSelectList.BorderColor = SystemColors.WindowFrame;
            LanguageSelectList.DropDownStyle = ComboBoxStyle.DropDownList;
            LanguageSelectList.FormattingEnabled = true;
            LanguageSelectList.Items.AddRange(new object[] { "(System)", "English", "Español", "Français", "Deutsch", "Português", "Italiano", "日本語", "中文", "Русский", "한국어" });
            LanguageSelectList.Location = new Point(116, 328);
            LanguageSelectList.Name = "LanguageSelectList";
            LanguageSelectList.Size = new Size(121, 23);
            LanguageSelectList.TabIndex = 19;
            LanguageSelectList.SelectedIndexChanged += LanguageSelectList_SelectedIndexChanged;
            // 
            // row2col1placeholder
            // 
            row2col1placeholder.AutoSize = true;
            row2col1placeholder.Dock = DockStyle.Top;
            row2col1placeholder.Location = new Point(3, 25);
            row2col1placeholder.Name = "row2col1placeholder";
            row2col1placeholder.Size = new Size(107, 15);
            row2col1placeholder.TabIndex = 15;
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
            ClientSize = new Size(540, 482);
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
        private Label FoldersLabel;
        private Label ExcludeFileTypesLabel;
        private TextBox IgnoreFilesTextBox;
        private Button CancelBtn;
        private Button SaveButton;
        private FolderBrowserDialog FolderDialog;
        private LinkLabel NewVersionLabel;
        private TableLayoutPanel tableLayout;
        private Button AddFolderButton;
        private FlowLayoutPanel FoldersLayout;
        private Label ThemeLabel;
        private Controls.ThemeToggle ThemeToggleButton;
        private Label ExcludeFoldersLabel;
        private Label MenuFontSizeLabel;
        private TextBox IgnoreFoldersTextBox;
        private CheckBox RunOnLoginCheckbox;
        private TableLayoutPanel fontImageSizeTableLayout;
        private NumericUpDown FontSizeInput;
        private Label IconSizeLabel;
        private RadioButton IconSizeLargeCheckbox;
        private RadioButton IconSizeSmallCheckbox;
        private Label row2col1placeholder;
        private TextBox IncludeFilesTextBox;
        private Label IncludeFileTypesLabel;
        private Label LanguageLabel;
        private Label row8col1placeholder;
        private TrayToolbar.Controls.CustomComboBox LanguageSelectList;
    }
}
