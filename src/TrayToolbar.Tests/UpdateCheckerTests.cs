using TrayToolbar.Models;
using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class UpdateCheckerTests
{
    [TestMethod]
    [DataRow(true, true, true, true)]
    [DataRow(true, false, true, false)]
    [DataRow(false, true, false, false)]
    [DataRow(false, false, false, false)]
    public void Update_checks_run_only_when_the_user_has_not_opted_out(bool checkForUpdates, bool notify, bool expectStartupCheck, bool expectPeriodicCheck)
    {
        var configuration = new TrayToolbarConfiguration { CheckForUpdates = checkForUpdates, NotifyOnUpdateAvailable = notify };

        Assert.AreEqual(expectStartupCheck, UpdateChecker.ShouldCheckOnStartup(configuration));
        Assert.AreEqual(expectPeriodicCheck, UpdateChecker.ShouldCheckPeriodically(configuration));
    }

    [TestMethod]
    public void Check_for_updates_is_on_by_default()
    {
        Assert.IsTrue(new TrayToolbarConfiguration().CheckForUpdates);
    }

    [TestMethod]
    public async Task CheckAsync_reports_up_to_date_for_the_running_version()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(CreateRelease($"v{ConfigHelper.ApplicationVersion}"));

        var result = await UpdateChecker.CheckAsync();

        Assert.AreEqual(UpdateCheckStatus.UpToDate, result.Status);
        Assert.IsNull(result.Version);
    }

    [TestMethod]
    public async Task CheckAsync_reports_an_available_update_with_its_release_page()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(CreateRelease("v99.0.0"));

        var result = await UpdateChecker.CheckAsync();

        Assert.AreEqual(UpdateCheckStatus.UpdateAvailable, result.Status);
        Assert.AreEqual("v99.0.0", result.Version);
        Assert.AreEqual("https://github.com/brondavies/TrayToolbar/releases/tag/v99.0.0", result.ReleaseUrl);
    }

    [TestMethod]
    public async Task CheckAsync_reports_a_prerelease_when_the_running_version_is_newer_than_the_latest_release()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(CreateRelease("v0.0.1"));

        var result = await UpdateChecker.CheckAsync();

        Assert.AreEqual(UpdateCheckStatus.Prerelease, result.Status);
        Assert.AreEqual("v0.0.1", result.Version);
    }

    [TestMethod]
    public async Task CheckAsync_reports_failure_when_the_release_metadata_is_unavailable()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(null);

        var result = await UpdateChecker.CheckAsync();

        Assert.AreEqual(UpdateCheckStatus.Failed, result.Status);
    }

    [TestMethod]
    public async Task CheckAsync_reports_failure_when_the_release_client_throws()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.ReleaseClient = new ThrowingReleaseClient();

        var result = await UpdateChecker.CheckAsync();

        Assert.AreEqual(UpdateCheckStatus.Failed, result.Status);
    }

    static Release CreateRelease(string version)
    {
        return new Release
        {
            TagName = version,
            Name = version,
            HtmlUrl = $"https://github.com/brondavies/TrayToolbar/releases/tag/{version}",
            Prerelease = false,
        };
    }

    sealed class FakeReleaseClient(Release? release) : IReleaseClient
    {
        public Task<Release?> GetLatestReleaseAsync() => Task.FromResult(release);
    }

    sealed class ThrowingReleaseClient : IReleaseClient
    {
        public Task<Release?> GetLatestReleaseAsync() => throw new HttpRequestException("offline");
    }
}
