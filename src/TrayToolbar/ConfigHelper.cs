using Microsoft.Win32;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using R = TrayToolbar.Resources;

namespace TrayToolbar
{
    internal class ConfigHelper
    {
        const string REGKEY_STARTUP = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string REGKEY_SHOWINTRAY = @"Software\Microsoft\Windows\CurrentVersion\RunNotification"; //StartupTNotiTrayToolbar (DWORD) = 1
        const string UPDATE_URL = "https://github.com/brondavies/TrayToolbar/releases/latest";

        internal static readonly string ApplicationRoot =
            new FileInfo(
                    Assembly.GetExecutingAssembly().Location
                ).DirectoryName!;
        internal static readonly string ApplicationVersion = Application.ProductVersion.Split('+')[0];
        internal static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        internal static string ProfileFolder => Path.Combine(LocalAppData, "TrayToolbar");
        internal static string ConfigurationFile = Path.Combine(ProfileFolder, "TrayToolbarConfig.json");
        internal static string LegacyConfigurationFile = Path.Combine(ApplicationRoot, "TrayToolbar.json");

        internal static bool GetStartupKey()
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_STARTUP);
            var val = key.GetValue("TrayToolbar");
            var value = (string?)val == Assembly.GetExecutingAssembly().Location;

            key.Close();
            return value;
        }

        internal static void SetStartupKey(bool value)
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(REGKEY_STARTUP);
            if (value)
            {
                key.SetValue("TrayToolbar", Assembly.GetExecutingAssembly().Location, RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue("TrayToolbar");
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
                File.WriteAllText(ConfigurationFile, json);
                File.SetAttributes(ConfigurationFile, FileAttributes.Hidden);
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
    }
}