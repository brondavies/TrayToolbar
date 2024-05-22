using System.Runtime.InteropServices;

namespace TrayToolbar
{
    public static class ShellIcons
    {
        public static Icon FetchIcon(string path, bool large = false)
        {
            var icon = ExtractFromPath(path, large);
            return icon;
        }

        private static Icon ExtractFromPath(string path, bool large = false)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | (large ? SHGFI_LARGEICON : SHGFI_SMALLICON));
            var icon = Icon.FromHandle(shinfo.hIcon);
            //DestroyIcon(shinfo.hIcon); //makes icon invisible?
            return icon;
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

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
    }
}
