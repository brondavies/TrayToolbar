using TrayToolbar.Extensions;

namespace TrayToolbar.Tests;

[TestClass]
public class ShellIconsTests
{
    // Windows writes property-store sections that repeat key names, and an
    // [InternetShortcut.W] section that repeats URL in UTF-7
    static readonly string[] UrlFileWithRepeatedKeys =
    [
        "[{000214A0-0000-0000-C000-000000000046}]",
        "Prop3=19,11",
        "[InternetShortcut]",
        "IDList=",
        "URL=https://example.com/",
        "IconIndex=3",
        "HotKey=0",
        "[{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}]",
        "Prop3=19,0",
        "[InternetShortcut.W]",
        "URL=+AGgAdAB0AHA-s://example.com/",
    ];

    [TestMethod]
    public void ReadInternetShortcutKeys_tolerates_repeated_keys_and_reads_only_the_shortcut_section()
    {
        var keys = ShellIcons.ReadInternetShortcutKeys(UrlFileWithRepeatedKeys);

        Assert.AreEqual("https://example.com/", keys["url"]);
        Assert.AreEqual("3", keys["IconIndex"]);
        Assert.IsFalse(keys.ContainsKey("Prop3"));
    }

    [TestMethod]
    public void ReadInternetShortcutKeys_keeps_the_last_value_of_a_key_repeated_in_the_section()
    {
        var keys = ShellIcons.ReadInternetShortcutKeys(["[InternetShortcut]", "URL=https://first.example/", "url=https://second.example/"]);

        Assert.AreEqual("https://second.example/", keys["URL"]);
    }

    [TestMethod]
    public void FetchIcon_returns_an_icon_for_url_files_that_repeat_keys()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShellIconsTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var urlPath = Path.Combine(tempDirectory, "Example.url");

        try
        {
            File.WriteAllLines(urlPath, UrlFileWithRepeatedKeys);

            // Before the fix the repeated Prop3 threw, which stalled the menu reload
            var icon = ShellIcons.FetchIcon(urlPath);

            Assert.IsNotNull(icon);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
