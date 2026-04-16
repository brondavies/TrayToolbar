namespace TrayToolbar.Services;

internal interface IUpdateInstaller
{
    void DownloadAndUpdate(string version);
}