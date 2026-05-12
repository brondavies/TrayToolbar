namespace TrayToolbar.Tests;

[TestClass]
public class UpdateHelperTests
{
    [TestMethod]
    public void CreateUpdaterStartInfo_uses_expected_update_contract()
    {
        var startInfo = UpdateHelper.CreateUpdaterStartInfo(@"C:\Stage\TrayToolbar.exe", @"C:\Installed\TrayToolbar.exe");

        Assert.AreEqual(@"C:\Stage\TrayToolbar.exe", startInfo.FileName);
        Assert.IsTrue(startInfo.UseShellExecute);
        CollectionAssert.AreEqual(new[] { "--update", @"C:\Installed\TrayToolbar.exe" }, startInfo.ArgumentList.ToArray());
    }

    [TestMethod]
    public void CreateRestartStartInfo_uses_show_and_newversion_arguments()
    {
        var startInfo = UpdateHelper.CreateRestartStartInfo(@"C:\Installed\TrayToolbar.exe");

        Assert.AreEqual(@"C:\Installed\TrayToolbar.exe", startInfo.FileName);
        Assert.IsFalse(startInfo.UseShellExecute);
        CollectionAssert.AreEqual(new[] { "--show", "--newversion" }, startInfo.ArgumentList.ToArray());
    }
}
