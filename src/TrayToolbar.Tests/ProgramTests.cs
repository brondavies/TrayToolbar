using System.Diagnostics;
using System.Runtime.InteropServices;

using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class ProgramTests
{
    [TestMethod]
    public void Launch_returns_true_and_uses_injected_process_launcher_for_file_paths()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        var filePath = @"C:\Approved\shortcut.txt";

        var launched = Program.Launch(filePath);

        Assert.IsTrue(launched);
        Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
        Assert.AreEqual(filePath, processLauncher.StartedProcesses[0].FileName);
        Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
        Assert.AreEqual(0, processLauncher.StartedProcesses[0].ArgumentList.Count);
    }

    [TestMethod]
    public void Launch_returns_true_and_uses_injected_process_launcher_for_directory_paths()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        var directoryPath = @"C:\Approved";

        var launched = Program.Launch(directoryPath);

        Assert.IsTrue(launched);
        Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
        Assert.AreEqual(directoryPath, processLauncher.StartedProcesses[0].FileName);
        Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
    }

    [TestMethod]
    public void Launch_returns_true_and_uses_injected_process_launcher_for_urls()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        var url = "https://github.com/brondavies/TrayToolbar/releases/latest";

        var launched = Program.Launch(url);

        Assert.IsTrue(launched);
        Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
        Assert.AreEqual(url, processLauncher.StartedProcesses[0].FileName);
        Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
    }

    [TestMethod]
    public void Launch_resolves_executable_shortcuts_and_preserves_shortcut_metadata()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShortcutTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var shortcutPath = Path.Combine(tempDirectory, "Magnify.lnk");
        var targetPath = Path.Combine(Environment.SystemDirectory, "magnify.exe");

        try
        {
            CreateShortcut(shortcutPath, targetPath, "/test-argument", tempDirectory, windowStyle: 3);

            var launched = Program.Launch(shortcutPath);

            Assert.IsTrue(launched);
            Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
            Assert.IsTrue(
                string.Equals(targetPath, processLauncher.StartedProcesses[0].FileName, StringComparison.OrdinalIgnoreCase),
                $"Expected shortcut target '{targetPath}' but got '{processLauncher.StartedProcesses[0].FileName}'.");
            Assert.AreEqual("/test-argument", processLauncher.StartedProcesses[0].Arguments);
            Assert.AreEqual(tempDirectory, processLauncher.StartedProcesses[0].WorkingDirectory);
            Assert.AreEqual(ProcessWindowStyle.Maximized, processLauncher.StartedProcesses[0].WindowStyle);
            Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void TryCreateShortcutStartInfo_maps_runas_shortcuts_to_the_runas_verb()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShortcutTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var shortcutPath = Path.Combine(tempDirectory, "Cmd.lnk");
        var targetPath = Path.Combine(Environment.SystemDirectory, "cmd.exe");

        try
        {
            CreateShortcut(shortcutPath, targetPath, "/c exit 0", tempDirectory, runAsUser: true);

            var created = ShortcutTargetResolver.TryCreateShortcutStartInfo(shortcutPath, out var startInfo);

            Assert.IsTrue(created);
            Assert.IsTrue(string.Equals(targetPath, startInfo.FileName, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("/c exit 0", startInfo.Arguments);
            Assert.AreEqual(tempDirectory, startInfo.WorkingDirectory);
            Assert.AreEqual("runas", startInfo.Verb);
            Assert.IsTrue(startInfo.UseShellExecute);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Launch_resolves_non_executable_shortcuts_without_arguments()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShortcutTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var documentPath = Path.Combine(tempDirectory, "notes.txt");
        var shortcutPath = Path.Combine(tempDirectory, "Notes.lnk");
        File.WriteAllText(documentPath, "hello");

        try
        {
            CreateShortcut(shortcutPath, documentPath, string.Empty, tempDirectory);

            var launched = Program.Launch(shortcutPath);

            Assert.IsTrue(launched);
            Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
            Assert.AreEqual(documentPath, processLauncher.StartedProcesses[0].FileName);
            Assert.AreEqual(tempDirectory, processLauncher.StartedProcesses[0].WorkingDirectory);
            Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Launch_keeps_shell_launch_behavior_for_non_executable_shortcuts_with_arguments()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShortcutTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var documentPath = Path.Combine(tempDirectory, "notes.txt");
        var shortcutPath = Path.Combine(tempDirectory, "Notes.lnk");
        File.WriteAllText(documentPath, "hello");

        try
        {
            CreateShortcut(shortcutPath, documentPath, "/unexpected-argument", tempDirectory);

            var launched = Program.Launch(shortcutPath);

            Assert.IsTrue(launched);
            Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
            Assert.AreEqual(shortcutPath, processLauncher.StartedProcesses[0].FileName);
            Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    static void CreateShortcut(
        string shortcutPath,
        string targetPath,
        string arguments,
        string workingDirectory,
        int windowStyle = 1,
        bool runAsUser = false)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        Assert.IsNotNull(shellType);

        dynamic? shell = Activator.CreateInstance(shellType!);
        Assert.IsNotNull(shell);
        dynamic nonNullShell = shell!;

        dynamic? shortcut = nonNullShell.CreateShortcut(shortcutPath);
        Assert.IsNotNull(shortcut);
        dynamic nonNullShortcut = shortcut!;
        nonNullShortcut.TargetPath = targetPath;
        nonNullShortcut.Arguments = arguments;
        nonNullShortcut.WorkingDirectory = workingDirectory;
        nonNullShortcut.WindowStyle = windowStyle;
        nonNullShortcut.Save();

        if (runAsUser)
        {
            SetShortcutRunAsUser(shortcutPath);
        }
    }

    static void SetShortcutRunAsUser(string shortcutPath)
    {
        object? shellLink = null;

        try
        {
            var shellLinkType = Type.GetTypeFromCLSID(ShellLinkClsid, throwOnError: true);
            shellLink = Activator.CreateInstance(shellLinkType!);
            Assert.IsNotNull(shellLink);

            var persistFile = (IPersistFile)shellLink!;
            persistFile.Load(shortcutPath, StgmReadWrite);

            var dataList = (IShellLinkDataList)shellLink!;
            dataList.GetFlags(out var flags);
            dataList.SetFlags(flags | SldfRunAsUser);

            persistFile.Save(shortcutPath, true);
        }
        finally
        {
            if (shellLink != null && Marshal.IsComObject(shellLink))
            {
                Marshal.FinalReleaseComObject(shellLink);
            }
        }
    }

    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [ComImport]
    [Guid("45E2B4AE-B1C3-11D0-B92F-00A0C90312E1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLinkDataList
    {
        void AddDataBlock(IntPtr pDataBlock);
        void CopyDataBlock(uint dwSig, out IntPtr ppDataBlock);
        void RemoveDataBlock(uint dwSig);
        void GetFlags(out uint pdwFlags);
        void SetFlags(uint dwFlags);
    }

    static readonly Guid ShellLinkClsid = new("00021401-0000-0000-C000-000000000046");
    const uint StgmReadWrite = 0x00000002;
    const uint SldfRunAsUser = 0x00002000;
}