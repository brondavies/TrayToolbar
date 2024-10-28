using System.Runtime.InteropServices;

namespace TrayToolbar.Extensions
{
    public static class ShellIcons
    {
        public static Icon FetchIcon(string path, bool large = false)
        {
            var icon = ExtractFromPath(path, large);
            return icon;
        }

        static uint SizeOfSHGetFileInfo = (uint)Marshal.SizeOf(new SHFILEINFO());
        private static Icon ExtractFromPath(string path, bool large = false)
        {
            var shinfo = new SHFILEINFO();
            var himl = SHGetFileInfo(
                path,
                0, ref shinfo, SizeOfSHGetFileInfo,
                SHGFI_SYSICONINDEX | (large ? SHGFI_LARGEICON : SHGFI_SMALLICON));

            Icon? icon = null;
            var iconHandle = ImageList_GetIcon(himl, shinfo.iIcon, ILD_NORMAL);
            if (iconHandle != 0)
            {
                icon = Icon.FromHandle(iconHandle);
            }
            return icon ?? SystemIcons.Application;
        }

        /// <summary>
        /// Struct used by SHGetFileInfo function
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("Comctl32.dll")]
        private static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        private const int ILD_NORMAL = 0x00000000;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_SYSICONINDEX = 0x4000;
    }
}
