using System.Text.RegularExpressions;

using TrayToolbar.Extensions;

namespace TrayToolbar.Models;

/// <summary>
/// Matches files against Include/Exclude file patterns. An entry wrapped in slashes, such as
/// <c>/^[a-z]{4,6}\d{6,}/</c>, is a regular expression tested against the file name only;
/// any other entry is a wildcard-style pattern tested against the full path.
/// </summary>
internal static class FilePattern
{
    const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

    internal static bool IsRegex(string pattern) => pattern.Length > 2 && pattern[0] == '/' && pattern[^1] == '/';

    /// <summary>
    /// Invalid patterns and regular expressions that time out never match
    /// </summary>
    internal static bool Matches(string filePath, string pattern)
    {
        try
        {
            return IsRegex(pattern)
                ? Regex.IsMatch(Path.GetFileName(filePath), pattern[1..^1], Options, MatchTimeout)
                : filePath.IsMatch(ToWildcardRegex(pattern));
        }
        catch (Exception e) when (e is ArgumentException or RegexMatchTimeoutException)
        {
            return false;
        }
    }

    internal static bool IsValid(string pattern)
    {
        try
        {
            _ = new Regex(IsRegex(pattern) ? pattern[1..^1] : ToWildcardRegex(pattern), Options, MatchTimeout);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    static string ToWildcardRegex(string pattern) => "." + pattern.Replace(".", "\\.");
}