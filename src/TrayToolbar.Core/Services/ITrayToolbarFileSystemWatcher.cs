namespace TrayToolbar.Services;

internal interface ITrayToolbarFileSystemWatcher : IDisposable
{
    string Filter { get; set; }
    bool IncludeSubdirectories { get; set; }
    bool EnableRaisingEvents { get; set; }
    NotifyFilters NotifyFilter { get; set; }

    event FileSystemEventHandler Changed;
    event FileSystemEventHandler Created;
    event FileSystemEventHandler Deleted;
    event RenamedEventHandler Renamed;
}