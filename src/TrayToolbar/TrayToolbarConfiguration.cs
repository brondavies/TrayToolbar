namespace TrayToolbar
{
    internal class TrayToolbarConfiguration
    {
        public string[] IgnoreFiles { get; set; } = [".bak", ".dll", ".ini"];

        public string? Folder { get; set; }
    }
}