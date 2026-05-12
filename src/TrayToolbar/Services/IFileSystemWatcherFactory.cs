namespace TrayToolbar.Services;

internal interface IFileSystemWatcherFactory
{
    ITrayToolbarFileSystemWatcher Create(string path);
}