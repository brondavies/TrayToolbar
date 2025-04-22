﻿using System.Collections.ObjectModel;
using TrayToolbar.Extensions;

namespace TrayToolbar.Models;

public class MenuItemCollection : ObservableCollection<ToolStripMenuItem>
{
    public MenuItemCollection() { }
    public bool NeedsRefresh { get; set; } = true;

    public ToolStripMenuItem? CreateFolder(string path, string target, TrayToolbarConfiguration configuration, ToolStripItemClickedEventHandler clickHandler, MouseEventHandler mouseDownHandler)
    {
        var parts = path.Split(Path.DirectorySeparatorChar);
        ToolStripMenuItem? parent = null;
        foreach (var part in parts)
        {
            var exists = AddFolder(parent, part.ToMenuName(), target, clickHandler, mouseDownHandler, configuration, out ToolStripMenuItem? menu);
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
        var result = false;//true if it was already added
        if (parent != null)
        {
            menu = (ToolStripMenuItem?)parent.DropDownItems.Find(name, false).FirstOrDefault();
            result = menu != null;
        }
        else
        {
            menu = this.FirstOrDefault(m => m.Name == name);
            result = menu != null;
        }
        if (menu == null)
        {
            menu = new ToolStripMenuItem(name)
            {
                Name = name,
                Tag = target,
                Image = target.GetImage(configuration.LargeIcons),
                ImageScaling = ToolStripItemImageScaling.None
            };
            menu.MouseDown += mouseDownHandler;
            menu.DropDownItemClicked += handler;
            result = parent != null;
            if (result)
            {
                var items = parent!.DropDownItems;
                //Adds to the menu with folders first, alphabetically
                items?.Insert(LastSubMenuIndex(parent.DropDownItems, menu.Name), menu);
            }
        }
        return result;
    }

    internal void CreateMenuItem(
        bool sequential,
        string file,
        FolderConfig folder,
        TrayToolbarConfiguration configuration,
        ToolStripItemClickedEventHandler clickHandler,
        MouseEventHandler mouseDownHandler)
    {
        ToolStripMenuItem? submenu = null;
        var parentPath = Path.GetDirectoryName(file);
        if (parentPath.HasValue() && !parentPath.Is(folder.Name!.ToLocalPath()))
        {
            if (configuration.IgnoreAllDotFiles && parentPath.Contains(@"\."))
                return; //it's in a dot folder like .git or it's a dot file
            if (configuration.IgnoreFolders.Contains(Path.GetFileName(parentPath)))
                return; //it's in an ignored folder name
            var relativePath = Path.GetRelativePath(folder.Name!.ToLocalPath(), parentPath);
            submenu = CreateFolder(relativePath, parentPath, configuration, clickHandler, mouseDownHandler);
        }
        var menuText = Path.GetFileName(file);
        if (configuration.HideFileExtensions || file.FileExtension().IsOneOf(".lnk", ".url"))
        {
            menuText = Path.GetFileNameWithoutExtension(file);
        }
        var entry = new ToolStripMenuItem
        {
            Name = menuText,
            Text = menuText.ToMenuName(),
            CommandParameter = file,
            Image = file.GetImage(configuration.LargeIcons),
            ImageScaling = ToolStripItemImageScaling.None
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

    private static int IndexOfItem(ToolStripItemCollection list, string text)
    {
        var i = 0;
        foreach (ToolStripMenuItem entry in list)
        {
            if (entry.CommandParameter != null && StringComparer.OrdinalIgnoreCase.Compare(entry.Text, text) > 0) break;
            i++;
        }
        return i;
    }

    private int IndexOfItem(string text)
    {
        var i = 0;
        foreach (var entry in this)
        {
            if (entry.CommandParameter != null && StringComparer.OrdinalIgnoreCase.Compare(entry.Text, text) > 0) break;
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