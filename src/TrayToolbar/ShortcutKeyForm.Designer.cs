namespace TrayToolbar
{
    partial class ShortcutKeyForm
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
            CancelButton = new Button();
            OKButton = new Button();
            MessageLabel = new Label();
            HotkeyValue = new Label();
            SuspendLayout();
            // 
            // CancelButton
            // 
            CancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CancelButton.DialogResult = DialogResult.Cancel;
            CancelButton.Location = new Point(128, 162);
            CancelButton.Name = "CancelButton";
            CancelButton.Size = new Size(100, 27);
            CancelButton.TabIndex = 1;
            CancelButton.Text = "Cancel";
            CancelButton.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Location = new Point(12, 162);
            OKButton.Name = "OKButton";
            OKButton.Size = new Size(100, 27);
            OKButton.TabIndex = 0;
            OKButton.Text = "OK";
            OKButton.UseVisualStyleBackColor = true;
            // 
            // MessageLabel
            // 
            MessageLabel.Dock = DockStyle.Top;
            MessageLabel.Location = new Point(0, 0);
            MessageLabel.Margin = new Padding(0);
            MessageLabel.Name = "MessageLabel";
            MessageLabel.Padding = new Padding(10, 20, 10, 10);
            MessageLabel.Size = new Size(240, 72);
            MessageLabel.TabIndex = 2;
            MessageLabel.Text = "Press the keys to use as the shortcut key";
            MessageLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // HotkeyValue
            // 
            HotkeyValue.Dock = DockStyle.Top;
            HotkeyValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            HotkeyValue.Location = new Point(0, 72);
            HotkeyValue.Name = "HotkeyValue";
            HotkeyValue.Padding = new Padding(5);
            HotkeyValue.Size = new Size(240, 33);
            HotkeyValue.TabIndex = 3;
            HotkeyValue.Text = "CTRL + SHIFT + F1";
            HotkeyValue.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ShortcutKeyForm
            // 
            AcceptButton = OKButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = CancelButton;
            ClientSize = new Size(240, 201);
            Controls.Add(HotkeyValue);
            Controls.Add(MessageLabel);
            Controls.Add(OKButton);
            Controls.Add(CancelButton);
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ShortcutKeyForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Shortcut Key";
            KeyDown += ShortcutKeyForm_KeyDown;
            ResumeLayout(false);
        }

        #endregion

        private Button CancelButton;
        private Button OKButton;
        private Label MessageLabel;
        private Label HotkeyValue;
    }
}