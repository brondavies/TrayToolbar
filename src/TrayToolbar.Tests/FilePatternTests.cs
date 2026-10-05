using System.Diagnostics;

using TrayToolbar.Extensions;
using TrayToolbar.Models;

namespace TrayToolbar.Tests;

[TestClass]
public class FilePatternTests
{
    const string HoneypotPattern = @"/^[a-z]{4,6}\d{6,}/";

    [TestMethod]
    [DataRow(@"C:\Desktop\SWJJ123456.docx")]
    [DataRow(@"C:\Desktop\WHkT98765432.xlsx")]
    [DataRow(@"C:\Desktop\abcdef000000")]
    public void IncludesFile_excludes_files_matching_a_regex_pattern(string file)
    {
        var configuration = new TrayToolbarConfiguration { IgnoreFiles = [HoneypotPattern] };

        Assert.IsFalse(configuration.IncludesFile(file));
    }

    [TestMethod]
    [DataRow(@"C:\Desktop\Report2024.docx")]
    [DataRow(@"C:\Desktop\SWJJ12345.docx")]
    [DataRow(@"C:\Desktop\Notes.txt")]
    public void IncludesFile_keeps_files_not_matching_a_regex_pattern(string file)
    {
        var configuration = new TrayToolbarConfiguration { IgnoreFiles = [HoneypotPattern] };

        Assert.IsTrue(configuration.IncludesFile(file));
    }

    [TestMethod]
    public void Regex_patterns_match_the_file_name_and_not_the_folder_path()
    {
        Assert.IsFalse(FilePattern.Matches(@"C:\SWJJ123456\Report.docx", HoneypotPattern));
        Assert.IsTrue(FilePattern.Matches(@"C:\Folder\SWJJ123456.docx", HoneypotPattern));
    }

    [TestMethod]
    public void Regex_patterns_are_case_insensitive()
    {
        Assert.IsTrue(FilePattern.Matches(@"C:\Root\README.MD", "/^readme\\.md$/"));
    }

    [TestMethod]
    public void IncludesFile_supports_regex_patterns_in_IncludeFiles()
    {
        var configuration = new TrayToolbarConfiguration
        {
            IgnoreFiles = [],
            IncludeFiles = [@"/\.(exe|lnk)$/"]
        };

        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\tool.exe"));
        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\App.LNK"));
        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\notes.txt"));
    }

    [TestMethod]
    public void IncludesFile_mixes_regex_and_wildcard_patterns()
    {
        var configuration = new TrayToolbarConfiguration { IgnoreFiles = [".bak", HoneypotPattern] };

        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\old.bak"));
        Assert.IsFalse(configuration.IncludesFile(@"C:\Root\SWJJ123456.docx"));
        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\Report.docx"));
    }

    [TestMethod]
    [DataRow("/")]
    [DataRow("//")]
    [DataRow(".bak")]
    [DataRow("*.exe")]
    public void Short_or_unwrapped_entries_are_not_regex_patterns(string pattern)
    {
        Assert.IsFalse(FilePattern.IsRegex(pattern));
    }

    [TestMethod]
    public void Invalid_regex_never_matches_and_does_not_throw()
    {
        Assert.IsFalse(FilePattern.Matches(@"C:\Root\file[.txt", "/file[/"));
    }

    [TestMethod]
    public void Invalid_wildcard_pattern_never_matches_and_does_not_throw()
    {
        Assert.IsFalse(FilePattern.Matches(@"C:\Root\file(.txt", "file("));
    }

    [TestMethod]
    public void Invalid_regex_in_IgnoreFiles_does_not_hide_files()
    {
        var configuration = new TrayToolbarConfiguration { IgnoreFiles = ["/file[/"] };

        Assert.IsTrue(configuration.IncludesFile(@"C:\Root\file[.txt"));
    }

    [TestMethod]
    [DataRow(HoneypotPattern, true)]
    [DataRow(".bak", true)]
    [DataRow("*.exe", true)]
    [DataRow("/file[/", false)]
    [DataRow("/(unclosed/", false)]
    [DataRow("file(", false)]
    public void IsValid_reports_patterns_that_cannot_be_parsed(string pattern, bool expected)
    {
        Assert.AreEqual(expected, FilePattern.IsValid(pattern));
    }

    [TestMethod]
    public void Regex_that_times_out_is_treated_as_no_match()
    {
        var fileName = @"C:\Root\" + new string('a', 5000) + "!";
        var stopwatch = Stopwatch.StartNew();

        var matched = FilePattern.Matches(fileName, "/^(a|aa)+$/");

        stopwatch.Stop();
        Assert.IsFalse(matched);
        Assert.IsLessThan(5000L, stopwatch.ElapsedMilliseconds);
    }

    [TestMethod]
    public void SplitPatterns_keeps_separators_inside_regex_entries()
    {
        var patterns = @"/^[a-z]{4,6}\d{6,}/; .bak, /a;b/ ; .ini".SplitPatterns();

        CollectionAssert.AreEqual(new[] { HoneypotPattern, ".bak", "/a;b/", ".ini" }, patterns);
    }

    [TestMethod]
    public void SplitPatterns_matches_SplitPaths_for_plain_entries()
    {
        const string value = " .bak; .config ,.dll;; *.exe ";

        CollectionAssert.AreEqual(value.SplitPaths(), value.SplitPatterns());
    }

    [TestMethod]
    public void SplitPatterns_splits_an_unterminated_regex_like_a_plain_entry()
    {
        CollectionAssert.AreEqual(new[] { "/abc", ".bak" }, "/abc; .bak".SplitPatterns());
    }

    [TestMethod]
    public void SplitPatterns_round_trips_the_settings_display_format()
    {
        string[] patterns = [".bak", HoneypotPattern, "/x{1,2}/"];

        CollectionAssert.AreEqual(patterns, patterns.Join("; ").SplitPatterns());
    }

    [TestMethod]
    public void SplitPatterns_returns_empty_for_blank_input()
    {
        Assert.IsEmpty("  ".SplitPatterns());
    }
}
