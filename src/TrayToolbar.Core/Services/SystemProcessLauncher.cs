using System.Diagnostics;

namespace TrayToolbar.Services;

internal sealed class SystemProcessLauncher : IProcessLauncher
{
    public void Start(ProcessStartInfo startInfo)
    {
        Process.Start(startInfo);
    }
}