using BenchmarkDotNet.Running;

namespace TrayToolbar.Benchmarks;

internal static class BenchmarksProgram
{
    static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(BenchmarksProgram).Assembly).Run(args);
    }
}
