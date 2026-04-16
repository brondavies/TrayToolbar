using TrayToolbar.Services;

namespace TrayToolbar.Tests;

internal sealed class FakeFileSystem : IFileSystem
{
    readonly Dictionary<string, string> files = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> directories = new(StringComparer.OrdinalIgnoreCase);

    public void AddDirectory(string path)
    {
        CreateDirectory(path);
    }

    public void AddFile(string path, string contents = "")
    {
        var normalizedPath = Normalize(path);
        var directory = Path.GetDirectoryName(normalizedPath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectory(directory);
        }

        files[normalizedPath] = contents;
    }

    public string GetFileContents(string path)
    {
        return files[Normalize(path)];
    }

    public bool FileExists(string path) => files.ContainsKey(Normalize(path));

    public bool DirectoryExists(string path) => directories.Contains(Normalize(path));

    public void CreateDirectory(string path)
    {
        var current = Normalize(path);
        while (!string.IsNullOrEmpty(current) && directories.Add(current))
        {
            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrEmpty(parent) || current.Equals(parent, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            current = Normalize(parent);
        }
    }

    public string ReadAllText(string path)
    {
        var normalizedPath = Normalize(path);
        if (!files.TryGetValue(normalizedPath, out var contents))
        {
            throw new FileNotFoundException("File was not found.", normalizedPath);
        }

        return contents;
    }

    public void WriteAllText(string path, string contents)
    {
        var normalizedPath = Normalize(path);
        var directory = Path.GetDirectoryName(normalizedPath);
        if (string.IsNullOrEmpty(directory) || !DirectoryExists(directory))
        {
            throw new DirectoryNotFoundException(directory);
        }

        files[normalizedPath] = contents;
    }

    public void DeleteFile(string path)
    {
        files.Remove(Normalize(path));
    }

    public void MoveFile(string sourceFileName, string destFileName)
    {
        var sourcePath = Normalize(sourceFileName);
        if (!files.TryGetValue(sourcePath, out var contents))
        {
            throw new FileNotFoundException("File was not found.", sourcePath);
        }

        var destinationPath = Normalize(destFileName);
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (string.IsNullOrEmpty(destinationDirectory) || !DirectoryExists(destinationDirectory))
        {
            throw new DirectoryNotFoundException(destinationDirectory);
        }

        files.Remove(sourcePath);
        files[destinationPath] = contents;
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        var normalizedPath = Normalize(path);
        var entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var directory in directories)
        {
            if (string.Equals(Path.GetDirectoryName(directory), normalizedPath, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(directory);
            }
        }

        foreach (var file in files.Keys)
        {
            if (string.Equals(Path.GetDirectoryName(file), normalizedPath, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(file);
            }
        }

        return entries.OrderBy(entry => entry, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    static string Normalize(string path)
    {
        var normalizedPath = Path.GetFullPath(path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        var root = Path.GetPathRoot(normalizedPath) ?? string.Empty;
        if (normalizedPath.Length > root.Length)
        {
            normalizedPath = normalizedPath.TrimEnd(Path.DirectorySeparatorChar);
        }
        return normalizedPath;
    }
}
