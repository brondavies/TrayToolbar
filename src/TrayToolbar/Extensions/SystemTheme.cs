using TrayToolbar.Controls;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using static Windows.Win32.PInvoke;

namespace TrayToolbar.Extensions
{
    internal class SystemTheme
    {
        const string REGKEY_THEMES = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        public unsafe static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsDarkModeSupported())
            {
                var form = Control.FromHandle(handle);
                ThemeColors.Current = enabled ? ThemeColors.Dark : ThemeColors.Light;
                SetThemeColors(form, enabled);
                var attribute = DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;
                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute((HWND)handle, attribute, &useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        public static void SetThemeColors(Control? control, bool dark)
        {
            if (control != null)
            {
                var backColor = ThemeColors.Current.DefaultBackColor;
                var foreColor = ThemeColors.Current.DefaultForeColor;
                if (control is CustomComboBox combo)
                {
                    backColor = ThemeColors.Current.ControlBackColor;
                    combo.BorderColor = ThemeColors.Current.ComboBorderColor;
                }
                if (control is ComboBox)
                {
                    backColor = ThemeColors.Current.ControlBackColor;
                }
                else if (control is TextBox textBox)
                {
                    backColor = ThemeColors.Current.ControlBackColor;
                    textBox.BorderStyle = ThemeColors.Current.TextBoxBorderStyle;
                }
                else if (control is Button button)
                {
                    backColor = ThemeColors.Current.ControlBackColor;
                    button.FlatStyle = ThemeColors.Current.ButtonFlatStyle;
                    button.FlatAppearance.BorderColor = ThemeColors.Current.ButtonBorderColor;
                }
                else if (control is LinkLabel label)
                {
                    label.LinkColor = ThemeColors.Current.LinkColor;
                }
                else if (control is ContextMenuStrip)
                {
                    backColor = ThemeColors.Current.MenuStripBackColor;
                }
                if (control is not PictureBox)
                {
                    control.BackColor = backColor;
                    control.ForeColor = foreColor;
                }
                foreach(Control child in control.Controls)
                {
                    SetThemeColors(child, dark);
                }
                control.GetType().GetMethod("OnThemeChanged")?.Invoke(control, [dark]);
            }
        }

        public static bool IsColorLight(Color clr)
        {
            return (((5 * clr.G) + (2 * clr.R) + clr.B) > (8 * 128));
        }

        public static bool IsDarkModeEnabled()
        {
            if (IsDarkModeSupported())
            {
                try
                {
                    var theme = (int?)Microsoft.Win32.Registry.GetValue(REGKEY_THEMES, "AppsUseLightTheme", null);
                    return theme == 0;
                }
                catch { }
            }
            return false;
        }

        public static bool IsDarkModeSupported() => IsWindows10OrGreater(18985);

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
    }
}
