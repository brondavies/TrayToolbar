using TrayToolbar.Services;

namespace TrayToolbar.Tests;

internal sealed class ConfigHelperStateScope : IDisposable
{
    readonly IFileSystem fileSystem = ConfigHelper.FileSystem;
    readonly IReleaseClient releaseClient = ConfigHelper.ReleaseClient;
    readonly IUpdateInstaller updateInstaller = ConfigHelper.UpdateInstaller;
    readonly string localAppData = ConfigHelper.LocalAppData;
    readonly string profileFolder = ConfigHelper.ProfileFolder;
    readonly string configurationFile = ConfigHelper.ConfigurationFile;
    readonly string legacyConfigurationFile = ConfigHelper.LegacyConfigurationFile;

    public void Dispose()
    {
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.ReleaseClient = releaseClient;
        ConfigHelper.UpdateInstaller = updateInstaller;
        ConfigHelper.LocalAppData = localAppData;
        ConfigHelper.ProfileFolder = profileFolder;
        ConfigHelper.ConfigurationFile = configurationFile;
        ConfigHelper.LegacyConfigurationFile = legacyConfigurationFile;
    }
}
