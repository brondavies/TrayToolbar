namespace TrayToolbar.Services;

internal sealed class SystemFileSystemWatcherFactory : IFileSystemWatcherFactory
{
    public ITrayToolbarFileSystemWatcher Create(string path)
    {
        return new SystemFileSystemWatcher(new FileSystemWatcher(path));
    }
}