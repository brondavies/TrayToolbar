using System.Runtime.InteropServices;

namespace TrayToolbar.Tests;

[TestClass]
public class AboutInfoTests
{
    [TestMethod]
    public void Version_matches_the_project_version()
    {
        Assert.IsTrue(Version.TryParse(AboutInfo.Version, out var version), $"'{AboutInfo.Version}' is not a version");
        Assert.AreEqual(3, version.ToString().Split('.').Length);
        Assert.AreEqual(ConfigHelper.ApplicationVersion, AboutInfo.Version);
    }

    [TestMethod]
    public void Architecture_is_the_lower_case_process_architecture()
    {
        Assert.AreEqual(RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(), AboutInfo.Architecture);
        CollectionAssert.Contains(new[] { "x64", "arm64" }, AboutInfo.Architecture);
    }

    [TestMethod]
    public void Runtime_and_copyright_are_populated()
    {
        StringAssert.StartsWith(AboutInfo.Runtime, ".NET");
        StringAssert.Contains(AboutInfo.Copyright, "Brontech");
    }

    [TestMethod]
    public void Links_point_at_the_project()
    {
        Assert.AreEqual("https://github.com/brondavies/TrayToolbar/releases", AboutInfo.ReleasesUrl);
        StringAssert.StartsWith(AboutInfo.IssuesUrl, "https://github.com/brondavies/TrayToolbar/issues");
        Assert.IsTrue(UpdateLogic.TryGetAllowedRemoteLaunchUri(AboutInfo.ReleasesUrl, out _));
    }
}
