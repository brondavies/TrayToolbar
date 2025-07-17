using System.Collections.ObjectModel;
using TrayToolbar.Extensions;

namespace TrayToolbar.Models
{
    public class MenuItemCollection2 : ObservableCollection<ToolStripMenuItem>
    {
        public MenuItemCollection2() { }
        public bool NeedsRefresh { get; set; } = true;

        internal void CreateMenuItem(
            bool sequential,
            string file,
            FolderConfig folder,
            TrayToolbarConfiguration config,
            ToolStripItemClickedEventHandler clickHandler,
            MouseEventHandler mouseDownHandler,
            string root = "")
        {
            var parentPath = Path.GetDirectoryName(file);
            var folderPath = folder.Name!.ToLocalPath();
            if (parentPath.HasValue() && !parentPath.Is(folderPath))
            {
                if (config.IgnoreAllDotFiles && parentPath.Contains(@"\."))
                    return; //it's in a dot folder like .git or it's a dot file
                if (config.IgnoreFolders.Contains(Path.GetFileName(parentPath)))
                    return; //it's in an ignored folder name
            }
            var relativePath = Path.GetRelativePath(folderPath, parentPath ?? folderPath);
            if (relativePath == ".") relativePath = "";
            if (config.ShowFolderLinksAsSubMenu && file.FileExtension() == ".lnk")
            {
                // Check if file points to a folder
                try
                {
                    var targetPath = file.ResolveShortcutTarget();
                    if (targetPath.IsDirectory())
                    {
                        //var targetDirName = Path.GetFileName(targetPath);
                        var submenuName = Path.GetFileNameWithoutExtension(file);
                        foreach (var f in EnumerateFiles(targetPath, folder.Recursive, config))
                        {
                            CreateMenuItem(sequential, f, folder.WithPath(targetPath), config, clickHandler, mouseDownHandler, Path.Combine(root, relativePath, submenuName));
                        }
                        return; // Skip adding the link itself, as it's now a submenu
                    }
                }
                catch { } // If resolving fails, treat as a regular file
            }
            var menuText = (config.HideFileExtensions || file.FileExtension().IsOneOf(".lnk", ".url"))
                ? Path.GetFileNameWithoutExtension(file)
                : Path.GetFileName(file);
            var entry = new ToolStripMenuItem
            {
                Name = menuText,
                Text = menuText.ToMenuName(),
                CommandParameter = file,
                Image = file.GetImage(config.LargeIcons),
                ImageScaling = ToolStripItemImageScaling.None,
                ToolTipText = file,
                AccessibleRole = AccessibleRole.MenuItem
            };
            var eventArgs = new ToolStripItemClickedEventArgs(entry);
            entry.Click += (s, e) => clickHandler.Invoke(s, eventArgs);
            entry.MouseDown += mouseDownHandler;

            var parent = GetParent(Path.Combine(root, relativePath), config, clickHandler, mouseDownHandler);
            if (parent != null)
            {
                parent.DropDownItems.Insert(IndexOfItem(parent.DropDownItems, entry.Text, AccessibleRole.MenuItem), entry);
            }
            else if (!this.Any(i => i.Name == entry.Name))
            {
                Insert(IndexOfItem(entry.Text, AccessibleRole.MenuItem), entry);
            }
        }

        private ToolStripMenuItem? GetParent(string path, TrayToolbarConfiguration config, ToolStripItemClickedEventHandler clickHandler, MouseEventHandler mouseDownHandler)
        {
            if (!path.HasValue()) return null;
            var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            ToolStripMenuItem? parent = null;
            var tag = "";
            foreach (var part in parts)
            {
                tag = Path.Combine(tag, part);
                if (parent == null)
                {
                    parent = this.FirstOrDefault(i => $"{i.Tag}" == tag);
                    if (parent == null)
                    {
                        parent = CreateSubMenu(config, tag, part, clickHandler, mouseDownHandler);
                        Insert(IndexOfItem(parent.Text!, AccessibleRole.MenuPopup), parent);
                    }
                }
                else
                {
                    var submenu = parent.DropDownItems
                        .OfType<ToolStripMenuItem>()
                        .FirstOrDefault(i => $"{i.Tag}" == tag);
                    if (submenu == null)
                    {
                        submenu = CreateSubMenu(config, tag, part, clickHandler, mouseDownHandler);
                        parent.DropDownItems.Insert(IndexOfItem(parent.DropDownItems, submenu.Text!, AccessibleRole.MenuPopup), submenu);
                    }
                    parent = submenu;
                }
            }
            return parent;
        }

        private static ToolStripMenuItem CreateSubMenu(TrayToolbarConfiguration config, string tag, string name, ToolStripItemClickedEventHandler clickHandler, MouseEventHandler mouseDownHandler)
        {
            const string target = "C:\\Users";
            var menu = new ToolStripMenuItem
            {
                Name = name,
                Tag = tag,
                Text = name.ToMenuName(),
                CommandParameter = target,
                Image = target.GetImage(config.LargeIcons),
                ImageScaling = ToolStripItemImageScaling.None,
                AccessibleRole = AccessibleRole.MenuPopup
            };

            menu.MouseDown += mouseDownHandler;
            var eventArgs = new ToolStripItemClickedEventArgs(menu);
            menu.Click += (s, e) => clickHandler.Invoke(s, eventArgs);
            return menu;
        }

        internal static IEnumerable<string> EnumerateFiles(string path, bool recursive, TrayToolbarConfiguration config)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = recursive,
                ReturnSpecialDirectories = false,
            };
            return Directory.EnumerateFiles(path, "*.*", options)
                .Where(config.IncludesFile)
                .OrderBy(f => f.ToUpper());
        }

        private static int IndexOfItem(ToolStripItemCollection list, string text, AccessibleRole role)
        {
            var i = 0;
            foreach (ToolStripMenuItem entry in list)
            {
                if (entry.AccessibleRole == role)
                {
                    if (StringComparer.OrdinalIgnoreCase.Compare(entry.Text, text) > 0) break;
                    i++;
                }
                else if (role == AccessibleRole.MenuItem)
                {
                    i++;
                }
            }
            return i;
        }

        private int IndexOfItem(string text, AccessibleRole role)
        {
            var i = 0;
            foreach (var entry in this)
            {
                if (entry.AccessibleRole == role)
                {
                    if (StringComparer.OrdinalIgnoreCase.Compare(entry.Text, text) > 0) break;
                    i++;
                }
                else if (role == AccessibleRole.MenuItem)
                {
                    i++;
                }
            }
            return i;
        }

        internal bool DeleteMenu(string fullPath)
        {
            foreach (var item in this)
            {
                if ($"{item.CommandParameter}".Is(fullPath) || $"{item.Tag}".Is(fullPath))
                {
                    Remove(item);
                    return true;
                }
                if (DeleteMenu(fullPath, item.DropDownItems)) return true;
            }
            return false;
        }

        private static bool DeleteMenu(string fullPath, ToolStripItemCollection dropDownItems)
        {
            foreach (ToolStripMenuItem submenu in dropDownItems)
            {
                if ($"{submenu.CommandParameter}".Is(fullPath) || $"{submenu.Tag}".Is(fullPath))
                {
                    dropDownItems.Remove(submenu);
                    return true;
                }
                if (DeleteMenu(fullPath, submenu.DropDownItems)) return true;
            }
            return false;
        }
    }
}
