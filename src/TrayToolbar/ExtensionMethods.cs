namespace TrayToolbar
{
    public static class ExtensionMethods
    {
        public static string FileExtension(this string file)
        {
            return Path.GetExtension(file).ToLowerInvariant();
        }

        public static bool HasValue(this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static string Join(this string[] value, string separator = ", ")
        {
            return string.Join(separator, value);
        }

        public static Bitmap GetImage(this string file)
        {
            return (Icon.ExtractAssociatedIcon(file) ?? SystemIcons.Application).ToBitmap();
        }
    }
}
