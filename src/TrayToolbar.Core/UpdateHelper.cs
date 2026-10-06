using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;

using TrayToolbar.Extensions;
using TrayToolbar.Models;
using TrayToolbar.Services;

namespace TrayToolbar;

internal class UpdateHelper
{
    const string ExpectedUpdaterFileName = "TrayToolbar.exe";
    // A release may ship a folder (runtime libraries plus per-language resources) rather than one exe
    const int MaxArchiveEntries = 512;
    const long MaxArchiveBytes = 512L * 1024 * 1024;
    const string UpdateVerificationFailureMessage = "The downloaded update could not be verified and was not installed.";
    static readonly TimeSpan StaleUpdateAge = TimeSpan.FromDays(1);

    internal static string UpdatesRootDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "TrayToolbar", "Updates");

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
            var updaterPath = ExtractUpdatePackage(zipFileName, extractionDirectory);
            EnsureTrustedLibraries(extractionDirectory, updaterPath);
            StartVerifiedUpdater(updaterPath, ConfigHelper.ApplicationExe, operationDirectory);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TrayToolbar update failed: {ex}");
            CleanupDirectory(operationDirectory);
            ConfigHelper.ReportError(ex.Message);
        }
    }

    internal static bool ProcessUpdate()
    {
        var args = Environment.GetCommandLineArgs();
        if (TryGetUpdateTargetExe(args, Path.GetFileName(ConfigHelper.ApplicationExe), out var targetExe))
        {
            try
            {
                _ = ApplyVerifiedUpdate(
                    ConfigHelper.ApplicationExe,
                    targetExe,
                    () =>
                    {
                        InstanceMessages.RequestExit();
                        Thread.Sleep(2000); //wait for existing process to exit
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrayToolbar staged update failed: {ex}");
                ConfigHelper.ReportError(ex.Message);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// A staged updater cannot delete the folder it runs from, so the next normal start removes
    /// update folders that are older than a day
    /// </summary>
    internal static void CleanupStaleUpdateDirectories()
    {
        try
        {
            if (!Directory.Exists(UpdatesRootDirectory))
            {
                return;
            }

            var cutoff = DateTime.UtcNow - StaleUpdateAge;
            foreach (var directory in Directory.EnumerateDirectories(UpdatesRootDirectory))
            {
                if (Directory.GetCreationTimeUtc(directory) < cutoff)
                {
                    CleanupDirectory(directory);
                }
            }
        }
        catch { }
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

    internal static ProcessStartInfo CreateRestartStartInfo(string targetExe)
    {
        return new ProcessStartInfo
        {
            FileName = targetExe,
            UseShellExecute = false,
            ArgumentList = { "--show", "--newversion" },
        };
    }

    internal static ProcessStartInfo CreateUpdaterStartInfo(string updaterPath, string targetExe)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = updaterPath,
            UseShellExecute = true
        };
        startInfo.ArgumentList.Add("--update");
        startInfo.ArgumentList.Add(targetExe);
        return startInfo;
    }

    internal static void StartVerifiedUpdater(string updaterPath, string targetExe, string? operationDirectory = null)
    {
        try
        {
            EnsureTrustedUpdater(updaterPath);
            StartUpdater(updaterPath, targetExe);
        }
        catch
        {
            CleanupDirectory(operationDirectory);
            throw;
        }
    }

    internal static bool ApplyVerifiedUpdate(string currentExe, string targetExe, Action beforeCopyAttempt)
    {
        EnsureTrustedUpdater(currentExe);

        var stagedDirectory = Path.GetDirectoryName(currentExe)!;
        var targetDirectory = Path.GetDirectoryName(targetExe)!;
        var retries = 3;
        var success = false;

        while (0 < retries && !success)
        {
            try
            {
                beforeCopyAttempt();
                CopyStagedFiles(stagedDirectory, currentExe, targetDirectory, targetExe);
                success = true;
            }
            catch
            {
                retries--;
            }
        }

        if (!success)
        {
            return false;
        }

        ConfigHelper.ProcessLauncher.Start(CreateRestartStartInfo(targetExe));
        return true;
    }

    /// <summary>
    /// Copies everything the new version shipped over the installed version. Libraries and
    /// resources go first and the executable last, so a failed copy leaves the old version
    /// runnable. Files the new version does not ship are left alone; the install folder may
    /// hold the user's own scripts or logs.
    /// </summary>
    internal static void CopyStagedFiles(string stagedDirectory, string stagedExe, string targetDirectory, string targetExe)
    {
        foreach (var source in Directory.EnumerateFiles(stagedDirectory, "*", SearchOption.AllDirectories))
        {
            if (source.Is(stagedExe))
            {
                continue;
            }

            var destination = Path.Combine(targetDirectory, Path.GetRelativePath(stagedDirectory, source));
            if (destination.Is(source))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, true);
        }

        File.Copy(stagedExe, targetExe, true);
    }

    internal static string CreateUpdateVerificationErrorMessage(UpdateSignatureVerificationResult result)
    {
        return $"{UpdateVerificationFailureMessage}{Environment.NewLine}{Environment.NewLine}Reason: {result.UserMessage}";
    }

    static string CreateOperationDirectory(Version version)
    {
        var directory = Path.Combine(
            UpdatesRootDirectory,
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

    /// <summary>
    /// Validates every entry, then extracts the whole package and returns the path of the
    /// updater executable at its root
    /// </summary>
    internal static string ExtractUpdatePackage(string zipFileName, string extractionDirectory)
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

            var entryPath = entry.FullName.Replace('\\', '/');
            if (!IsSafeRelativePath(entryPath))
            {
                throw new InvalidDataException("The update package contains an entry with an unsafe path.");
            }

            if (!Path.GetFileName(entryPath).Is(ExpectedUpdaterFileName))
            {
                continue;
            }

            if (!entryPath.Is(ExpectedUpdaterFileName))
            {
                throw new InvalidDataException("The update package contains an updater executable outside the archive root.");
            }

            if (updaterEntry != null)
            {
                throw new InvalidDataException("The update package contains multiple updater executables.");
            }

            updaterEntry = entry;
        }

        if (updaterEntry == null)
        {
            throw new InvalidDataException("The update package does not contain the expected updater executable.");
        }

        archive.ExtractToDirectory(extractionDirectory, overwriteFiles: false);
        return Path.Combine(extractionDirectory, ExpectedUpdaterFileName);
    }

    static bool IsSafeRelativePath(string entryPath)
    {
        if (!entryPath.HasValue() || Path.IsPathRooted(entryPath) || entryPath.Contains(':'))
        {
            return false;
        }

        return !entryPath.Split('/').Any(segment => segment is "." or "..");
    }

    /// <summary>
    /// Every other executable and library in the package must carry an allowed signature; the
    /// zip digest already covers the non-executable files
    /// </summary>
    internal static void EnsureTrustedLibraries(string extractionDirectory, string updaterPath)
    {
        foreach (var file in Directory.EnumerateFiles(extractionDirectory, "*", SearchOption.AllDirectories))
        {
            if (file.Is(updaterPath) || !file.FileExtension().IsOneOf(".dll", ".exe"))
            {
                continue;
            }

            var result = ConfigHelper.UpdateSignatureVerifier.Verify(file, UpdateSignerPolicy.Libraries);
            if (!result.IsSuccess)
            {
                Debug.WriteLine($"TrayToolbar update signature validation failed for '{file}'. Reason: {result.FailureReason}. {result.DiagnosticMessage}");
                throw new InvalidDataException(CreateUpdateVerificationErrorMessage(result));
            }
        }
    }

    static void StartUpdater(string updaterPath, string targetExe)
    {
        ConfigHelper.ProcessLauncher.Start(CreateUpdaterStartInfo(updaterPath, targetExe));
    }

    static void EnsureTrustedUpdater(string updaterPath)
    {
        var result = ConfigHelper.UpdateSignatureVerifier.VerifyForUpdate(updaterPath);
        if (result.IsSuccess)
        {
            return;
        }

        Debug.WriteLine($"TrayToolbar update signature validation failed for '{updaterPath}'. Reason: {result.FailureReason}. {result.DiagnosticMessage}");
        throw new InvalidDataException(CreateUpdateVerificationErrorMessage(result));
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