using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using TrayToolbar.Extensions;
using TrayToolbar.Models;

namespace TrayToolbar;

internal static class UpdateLogic
{
    const string DownloadBaseUrl = "https://github.com/brondavies/TrayToolbar/releases/download";
    const string ReleaseHost = "github.com";
    const string ReleasePathPrefix = "/brondavies/TrayToolbar/releases";

    static readonly HashSet<string> SupportedZipContentTypes = [
        "application/zip",
        "application/x-zip-compressed",
        "application/octet-stream"
    ];

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

        if (!TryGetReleasePageUrl(release, out updateUrl))
        {
            version = string.Empty;
            return false;
        }

        return true;
    }

    internal static bool TryGetAvailableUpdateVersion(Release? release, string currentVersion, out string version)
    {
        version = string.Empty;
        if (release == null || release.Prerelease || !release.TagName.HasValue())
        {
            return false;
        }

        if (!TryParseReleaseVersion(currentVersion, out var current)
            || !TryParseReleaseVersion(release.TagName, out var latest)
            || latest == current)
        {
            return false;
        }

        version = release.TagName;
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

    internal static bool TryCreateUpdatePackage(
        Release? release,
        string currentVersion,
        Architecture architecture,
        out UpdatePackage? package)
    {
        package = null;
        if (!TryGetAvailableUpdateVersion(release, currentVersion, out var version)
            || !TryGetReleasePageUrl(release, out var releaseUrl)
            || !TryGetPortableAssetName(version, architecture, out var expectedAssetName)
            || !TryGetPortableDownloadUrl(version, architecture, out var expectedDownloadUrl)
            || !TryGetReleaseAsset(release, expectedAssetName, out var asset)
            || !IsSupportedZipAsset(asset, expectedAssetName, expectedDownloadUrl)
            || !TryGetSha256Digest(asset.Digest, out var sha256Digest))
        {
            return false;
        }

        package = new UpdatePackage(
            version,
            architecture,
            releaseUrl,
            expectedAssetName,
            expectedDownloadUrl,
            sha256Digest);

        return true;
    }

    internal static bool TryGetReleasePageUrl(Release? release, out string updateUrl)
    {
        updateUrl = string.Empty;
        if (!TryGetAllowedRemoteLaunchUri(release?.HtmlUrl, out var uri))
        {
            return false;
        }

        updateUrl = uri.AbsoluteUri;
        return true;
    }

    internal static bool TryGetAllowedRemoteLaunchUri(string? value, out Uri uri)
    {
        uri = null!;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var parsedUri)
            || !parsedUri.Scheme.Is(Uri.UriSchemeHttps)
            || !parsedUri.Host.Is(ReleaseHost)
            || !parsedUri.AbsolutePath.StartsWith(ReleasePathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        uri = parsedUri;
        return true;
    }

    internal static bool TryGetSha256Digest(string? digest, out string normalizedDigest)
    {
        normalizedDigest = string.Empty;
        if (!digest.HasValue())
        {
            return false;
        }

        var match = Regex.Match(digest, "^sha256:([0-9a-fA-F]{64})$", RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return false;
        }

        normalizedDigest = match.Groups[1].Value.ToUpperInvariant();
        return true;
    }

    static bool TryGetReleaseAsset(Release? release, string expectedAssetName, out ReleaseAsset asset)
    {
        asset = null!;
        if (release?.Assets == null)
        {
            return false;
        }

        var matches = release.Assets
            .Where(a => a.Name.Is(expectedAssetName))
            .Take(2)
            .ToArray();

        if (matches.Length != 1)
        {
            return false;
        }

        asset = matches[0];
        return true;
    }

    static bool IsSupportedZipAsset(ReleaseAsset asset, string expectedAssetName, string expectedDownloadUrl)
    {
        return asset.Name.Is(expectedAssetName)
            && expectedAssetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            && asset.BrowserDownloadUrl.Is(expectedDownloadUrl)
            && asset.ContentType.HasValue()
            && SupportedZipContentTypes.Contains(asset.ContentType, StringComparer.OrdinalIgnoreCase);
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