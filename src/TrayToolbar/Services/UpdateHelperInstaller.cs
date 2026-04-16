namespace TrayToolbar.Services;

internal sealed class UpdateHelperInstaller : IUpdateInstaller
{
    public void DownloadAndUpdate(string version)
    {
        UpdateHelper.DownloadAndUpdate(version);
    }
}