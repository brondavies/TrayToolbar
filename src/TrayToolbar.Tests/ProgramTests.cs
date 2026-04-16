namespace TrayToolbar.Tests;

[TestClass]
public class ProgramTests
{
    [TestMethod]
    public void CanLaunch_allows_existing_file_and_directory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ProgramTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "shortcut.txt");
        File.WriteAllText(file, "hello");

        try
        {
            Assert.IsTrue(Program.CanLaunch(directory));
            Assert.IsTrue(Program.CanLaunch(file));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void CanLaunch_restricts_direct_remote_urls_to_expected_release_pages()
    {
        Assert.IsTrue(Program.CanLaunch("https://github.com/brondavies/TrayToolbar/releases/latest"));
        Assert.IsFalse(Program.CanLaunch("https://github.com/brondavies/OtherRepo/releases/latest"));
        Assert.IsFalse(Program.CanLaunch("https://example.com/anything"));
        Assert.IsFalse(Program.CanLaunch("http://github.com/brondavies/TrayToolbar/releases/latest"));
    }
}