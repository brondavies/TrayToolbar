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
            var icon = ExtractFromPath(path, large);
            return icon;
        }

        static uint SizeOfSHGetFileInfo = (uint)Unsafe.SizeOf<SHFILEINFOW>();
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
