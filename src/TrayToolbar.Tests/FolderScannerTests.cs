using TrayToolbar.Models;
using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class FolderScannerTests
{
    [TestMethod]
    public void IncludesFile_prioritizes_ignored_patterns_over_included_patterns()
    {
        var configuration = new TrayToolbarConfiguration
        {
            IgnoreFiles = [".txt"],
            IncludeFiles = ["*.txt"]
        };

        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\notes.txt"));
    }

    [TestMethod]
    public void IncludesFile_supports_common_wildcards()
    {
        var configuration = new TrayToolbarConfiguration
        {
            IgnoreFiles = [],
            IncludeFiles = ["*.exe", "readme.*"]
        };

        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\tool.exe"));
        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\README.md"));
        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\notes.txt"));
    }

    [TestMethod]
    public void IncludesFile_uses_default_excluded_extensions()
    {
        var configuration = new TrayToolbarConfiguration();

        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\library.dll"));
        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\settings.ini"));
        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\script.cmd"));
    }

    [TestMethod]
    public void EnumerateFiles_skips_ignored_folders_when_recursive()
    {
        var fileSystem = new FakeFileSystem();
        var root = @"C:\Root";
        fileSystem.AddFile(Path.Combine(root, "app.exe"));
        fileSystem.AddFile(Path.Combine(root, ".git", "hidden.exe"));
        fileSystem.AddFile(Path.Combine(root, ".github", "workflow.exe"));
        fileSystem.AddFile(Path.Combine(root, "Sub", "tool.exe"));
        var scanner = new FolderScanner(fileSystem);

        var files = scanner.EnumerateFiles(root, true, new TrayToolbarConfiguration()).ToArray();

        CollectionAssert.AreEquivalent(
            new[] { Path.Combine(root, "app.exe"), Path.Combine(root, "Sub", "tool.exe") },
            files);
    }

    [TestMethod]
    public void EnumerateFiles_honors_recursive_flag()
    {
        var fileSystem = new FakeFileSystem();
        var root = @"C:\Root";
        fileSystem.AddFile(Path.Combine(root, "app.exe"));
        fileSystem.AddFile(Path.Combine(root, "Sub", "tool.exe"));
        var scanner = new FolderScanner(fileSystem);
        var configuration = new TrayToolbarConfiguration();

        var nonRecursive = scanner.EnumerateFiles(root, false, configuration).ToArray();
        var recursive = scanner.EnumerateFiles(root, true, configuration).ToArray();

        CollectionAssert.AreEquivalent(new[] { Path.Combine(root, "app.exe") }, nonRecursive);
        CollectionAssert.AreEquivalent(
            new[] { Path.Combine(root, "app.exe"), Path.Combine(root, "Sub", "tool.exe") },
            recursive);
    }

    [TestMethod]
    public void TryResolveFolderLinkTarget_separates_submenu_links_from_regular_launches()
    {
        var fileSystem = new FakeFileSystem();
        var root = @"C:\Root";
        var linkPath = Path.Combine(root, "Docs.lnk");
        var folderTarget = @"C:\Targets\Docs";
        var fileTarget = @"C:\Targets\Docs.url";
        fileSystem.AddFile(linkPath);
        fileSystem.AddDirectory(folderTarget);
        fileSystem.AddFile(fileTarget);
        var scanner = new FolderScanner(fileSystem);
        var configuration = new TrayToolbarConfiguration { ShowFolderLinksAsSubMenus = true };

        var isSubmenu = scanner.TryResolveFolderLinkTarget(linkPath, configuration, out var resolvedTarget, _ => folderTarget);
        var isRegularLink = scanner.TryResolveFolderLinkTarget(linkPath, configuration, out _, _ => fileTarget);
        var whenDisabled = scanner.TryResolveFolderLinkTarget(
            linkPath,
            new TrayToolbarConfiguration { ShowFolderLinksAsSubMenus = false },
            out _,
            _ => folderTarget);

        Assert.IsTrue(isSubmenu);
        Assert.AreEqual(folderTarget, resolvedTarget);
        Assert.IsFalse(isRegularLink);
        Assert.IsFalse(whenDisabled);
    }
}
