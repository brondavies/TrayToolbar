using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TrayToolbar.Extensions
{
    public static class ShellIcons
    {
        public static unsafe Bitmap FetchIconAsBitmap(string path, bool large)
        {
            var icon = FetchIcon(path, large);
            var bmp = new Bitmap(icon.Width, icon.Height);
            using (Graphics gp = Graphics.FromImage(bmp))
            {
                gp.Clear(Color.Transparent);
                gp.DrawIcon(icon, new Rectangle(0, 0, icon.Width, icon.Height));
            }
            PInvoke.DestroyIcon((HICON)icon.Handle);
            return bmp;
        }

        public static unsafe Icon FetchIcon(string path, bool large = false)
        {
            if (Path.GetExtension(path).Is(".url"))
            {
                return GetIconFromUrlFile(path, large);
            }
            return ExtractFromPath(path, large);
        }

        private static Icon GetIconFromUrlFile(string path, bool large)
        {
            var fi = new FileInfo(path);
            if (fi.Exists && fi.Length < 1_000_000) // limit to files under 1MB to guard against potential memory overuse
            {
                var dict = new Dictionary<string, string>();
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        dict.Add(parts[0].ToLowerInvariant(), parts[1]);
                    }
                }
                dict.TryGetValue("url", out var url);
                dict.TryGetValue("iconfile", out var iconFile);
                dict.TryGetValue("iconindex", out var iconIndex);
                if (iconFile.HasValue() && File.Exists(iconFile.ToLocalPath()))
                {
                    _ = int.TryParse(iconIndex, out int id); // will be 0 if parse fails
                    var icon = Icon.ExtractIcon(iconFile, id, !large);
                    if (icon != null) return icon;
                }
                //no iconfile was set or it didn't contain an icon, check the url
                if (url.HasValue())
                {
                    //special case urls
                    if (url.StartsWith("ms-settings:"))
                    {
                        //TODO: https://github.com/dotnet/winforms/issues/12447
                        //return SystemIcons.GetStockIcon(StockIconId.Settings, large ? StockIconOptions.Default : StockIconOptions.SmallIcon);
                        return Icon.ExtractIcon(Shell32Dll, SETTINGS_ICON_INDEX, !large) ?? SystemIcons.Application;
                    }
                    //try getting an icon from the target if it's a local path
                    if (File.Exists(url.ToLocalPath()))
                    {
                        return ExtractFromPath(url, large);
                    }
                }
            }
            return ExtractFromPath(path, large);
        }

        static readonly int SETTINGS_ICON_INDEX = ConfigHelper.WindowsMajorVersion == 11 ? 314 : 316;
        static readonly string Shell32Dll = Path.Combine(Environment.SystemDirectory, "SHELL32.dll");
        static readonly uint SizeOfSHGetFileInfo = (uint)Unsafe.SizeOf<SHFILEINFOW>();
        private static unsafe Icon ExtractFromPath(string path, bool large = false)
        {
            var shinfo = new SHFILEINFOW();
            var himl = PInvoke.SHGetFileInfo(
                path,
                0, &shinfo, SizeOfSHGetFileInfo,
                SHGFI_FLAGS.SHGFI_SYSICONINDEX | (large ? SHGFI_FLAGS.SHGFI_LARGEICON : SHGFI_FLAGS.SHGFI_SMALLICON));

            Icon? icon = null;
            var iconHandle = PInvoke.ImageList_GetIcon(new HIMAGELIST((nint)himl), shinfo.iIcon, IMAGE_LIST_DRAW_STYLE.ILD_NORMAL);
            if (iconHandle != 0)
            {
                icon = Icon.FromHandle(iconHandle);
            }
            return icon ?? SystemIcons.Application;
        }
    }
}
