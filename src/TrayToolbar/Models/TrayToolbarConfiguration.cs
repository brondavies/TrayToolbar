using System.Text.Json.Serialization;
using TrayToolbar.Extensions;

namespace TrayToolbar
{
    public class TrayToolbarConfiguration
    {
        public string[] IgnoreFileTypes { get; set; } = [".bak", ".config", ".dll", ".ico", ".ini"];

        // mtanner
        public string[] IgnoreFolders { get; set; } = [".git", ".github"];

        public int MaxRecursionDepth { get; set; } = 3;

        public int Theme { get; set; } = 0;

        [Obsolete]
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