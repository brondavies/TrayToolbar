using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using TrayToolbar.Extensions;
using TrayToolbar.Models;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar
{
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

        internal static string ApplicationExe => Environment.ProcessPath!;
        internal static readonly string ApplicationRoot = new FileInfo(ApplicationExe!).DirectoryName!;
        internal static readonly string ApplicationVersion = Application.ProductVersion.Split('+')[0];
        internal static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        internal static string ProfileFolder => Path.Combine(LocalAppData, "TrayToolbar");
        internal static string ConfigurationFile = Path.Combine(ProfileFolder, "TrayToolbarConfig.json");
        internal static string LegacyConfigurationFile = Path.Combine(ApplicationRoot, "TrayToolbar.json");
        internal static int WindowsMajorVersion = Environment.OSVersion.Version.Build >= 22000 ? 11 : 10;

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
            try
            {
                if (File.Exists(ConfigurationFile))
                {
                    return JsonSerializer.Deserialize<TrayToolbarConfiguration>(File.ReadAllText(ConfigurationFile))!;
                }
            }
            catch { }

            return new();
        }

        static readonly JsonSerializerOptions jsonOption = new() { WriteIndented = true };
        internal static bool WriteConfiguration(TrayToolbarConfiguration configuration)
        {
            try
            {
                if (!Directory.Exists(ProfileFolder))
                {
                    Directory.CreateDirectory(ProfileFolder);
                }
                var json = JsonSerializer.Serialize(configuration, jsonOption);
                if (File.Exists(ConfigurationFile))
                {
                    File.Delete(ConfigurationFile);
                }
                File.WriteAllText(ConfigurationFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        internal static void MigrateConfiguration()
        {
            try
            {
                if (!File.Exists(ConfigurationFile) && File.Exists(LegacyConfigurationFile))
                {
                    if (!Directory.Exists(ProfileFolder))
                    {
                        Directory.CreateDirectory(ProfileFolder);
                    }
                    File.Move(LegacyConfigurationFile, ConfigurationFile);
                }
            }
            catch { }
        }

        internal static async Task<Release?> CheckForUpdate()
        {
            Release? version = null;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new("application/json"));
            try
            {
                version = await client.GetFromJsonAsync<Release?>(UPDATE_URL);
            }
            catch { }
            return version;
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

        internal static void UpdateToLatestVersion()
        {
            CheckForUpdate().ContinueWith(r =>
            {
                if (r.Result?.Name != null && r.Result.Name != "v" + ApplicationVersion)
                {
                    UpdateHelper.DownloadAndUpdate(r.Result.Name);
                }
            });
        }
    }
}