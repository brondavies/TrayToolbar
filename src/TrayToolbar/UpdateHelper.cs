using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using TrayToolbar.Extensions;
using TrayToolbar.Models;

using Windows.Win32;
using Windows.Win32.Foundation;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

internal class UpdateHelper
{
    const string ExpectedUpdaterFileName = "TrayToolbar.exe";
    const int MaxArchiveEntries = 32;
    const long MaxArchiveBytes = 512L * 1024 * 1024;

    internal static void DownloadAndUpdate(UpdatePackage package)
    {
        _ = DownloadAndUpdateAsync(package);
    }

    internal static async Task DownloadAndUpdateAsync(UpdatePackage package, CancellationToken cancellationToken = default)
    {
        string? operationDirectory = null;
        try
        {
            if (!UpdateLogic.TryParseReleaseVersion(package.Version, out var latestVersion)
                || !UpdateLogic.TryGetPortableDownloadUrl(package.Version, package.Architecture, out var downloadUrl)
                || !downloadUrl.Is(package.DownloadUrl))
            {
                return;
            }

            operationDirectory = CreateOperationDirectory(latestVersion);
            var zipFileName = Path.Combine(operationDirectory, package.AssetName);
            var extractionDirectory = Path.Combine(operationDirectory, "extract");
            Directory.CreateDirectory(extractionDirectory);

            using var client = new HttpClient();
            using var response = await client.GetAsync(package.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using (var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            {
                await using var fileStream = new FileStream(zipFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await downloadStream.CopyToAsync(fileStream, cancellationToken);
            }

            VerifyDownloadedArchive(zipFileName, package);
            var updaterPath = ExtractUpdaterExecutable(zipFileName, extractionDirectory);
            StartUpdater(updaterPath, ConfigHelper.ApplicationExe);
        }
        catch (Exception ex)
        {
            CleanupDirectory(operationDirectory);
            MessageBox.Show(ex.Message, R.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal static bool ProcessUpdate()
    {
        var args = Environment.GetCommandLineArgs();
        if (TryGetUpdateTargetExe(args, Path.GetFileName(ConfigHelper.ApplicationExe), out var targetExe))
        {
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
            if (!success)
            {
                return true;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = targetExe,
                UseShellExecute = false,
                ArgumentList = { "--show", "--newversion" },
            });
            return true;
        }
        return false;
    }

    internal static bool TryGetUpdateTargetExe(string[] args, string expectedFileName, out string targetExe)
    {
        targetExe = string.Empty;
        var updateIndex = Array.IndexOf(args, "--update");
        if (updateIndex < 0 || updateIndex + 1 >= args.Length)
        {
            return false;
        }

        var candidate = args[updateIndex + 1];
        if (!candidate.HasValue() || !Path.IsPathFullyQualified(candidate))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(candidate);
        var directory = Path.GetDirectoryName(fullPath);
        if (!Path.GetFileName(fullPath).Is(expectedFileName)
            || !directory.HasValue()
            || !Directory.Exists(directory))
        {
            return false;
        }

        targetExe = fullPath;
        return true;
    }

    static string CreateOperationDirectory(Version version)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "TrayToolbar",
            "Updates",
            $"{version}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    static void VerifyDownloadedArchive(string zipFileName, UpdatePackage package)
    {
        using var stream = File.OpenRead(zipFileName);
        var hash = SHA256.HashData(stream);
        var actualDigest = Convert.ToHexString(hash);
        if (!actualDigest.Is(package.Sha256Digest))
        {
            throw new InvalidDataException("The downloaded update package failed SHA-256 verification.");
        }
    }

    static string ExtractUpdaterExecutable(string zipFileName, string extractionDirectory)
    {
        using var archive = ZipFile.OpenRead(zipFileName);
        if (archive.Entries.Count == 0 || archive.Entries.Count > MaxArchiveEntries)
        {
            throw new InvalidDataException("The update package contains an unexpected number of files.");
        }

        long totalUncompressedBytes = 0;
        ZipArchiveEntry? updaterEntry = null;
        foreach (var entry in archive.Entries)
        {
            totalUncompressedBytes += entry.Length;
            if (totalUncompressedBytes > MaxArchiveBytes)
            {
                throw new InvalidDataException("The update package is larger than expected.");
            }

            if (entry.FullName.Is(ExpectedUpdaterFileName))
            {
                if (updaterEntry != null)
                {
                    throw new InvalidDataException("The update package contains multiple updater executables.");
                }

                updaterEntry = entry;
            }
        }

        if (updaterEntry == null)
        {
            throw new InvalidDataException("The update package does not contain the expected updater executable.");
        }

        var outputPath = Path.Combine(extractionDirectory, ExpectedUpdaterFileName);
        updaterEntry.ExtractToFile(outputPath, overwrite: true);
        return outputPath;
    }

    static void StartUpdater(string updaterPath, string targetExe)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = updaterPath,
            UseShellExecute = true
        };
        startInfo.ArgumentList.Add("--update");
        startInfo.ArgumentList.Add(targetExe);
        Process.Start(startInfo);
    }

    static void CleanupDirectory(string? operationDirectory)
    {
        if (!operationDirectory.HasValue() || !Directory.Exists(operationDirectory))
        {
            return;
        }

        try
        {
            Directory.Delete(operationDirectory, recursive: true);
        }
        catch { }
    }
}