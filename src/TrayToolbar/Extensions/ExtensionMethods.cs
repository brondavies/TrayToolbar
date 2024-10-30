using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

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
            return ShellIcons.FetchIcon(path, false);
        }

        public static Bitmap GetImage(this string file, bool large = false)
        {
            //return ShellIcons.FetchIconAsBitmap(file, large);
            //return Bitmap.FromHicon(ShellIcons.FetchIcon(file, large).Handle);
            var icon = ShellIcons.FetchIcon(file, large);
            Bitmap bmp = new Bitmap(icon.Width, icon.Height);
            using (Graphics gp = Graphics.FromImage(bmp))
            {
                gp.Clear(Color.Transparent);
                gp.DrawIcon(icon, new Rectangle(0, 0, icon.Width, icon.Height));
            }
            return bmp;
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

        public static bool IsMatch(this string? value, string pattern)
        {
            if (value == null) return false;
            return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
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

        public static string[] SplitPaths(this string value, char[]? splitchars = null)
        {
            return value.Split(splitchars ?? [';', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToMenuName(this string? value)
        {
            return value?.Replace("&", "&&") ?? "";
        }
    }
}
