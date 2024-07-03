using System.ComponentModel;
using TrayToolbar.Models;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar.Controls
{
    public partial class ThemeToggle : UserControl
    {

        public ThemeToggle()
        {
            InitializeComponent();
            SystemRadioButton.Text = R.System;
            LightRadioButton.Text = R.Light;
            DarkRadioButton.Text = R.Dark;

            SystemRadioButton.CheckedChanged += Changed;
            LightRadioButton.CheckedChanged += Changed;
            DarkRadioButton.CheckedChanged += Changed;
        }

        private void Changed(object? sender, EventArgs e)
        {
            ((EventHandler?)Events[nameof(ThemeChanged)])?.Invoke(this, EventArgs.Empty);
        }

        [DisplayName("Theme")]
        [Category("Appearance")]
        [Description("The selected theme")]
        public ThemeToggleEnum Theme
        {
            get
            {
                return DarkRadioButton.Checked
                    ? ThemeToggleEnum.DARK_THEME
                    : LightRadioButton.Checked
                        ? ThemeToggleEnum.LIGHT_THEME
                        : ThemeToggleEnum.SYSTEM_THEME;
            }
            set
            {
                var trigger = (Theme != value);
                DarkRadioButton.Checked = value == ThemeToggleEnum.DARK_THEME;
                LightRadioButton.Checked = value == ThemeToggleEnum.LIGHT_THEME;
                SystemRadioButton.Checked = value != ThemeToggleEnum.DARK_THEME && value != ThemeToggleEnum.LIGHT_THEME;
                if (trigger) 
                {
                    Changed(this, EventArgs.Empty);
                }
            }
        }

        [Category("Property Changed")]
        [Description("Occurs when the value of Theme is changed")]
        public event EventHandler? ThemeChanged
        {
            add => Events.AddHandler(nameof(ThemeChanged), value);
            remove => Events.RemoveHandler(nameof(ThemeChanged), value);
        }
    }
}
