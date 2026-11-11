using System.Text.RegularExpressions;

namespace TrayToolbar.Tests;

[TestClass]
public class ReleaseNotesTests
{
    [TestMethod]
    public void Release_notes_use_absolute_links_because_they_are_posted_as_the_release_body()
    {
        var path = FindRepositoryFile(Path.Combine("docs", "release-notes.md"));

        var relativeLinks = Regex.Matches(File.ReadAllText(path), @"\]\((?<target>[^)\s]+)")
            .Select(match => match.Groups["target"].Value)
            .Where(target => !Regex.IsMatch(target, "^(https?:|mailto:|#)"))
            .ToArray();

        Assert.IsEmpty(relativeLinks,
            "GitHub resolves relative links in a release body against the release page, so use absolute URLs: "
            + string.Join(", ", relativeLinks));
    }

    static string FindRepositoryFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate)) return candidate;
        }

        throw new AssertFailedException($"{relativePath} was not found above {AppContext.BaseDirectory}");
    }
}
