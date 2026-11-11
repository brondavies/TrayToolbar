using System.Runtime.InteropServices;

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
    public void Launch_opens_the_releases_page()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;

        var launched = Program.Launch(UpdateLogic.ReleasesPageUrl);

        Assert.IsTrue(launched);
        Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
        Assert.AreEqual(UpdateLogic.ReleasesPageUrl, processLauncher.StartedProcesses[0].FileName);
        Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
    }

    [TestMethod]
    [DataRow("magnify.exe", "/test-argument", false, DisplayName = "Executable with arguments")]
    [DataRow("cmd.exe", "/c exit 0", true, DisplayName = "Run as administrator")]
    [DataRow("mmc.exe", @"""%windir%\System32\virtmgmt.msc""", false, DisplayName = "Environment variables in arguments")]
    public void Launch_shell_executes_executable_shortcuts(string target, string arguments, bool runAsUser)
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShortcutTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var shortcutPath = Path.Combine(tempDirectory, "Executable.lnk");

        try
        {
            // The shell expands environment variables and honors the run-as flag only when it
            // launches the .lnk itself, as Explorer does
            CreateShortcut(shortcutPath, Path.Combine(Environment.SystemDirectory, target), arguments, tempDirectory, windowStyle: 3, runAsUser: runAsUser);

            var launched = Program.Launch(shortcutPath);

            Assert.IsTrue(launched);
            Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
            Assert.AreEqual(shortcutPath, processLauncher.StartedProcesses[0].FileName);
            Assert.AreEqual(string.Empty, processLauncher.StartedProcesses[0].Arguments);
            Assert.AreEqual(string.Empty, processLauncher.StartedProcesses[0].Verb);
            Assert.IsTrue(processLauncher.StartedProcesses[0].UseShellExecute);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Launch_shell_executes_advertised_shortcuts_instead_of_the_reported_target()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-ShortcutTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        var shortcutPath = Path.Combine(tempDirectory, "Advertised.lnk");
        var targetPath = Path.Combine(Environment.SystemDirectory, "cmd.exe");

        try
        {
            // An MSI advertised shortcut carries a Darwin descriptor; GetPath reports a stub,
            // so the .lnk itself must be shell-executed for the launch to work
            CreateShortcut(shortcutPath, targetPath, string.Empty, tempDirectory);
            MakeShortcutAdvertised(shortcutPath);

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

    [TestMethod]
    [DataRow("", DisplayName = "Without arguments")]
    [DataRow("/unexpected-argument", DisplayName = "With arguments")]
    public void Launch_shell_executes_non_executable_shortcuts(string arguments)
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
            CreateShortcut(shortcutPath, documentPath, arguments, tempDirectory);

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
            SetShortcutFlags(shortcutPath, SldfRunAsUser);
        }
    }

    static void MakeShortcutAdvertised(string shortcutPath)
    {
        object? shellLink = null;
        var block = IntPtr.Zero;

        try
        {
            var shellLinkType = Type.GetTypeFromCLSID(ShellLinkClsid, throwOnError: true);
            shellLink = Activator.CreateInstance(shellLinkType!);
            Assert.IsNotNull(shellLink);

            var persistFile = (IPersistFile)shellLink!;
            persistFile.Load(shortcutPath, StgmReadWrite);

            // EXP_DARWIN_LINK: DATABLOCK_HEADER + szDarwinID[260] (ANSI) + szwDarwinID[260] (wide);
            // saving the block makes the shell set SLDF_HAS_DARWINID in the link header
            const int blockSize = 4 + 4 + 260 + 520;
            block = Marshal.AllocHGlobal(blockSize);
            for (var offset = 0; offset < blockSize; offset += 4)
            {
                Marshal.WriteInt32(block, offset, 0);
            }
            Marshal.WriteInt32(block, 0, blockSize);
            Marshal.WriteInt32(block, 4, unchecked((int)ExpDarwinIdSignature));
            const string darwinId = "TestDarwinDescriptor";
            var ansiId = System.Text.Encoding.ASCII.GetBytes(darwinId);
            Marshal.Copy(ansiId, 0, block + 8, ansiId.Length);
            var wideId = System.Text.Encoding.Unicode.GetBytes(darwinId);
            Marshal.Copy(wideId, 0, block + 8 + 260, wideId.Length);

            var dataList = (IShellLinkDataList)shellLink!;
            dataList.AddDataBlock(block);
            dataList.GetFlags(out var flags);
            dataList.SetFlags(flags | SldfHasDarwinId);

            persistFile.Save(shortcutPath, true);
        }
        finally
        {
            if (block != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(block);
            }
            if (shellLink != null && Marshal.IsComObject(shellLink))
            {
                Marshal.FinalReleaseComObject(shellLink);
            }
        }
    }

    static void SetShortcutFlags(string shortcutPath, uint additionalFlags)
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
            dataList.SetFlags(flags | additionalFlags);

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
    const uint SldfHasDarwinId = 0x00001000;
    const uint ExpDarwinIdSignature = 0xA0000006;
}