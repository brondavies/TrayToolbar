using System.Reflection;

namespace TrayToolbar.Extensions;

public static class UiExtensions
{
    public static Icon GetIcon(this string path)
    {
        return ShellIcons.FetchIcon(path, false);
    }

    public static Bitmap GetImage(this string file, bool large = false)
    {
        return ShellIcons.FetchIconAsBitmap(file, large);
    }

    public static void ShowContextMenu(this NotifyIcon notifyIcon)
    {
        MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        mi?.Invoke(notifyIcon, null);
    }
}