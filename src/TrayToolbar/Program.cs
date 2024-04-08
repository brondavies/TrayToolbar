using System.Diagnostics;

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
            Application.Run(new SettingsForm());
        }

        internal static void Launch(string fileName)
        {
            if (File.Exists(fileName) || fileName.IsHttps())
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