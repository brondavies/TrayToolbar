using BenchmarkDotNet.Attributes;

using TrayToolbar.Models;
using TrayToolbar.Services;

namespace TrayToolbar.Benchmarks;

[MemoryDiagnoser]
public class FolderMenuBenchmarks
{
    readonly FolderScanner scanner = new(new SystemFileSystem());

    TrayToolbarConfiguration configuration = null!;
    FolderConfig folder = null!;
    string root = string.Empty;
    string[] files = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        root = Path.Combine(Path.GetTempPath(), $"TrayToolbar-Benchmarks-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        CreateDataset(root);

        configuration = new TrayToolbarConfiguration
        {
            IgnoreFiles = [".tmp"],
            IncludeFiles = ["*.cmd", "*.exe", "*.lnk", "*.url"],
            IgnoreFolders = [".git", ".github"],
            ShowFolderLinksAsSubMenus = false,
        };
        folder = new FolderConfig(root) { Recursive = true };
        files = scanner.EnumerateFiles(root, true, configuration).ToArray();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Benchmark]
    public int ScanRecursive()
    {
        return scanner.EnumerateFiles(root, true, configuration).Count();
    }

    [Benchmark]
    public int FilterKnownFiles()
    {
        return files.Count(configuration.IncludesFile);
    }

    [Benchmark]
    public int BuildMenuItems()
    {
        var menu = new MenuItemCollection(configuration, (_, _) => { }, (_, _) => { });
        foreach (var file in files.Take(64))
        {
            menu.CreateMenuItem(file, folder);
        }

        return CountMenuItems(menu);
    }

    static int CountMenuItems(IEnumerable<ToolStripMenuItem> items)
    {
        var count = 0;
        foreach (var item in items)
        {
            count++;
            count += CountMenuItems(item.DropDownItems.OfType<ToolStripMenuItem>());
        }

        return count;
    }

    static void CreateDataset(string root)
    {
        foreach (var group in Enumerable.Range(0, 4))
        {
            var groupDirectory = Path.Combine(root, $"Group-{group:00}");
            Directory.CreateDirectory(groupDirectory);

            foreach (var nested in Enumerable.Range(0, 3))
            {
                var nestedDirectory = Path.Combine(groupDirectory, $"Nested-{nested:00}");
                Directory.CreateDirectory(nestedDirectory);

                foreach (var file in Enumerable.Range(0, 12))
                {
                    var extension = (file % 4) switch
                    {
                        0 => ".exe",
                        1 => ".cmd",
                        2 => ".lnk",
                        _ => ".url",
                    };
                    File.WriteAllText(Path.Combine(nestedDirectory, $"Item-{file:00}{extension}"), "benchmark");
                }

                File.WriteAllText(Path.Combine(nestedDirectory, "ignored.tmp"), "ignore me");
            }
        }
    }
}
