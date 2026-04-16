using TrayToolbar.Models;

namespace TrayToolbar.Tests;

[TestClass]
public class ConfigHelperTests
{
    [TestMethod]
    public void ReadConfiguration_returns_defaults_when_config_file_is_missing()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.FileSystem = new FakeFileSystem();
        ConfigHelper.ConfigurationFile = @"C:\Profiles\TrayToolbar\TrayToolbarConfig.json";

        var configuration = ConfigHelper.ReadConfiguration();

        Assert.AreEqual(0, configuration.Folders.Count);
        Assert.IsTrue(configuration.CheckForUpdates);
        CollectionAssert.AreEqual(new[] { ".bak", ".config", ".dll", ".ico", ".ini" }, configuration.IgnoreFiles);
    }

    [TestMethod]
    public void ReadConfiguration_tolerates_malformed_json()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.ConfigurationFile = @"C:\Profiles\TrayToolbar\TrayToolbarConfig.json";
        fileSystem.AddFile(ConfigHelper.ConfigurationFile, "{ this is not valid json");

        var configuration = ConfigHelper.ReadConfiguration();

        Assert.AreEqual(0, configuration.Folders.Count);
        Assert.IsTrue(configuration.CheckForUpdates);
    }

    [TestMethod]
    public void WriteConfiguration_creates_profile_directory_and_round_trips_key_fields()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.LocalAppData = @"C:\Profiles";
        ConfigHelper.ProfileFolder = @"C:\Profiles\TrayToolbar";
        ConfigHelper.ConfigurationFile = Path.Combine(ConfigHelper.ProfileFolder, "TrayToolbarConfig.json");

        var configuration = new TrayToolbarConfiguration
        {
            Theme = -1,
            CheckForUpdates = false,
            NotifyOnUpdateAvailable = true,
            UpdateCheckInterval = 30,
            ShowFolderLinksAsSubMenus = true,
            IgnoreFiles = [".log", ".tmp"],
            IncludeFiles = ["*.exe", "readme.*"],
            Folders = [new FolderConfig { Name = @"C:\Shortcuts", Recursive = true, Hotkey = "Ctrl+Alt+T" }]
        };

        var written = ConfigHelper.WriteConfiguration(configuration);
        var roundTrip = ConfigHelper.ReadConfiguration();

        Assert.IsTrue(written);
        Assert.IsTrue(fileSystem.DirectoryExists(ConfigHelper.ProfileFolder));
        Assert.AreEqual(configuration.Theme, roundTrip.Theme);
        Assert.AreEqual(configuration.CheckForUpdates, roundTrip.CheckForUpdates);
        Assert.AreEqual(configuration.NotifyOnUpdateAvailable, roundTrip.NotifyOnUpdateAvailable);
        Assert.AreEqual(configuration.UpdateCheckInterval, roundTrip.UpdateCheckInterval);
        Assert.AreEqual(configuration.ShowFolderLinksAsSubMenus, roundTrip.ShowFolderLinksAsSubMenus);
        CollectionAssert.AreEqual(configuration.IgnoreFiles, roundTrip.IgnoreFiles);
        CollectionAssert.AreEqual(configuration.IncludeFiles, roundTrip.IncludeFiles);
        Assert.AreEqual(configuration.Folders.Count, roundTrip.Folders.Count);
        Assert.AreEqual(configuration.Folders[0].Name, roundTrip.Folders[0].Name);
        Assert.AreEqual(configuration.Folders[0].Recursive, roundTrip.Folders[0].Recursive);
        Assert.AreEqual(configuration.Folders[0].Hotkey, roundTrip.Folders[0].Hotkey);
    }

    [TestMethod]
    public void MigrateConfiguration_moves_legacy_file_only_when_new_file_is_missing()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.LocalAppData = @"C:\Profiles";
        ConfigHelper.ProfileFolder = @"C:\Profiles\TrayToolbar";
        ConfigHelper.ConfigurationFile = Path.Combine(ConfigHelper.ProfileFolder, "TrayToolbarConfig.json");
        ConfigHelper.LegacyConfigurationFile = @"C:\App\TrayToolbar.json";
        fileSystem.AddFile(ConfigHelper.LegacyConfigurationFile, "{\"Theme\":1}");

        ConfigHelper.MigrateConfiguration();

        Assert.IsFalse(fileSystem.FileExists(ConfigHelper.LegacyConfigurationFile));
        Assert.IsTrue(fileSystem.FileExists(ConfigHelper.ConfigurationFile));
    }

    [TestMethod]
    public void MigrateConfiguration_does_not_overwrite_existing_new_config()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.LocalAppData = @"C:\Profiles";
        ConfigHelper.ProfileFolder = @"C:\Profiles\TrayToolbar";
        ConfigHelper.ConfigurationFile = Path.Combine(ConfigHelper.ProfileFolder, "TrayToolbarConfig.json");
        ConfigHelper.LegacyConfigurationFile = @"C:\App\TrayToolbar.json";
        fileSystem.AddFile(ConfigHelper.ConfigurationFile, "{\"Theme\":-1}");
        fileSystem.AddFile(ConfigHelper.LegacyConfigurationFile, "{\"Theme\":1}");

        ConfigHelper.MigrateConfiguration();

        Assert.IsTrue(fileSystem.FileExists(ConfigHelper.LegacyConfigurationFile));
        Assert.AreEqual("{\"Theme\":-1}", fileSystem.GetFileContents(ConfigHelper.ConfigurationFile));
    }

    [TestMethod]
    public void ReadConfiguration_migrates_legacy_json_properties_into_current_schema()
    {
        using var scope = new ConfigHelperStateScope();
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.ConfigurationFile = @"C:\Profiles\TrayToolbar\TrayToolbarConfig.json";
        fileSystem.AddFile(
            ConfigHelper.ConfigurationFile,
            """
            {
              "Folder": "C:\\LegacyShortcuts",
              "IgnoreFileTypes": [".tmp", ".bak"]
            }
            """);

        var configuration = ConfigHelper.ReadConfiguration();

        Assert.AreEqual(1, configuration.Folders.Count);
        Assert.AreEqual(@"C:\LegacyShortcuts", configuration.Folders[0].Name);
        CollectionAssert.AreEqual(new[] { ".tmp", ".bak" }, configuration.IgnoreFiles);
    }
}
