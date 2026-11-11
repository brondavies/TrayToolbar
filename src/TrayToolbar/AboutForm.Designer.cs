namespace TrayToolbar
{
    partial class AboutForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            headerLayout = new TableLayoutPanel();
            AppIcon = new PictureBox();
            TitleLabel = new Label();
            VersionLabel = new Label();
            RuntimeLabel = new Label();
            CopyrightLabel = new Label();
            GitHubLink = new LinkLabel();
            IssuesLink = new LinkLabel();
            SponsorLink = new LinkLabel();
            StatusLabel = new Label();
            LastCheckedLabel = new Label();
            buttonsLayout = new FlowLayoutPanel();
            OkButton = new Button();
            UpdateNowButton = new Button();
            CheckUpdatesButton = new Button();
            mainLayout.SuspendLayout();
            headerLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AppIcon).BeginInit();
            buttonsLayout.SuspendLayout();
            SuspendLayout();
            //
            // mainLayout
            //
            mainLayout.AutoSize = true;
            mainLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle());
            mainLayout.Controls.Add(headerLayout, 0, 0);
            mainLayout.Controls.Add(SponsorLink, 0, 1);
            mainLayout.Controls.Add(StatusLabel, 0, 2);
            mainLayout.Controls.Add(LastCheckedLabel, 0, 3);
            mainLayout.Controls.Add(buttonsLayout, 0, 4);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(12, 12);
            mainLayout.Name = "mainLayout";
            mainLayout.RowCount = 5;
            mainLayout.RowStyles.Add(new RowStyle());
            mainLayout.RowStyles.Add(new RowStyle());
            mainLayout.RowStyles.Add(new RowStyle());
            mainLayout.RowStyles.Add(new RowStyle());
            mainLayout.RowStyles.Add(new RowStyle());
            mainLayout.Size = new Size(456, 300);
            mainLayout.TabIndex = 0;
            //
            // headerLayout
            //
            headerLayout.AutoSize = true;
            headerLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            headerLayout.ColumnCount = 2;
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
            headerLayout.ColumnStyles.Add(new ColumnStyle());
            headerLayout.Controls.Add(AppIcon, 0, 0);
            headerLayout.Controls.Add(TitleLabel, 1, 0);
            headerLayout.Controls.Add(VersionLabel, 1, 1);
            headerLayout.Controls.Add(RuntimeLabel, 1, 2);
            headerLayout.Controls.Add(CopyrightLabel, 1, 3);
            headerLayout.Controls.Add(GitHubLink, 1, 4);
            headerLayout.Controls.Add(IssuesLink, 1, 5);
            headerLayout.Location = new Point(3, 3);
            headerLayout.Name = "headerLayout";
            headerLayout.RowCount = 6;
            headerLayout.RowStyles.Add(new RowStyle());
            headerLayout.RowStyles.Add(new RowStyle());
            headerLayout.RowStyles.Add(new RowStyle());
            headerLayout.RowStyles.Add(new RowStyle());
            headerLayout.RowStyles.Add(new RowStyle());
            headerLayout.RowStyles.Add(new RowStyle());
            headerLayout.Size = new Size(300, 150);
            headerLayout.TabIndex = 0;
            //
            // AppIcon
            //
            AppIcon.Image = Resources.Images.TrayIcon;
            AppIcon.Location = new Point(3, 3);
            AppIcon.Name = "AppIcon";
            headerLayout.SetRowSpan(AppIcon, 6);
            AppIcon.Size = new Size(64, 64);
            AppIcon.SizeMode = PictureBoxSizeMode.Zoom;
            AppIcon.TabIndex = 0;
            AppIcon.TabStop = false;
            //
            // TitleLabel
            //
            TitleLabel.AutoSize = true;
            TitleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            TitleLabel.Location = new Point(75, 0);
            TitleLabel.Name = "TitleLabel";
            TitleLabel.Padding = new Padding(0, 0, 0, 4);
            TitleLabel.Size = new Size(120, 29);
            TitleLabel.TabIndex = 1;
            TitleLabel.Text = "TrayToolbar";
            //
            // VersionLabel
            //
            VersionLabel.AutoSize = true;
            VersionLabel.Location = new Point(75, 29);
            VersionLabel.Name = "VersionLabel";
            VersionLabel.Size = new Size(100, 15);
            VersionLabel.TabIndex = 2;
            VersionLabel.Text = "Version 1.9.0 (x64)";
            //
            // RuntimeLabel
            //
            RuntimeLabel.AutoSize = true;
            RuntimeLabel.Location = new Point(75, 44);
            RuntimeLabel.Name = "RuntimeLabel";
            RuntimeLabel.Size = new Size(70, 15);
            RuntimeLabel.TabIndex = 3;
            RuntimeLabel.Text = ".NET 10.0";
            //
            // CopyrightLabel
            //
            CopyrightLabel.AutoSize = true;
            CopyrightLabel.Location = new Point(75, 59);
            CopyrightLabel.Name = "CopyrightLabel";
            CopyrightLabel.Padding = new Padding(0, 0, 0, 8);
            CopyrightLabel.Size = new Size(90, 23);
            CopyrightLabel.TabIndex = 4;
            CopyrightLabel.Text = "© Brontech, LLC";
            //
            // GitHubLink
            //
            GitHubLink.AutoSize = true;
            GitHubLink.Location = new Point(75, 82);
            GitHubLink.Name = "GitHubLink";
            GitHubLink.Size = new Size(120, 15);
            GitHubLink.TabIndex = 5;
            GitHubLink.TabStop = true;
            GitHubLink.Text = "TrayToolbar on GitHub";
            GitHubLink.LinkClicked += GitHubLink_LinkClicked;
            //
            // IssuesLink
            //
            IssuesLink.AutoSize = true;
            IssuesLink.Location = new Point(75, 97);
            IssuesLink.Name = "IssuesLink";
            IssuesLink.Size = new Size(90, 15);
            IssuesLink.TabIndex = 6;
            IssuesLink.TabStop = true;
            IssuesLink.Text = "Report an issue";
            IssuesLink.LinkClicked += IssuesLink_LinkClicked;
            //
            // SponsorLink
            //
            SponsorLink.AutoSize = true;
            SponsorLink.Location = new Point(3, 156);
            SponsorLink.Margin = new Padding(3, 12, 3, 12);
            SponsorLink.MaximumSize = new Size(450, 0);
            SponsorLink.Name = "SponsorLink";
            SponsorLink.Size = new Size(450, 30);
            SponsorLink.TabIndex = 1;
            SponsorLink.Text = "Free code signing on Windows provided by SignPath.io, certificate by SignPath Foundation";
            SponsorLink.LinkClicked += SponsorLink_LinkClicked;
            //
            // StatusLabel
            //
            StatusLabel.AutoSize = true;
            StatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            StatusLabel.Location = new Point(3, 198);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(0, 15);
            StatusLabel.TabIndex = 2;
            //
            // LastCheckedLabel
            //
            LastCheckedLabel.AutoSize = true;
            LastCheckedLabel.Location = new Point(3, 213);
            LastCheckedLabel.Name = "LastCheckedLabel";
            LastCheckedLabel.Size = new Size(0, 15);
            LastCheckedLabel.TabIndex = 3;
            LastCheckedLabel.Visible = false;
            //
            // buttonsLayout
            //
            buttonsLayout.AutoSize = true;
            buttonsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonsLayout.Controls.Add(OkButton);
            buttonsLayout.Controls.Add(UpdateNowButton);
            buttonsLayout.Controls.Add(CheckUpdatesButton);
            buttonsLayout.Dock = DockStyle.Fill;
            buttonsLayout.FlowDirection = FlowDirection.RightToLeft;
            buttonsLayout.Location = new Point(0, 231);
            buttonsLayout.Margin = new Padding(0, 12, 0, 0);
            buttonsLayout.Name = "buttonsLayout";
            buttonsLayout.Size = new Size(456, 33);
            buttonsLayout.TabIndex = 4;
            //
            // OkButton
            //
            OkButton.AutoSize = true;
            OkButton.DialogResult = DialogResult.OK;
            OkButton.Location = new Point(353, 3);
            OkButton.MinimumSize = new Size(100, 27);
            OkButton.Name = "OkButton";
            OkButton.Size = new Size(100, 27);
            OkButton.TabIndex = 0;
            OkButton.Text = "OK";
            OkButton.UseVisualStyleBackColor = true;
            //
            // UpdateNowButton
            //
            UpdateNowButton.AutoSize = true;
            UpdateNowButton.Enabled = false;
            UpdateNowButton.Location = new Point(247, 3);
            UpdateNowButton.MinimumSize = new Size(100, 27);
            UpdateNowButton.Name = "UpdateNowButton";
            UpdateNowButton.Size = new Size(100, 27);
            UpdateNowButton.TabIndex = 1;
            UpdateNowButton.Text = "Update now";
            UpdateNowButton.UseVisualStyleBackColor = true;
            UpdateNowButton.Click += UpdateNowButton_Click;
            //
            // CheckUpdatesButton
            //
            CheckUpdatesButton.AutoSize = true;
            CheckUpdatesButton.Location = new Point(121, 3);
            CheckUpdatesButton.MinimumSize = new Size(120, 27);
            CheckUpdatesButton.Name = "CheckUpdatesButton";
            CheckUpdatesButton.Size = new Size(120, 27);
            CheckUpdatesButton.TabIndex = 2;
            CheckUpdatesButton.Text = "Check for updates";
            CheckUpdatesButton.UseVisualStyleBackColor = true;
            CheckUpdatesButton.Click += CheckUpdatesButton_Click;
            //
            // AboutForm
            //
            AcceptButton = OkButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CancelButton = OkButton;
            ClientSize = new Size(480, 324);
            Controls.Add(mainLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            Padding = new Padding(12);
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            headerLayout.ResumeLayout(false);
            headerLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)AppIcon).EndInit();
            buttonsLayout.ResumeLayout(false);
            buttonsLayout.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel headerLayout;
        private PictureBox AppIcon;
        private Label TitleLabel;
        private Label VersionLabel;
        private Label RuntimeLabel;
        private Label CopyrightLabel;
        private LinkLabel GitHubLink;
        private LinkLabel IssuesLink;
        private LinkLabel SponsorLink;
        private Label StatusLabel;
        private Label LastCheckedLabel;
        private FlowLayoutPanel buttonsLayout;
        private Button OkButton;
        private Button UpdateNowButton;
        private Button CheckUpdatesButton;
    }
}
