using TrayToolbar.Extensions;

namespace TrayToolbar.Models;

internal static class FolderConfigExtensions
{
    internal static Bitmap? GetIcon(this FolderConfig folder, bool small = false)
    {
        if (folder.Icon.HasValue() && File.Exists(folder.Icon.ToLocalPath()))
        {
            try
            {
                return System.Drawing.Icon.ExtractIcon(folder.Icon.ToLocalPath(), folder.IconIndex, small)?.ToBitmap()
                    ?? GetDefaultIcon(folder, small);
            }
            catch { }
        }
        return GetDefaultIcon(folder, small);
    }

    private static Bitmap? GetDefaultIcon(FolderConfig folder, bool small)
    {
        return folder.Name?.ToLocalPath().GetImage(!small);
    }
}