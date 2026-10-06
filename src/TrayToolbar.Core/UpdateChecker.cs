using TrayToolbar.Models;

namespace TrayToolbar;

internal enum UpdateCheckStatus
{
    UpToDate,
    UpdateAvailable,
    Prerelease,
    Failed,
}

internal sealed record UpdateCheckResult(UpdateCheckStatus Status, string? Version, string? ReleaseUrl);

internal static class UpdateChecker
{
    /// <summary>
    /// A user who turns off "Check for updates" never contacts GitHub, which is how someone
    /// staying on the 1.x line opts out of later major versions
    /// </summary>
    internal static bool ShouldCheckOnStartup(TrayToolbarConfiguration configuration)
    {
        return configuration.CheckForUpdates;
    }

    internal static bool ShouldCheckPeriodically(TrayToolbarConfiguration configuration)
    {
        return configuration.CheckForUpdates && configuration.NotifyOnUpdateAvailable;
    }

    internal static async Task<UpdateCheckResult> CheckAsync()
    {
        Release? release;
        try
        {
            release = await ConfigHelper.CheckForUpdate();
        }
        catch
        {
            release = null;
        }

        if (release == null)
        {
            return new UpdateCheckResult(UpdateCheckStatus.Failed, null, null);
        }

        if (!UpdateLogic.TryGetAvailableUpdate(release, ConfigHelper.ApplicationVersion, out var version, out var releaseUrl))
        {
            return new UpdateCheckResult(UpdateCheckStatus.UpToDate, null, null);
        }

        var status = UpdateLogic.IsPrereleaseVersion(ConfigHelper.ApplicationVersion, version)
            ? UpdateCheckStatus.Prerelease
            : UpdateCheckStatus.UpdateAvailable;
        return new UpdateCheckResult(status, version, releaseUrl);
    }
}
