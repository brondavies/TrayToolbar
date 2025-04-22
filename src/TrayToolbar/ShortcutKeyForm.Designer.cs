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
            ButtonCancel = new Button();
            ButtonOK = new Button();
            MessageLabel = new Label();
            HotkeyValue = new Label();
            SuspendLayout();
            // 
            // ButtonCancel
            // 
            ButtonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ButtonCancel.DialogResult = DialogResult.Cancel;
            ButtonCancel.Location = new Point(128, 162);
            ButtonCancel.Name = "ButtonCancel";
            ButtonCancel.Size = new Size(100, 27);
            ButtonCancel.TabIndex = 1;
            ButtonCancel.Text = "Cancel";
            ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonOK
            // 
            ButtonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ButtonOK.DialogResult = DialogResult.OK;
            ButtonOK.Location = new Point(12, 162);
            ButtonOK.Name = "ButtonOK";
            ButtonOK.Size = new Size(100, 27);
            ButtonOK.TabIndex = 0;
            ButtonOK.Text = "OK";
            ButtonOK.UseVisualStyleBackColor = true;
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
            AcceptButton = ButtonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = ButtonCancel;
            ClientSize = new Size(240, 201);
            Controls.Add(HotkeyValue);
            Controls.Add(MessageLabel);
            Controls.Add(ButtonOK);
            Controls.Add(ButtonCancel);
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

        private Button ButtonCancel;
        private Button ButtonOK;
        private Label MessageLabel;
        private Label HotkeyValue;
    }
}