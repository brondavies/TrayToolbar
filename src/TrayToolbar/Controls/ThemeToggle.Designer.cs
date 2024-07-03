namespace TrayToolbar.Controls
{
    partial class ThemeToggle
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
            flowLayoutPanel1 = new FlowLayoutPanel();
            SystemRadioButton = new RadioButton();
            LightRadioButton = new RadioButton();
            DarkRadioButton = new RadioButton();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(SystemRadioButton);
            flowLayoutPanel1.Controls.Add(LightRadioButton);
            flowLayoutPanel1.Controls.Add(DarkRadioButton);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(182, 25);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // SystemRadioButton
            // 
            SystemRadioButton.AutoSize = true;
            SystemRadioButton.Checked = true;
            SystemRadioButton.Location = new Point(3, 3);
            SystemRadioButton.Name = "SystemRadioButton";
            SystemRadioButton.Size = new Size(63, 19);
            SystemRadioButton.TabIndex = 0;
            SystemRadioButton.TabStop = true;
            SystemRadioButton.Text = "System";
            SystemRadioButton.UseVisualStyleBackColor = true;
            // 
            // LightRadioButton
            // 
            LightRadioButton.AutoSize = true;
            LightRadioButton.Location = new Point(72, 3);
            LightRadioButton.Name = "LightRadioButton";
            LightRadioButton.Size = new Size(52, 19);
            LightRadioButton.TabIndex = 1;
            LightRadioButton.Text = "Light";
            LightRadioButton.UseVisualStyleBackColor = true;
            // 
            // DarkRadioButton
            // 
            DarkRadioButton.AutoSize = true;
            DarkRadioButton.Location = new Point(130, 3);
            DarkRadioButton.Name = "DarkRadioButton";
            DarkRadioButton.Size = new Size(49, 19);
            DarkRadioButton.TabIndex = 2;
            DarkRadioButton.Text = "Dark";
            DarkRadioButton.UseVisualStyleBackColor = true;
            // 
            // ThemeToggle
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(flowLayoutPanel1);
            Name = "ThemeToggle";
            Size = new Size(182, 25);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private RadioButton SystemRadioButton;
        private RadioButton LightRadioButton;
        private RadioButton DarkRadioButton;
    }
}
