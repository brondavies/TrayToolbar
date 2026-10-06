using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal sealed class UpdateHelperInstaller : IUpdateInstaller
{
    public void DownloadAndUpdate(UpdatePackage package)
    {
        UpdateHelper.DownloadAndUpdate(package);
    }
}