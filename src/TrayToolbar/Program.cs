using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using TrayToolbar.Extensions;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TrayToolbar
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Single-instance (per-user) enforcement
            if (!EnsureSingleInstance())
            {
                // Notify existing instance to show the SettingsForm
                NotifyExistingInstance();
                return;
            }

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

        #region Single Instance

        internal static readonly uint WM_SHOWSETTINGSFORM = PInvoke.RegisterWindowMessage("TrayToolbar.ShowSettingsForm.729a0e10-3131-4e69-ba45-23660c5a91bf");
        private static Mutex? _mutex;

        internal static bool EnsureSingleInstance()
        {
            var sid = WindowsIdentity.GetCurrent()?.User?.Value ?? Environment.UserName;
            var name = $"Local\\TrayToolbar_{sid}";
            _mutex = new Mutex(initiallyOwned: true, name, out bool created);
            if (!created)
            {
                // Release immediately if not owner
                try { _mutex.Dispose(); } catch { }
            }
            return created;
        }

        internal static void NotifyExistingInstance()
        {
            PInvoke.PostMessage(HWND.HWND_BROADCAST, WM_SHOWSETTINGSFORM, 0, 0);
        }

        #endregion
    }
}