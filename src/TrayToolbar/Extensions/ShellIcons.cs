using System.Runtime.InteropServices;

namespace TrayToolbar.Extensions
{
    public static class ShellIcons
    {
        public static Bitmap FetchIconAsBitmap(string path, bool large)
        {
            var shfi = new SHFILEINFO();
            var imageList = SHGetFileInfoAsImageList(path, 0, ref shfi, SizeOfSHGetFileInfo, SHGFI_SYSICONINDEX | (large ? SHGFI_LARGEICON : SHGFI_SMALLICON));
            if (imageList != null)
            {
                var hIcon = IntPtr.Zero;
                imageList.GetIcon(shfi.iIcon, ILD_NORMAL, ref hIcon);
                //Marshal.FinalReleaseComObject(imageList);
                if (hIcon != IntPtr.Zero)
                {
                    var image = Bitmap.FromHicon(hIcon);
                    DestroyIcon(hIcon);
                    return image;
                }
            }
            return new Bitmap(large ? 32 : 16, large ? 32 : 16);
        }

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
            DestroyIcon(shinfo.hIcon);
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

        [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo", CharSet = CharSet.Auto)]
        private static extern IImageList SHGetFileInfoAsImageList(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);


        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("Comctl32.dll")]
        private static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        private const int ILD_NORMAL = 0x00000000;
        private const int ILD_TRANSPARENT = 0x00000001;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_SYSICONINDEX = 0x4000;


        [ComImport,
        Guid("46EB5926-582E-4017-9FDF-E8998DAA0950"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IImageList
        {
            [PreserveSig]
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
            [PreserveSig]
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);
            [PreserveSig]
            int SetOverlayImage(int iImage, int iOverlay);
            [PreserveSig]
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
            [PreserveSig]
            int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
            [PreserveSig]
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);
            [PreserveSig]
            int Remove(int i);
            [PreserveSig]
            int GetIcon(int i, int flags, ref IntPtr picon);
            [PreserveSig]
            int GetImageInfo(int i, ref IMAGEINFO pImageInfo);
            [PreserveSig]
            int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);
            [PreserveSig]
            int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);
            [PreserveSig]
            int Clone(ref Guid riid, ref IntPtr ppv);
            [PreserveSig]
            int GetImageRect(int i, ref RECT prc);
            [PreserveSig]
            int GetIconSize(ref int cx, ref int cy);
            [PreserveSig]
            int SetIconSize(int cx, int cy);
            [PreserveSig]
            int GetImageCount(ref int pi);
            [PreserveSig]
            int SetImageCount(int uNewCount);
            [PreserveSig]
            int SetBkColor(int clrBk, ref int pclr);
            [PreserveSig]
            int GetBkColor(ref int pclr);
            [PreserveSig]
            int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);
            [PreserveSig]
            int EndDrag();
            [PreserveSig]
            int DragEnter(IntPtr hwndLock, int x, int y);
            [PreserveSig]
            int DragLeave(IntPtr hwndLock);
            [PreserveSig]
            int DragMove(int x, int y);
            [PreserveSig]
            int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);
            [PreserveSig]
            int DragShowNolock(int fShow);
            [PreserveSig]
            int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);
            [PreserveSig]
            int GetItemFlags(int i, ref int dwFlags);
            [PreserveSig]
            int GetOverlayImage(int iOverlay, ref int piIndex);
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;
            public int yBitmap;
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
            public RECT(int l, int t, int r, int b)
            {
                Left = l;
                Top = t;
                Right = r;
                Bottom = b;
            }

            public RECT(Rectangle r)
            {
                Left = r.Left;
                Top = r.Top;
                Right = r.Right;
                Bottom = r.Bottom;
            }

            public Rectangle ToRectangle()
            {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }

            public void Inflate(int width, int height)
            {
                Left -= width;
                Top -= height;
                Right += width;
                Bottom += height;
            }

            public override string ToString()
            {
                return string.Format("x:{0},y:{1},width:{2},height:{3}", Left, Top, Right - Left, Bottom - Top);
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public POINT(Point pt)
            {
                this.X = pt.X;
                this.Y = pt.Y;
            }

            public Point ToPoint()
            {
                return new Point(X, Y);
            }
        }
    }
}
