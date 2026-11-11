using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal interface IUpdateInstaller
{
    void DownloadAndUpdate(UpdatePackage package);
}