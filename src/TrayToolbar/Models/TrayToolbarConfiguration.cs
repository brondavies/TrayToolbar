using System.Text.Json.Serialization;
using TrayToolbar.Extensions;

namespace TrayToolbar.Models
{
    public class TrayToolbarConfiguration
    {
        /// <summary>
        /// Whether file extensions should be hidden in the menu.
        /// </summary>
        public bool HideFileExtensions { get; set; }

        /// <summary>
        /// Whether files with names starting with a dot (".") should be ignored.
        /// </summary>
        public bool IgnoreAllDotFiles { get; set; }

        /// <summary>
        /// The list of file extensions or names to be ignored during processing.
        /// </summary>
        public string[] IgnoreFiles { get; set; } = [".bak", ".config", ".dll", ".ico", ".ini"];

        /// <summary>
        /// The list of file patterns to include during processing.
        /// </summary>
        /// <remarks>File patterns can use wildcard characters to specify matching criteria. For example, <c>"*.exe"</c> matches all executable files</remarks>
        public string[] IncludeFiles { get; set; } = [".*"];

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

        /// <summary>
        /// The list of folder names to be ignored during processing.
        /// </summary>
        public string[] IgnoreFolders { get; set; } = [".git", ".github"];

        [Obsolete("MaxRecursionDepth is Obsolete", true)]
        [JsonIgnore]
        public int MaxRecursionDepth { get; set; }

        public int MaxMenuPath { get; set; } = 512;

        /// <summary>
        /// Gets or sets the theme for the application.
        /// 0 - System Default, 1 - Light, -1 - Dark.
        /// <para/>See <see cref="ThemeToggleEnum"/>
        /// </summary>
        public int Theme { get; set; } = 0;

        /// <summary>
        /// Sets the font size used for rendering menu text.
        /// </summary>
        public float FontSize { get; set; } = 9;

        /// <summary>
        /// Whether large or small icons are displayed.
        /// </summary>
        public bool LargeIcons { get; set; }

        /// <summary>
        /// Gets or sets the language perferred by the user.
        /// If null, the system language will be used.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Whether the application should check for updates.
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// Whether folder links should be displayed as submenus.
        /// </summary>
        public bool ShowFolderLinksAsSubMenus { get; set; }

        /// <summary>
        /// Whether to show a notification when an update is available.
        /// Requires CheckForUpdates to be true.
        /// This also enables an update check timer set to the <see cref="UpdateCheckInterval"></see>.
        /// </summary>
        public bool NotifyOnUpdateAvailable { get; set; }

        /// <summary>
        /// How often to check for updates, in minutes.
        /// </summary>
        public double UpdateCheckInterval { get; set; } = 1440; // 1 day

        /// <summary>
        /// This is mainly used for debugging purposes but could also be helpful for some users for accessibility.
        /// </summary>
        public bool ShowToolTips { get; set; }

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


        internal bool IncludesFile(string f) => !IgnoreFiles.Any(i => f.IsMatch("." + i.Replace(".", "\\.")))
            && IncludeFiles.DefaultIfEmpty(".*").Any(i => f.IsMatch("." + i.Replace(".", "\\.")));
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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int IconIndex { get; set; }

        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Recursive { get; set; }

        public string? Hotkey { get; set; }

        internal FolderConfig WithPath(string targetPath)
        {
            return new FolderConfig
            {
                Hotkey = Hotkey,
                Icon = Icon,
                Name = targetPath,
                Recursive = Recursive
            };
        }

        internal Bitmap? GetIcon()
        {
            if (Icon.HasValue() && File.Exists(Icon.ToLocalPath()))
            {
                try
                {
                    return System.Drawing.Icon.ExtractIcon(Icon.ToLocalPath(), IconIndex)?.ToBitmap()
                        ?? GetDefaultIcon();
                }
                catch { }
            }
            return GetDefaultIcon();
        }

        private Bitmap? GetDefaultIcon()
        {
            return Name?.ToLocalPath().GetImage(true);
        }
    }
}