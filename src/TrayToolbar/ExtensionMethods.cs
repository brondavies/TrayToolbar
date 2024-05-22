using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TrayToolbar
{
    public static class ExtensionMethods
    {
        public static RowStyle Clone(this RowStyle self)
        {
            return new RowStyle
            {
                Height = self.Height,
                SizeType = self.SizeType,
            };
        }

        public static string FileExtension(this string file)
        {
            return Path.GetExtension(file).ToLowerInvariant();
        }

        public static bool HasValue([NotNullWhen(true)] this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsHttps(this string value) 
        {
            return value.HasValue() && value.StartsWith("https://");
        }

        public static string Join(this string[] value, string separator = ", ")
        {
            return string.Join(separator, value);
        }

        public static Bitmap GetImage(this string file)
        {
            return (Icon.ExtractAssociatedIcon(file) ?? SystemIcons.Application).ToBitmap();
        }

        public static string ToLocalPath(this string value)
        {
            return Environment.ExpandEnvironmentVariables(value);
        }

        public static void ShowContextMenu(this NotifyIcon notifyIcon)
        {
            MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            mi?.Invoke(notifyIcon, null);
        }
    }
}
