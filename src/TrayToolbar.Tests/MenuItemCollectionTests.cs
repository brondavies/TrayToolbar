using System.Windows.Forms;

using TrayToolbar.Models;

namespace TrayToolbar.Tests;

[TestClass]
public class MenuItemCollectionTests
{
    [TestMethod]
    public void CreateMenuItem_shows_cyclic_folder_links_as_regular_items()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        var root = @"C:\Root";
        var linkPath = Path.Combine(root, "Loop.lnk");
        fileSystem.AddFile(linkPath);
        fileSystem.AddFile(Path.Combine(root, "app.exe"));
        var configuration = new TrayToolbarConfiguration { ShowFolderLinksAsSubMenus = true };
        var collection = new MenuItemCollection(configuration, (_, _) => { }, (_, _) => { })
        {
            // The link resolves back to the folder being scanned; without the cycle
            // guard this recursed until the app hung
            ShortcutResolver = _ => root,
        };
        var folder = new FolderConfig(root) { Recursive = true };

        collection.CreateMenuItem(linkPath, folder);

        Assert.AreEqual(1, collection.Count);
        Assert.AreEqual("Loop", collection[0].Name);
        Assert.AreEqual(AccessibleRole.MenuItem, collection[0].AccessibleRole);
        Assert.AreEqual(0, collection[0].DropDownItems.Count);
    }

    [TestMethod]
    public void CreateMenuItem_still_expands_folder_links_to_other_folders_as_submenus()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        var root = @"C:\Root";
        var target = @"C:\Elsewhere";
        var linkPath = Path.Combine(root, "Docs.lnk");
        fileSystem.AddFile(linkPath);
        fileSystem.AddDirectory(target);
        fileSystem.AddFile(Path.Combine(target, "tool.exe"));
        var configuration = new TrayToolbarConfiguration { ShowFolderLinksAsSubMenus = true };
        var collection = new MenuItemCollection(configuration, (_, _) => { }, (_, _) => { })
        {
            ShortcutResolver = _ => target,
        };
        var folder = new FolderConfig(root) { Recursive = true };

        collection.CreateMenuItem(linkPath, folder);

        Assert.AreEqual(1, collection.Count);
        Assert.AreEqual("Docs", collection[0].Name);
        Assert.AreEqual(AccessibleRole.MenuPopup, collection[0].AccessibleRole);
        Assert.AreEqual(1, collection[0].DropDownItems.Count);
        Assert.AreEqual("tool.exe", collection[0].DropDownItems[0].Name);
    }

    [TestMethod]
    public void CreateMenuItem_stops_mutual_folder_link_cycles()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        var root = @"C:\Root";
        var other = @"C:\Other";
        var linkToOther = Path.Combine(root, "Other.lnk");
        var linkBack = Path.Combine(other, "Root.lnk");
        fileSystem.AddFile(linkToOther);
        fileSystem.AddDirectory(other);
        fileSystem.AddFile(linkBack);
        var configuration = new TrayToolbarConfiguration { ShowFolderLinksAsSubMenus = true };
        var collection = new MenuItemCollection(configuration, (_, _) => { }, (_, _) => { })
        {
            ShortcutResolver = path => path.Equals(linkToOther, StringComparison.OrdinalIgnoreCase) ? other : root,
        };
        var folder = new FolderConfig(root) { Recursive = true };

        collection.CreateMenuItem(linkToOther, folder);

        // Root.lnk points back at C:\Root which is already being expanded, so it must
        // be rendered as a plain item inside the "Other" submenu instead of recursing
        Assert.AreEqual(1, collection.Count);
        Assert.AreEqual("Other", collection[0].Name);
        Assert.AreEqual(1, collection[0].DropDownItems.Count);
        Assert.AreEqual("Root", collection[0].DropDownItems[0].Name);
        Assert.AreEqual(AccessibleRole.MenuItem, ((ToolStripMenuItem)collection[0].DropDownItems[0]).AccessibleRole);
    }
}
