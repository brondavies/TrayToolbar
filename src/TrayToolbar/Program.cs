using System.Security.Principal;

using TrayToolbar.Extensions;
using TrayToolbar.Services;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        ConfigHelper.ReportError = message => MessageBox.Show(message, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

        // Handle automatic update
        if (UpdateHelper.ProcessUpdate())
        {
            return;
        }

        // Single-instance (per-user) enforcement
        if (!EnsureSingleInstance())
        {
            // Notify existing instance to show the SettingsForm
            InstanceMessages.NotifyExistingInstance();
            return;
        }
        UpdateHelper.CleanupStaleUpdateDirectories();

        if (ConfigHelper.SupportsToastNotifications)
        {
            NotificationsHelper.Activate();
        }
        ConfigHelper.SetShowInTray();
        ConfigHelper.MigrateConfiguration();
        DropDownMenuScrollWheelHandler.Enable(true);
        ThemeChangeMessageFilter.Enable(true);
        HotKeys.Enable(true);
        try
        {
            var form = new SettingsForm();
            var args = Environment.GetCommandLineArgs();
            if (args.Contains("--show"))
            {
                form.NewVersionMessage = args.Contains("--newversion");
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
        LaunchLogger.Flush();
    }

    internal static bool Launch(string fileName)
    {
        return Launcher.Launch(fileName);
    }

    #region Single Instance

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

    #endregion
}