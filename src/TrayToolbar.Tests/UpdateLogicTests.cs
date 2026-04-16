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
        ConfigHelper.ReleaseClient = new FakeReleaseClient(CreateRelease($"v{ConfigHelper.ApplicationVersion}", RuntimeInformation.ProcessArchitecture));
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
        ConfigHelper.ReleaseClient = new FakeReleaseClient(CreateRelease("v99.0.0", RuntimeInformation.ProcessArchitecture));
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
        ConfigHelper.ReleaseClient = new FakeReleaseClient(new Release { TagName = null, HtmlUrl = null });
        ConfigHelper.UpdateInstaller = installer;

        var missingReleaseUpdated = await ConfigHelper.UpdateToLatestVersionAsync();
        var invalidVersion = UpdateLogic.TryGetPortableDownloadUrl("definitely-not-a-version", Architecture.X64, out var ignoredDownloadUrl);
        var missingUpdate = UpdateLogic.TryGetAvailableUpdate(
            new Release { TagName = "v99.0.0", HtmlUrl = null },
            ConfigHelper.ApplicationVersion,
            out var ignoredVersion,
            out var ignoredUpdateUrl);

        Assert.IsFalse(missingReleaseUpdated);
        Assert.IsFalse(invalidVersion);
        Assert.IsFalse(missingUpdate);
        Assert.AreEqual(0, installer.RequestedVersions.Count);
    }

    [TestMethod]
    public void TryCreateUpdatePackage_requires_expected_asset_contract()
    {
        var release = CreateRelease("v9.9.9", Architecture.X64);
        release.Assets[0].Name = "totally-different.zip";

        var canCreatePackage = UpdateLogic.TryCreateUpdatePackage(release, "1.0.0", Architecture.X64, out var package);

        Assert.IsFalse(canCreatePackage);
        Assert.IsNull(package);
    }

    [TestMethod]
    public void TryCreateUpdatePackage_requires_sha256_digest()
    {
        var release = CreateRelease("v9.9.9", Architecture.X64);
        release.Assets[0].Digest = null;

        var canCreatePackage = UpdateLogic.TryCreateUpdatePackage(release, "1.0.0", Architecture.X64, out var package);

        Assert.IsFalse(canCreatePackage);
        Assert.IsNull(package);
    }

    [TestMethod]
    public void TryGetAvailableUpdateVersion_rejects_prerelease_release_metadata()
    {
        var release = CreateRelease("v9.9.9", Architecture.X64, prerelease: true);

        var hasUpdate = UpdateLogic.TryGetAvailableUpdateVersion(release, "1.0.0", out var version);

        Assert.IsFalse(hasUpdate);
        Assert.AreEqual(string.Empty, version);
    }

    [TestMethod]
    public void TryGetAllowedRemoteLaunchUri_accepts_expected_release_url_only()
    {
        var expected = UpdateLogic.TryGetAllowedRemoteLaunchUri("https://github.com/brondavies/TrayToolbar/releases/tag/v1.2.3", out var releaseUri);
        var unexpected = UpdateLogic.TryGetAllowedRemoteLaunchUri("https://example.com/update", out var ignoredUri);

        Assert.IsTrue(expected);
        Assert.AreEqual("https://github.com/brondavies/TrayToolbar/releases/tag/v1.2.3", releaseUri.AbsoluteUri);
        Assert.IsFalse(unexpected);
    }

    [TestMethod]
    public void TryGetUpdateTargetExe_requires_rooted_matching_executable_path()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-UpdateTarget-{Guid.NewGuid():N}");
        var targetPath = Path.Combine(directory, "TrayToolbar.exe");
        var args = new[] { "TrayToolbar.exe", "--update", targetPath };
        Directory.CreateDirectory(directory);

        try
        {
            var valid = UpdateHelper.TryGetUpdateTargetExe(args, "TrayToolbar.exe", out var targetExe);
            var invalid = UpdateHelper.TryGetUpdateTargetExe(["TrayToolbar.exe", "--update", @"relative\TrayToolbar.exe"], "TrayToolbar.exe", out _);

            Assert.IsTrue(valid);
            Assert.AreEqual(targetPath, targetExe);
            Assert.IsFalse(invalid);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    static Release CreateRelease(string version, Architecture architecture, bool prerelease = false)
    {
        UpdateLogic.TryGetPortableAssetName(version, architecture, out var assetName);
        UpdateLogic.TryGetPortableDownloadUrl(version, architecture, out var downloadUrl);

        return new Release
        {
            TagName = version,
            Name = version,
            HtmlUrl = $"https://github.com/brondavies/TrayToolbar/releases/tag/{version}",
            Prerelease = prerelease,
            Assets = [
                new ReleaseAsset
                {
                    Name = assetName,
                    BrowserDownloadUrl = downloadUrl,
                    ContentType = "application/zip",
                    Digest = "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
                }
            ]
        };
    }

    sealed class FakeReleaseClient(Release? release) : IReleaseClient
    {
        public Task<Release?> GetLatestReleaseAsync() => Task.FromResult(release);
    }

    sealed class RecordingUpdateInstaller : IUpdateInstaller
    {
        public List<string> RequestedVersions { get; } = [];

        public void DownloadAndUpdate(UpdatePackage package)
        {
            RequestedVersions.Add(package.Version);
        }
    }
}
