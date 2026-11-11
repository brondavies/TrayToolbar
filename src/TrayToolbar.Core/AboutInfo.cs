using System.Reflection;
using System.Runtime.InteropServices;

namespace TrayToolbar;

/// <summary>
/// What the About dialog shows: version, platform and the project links
/// </summary>
internal static class AboutInfo
{
    internal const string ReleasesUrl = UpdateLogic.ReleasesPageUrl;
    internal const string IssuesUrl = "https://github.com/brondavies/TrayToolbar/issues/new/choose";
    internal const string SignPathUrl = "https://signpath.io/";
    internal const string SignPathFoundationUrl = "https://signpath.org/";

    internal static string Version => ConfigHelper.ApplicationVersion;

    internal static string Architecture => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

    internal static string Runtime => RuntimeInformation.FrameworkDescription;

    internal static string Copyright => typeof(AboutInfo).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
}
