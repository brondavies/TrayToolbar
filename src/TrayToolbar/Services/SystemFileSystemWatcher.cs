namespace TrayToolbar.Services;

internal sealed class SystemFileSystemWatcher(FileSystemWatcher watcher) : ITrayToolbarFileSystemWatcher
{
    readonly FileSystemWatcher _watcher = watcher;

    public string Filter
    {
        get => _watcher.Filter;
        set => _watcher.Filter = value;
    }

    public bool IncludeSubdirectories
    {
        get => _watcher.IncludeSubdirectories;
        set => _watcher.IncludeSubdirectories = value;
    }

    public bool EnableRaisingEvents
    {
        get => _watcher.EnableRaisingEvents;
        set => _watcher.EnableRaisingEvents = value;
    }

    public NotifyFilters NotifyFilter
    {
        get => _watcher.NotifyFilter;
        set => _watcher.NotifyFilter = value;
    }

    public event FileSystemEventHandler Changed
    {
        add => _watcher.Changed += value;
        remove => _watcher.Changed -= value;
    }

    public event FileSystemEventHandler Created
    {
        add => _watcher.Created += value;
        remove => _watcher.Created -= value;
    }

    public event FileSystemEventHandler Deleted
    {
        add => _watcher.Deleted += value;
        remove => _watcher.Deleted -= value;
    }

    public event RenamedEventHandler Renamed
    {
        add => _watcher.Renamed += value;
        remove => _watcher.Renamed -= value;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}