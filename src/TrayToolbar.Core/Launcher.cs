using System.Diagnostics;

using TrayToolbar.Services;

namespace TrayToolbar;

internal static class Launcher
{
    internal static bool Launch(string fileName)
    {
        if (ShortcutTargetResolver.TryCreateShortcutStartInfo(fileName, out var shortcutStartInfo)
            && TryLaunch(shortcutStartInfo))
        {
            return true;
        }

        return TryLaunch(new ProcessStartInfo(fileName)
        {
            UseShellExecute = true,
        });
    }

    static bool TryLaunch(ProcessStartInfo startInfo)
    {
        try
        {
            ConfigHelper.ProcessLauncher.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
