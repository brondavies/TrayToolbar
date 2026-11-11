using System.Runtime.InteropServices;

namespace TrayToolbar.Models;

internal sealed record UpdatePackage(
    string Version,
    Architecture Architecture,
    string ReleaseUrl,
    string AssetName,
    string DownloadUrl,
    string Sha256Digest);