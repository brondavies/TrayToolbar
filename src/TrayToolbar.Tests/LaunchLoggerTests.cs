using System.Text.Json;

using TrayToolbar.Models;
using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class LaunchLoggerTests
{
    static readonly DateTimeOffset TestTime = new(2026, 9, 2, 13, 45, 30, 123, TimeSpan.FromHours(-6));
    const string TestTimestamp = "2026-09-02T13:45:30.123-06:00";
    const string ProfileFolder = @"C:\Profile\TrayToolbar";

    sealed class LaunchLoggerStateScope : IDisposable
    {
        readonly Func<DateTimeOffset> clock = LaunchLogger.Clock;
        readonly Func<string> userName = LaunchLogger.UserName;

        public void Dispose()
        {
            LaunchLogger.Clock = clock;
            LaunchLogger.UserName = userName;
        }
    }

    static FakeFileSystem Setup()
    {
        var fileSystem = new FakeFileSystem();
        ConfigHelper.FileSystem = fileSystem;
        ConfigHelper.ProfileFolder = ProfileFolder;
        LaunchLogger.Clock = () => TestTime;
        LaunchLogger.UserName = () => "testuser";
        return fileSystem;
    }

    static TrayToolbarConfiguration EnabledConfiguration(string? format = null, string? file = null)
    {
        return new TrayToolbarConfiguration
        {
            LaunchLogEnabled = true,
            LaunchLogFormat = format,
            LaunchLogFile = file,
        };
    }

    static string DefaultLogFile => Path.Combine(ProfileFolder, LaunchLogger.DefaultLogFileName);

    [TestMethod]
    public void Log_is_disabled_by_default()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();

        LaunchLogger.Log(new TrayToolbarConfiguration(), "Notes", @"C:\Root\Notes.lnk");
        LaunchLogger.Flush();

        Assert.IsFalse(new TrayToolbarConfiguration().LaunchLogEnabled);
        Assert.IsFalse(fileSystem.FileExists(DefaultLogFile));
    }

    [TestMethod]
    public void Log_writes_csv_by_default_with_header_and_iso8601_timestamp()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();
        var configuration = EnabledConfiguration();

        LaunchLogger.Log(configuration, "Notes, with comma", @"C:\Root\Notes.lnk");
        LaunchLogger.Log(configuration, "Second", @"C:\Root\Second.lnk");
        LaunchLogger.Flush();

        var lines = fileSystem.GetFileContents(DefaultLogFile)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(3, lines.Length);
        Assert.AreEqual("timestamp,user,name,target", lines[0]);
        Assert.AreEqual($"{TestTimestamp},testuser,\"Notes, with comma\",C:\\Root\\Notes.lnk", lines[1]);
        Assert.AreEqual($"{TestTimestamp},testuser,Second,C:\\Root\\Second.lnk", lines[2]);
    }

    [TestMethod]
    public void Log_writes_tsv_with_header()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();
        var configuration = EnabledConfiguration("tsv");

        LaunchLogger.Log(configuration, "Notes\twith tab", @"C:\Root\Notes.lnk");
        LaunchLogger.Flush();

        var lines = fileSystem.GetFileContents(DefaultLogFile)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(2, lines.Length);
        Assert.AreEqual("timestamp\tuser\tname\ttarget", lines[0]);
        Assert.AreEqual($"{TestTimestamp}\ttestuser\tNotes with tab\tC:\\Root\\Notes.lnk", lines[1]);
    }

    [TestMethod]
    public void Log_writes_jsonl_entries()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();
        var configuration = EnabledConfiguration("jsonl");

        LaunchLogger.Log(configuration, "Notes", @"C:\Root\Notes.lnk");
        LaunchLogger.Flush();

        var lines = fileSystem.GetFileContents(DefaultLogFile)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(1, lines.Length);
        using var document = JsonDocument.Parse(lines[0]);
        Assert.AreEqual(TestTimestamp, document.RootElement.GetProperty("timestamp").GetString());
        Assert.AreEqual("testuser", document.RootElement.GetProperty("user").GetString());
        Assert.AreEqual("Notes", document.RootElement.GetProperty("name").GetString());
        Assert.AreEqual(@"C:\Root\Notes.lnk", document.RootElement.GetProperty("target").GetString());
        Assert.AreEqual(TestTime, DateTimeOffset.Parse(document.RootElement.GetProperty("timestamp").GetString()!));
    }

    [TestMethod]
    public void Log_writes_rfc5424_syslog_lines()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();
        var configuration = EnabledConfiguration("syslog");

        LaunchLogger.Log(configuration, "Notes", @"C:\Root\Notes.lnk");
        LaunchLogger.Flush();

        var lines = fileSystem.GetFileContents(DefaultLogFile)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(1, lines.Length);
        StringAssert.StartsWith(lines[0], $"<14>1 {TestTimestamp} {Environment.MachineName} TrayToolbar ");
        StringAssert.Contains(lines[0], "user=\"testuser\"");
        StringAssert.Contains(lines[0], "name=\"Notes\"");
        StringAssert.Contains(lines[0], "target=\"C:\\Root\\Notes.lnk\"");
    }

    [TestMethod]
    public void Log_writes_cef_lines_with_escaped_extensions()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();
        var configuration = EnabledConfiguration("cef");

        LaunchLogger.Log(configuration, "Name=Equals", @"C:\Root\Notes.lnk");
        LaunchLogger.Flush();

        var lines = fileSystem.GetFileContents(DefaultLogFile)
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(1, lines.Length);
        StringAssert.StartsWith(lines[0], "CEF:0|brondavies|TrayToolbar|");
        StringAssert.Contains(lines[0], $"rt={TestTimestamp}");
        StringAssert.Contains(lines[0], "suser=testuser");
        StringAssert.Contains(lines[0], @"fname=Name\=Equals");
        StringAssert.Contains(lines[0], @"filePath=C:\\Root\\Notes.lnk");
    }

    [TestMethod]
    public void Log_writes_to_the_configured_file_and_expands_environment_variables()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        var fileSystem = Setup();
        Environment.SetEnvironmentVariable("TRAYTOOLBAR_TEST_LOGDIR", @"C:\Logs");
        try
        {
            var configuration = EnabledConfiguration(file: @"%TRAYTOOLBAR_TEST_LOGDIR%\launches.csv");

            LaunchLogger.Log(configuration, "Notes", @"C:\Root\Notes.lnk");
            LaunchLogger.Flush();

            Assert.IsTrue(fileSystem.FileExists(@"C:\Logs\launches.csv"));
            Assert.IsFalse(fileSystem.FileExists(DefaultLogFile));
        }
        finally
        {
            Environment.SetEnvironmentVariable("TRAYTOOLBAR_TEST_LOGDIR", null);
        }
    }

    [TestMethod]
    public void Log_swallows_write_failures()
    {
        using var scope = new ConfigHelperStateScope();
        using var loggerScope = new LaunchLoggerStateScope();
        Setup();
        var configuration = EnabledConfiguration(file: "::::invalid::::");

        LaunchLogger.Log(configuration, "Notes", @"C:\Root\Notes.lnk");
        LaunchLogger.Flush();
        // no exception means logging can never break a launch
    }

    [TestMethod]
    public void ParseFormat_defaults_to_csv_for_unknown_values()
    {
        Assert.AreEqual(LaunchLogger.LogFormat.Csv, LaunchLogger.ParseFormat(null));
        Assert.AreEqual(LaunchLogger.LogFormat.Csv, LaunchLogger.ParseFormat("unknown"));
        Assert.AreEqual(LaunchLogger.LogFormat.Csv, LaunchLogger.ParseFormat("CSV"));
        Assert.AreEqual(LaunchLogger.LogFormat.Tsv, LaunchLogger.ParseFormat(" TSV "));
        Assert.AreEqual(LaunchLogger.LogFormat.Jsonl, LaunchLogger.ParseFormat("jsonl"));
        Assert.AreEqual(LaunchLogger.LogFormat.Syslog, LaunchLogger.ParseFormat("Syslog"));
        Assert.AreEqual(LaunchLogger.LogFormat.Cef, LaunchLogger.ParseFormat("cef"));
    }
}
