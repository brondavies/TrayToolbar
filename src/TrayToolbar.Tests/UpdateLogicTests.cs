using System.Runtime.InteropServices;
using TrayToolbar.Models;
using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class UpdateLogicTests
{
    [TestMethod]
    public async Task UpdateToLatestVersionAsync_does_not_trigger_for_matching_release()
    {
        using var scope = new ConfigHelperStateScope();
        var installer = new RecordingUpdateInstaller();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(new Release
        {
            Name = $"v{ConfigHelper.ApplicationVersion}",
            UpdateUrl = "/brondavies/TrayToolbar/releases/tag/v" + ConfigHelper.ApplicationVersion
        });
        ConfigHelper.UpdateInstaller = installer;

        var updated = await ConfigHelper.UpdateToLatestVersionAsync();

        Assert.IsFalse(updated);
        Assert.AreEqual(0, installer.RequestedVersions.Count);
    }

    [TestMethod]
    public async Task UpdateToLatestVersionAsync_triggers_for_different_release()
    {
        using var scope = new ConfigHelperStateScope();
        var installer = new RecordingUpdateInstaller();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(new Release
        {
            Name = "v99.0.0",
            UpdateUrl = "/brondavies/TrayToolbar/releases/tag/v99.0.0"
        });
        ConfigHelper.UpdateInstaller = installer;

        var updated = await ConfigHelper.UpdateToLatestVersionAsync();

        Assert.IsTrue(updated);
        CollectionAssert.AreEqual(new[] { "v99.0.0" }, installer.RequestedVersions);
    }

    [TestMethod]
    public void IsPrereleaseVersion_remains_explicit()
    {
        Assert.IsTrue(UpdateLogic.IsPrereleaseVersion("2.0.0", "1.9.0"));
        Assert.IsFalse(UpdateLogic.IsPrereleaseVersion("1.9.0", "2.0.0"));
        Assert.IsFalse(UpdateLogic.IsPrereleaseVersion("1.9.0", "not-a-version"));
    }

    [TestMethod]
    public void TryGetPortableAssetName_uses_architecture_specific_names()
    {
        var arm64Result = UpdateLogic.TryGetPortableAssetName("v1.2.3", Architecture.Arm64, out var arm64Asset);
        var x64Result = UpdateLogic.TryGetPortableAssetName("v1.2.3", Architecture.X64, out var x64Asset);

        Assert.IsTrue(arm64Result);
        Assert.IsTrue(x64Result);
        Assert.AreEqual("TrayToolbar-win-arm64-portable-1.2.3.zip", arm64Asset);
        Assert.AreEqual("TrayToolbar-win-x64-portable-1.2.3.zip", x64Asset);
    }

    [TestMethod]
    public async Task UpdateToLatestVersionAsync_fails_safely_for_missing_or_invalid_release_data()
    {
        using var scope = new ConfigHelperStateScope();
        var installer = new RecordingUpdateInstaller();
        ConfigHelper.ReleaseClient = new FakeReleaseClient(new Release { Name = null, UpdateUrl = null });
        ConfigHelper.UpdateInstaller = installer;

        var missingReleaseUpdated = await ConfigHelper.UpdateToLatestVersionAsync();
        var invalidVersion = UpdateLogic.TryGetPortableDownloadUrl("definitely-not-a-version", Architecture.X64, out var ignoredDownloadUrl);
        var missingUpdate = UpdateLogic.TryGetAvailableUpdate(
            new Release { Name = "v99.0.0", UpdateUrl = null },
            ConfigHelper.ApplicationVersion,
            out var ignoredVersion,
            out var ignoredUpdateUrl);

        Assert.IsFalse(missingReleaseUpdated);
        Assert.IsFalse(invalidVersion);
        Assert.IsFalse(missingUpdate);
        Assert.AreEqual(0, installer.RequestedVersions.Count);
    }

    sealed class FakeReleaseClient(Release? release) : IReleaseClient
    {
        public Task<Release?> GetLatestReleaseAsync() => Task.FromResult(release);
    }

    sealed class RecordingUpdateInstaller : IUpdateInstaller
    {
        public List<string> RequestedVersions { get; } = [];

        public void DownloadAndUpdate(string version)
        {
            RequestedVersions.Add(version);
        }
    }
}
