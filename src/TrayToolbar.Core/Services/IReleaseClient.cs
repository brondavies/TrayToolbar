using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal interface IReleaseClient
{
    Task<Release?> GetLatestReleaseAsync();
}