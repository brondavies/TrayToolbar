using System.Text.Json.Serialization;
using TrayToolbar.Extensions;

namespace TrayToolbar.Models
{
    public class TrayToolbarConfiguration
    {
        public bool HideFileExtensions { get; set; }

        public bool IgnoreAllDotFiles { get; set; }

        public string[] IgnoreFiles { get; set; } = [".bak", ".config", ".dll", ".ico", ".ini"];

        [Obsolete("IgnoreFileTypes is obsolete", true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? IgnoreFileTypes
        {
            get => null;
            set
            {
                if (value != null)
                    IgnoreFiles = value;
            }
        }

        public string[] IgnoreFolders { get; set; } = [".git", ".github"];

        public int MaxRecursionDepth { get; set; } = 3;

        public int Theme { get; set; } = 0;

        public float FontSize { get; set; } = 9;

        public bool LargeIcons { get; set; }

        [Obsolete("Folder is obsolete", true)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Folder
        {
            get => null;
            set
            {
                if (value.HasValue())
                {
                    Folders.Add(new FolderConfig(value));
                }
            }
        }

        public List<FolderConfig> Folders { get; set; } = [];
    }

    public class FolderConfig
    {
        public FolderConfig() { }
        public FolderConfig(string name)
        {
            Name = name;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
        public string? Icon { get; set; }

        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Recursive { get; set; }
    }
}