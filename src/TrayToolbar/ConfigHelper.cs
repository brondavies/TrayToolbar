using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;

using Microsoft.Win32;

using TrayToolbar.Extensions;
using TrayToolbar.Models;
using TrayToolbar.Services;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

[SupportedOSPlatform("windows")]
internal class ConfigHelper
{
    const string REGKEY_STARTUP = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    const string REGKEY_SHOWINTRAY = @"Software\Microsoft\Windows\CurrentVersion\RunNotification"; //StartupTNotiTrayToolbar (DWORD) = 1
    const string REGKEY_SYSTEM_ENVIRONMENT = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
    const string REGKEY_USER_ENVIRONMENT = @"Environment";
    const string UPDATE_URL = "https://github.com/brondavies/TrayToolbar/releases/latest";
    const string STARTUP_VALUE = "TrayToolbar";
    const string PathEnvVar = "Path";
    const string UsernameEnvVar = "username";

    internal static IFileSystem FileSystem { get; set; } = new SystemFileSystem();
    internal static IReleaseClient ReleaseClient { get; set; } = new GitHubReleaseClient();
    internal static IUpdateInstaller UpdateInstaller { get; set; } = new UpdateHelperInstaller();

    internal static string ApplicationExe => Environment.ProcessPath!;
    internal static readonly string ApplicationRoot = new FileInfo(ApplicationExe!).DirectoryName!;
    internal static readonly string ApplicationVersion = Application.ProductVersion.Split('+')[0];
    internal static string LocalAppData { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    internal static string ProfileFolder { get; set; } = Path.Combine(LocalAppData, "TrayToolbar");
    internal static string ConfigurationFile = Path.Combine(ProfileFolder, "TrayToolbarConfig.json");
    internal static string LegacyConfigurationFile = Path.Combine(ApplicationRoot, "TrayToolbar.json");
    internal static int WindowsMajorVersion = Environment.OSVersion.Version.Build >= 22000 ? 11 : 10;

    [SupportedOSPlatformGuard("windows10.0.10240.0")]
    internal static bool SupportsToastNotifications => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240);

    internal static bool IsAutoStartupConfigured()
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_STARTUP);
        var val = key.GetValue(STARTUP_VALUE);
        var value = (string?)val == ApplicationExe;

        key.Close();
        return value;
    }

    internal static void SetStartupKey(bool value)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_STARTUP);
        if (value)
        {
            key.SetValue(STARTUP_VALUE, ApplicationExe, RegistryValueKind.String);
        }
        else if (key.GetValueNames().Contains(STARTUP_VALUE))
        {
            if ($"{key.GetValue(STARTUP_VALUE)}".Is(ApplicationExe))
            {
                key.DeleteValue(STARTUP_VALUE);
            }
        }
        key.Close();
    }

    internal static void SetShowInTray()
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_SHOWINTRAY);
        key.SetValue("StartupTNotiTrayToolbar", 1, RegistryValueKind.DWord);
        key.Close();
    }


    internal static TrayToolbarConfiguration ReadConfiguration()
    {
        return new ConfigurationStore(FileSystem).ReadConfiguration(ConfigurationFile);
    }

    internal static bool WriteConfiguration(TrayToolbarConfiguration configuration)
    {
        return new ConfigurationStore(FileSystem).WriteConfiguration(
            configuration,
            ProfileFolder,
            ConfigurationFile,
            message => MessageBox.Show(message, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Error));
    }

    internal static void MigrateConfiguration()
    {
        new ConfigurationStore(FileSystem).MigrateConfiguration(ProfileFolder, ConfigurationFile, LegacyConfigurationFile);
    }

    internal static async Task<Release?> CheckForUpdate()
    {
        return await ReleaseClient.GetLatestReleaseAsync();
    }

    internal static void RefreshProcessEnvironmentFromRegistry()
    {
        try
        {
            var allVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            LoadKeyValues(Registry.LocalMachine.OpenSubKey(REGKEY_SYSTEM_ENVIRONMENT), allVariables);
            // user overrides machine (except Path is merged)
            LoadKeyValues(Registry.CurrentUser.OpenSubKey(REGKEY_USER_ENVIRONMENT), allVariables);

            foreach (var kvp in allVariables)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value, EnvironmentVariableTarget.Process);
            }
        }
        catch { }

        static void LoadKeyValues(RegistryKey? key, Dictionary<string, string> target)
        {
            if (key is null) return;
            using (key)
            {
                foreach (var name in key.GetValueNames().Where(n => !n.Is(UsernameEnvVar)))
                {
                    object? raw = key.GetValue(name);
                    if (raw is null) continue;
                    string value = raw.ToString() ?? string.Empty;

                    try
                    {
                        var kind = key.GetValueKind(name);
                        if (kind == RegistryValueKind.ExpandString)
                        {
                            value = Environment.ExpandEnvironmentVariables(value);
                        }
                    }
                    catch { }

                    // If variable is PATH merge instead of replace
                    if (name.Is(PathEnvVar) && target.TryGetValue(PathEnvVar, out var existingPath))
                    {
                        // Keep existing order. Append only new, non-duplicate segments.
                        var existingSegments = existingPath.SplitPaths([';']).ToList();
                        var existingSet = new HashSet<string>(existingSegments, StringComparer.OrdinalIgnoreCase);
                        var newSegments = value.SplitPaths([';']);
                        foreach (var seg in newSegments)
                        {
                            if (existingSet.Add(seg))
                            {
                                existingSegments.Add(seg);
                            }
                        }

                        target[PathEnvVar] = string.Join(';', existingSegments);
                    }
                    else
                    {
                        target[name] = value;
                    }
                }
            }
        }
    }

    internal static async Task<bool> UpdateToLatestVersionAsync()
    {
        var release = await CheckForUpdate();
        if (!UpdateLogic.TryGetAvailableUpdateVersion(release, ApplicationVersion, out var version))
        {
            return false;
        }

        UpdateInstaller.DownloadAndUpdate(version);
        return true;
    }

    internal static void UpdateToLatestVersion()
    {
        _ = UpdateToLatestVersionAsync();
    }
}

