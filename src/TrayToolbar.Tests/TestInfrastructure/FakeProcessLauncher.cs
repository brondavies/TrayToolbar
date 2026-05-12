using System.Diagnostics;

using TrayToolbar.Services;

namespace TrayToolbar.Tests;

internal sealed class FakeProcessLauncher : IProcessLauncher
{
    public List<ProcessStartInfo> StartedProcesses { get; } = [];

    public void Start(ProcessStartInfo startInfo)
    {
        var clone = new ProcessStartInfo
        {
            FileName = startInfo.FileName,
            Arguments = startInfo.Arguments,
            UseShellExecute = startInfo.UseShellExecute,
            WorkingDirectory = startInfo.WorkingDirectory,
            Verb = startInfo.Verb,
            WindowStyle = startInfo.WindowStyle,
        };

        foreach (var argument in startInfo.ArgumentList)
        {
            clone.ArgumentList.Add(argument);
        }

        StartedProcesses.Add(clone);
    }
}
