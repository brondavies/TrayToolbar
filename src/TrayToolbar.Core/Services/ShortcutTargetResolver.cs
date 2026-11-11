using TrayToolbar.Extensions;

namespace TrayToolbar.Services;

internal static class ShortcutTargetResolver
{
    internal static bool TryResolveFileShortcutTarget(string shortcutPath, out string targetPath, Func<string, string>? shortcutResolver = null)
    {
        targetPath = string.Empty;
        if (!shortcutPath.HasValue() || !shortcutPath.FileExtension().Is(".lnk"))
        {
            return false;
        }

        try
        {
            var resolvedPath = (shortcutResolver ?? (path => path.ResolveShortcutTarget()))(shortcutPath);
            if (!resolvedPath.HasValue() || resolvedPath.Is(shortcutPath))
            {
                return false;
            }

            targetPath = resolvedPath.ToLocalPath();
            return targetPath.HasValue();
        }
        catch
        {
            return false;
        }
    }
}