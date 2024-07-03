using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TrayToolbar.Extensions
{
    public static class ExtensionMethods
    {
        public static string FileExtension(this string file)
        {
            return Path.GetExtension(file).ToLowerInvariant();
        }

        public static Icon GetIcon(this string path)
        {
            return ShellIcons.FetchIcon(path, true);
        }

        public static Bitmap GetImage(this string file)
        {
            Icon? icon = null;
            try
            {
                icon = Icon.ExtractAssociatedIcon(file);
            }
            catch { }
            return (icon ?? SystemIcons.Application).ToBitmap();
        }

        public static bool HasValue([NotNullWhen(true)] this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool Is(this string? value, string? compare)
        {
            if (value == compare) return true;
            return (value ?? "").Equals(compare, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsDirectory(this string? value)
        {
            return value.HasValue() && File.GetAttributes(value).HasFlag(FileAttributes.Directory);
        }

        public static bool IsHttps(this string value)
        {
            return value.HasValue() && value.StartsWith("https://");
        }

        public static string Join(this string[] value, string separator = ", ")
        {
            return string.Join(separator, value);
        }

        public static string? Or(this string? value, string? defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
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
