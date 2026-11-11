using System.Diagnostics;

namespace TrayToolbar;

internal static class Launcher
{
    // Shortcuts are shell-executed like everything else, so Windows applies the same
    // environment-variable expansion, run-as-administrator flag and MSI resolution as Explorer.
    internal static bool Launch(string fileName)
    {
        try
        {
            ConfigHelper.ProcessLauncher.Start(new ProcessStartInfo(fileName)
            {
                UseShellExecute = true,
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
