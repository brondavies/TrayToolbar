using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace TrayToolbar
{
    internal class ConfigHelper
    {
        const string REGKEY_STARTUP = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string REGKEY_SHOWINTRAY = @"Software\Microsoft\Windows\CurrentVersion\RunNotification"; //StartupTNotiTrayToolbar (DWORD) = 1

        internal static bool GetStartupKey()
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_STARTUP);
            var val = key.GetValue("TrayToolbar");
            var value = (string?)val == Assembly.GetExecutingAssembly().Location;

            key.Close();
            return value;
        }

        internal static void SetStartupKey(bool value)
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_STARTUP);
            if (value)
            {
                key.SetValue("TrayToolbar", Assembly.GetExecutingAssembly().Location, RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue("TrayToolbar");
            }
            key.Close();
        }

        internal static void SetShowInTray()
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_SHOWINTRAY);
            key.SetValue("StartupTNotiTrayToolbar", 1, RegistryValueKind.DWord);
            key.Close();
        }

        internal static void RunConfig(bool runOnLogin)
        {
            var yes = runOnLogin ? "yes" : "";
            var startinfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Assembly.GetExecutingAssembly().Location,
                Arguments = $"--runonlogin {yes}",
                Verb = "runas"
            };
            Process.Start(startinfo);
        }
    }
}