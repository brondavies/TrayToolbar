using Microsoft.Win32;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar
{
    internal class ConfigHelper
    {
        const string REGKEY_STARTUP = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string REGKEY_SHOWINTRAY = @"Software\Microsoft\Windows\CurrentVersion\RunNotification"; //StartupTNotiTrayToolbar (DWORD) = 1
        const string UPDATE_URL = "https://github.com/brondavies/TrayToolbar/releases/latest";
        const string STARTUP_VALUE = "TrayToolbar";

        internal static string ApplicationExe => Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "exe")!;
        internal static readonly string ApplicationRoot = new FileInfo(ApplicationExe!).DirectoryName!;
        internal static readonly string ApplicationVersion = Application.ProductVersion.Split('+')[0];
        internal static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        internal static string ProfileFolder => Path.Combine(LocalAppData, "TrayToolbar");
        internal static string ConfigurationFile = Path.Combine(ProfileFolder, "TrayToolbarConfig.json");
        internal static string LegacyConfigurationFile = Path.Combine(ApplicationRoot, "TrayToolbar.json");

        internal static bool GetStartupKey()
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
            else
            {
                if (key.GetValueNames().Contains(STARTUP_VALUE))
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
    }
}