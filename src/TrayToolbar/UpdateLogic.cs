using System.Runtime.InteropServices;

using TrayToolbar.Extensions;
using TrayToolbar.Models;

namespace TrayToolbar;

internal static class UpdateLogic
{
    const string DownloadBaseUrl = "https://github.com/brondavies/TrayToolbar/releases/download";

    internal static bool TryGetAvailableUpdate(
        Release? release,
        string currentVersion,
        out string version,
        out string updateUrl)
    {
        version = string.Empty;
        updateUrl = string.Empty;
        if (!TryGetAvailableUpdateVersion(release, currentVersion, out version))
        {
            return false;
        }

        if (!release!.UpdateUrl.HasValue())
        {
            version = string.Empty;
            return false;
        }

        updateUrl = release.UpdateUrl;
        return true;
    }

    internal static bool TryGetAvailableUpdateVersion(Release? release, string currentVersion, out string version)
    {
        version = string.Empty;
        if (release == null || !release.Name.HasValue())
        {
            return false;
        }

        if (!TryParseReleaseVersion(currentVersion, out var current)
            || !TryParseReleaseVersion(release.Name, out var latest)
            || latest == current)
        {
            return false;
        }

        version = release.Name;
        return true;
    }

    internal static bool IsPrereleaseVersion(string currentVersion, string version)
    {
        return TryParseReleaseVersion(currentVersion, out var current)
            && TryParseReleaseVersion(version, out var latest)
            && current.CompareTo(latest) == 1;
    }

    internal static bool TryGetPortableAssetName(string version, Architecture architecture, out string assetName)
    {
        assetName = string.Empty;
        if (!TryParseReleaseVersion(version, out var parsedVersion))
        {
            return false;
        }

        var arch = architecture == Architecture.Arm64 ? "arm64" : "x64";
        assetName = $"TrayToolbar-win-{arch}-portable-{parsedVersion}.zip";
        return true;
    }

    internal static bool TryGetPortableDownloadUrl(string version, Architecture architecture, out string downloadUrl)
    {
        downloadUrl = string.Empty;
        if (!TryParseReleaseVersion(version, out var parsedVersion)
            || !TryGetPortableAssetName(version, architecture, out var assetName))
        {
            return false;
        }

        downloadUrl = $"{DownloadBaseUrl}/v{parsedVersion}/{assetName}";
        return true;
    }

    internal static bool TryParseReleaseVersion(string version, out Version parsedVersion)
    {
        try
        {
            parsedVersion = Version.Parse(version.TrimStart('v', 'V'));
            return true;
        }
        catch
        {
            parsedVersion = new Version();
            return false;
        }
    }
}