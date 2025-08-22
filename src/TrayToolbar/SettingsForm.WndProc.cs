using System.Runtime.InteropServices;
using TrayToolbar.Extensions;
using Windows.Win32;

namespace TrayToolbar;

partial class SettingsForm
{
    const string Environment = "Environment";
    const string ImmersiveColorSet = "ImmersiveColorSet";
    const string WindowsThemeElement = "WindowsThemeElement";

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == PInvoke.WM_SETTINGCHANGE)
        {
            string param = Marshal.PtrToStringUni(m.LParam) ?? "";
            if (param == Environment)
            {
                ConfigHelper.RefreshProcessEnvironmentFromRegistry();
            }
            if (param == ImmersiveColorSet || param == WindowsThemeElement)
            {
                ThemeChangeMessageFilter.TriggerThemeChanged();
            }
        }
        base.WndProc(ref m);
    }
}
