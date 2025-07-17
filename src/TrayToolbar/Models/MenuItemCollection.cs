using System.Collections.ObjectModel;
using TrayToolbar.Extensions;

namespace TrayToolbar.Models;

public class MenuItemCollection : ObservableCollection<ToolStripMenuItem>
{
    public MenuItemCollection() { }
    public bool NeedsRefresh { get; set; } = true;

    public ToolStripMenuItem? CreateFolder(string path, string parentPath, string? targetPath, TrayToolbarConfiguration configuration, ToolStripItemClickedEventHandler clickHandler, MouseEventHandler mouseDownHandler)
    {
        var parts = path.Split(Path.DirectorySeparatorChar);
        ToolStripMenuItem? parent = null;
        targetPath ??= parentPath;
        foreach (var part in parts)
        {
            targetPath = Path.Combine(targetPath, part);
            var exists = AddFolder(parent, part.Or(Path.GetFileName(targetPath)).ToMenuName(), targetPath, clickHandler, mouseDownHandler, configuration, out ToolStripMenuItem? menu);
            parent = menu;
            if (!exists && menu != null)
            {
                //Adds to the menu with folders first, alphabetically
                Insert(LastSubMenuIndex(this, menu.Name!), menu);
            }
        }
        return parent;
    }

    private static int LastSubMenuIndex(MenuItemCollection list, string name)
    {
        int i = 0;
        foreach (var item in list)
        {
            if (item.HasDropDown && item.Name!.IsLessThan(name)) i++;
            else break;
        }
        return i;
    }

    private static int LastSubMenuIndex(ToolStripItemCollection list, string name)
    {
        int i = 0;
        foreach (ToolStripMenuItem item in list)
        {
            if (item.Tag != null && item.Name!.IsLessThan(name)) i++;
            else break;
        }
        return i;
    }

    private bool AddFolder(ToolStripMenuItem? parent, string name, string target, ToolStripItemClickedEventHandler handler, MouseEventHandler mouseDownHandler, TrayToolbarConfiguration configuration, out ToolStripMenuItem? menu)
    {
        var exists = false;//true if it was already added
        if (parent != null)
        {
            menu = (ToolStripMenuItem?)parent.DropDownItems.Find(name, false).FirstOrDefault();
            exists = menu != null;
        }
        else
        {
            menu = this.FirstOrDefault(m => m.Name == name);
            exists = menu != null;
        }
        if (menu == null)
        {
            menu = new ToolStripMenuItem(name)
            {
                Name = name,
                Tag = target,
                Image = target.GetImage(configuration.LargeIcons),
                ImageScaling = ToolStripItemImageScaling.None,
                CommandParameter = target,
                ToolTipText = target
            };
            menu.MouseDown += mouseDownHandler;
            var eventArgs = new ToolStripItemClickedEventArgs(menu);
            menu.Click += (s, e) => handler.Invoke(s, eventArgs);
            menu.DropDownItemClicked += handler;
            exists = parent != null;
            if (exists)
            {
                var items = parent!.DropDownItems;
                //Adds to the menu with folders first, alphabetically
                items?.Insert(LastSubMenuIndex(parent.DropDownItems, menu.Name), menu);
            }
        }
        return exists;
    }

    internal void CreateMenuItem(
        bool sequential,
        string file,
        FolderConfig folder,
        TrayToolbarConfiguration config,
        ToolStripItemClickedEventHandler clickHandler,
        MouseEventHandler mouseDownHandler,
        ToolStripMenuItem? submenu = null)
    {
        var parentPath = Path.GetDirectoryName(file);
        var folderPath = folder.Name!.ToLocalPath();
        if (parentPath.HasValue() && !parentPath.Is(folderPath))
        {
            if (config.IgnoreAllDotFiles && parentPath.Contains(@"\."))
                return; //it's in a dot folder like .git or it's a dot file
            if (config.IgnoreFolders.Contains(Path.GetFileName(parentPath)))
                return; //it's in an ignored folder name
            var targetPath = submenu == null ? parentPath : file;
            var relativePath = Path.GetRelativePath(folderPath, targetPath);
            submenu ??= CreateFolder(relativePath, folderPath, null, config, clickHandler, mouseDownHandler);
        }
        if (file.FileExtension() == ".lnk" && config.ShowFolderLinksAsSubMenu)
        {
            // Check if file points to a folder
            try
            {
                var targetPath = file.ResolveShortcutTarget();
                if (targetPath.IsDirectory() && parentPath.HasValue())
                {
                    var targetDirName = Path.GetFileName(targetPath);
                    var submenuName = Path.GetFileNameWithoutExtension(file);
                    submenu = CreateFolder(submenuName == targetDirName ? "" : submenuName, parentPath, targetPath, config, clickHandler, mouseDownHandler);
                    foreach (var f in EnumerateFiles(targetPath, folder.Recursive, config))
                    {
                        CreateMenuItem(sequential, f, folder.WithPath(targetPath), config, clickHandler, mouseDownHandler, submenu);
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
        };
        entry.MouseDown += mouseDownHandler;
        if (submenu != null)
        {
            if (submenu.DropDownItems.Find(entry.Name!, false).Length == 0)
            {
                if (sequential)
                    submenu.DropDownItems.Add(entry);
                else
                    submenu.DropDownItems.Insert(IndexOfItem(submenu.DropDownItems, entry.Text), entry);
            }
        }
        else if (!this.Any(i => i.Name == entry.Name))
        {
            if (sequential)
                Add(entry);
            else
                Insert(IndexOfItem(entry.Text), entry);
        }
    }

    private static IEnumerable<string> EnumerateFiles(string path, bool recursive, TrayToolbarConfiguration config)
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

    private static int IndexOfItem(ToolStripItemCollection list, string text)
    {
        var i = 0;
        foreach (ToolStripMenuItem entry in list)
        {
            if (entry.Tag == null && StringComparer.OrdinalIgnoreCase.Compare(entry.Text, text) > 0) break;
            i++;
        }
        return i;
    }

    private int IndexOfItem(string text)
    {
        var i = 0;
        foreach (var entry in this)
        {
            if (entry.Tag == null && StringComparer.OrdinalIgnoreCase.Compare(entry.Text, text) > 0) break;
            i++;
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