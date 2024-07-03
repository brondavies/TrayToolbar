using System.Diagnostics;
using TrayToolbar.Extensions;

namespace TrayToolbar
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            ConfigHelper.SetShowInTray();
            ConfigHelper.MigrateConfiguration();
            DropDownMenuScrollWheelHandler.Enable(true);
            ThemeChangeMessageFilter.Enable(true);
            try
            {
                Application.Run(new SettingsForm());
            } catch (ObjectDisposedException) { }
        }

        internal static void Launch(string fileName)
        {
            if (File.Exists(fileName) || Directory.Exists(fileName) || fileName.IsHttps())
            {
                Process.Start(
                    new ProcessStartInfo(fileName)
                    {
                        UseShellExecute = true,
                    });
            }
        }

    }
}