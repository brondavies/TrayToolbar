using System.Globalization;
using System.Text.Json;

using TrayToolbar.Extensions;
using TrayToolbar.Models;

namespace TrayToolbar.Services;

/// <summary>
/// Optional launch logging. Configuration-file only; disabled by default.
/// Captures item name, target path, timestamp (ISO-8601 in every format), and username.
/// </summary>
internal static class LaunchLogger
{
    internal const string DefaultLogFileName = "launch.log";

    internal static Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.Now;
    internal static Func<string> UserName { get; set; } = () => Environment.UserName;

    internal enum LogFormat
    {
        Csv,
        Tsv,
        Jsonl,
        Syslog,
        Cef,
    }

    static readonly object WriteSync = new();
    static Task PendingWrites = Task.CompletedTask;

    /// <summary>
    /// Captures the entry details immediately and queues the file write to a background
    /// thread so logging never delays the launch. Writes are serialized so entries
    /// cannot interleave.
    /// </summary>
    internal static void Log(TrayToolbarConfiguration? configuration, string itemName, string targetPath)
    {
        try
        {
            if (configuration is not { LaunchLogEnabled: true }) return;
            var format = ParseFormat(configuration.LaunchLogFormat);
            var path = configuration.LaunchLogFile
                .Or(Path.Combine(ConfigHelper.ProfileFolder, DefaultLogFileName))!
                .ToLocalPath();
            var timestamp = Clock().ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
            var user = UserName();
            lock (WriteSync)
            {
                PendingWrites = PendingWrites.ContinueWith(
                    _ => WriteEntry(format, path, timestamp, user, itemName, targetPath),
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);
            }
        }
        catch
        {
            // Logging must never interfere with launching
        }
    }

    /// <summary>
    /// Waits for all queued log writes to finish. Called before the process exits
    /// and by tests that assert on log contents.
    /// </summary>
    internal static void Flush()
    {
        Task pending;
        lock (WriteSync)
        {
            pending = PendingWrites;
        }
        try
        {
            pending.Wait();
        }
        catch
        {
        }
    }

    static void WriteEntry(LogFormat format, string path, string timestamp, string user, string name, string target)
    {
        try
        {
            var header = NeedsHeader(format, path) ? FormatHeader(format) + Environment.NewLine : string.Empty;
            var line = FormatEntry(format, timestamp, user, name, target);
            ConfigHelper.FileSystem.AppendAllText(path, header + line + Environment.NewLine);
        }
        catch
        {
            // Logging must never interfere with launching
        }
    }

    internal static LogFormat ParseFormat(string? format)
    {
        return format?.Trim().ToLowerInvariant() switch
        {
            "tsv" => LogFormat.Tsv,
            "jsonl" => LogFormat.Jsonl,
            "syslog" => LogFormat.Syslog,
            "cef" => LogFormat.Cef,
            _ => LogFormat.Csv,
        };
    }

    static bool NeedsHeader(LogFormat format, string path)
    {
        return (format is LogFormat.Csv or LogFormat.Tsv) && !ConfigHelper.FileSystem.FileExists(path);
    }

    static string FormatHeader(LogFormat format)
    {
        var separator = format == LogFormat.Tsv ? '\t' : ',';
        return string.Join(separator, "timestamp", "user", "name", "target");
    }

    static string FormatEntry(LogFormat format, string timestamp, string user, string name, string target)
    {
        return format switch
        {
            LogFormat.Tsv => string.Join('\t',
                TsvField(timestamp), TsvField(user), TsvField(name), TsvField(target)),
            LogFormat.Jsonl => JsonSerializer.Serialize(new { timestamp, user, name, target }),
            LogFormat.Syslog =>
                $"<14>1 {timestamp} {Environment.MachineName} TrayToolbar {Environment.ProcessId} LAUNCH - " +
                $"user=\"{SingleLine(user)}\" name=\"{SingleLine(name)}\" target=\"{SingleLine(target)}\"",
            LogFormat.Cef =>
                $"CEF:0|brontech|TrayToolbar|{ConfigHelper.ApplicationVersion}|launch|Item launched|3|" +
                $"rt={CefField(timestamp)} suser={CefField(user)} fname={CefField(name)} filePath={CefField(target)}",
            _ => string.Join(',',
                CsvField(timestamp), CsvField(user), CsvField(name), CsvField(target)),
        };
    }

    static string CsvField(string value)
    {
        if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
        {
            return value;
        }
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    static string TsvField(string value)
    {
        return value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
    }

    static string SingleLine(string value)
    {
        return value.Replace('\r', ' ').Replace('\n', ' ');
    }

    static string CefField(string value)
    {
        return SingleLine(value).Replace(@"\", @"\\").Replace("=", @"\=");
    }
}