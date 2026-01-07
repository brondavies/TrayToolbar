using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using R = TrayToolbar.Resources.Resources;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TrayToolbar;

internal class UpdateHelper
{
    const string DOWNLOAD_URL = "https://github.com/brondavies/TrayToolbar/releases/download";

    internal static void DownloadAndUpdate(string version)
    {
        var latestVersion = new Version(version.TrimStart('v'));
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "arm64",
                _ => "x64"
            };
            var downloadUrl = $"{DOWNLOAD_URL}/v{latestVersion}/TrayToolbar-win-{arch}-portable-{latestVersion}.zip";
            var temp = Path.GetTempPath();
            var fileName = $"TrayToolbar-Update-{latestVersion}";
            var zipFileName = Path.Combine(temp, $"{fileName}.zip");
            var exeFileName = Path.Combine(temp, $"TrayToolbar.exe");
            var client = new HttpClient();
            {
                client.GetAsync(downloadUrl).ContinueWith(d =>
                {
                    if (!d.IsCompletedSuccessfully) return; //Show an error?
                    var response = d.Result;
                    response.EnsureSuccessStatusCode();
                    var fs = new FileStream(zipFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    response.Content.CopyToAsync(fs).ContinueWith(_ =>
                    {
                        fs.Flush();
                        fs.Close();
                        ZipFile.ExtractToDirectory(zipFileName, temp, true);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exeFileName,
                            Arguments = "--update " + $"\"{ConfigHelper.ApplicationExe}\"",
                            UseShellExecute = true
                        });
                    });
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal static bool ProcessUpdate()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("--update") && args.Length == 2)
        {
            var targetExe = args[1];
            var currentExe = ConfigHelper.ApplicationExe;
            var retries = 3;
            var success = false;

            while (0 < retries && !success)
            {
                try
                {
                    PInvoke.PostMessage(HWND.HWND_BROADCAST, Program.WM_EXITSETTINGSFORM, 0, 0);
                    Thread.Sleep(2000); //wait for existing process to exit
                    File.Copy(currentExe, targetExe, true);
                    success = true;
                }
                catch
                {
                    retries--;
                }
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = targetExe,
                ArgumentList = { "--show", "--newversion" },
                UseShellExecute = true
            });
            return true;
        }
        return false;
    }
}
