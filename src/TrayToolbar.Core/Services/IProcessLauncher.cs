using System.Diagnostics;

namespace TrayToolbar.Services;

internal interface IProcessLauncher
{
    void Start(ProcessStartInfo startInfo);
}