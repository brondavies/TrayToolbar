using System.Text.Json;

using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal sealed class ConfigurationStore(IFileSystem fileSystem)
{
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    internal TrayToolbarConfiguration ReadConfiguration(string configurationFile)
    {
        try
        {
            if (fileSystem.FileExists(configurationFile))
            {
                return JsonSerializer.Deserialize<TrayToolbarConfiguration>(fileSystem.ReadAllText(configurationFile)) ?? new();
            }
        }
        catch { }

        return new();
    }

    internal bool WriteConfiguration(
        TrayToolbarConfiguration configuration,
        string profileFolder,
        string configurationFile,
        Action<string>? showError = null)
    {
        try
        {
            if (!fileSystem.DirectoryExists(profileFolder))
            {
                fileSystem.CreateDirectory(profileFolder);
            }

            var json = JsonSerializer.Serialize(configuration, JsonOptions);
            if (fileSystem.FileExists(configurationFile))
            {
                fileSystem.DeleteFile(configurationFile);
            }
            fileSystem.WriteAllText(configurationFile, json);
            return true;
        }
        catch (Exception ex)
        {
            showError?.Invoke(ex.Message);
            return false;
        }
    }

    internal void MigrateConfiguration(string profileFolder, string configurationFile, string legacyConfigurationFile)
    {
        try
        {
            if (!fileSystem.FileExists(configurationFile) && fileSystem.FileExists(legacyConfigurationFile))
            {
                if (!fileSystem.DirectoryExists(profileFolder))
                {
                    fileSystem.CreateDirectory(profileFolder);
                }
                fileSystem.MoveFile(legacyConfigurationFile, configurationFile);
            }
        }
        catch { }
    }
}