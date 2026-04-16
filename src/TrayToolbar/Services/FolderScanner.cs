using TrayToolbar.Extensions;
using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal sealed class FolderScanner(IFileSystem fileSystem)
{
    internal IEnumerable<string> EnumerateFiles(string path, bool recursive, TrayToolbarConfiguration config)
    {
        return EnumerateFilesCore(path, recursive, config)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
    }

    internal bool TryResolveFolderLinkTarget(
        string fullPath,
        TrayToolbarConfiguration config,
        out string targetPath,
        Func<string, string>? shortcutResolver = null)
    {
        targetPath = string.Empty;
        if (!config.ShowFolderLinksAsSubMenus || fullPath.FileExtension() != ".lnk")
        {
            return false;
        }

        try
        {
            var resolvedPath = (shortcutResolver ?? (path => path.ResolveShortcutTarget()))(fullPath);
            if (!fileSystem.DirectoryExists(resolvedPath))
            {
                return false;
            }

            targetPath = resolvedPath;
            return true;
        }
        catch
        {
            return false;
        }
    }

    IEnumerable<string> EnumerateFilesCore(string path, bool recursive, TrayToolbarConfiguration config)
    {
        IEnumerable<string> entries;
        try
        {
            entries = fileSystem.EnumerateFileSystemEntries(path);
        }
        catch
        {
            yield break;
        }

        foreach (var entry in entries)
        {
            if (fileSystem.DirectoryExists(entry))
            {
                if (config.IgnoreFolders.Any(folder => folder.Is(Path.GetFileName(entry))))
                {
                    continue;
                }

                if (recursive)
                {
                    foreach (var child in EnumerateFilesCore(entry, recursive, config))
                    {
                        yield return child;
                    }
                }
                continue;
            }

            if (config.IncludesFile(entry))
            {
                yield return entry;
            }
        }
    }
}