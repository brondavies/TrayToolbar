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
            label3 = new Label();
            label1 = new Label();
            label2 = new Label();
            IgnoreFilesTextBox = new TextBox();
            RunOnLoginCheckbox = new CheckBox();
            AddFolderButton = new Button();
            FoldersLayout = new FlowLayoutPanel();
            ThemeToggleButton = new Controls.ThemeToggle();
            RightClickMenu = new ContextMenuStrip(components);
            LeftClickMenu = new ContextMenuStrip(components);
            FolderDialog = new FolderBrowserDialog();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            tableLayout.SuspendLayout();
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
            flowLayoutPanel1.Location = new Point(10, 233);
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
            CancelBtn.TabIndex = 0;
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
            NewVersionLabel.TabIndex = 2;
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
            tableLayout.Controls.Add(label3, 0, 3);
            tableLayout.Controls.Add(label1, 0, 0);
            tableLayout.Controls.Add(label2, 0, 2);
            tableLayout.Controls.Add(IgnoreFilesTextBox, 1, 2);
            tableLayout.Controls.Add(RunOnLoginCheckbox, 1, 4);
            tableLayout.Controls.Add(AddFolderButton, 1, 1);
            tableLayout.Controls.Add(FoldersLayout, 1, 0);
            tableLayout.Controls.Add(ThemeToggleButton, 1, 3);
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.Location = new Point(10, 10);
            tableLayout.Margin = new Padding(10);
            tableLayout.Name = "tableLayout";
            tableLayout.RowCount = 5;
            tableLayout.RowStyles.Add(new RowStyle());
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayout.Size = new Size(520, 223);
            tableLayout.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Top;
            label3.Location = new Point(3, 125);
            label3.Name = "label3";
            label3.Padding = new Padding(5);
            label3.Size = new Size(84, 25);
            label3.TabIndex = 12;
            label3.Text = "Theme";
            label3.TextAlign = ContentAlignment.MiddleRight;
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
            label2.Size = new Size(84, 25);
            label2.TabIndex = 1;
            label2.Text = "Exclude Files";
            label2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // IgnoreFilesTextBox
            // 
            IgnoreFilesTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            IgnoreFilesTextBox.BorderStyle = BorderStyle.FixedSingle;
            IgnoreFilesTextBox.Location = new Point(93, 78);
            IgnoreFilesTextBox.Name = "IgnoreFilesTextBox";
            IgnoreFilesTextBox.PlaceholderText = ".bak; .config; .dll; .ico; .ini";
            IgnoreFilesTextBox.Size = new Size(424, 23);
            IgnoreFilesTextBox.TabIndex = 3;
            // 
            // RunOnLoginCheckbox
            // 
            RunOnLoginCheckbox.AutoSize = true;
            RunOnLoginCheckbox.Location = new Point(93, 178);
            RunOnLoginCheckbox.Name = "RunOnLoginCheckbox";
            RunOnLoginCheckbox.Size = new Size(97, 19);
            RunOnLoginCheckbox.TabIndex = 8;
            RunOnLoginCheckbox.Text = "Run on log in";
            RunOnLoginCheckbox.UseVisualStyleBackColor = true;
            // 
            // AddFolderButton
            // 
            AddFolderButton.AutoSize = true;
            AddFolderButton.Image = (Image)resources.GetObject("AddFolderButton.Image");
            AddFolderButton.ImageAlign = ContentAlignment.MiddleLeft;
            AddFolderButton.Location = new Point(93, 28);
            AddFolderButton.Name = "AddFolderButton";
            AddFolderButton.Padding = new Padding(32, 0, 0, 0);
            AddFolderButton.Size = new Size(158, 29);
            AddFolderButton.TabIndex = 10;
            AddFolderButton.Text = "Add Folder";
            AddFolderButton.UseVisualStyleBackColor = true;
            AddFolderButton.Click += AddFolderButton_Click;
            // 
            // FoldersLayout
            // 
            FoldersLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FoldersLayout.AutoSize = true;
            FoldersLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FoldersLayout.FlowDirection = FlowDirection.TopDown;
            FoldersLayout.Location = new Point(90, 3);
            FoldersLayout.Margin = new Padding(0, 3, 0, 0);
            FoldersLayout.MaximumSize = new Size(450, 270);
            FoldersLayout.MinimumSize = new Size(420, 0);
            FoldersLayout.Name = "FoldersLayout";
            FoldersLayout.Size = new Size(430, 0);
            FoldersLayout.TabIndex = 11;
            FoldersLayout.WrapContents = false;
            // 
            // ThemeToggleButton
            // 
            ThemeToggleButton.AutoSize = true;
            ThemeToggleButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ThemeToggleButton.Location = new Point(93, 128);
            ThemeToggleButton.Name = "ThemeToggleButton";
            ThemeToggleButton.Size = new Size(182, 25);
            ThemeToggleButton.TabIndex = 13;
            ThemeToggleButton.Theme = Models.ThemeToggleEnum.SYSTEM_THEME;
            ThemeToggleButton.ThemeChanged += ThemeToggleButton_ThemeChanged;
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
            ClientSize = new Size(540, 282);
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
            Resize += SettingsForm_Resize;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            tableLayout.ResumeLayout(false);
            tableLayout.PerformLayout();
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
        private CheckBox RunOnLoginCheckbox;
        private LinkLabel NewVersionLabel;
        private TableLayoutPanel tableLayout;
        private Button AddFolderButton;
        private FlowLayoutPanel FoldersLayout;
        private Label label3;
        private Controls.ThemeToggle ThemeToggleButton;
    }
}
