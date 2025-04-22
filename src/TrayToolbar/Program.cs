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
            HotKeys.Enable(true);
            try
            {
                var form = new SettingsForm();
                if (Environment.GetCommandLineArgs().Contains("--show")) 
                {
                    form.Show();
                }
                Application.Run(form);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                File.WriteAllText(Path.Combine(ConfigHelper.ApplicationRoot, $"Error-{DateTime.Now:yyyyMMddHHmmss}.txt"), $"{e}");
            }
            HotKeys.UnregisterAll();
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