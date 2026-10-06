using System.IO.Compression;

using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class UpdateHelperTests
{
    static readonly UpdateSignatureVerificationResult TrustedResult = UpdateSignatureVerificationResult.Success(
        "WinVerifyTrust accepted the staged updater.",
        "CN=Brontech, LLC",
        "Brontech, LLC",
        "ABC123");

    static readonly UpdateSignatureVerificationResult InvalidSignatureResult = UpdateSignatureVerificationResult.Failure(
        UpdateSignatureFailureReason.InvalidSignature,
        "The staged update has an invalid or tampered Authenticode signature.",
        "WinVerifyTrust reported TRUST_E_BAD_DIGEST.");

    [TestMethod]
    public void CreateUpdaterStartInfo_uses_expected_update_contract()
    {
        var startInfo = UpdateHelper.CreateUpdaterStartInfo(@"C:\Stage\TrayToolbar.exe", @"C:\Installed\TrayToolbar.exe");

        Assert.AreEqual(@"C:\Stage\TrayToolbar.exe", startInfo.FileName);
        Assert.IsTrue(startInfo.UseShellExecute);
        CollectionAssert.AreEqual(new[] { "--update", @"C:\Installed\TrayToolbar.exe" }, startInfo.ArgumentList.ToArray());
    }

    [TestMethod]
    public void CreateRestartStartInfo_uses_show_and_newversion_arguments()
    {
        var startInfo = UpdateHelper.CreateRestartStartInfo(@"C:\Installed\TrayToolbar.exe");

        Assert.AreEqual(@"C:\Installed\TrayToolbar.exe", startInfo.FileName);
        Assert.IsFalse(startInfo.UseShellExecute);
        CollectionAssert.AreEqual(new[] { "--show", "--newversion" }, startInfo.ArgumentList.ToArray());
    }

    [TestMethod]
    public void StartVerifiedUpdater_starts_verified_updater()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(TrustedResult);

        using var temp = new TempDirectory();
        var updaterPath = Path.Combine(temp.Path, "TrayToolbar.exe");
        File.WriteAllText(updaterPath, "signed updater placeholder");

        UpdateHelper.StartVerifiedUpdater(updaterPath, @"C:\Installed\TrayToolbar.exe", temp.Path);

        Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
        Assert.AreEqual(updaterPath, processLauncher.StartedProcesses[0].FileName);
        Assert.IsTrue(Directory.Exists(temp.Path));
    }

    [TestMethod]
    public void StartVerifiedUpdater_rejects_invalid_signature_and_cleans_operation_directory()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(InvalidSignatureResult);

        using var temp = new TempDirectory();
        var updaterPath = Path.Combine(temp.Path, "TrayToolbar.exe");
        File.WriteAllText(updaterPath, "unsigned updater placeholder");

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => UpdateHelper.StartVerifiedUpdater(updaterPath, @"C:\Installed\TrayToolbar.exe", temp.Path));

        StringAssert.Contains(exception.Message, "could not be verified");
        StringAssert.Contains(exception.Message, "invalid or tampered Authenticode signature");
        Assert.AreEqual(0, processLauncher.StartedProcesses.Count);
        Assert.IsFalse(Directory.Exists(temp.Path));
    }

    [TestMethod]
    public void StartVerifiedUpdater_rejects_unexpected_publisher()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(
            UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.UnexpectedPublisher,
                "The staged update was signed by an unexpected publisher.",
                "Signer subject did not match the configured publisher allow-list.",
                "CN=Unexpected Publisher",
                "Unexpected Publisher",
                "DEF456"));

        using var temp = new TempDirectory();
        var updaterPath = Path.Combine(temp.Path, "TrayToolbar.exe");
        File.WriteAllText(updaterPath, "unexpected publisher updater placeholder");

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => UpdateHelper.StartVerifiedUpdater(updaterPath, @"C:\Installed\TrayToolbar.exe", temp.Path));

        StringAssert.Contains(exception.Message, "unexpected publisher");
        Assert.AreEqual(0, processLauncher.StartedProcesses.Count);
    }

    [TestMethod]
    public void ApplyVerifiedUpdate_rejects_untrusted_staged_executable_before_copy()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(InvalidSignatureResult);

        using var temp = new TempDirectory();
        var stagedExe = temp.WriteFile(@"stage\TrayToolbar.exe", "new version");
        var installedExe = temp.WriteFile(@"installed\TrayToolbar.exe", "old version");

        _ = Assert.ThrowsExactly<InvalidDataException>(() => UpdateHelper.ApplyVerifiedUpdate(stagedExe, installedExe, () => { }));

        Assert.AreEqual("old version", File.ReadAllText(installedExe));
        Assert.AreEqual(0, processLauncher.StartedProcesses.Count);
    }

    [TestMethod]
    public void ApplyVerifiedUpdate_copies_the_staged_folder_over_the_install_and_restarts_target()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(TrustedResult);

        using var temp = new TempDirectory();
        var stagedExe = temp.WriteFile(@"stage\TrayToolbar.exe", "new version");
        temp.WriteFile(@"stage\TrayToolbar.Core.dll", "new library");
        temp.WriteFile(@"stage\de\TrayToolbar.Core.resources.dll", "new resources");
        var installedExe = temp.WriteFile(@"installed\TrayToolbar.exe", "old version");
        temp.WriteFile(@"installed\TrayToolbar.Core.dll", "old library");
        var usersFile = temp.WriteFile(@"installed\restart.bat", "user's own file");

        var updated = UpdateHelper.ApplyVerifiedUpdate(stagedExe, installedExe, () => { });

        Assert.IsTrue(updated);
        Assert.AreEqual("new version", File.ReadAllText(installedExe));
        Assert.AreEqual("new library", File.ReadAllText(Path.Combine(temp.Path, "installed", "TrayToolbar.Core.dll")));
        Assert.AreEqual("new resources", File.ReadAllText(Path.Combine(temp.Path, "installed", "de", "TrayToolbar.Core.resources.dll")));
        Assert.AreEqual("user's own file", File.ReadAllText(usersFile));
        Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
        Assert.AreEqual(installedExe, processLauncher.StartedProcesses[0].FileName);
        CollectionAssert.AreEqual(new[] { "--show", "--newversion" }, processLauncher.StartedProcesses[0].ArgumentList.ToArray());
    }

    [TestMethod]
    public void ApplyVerifiedUpdate_retries_while_the_installed_executable_is_locked()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(TrustedResult);

        using var temp = new TempDirectory();
        var stagedExe = temp.WriteFile(@"stage\TrayToolbar.exe", "new version");
        var installedExe = temp.WriteFile(@"installed\TrayToolbar.exe", "old version");
        var attempts = 0;
        FileStream? @lock = File.Open(installedExe, FileMode.Open, FileAccess.Read, FileShare.None);

        var updated = UpdateHelper.ApplyVerifiedUpdate(stagedExe, installedExe, () =>
        {
            // the running instance exits after the first attempt
            if (++attempts == 2)
            {
                @lock?.Dispose();
                @lock = null;
            }
        });

        @lock?.Dispose();
        Assert.IsTrue(updated);
        Assert.AreEqual(2, attempts);
        Assert.AreEqual("new version", File.ReadAllText(installedExe));
    }

    [TestMethod]
    public void ExtractUpdatePackage_extracts_a_single_executable_package()
    {
        using var temp = new TempDirectory();
        var zip = temp.CreateZip("update.zip", ("TrayToolbar.exe", "exe"));
        var extract = temp.CreateDirectory("extract");

        var updaterPath = UpdateHelper.ExtractUpdatePackage(zip, extract);

        Assert.AreEqual(Path.Combine(extract, "TrayToolbar.exe"), updaterPath);
        Assert.AreEqual("exe", File.ReadAllText(updaterPath));
    }

    [TestMethod]
    public void ExtractUpdatePackage_extracts_a_folder_layout_with_more_than_32_entries()
    {
        using var temp = new TempDirectory();
        var entries = new List<(string, string)> { ("TrayToolbar.exe", "exe"), ("TrayToolbar.deps.json", "{}"), ("de/TrayToolbar.resources.dll", "de") };
        entries.AddRange(Enumerable.Range(1, 40).Select(i => ($"Library{i}.dll", $"lib{i}")));
        var zip = temp.CreateZip("update.zip", entries.ToArray());
        var extract = temp.CreateDirectory("extract");

        var updaterPath = UpdateHelper.ExtractUpdatePackage(zip, extract);

        Assert.AreEqual("exe", File.ReadAllText(updaterPath));
        Assert.AreEqual("de", File.ReadAllText(Path.Combine(extract, "de", "TrayToolbar.resources.dll")));
        Assert.AreEqual(43, Directory.EnumerateFiles(extract, "*", SearchOption.AllDirectories).Count());
    }

    [TestMethod]
    public void ExtractUpdatePackage_rejects_more_than_512_entries()
    {
        using var temp = new TempDirectory();
        var entries = new List<(string, string)> { ("TrayToolbar.exe", "exe") };
        entries.AddRange(Enumerable.Range(1, 512).Select(i => ($"Library{i}.dll", "lib")));
        var zip = temp.CreateZip("update.zip", entries.ToArray());
        var extract = temp.CreateDirectory("extract");

        var exception = Assert.ThrowsExactly<InvalidDataException>(() => UpdateHelper.ExtractUpdatePackage(zip, extract));

        StringAssert.Contains(exception.Message, "unexpected number of files");
        Assert.IsEmpty(Directory.EnumerateFileSystemEntries(extract));
    }

    [TestMethod]
    [DataRow("../evil.dll")]
    [DataRow("sub/../../evil.dll")]
    [DataRow("C:/evil.dll")]
    [DataRow("/evil.dll")]
    [DataRow("sub\\..\\..\\evil.dll")]
    public void ExtractUpdatePackage_rejects_entries_that_escape_the_extraction_directory(string entryName)
    {
        using var temp = new TempDirectory();
        var zip = temp.CreateZip("update.zip", ("TrayToolbar.exe", "exe"), (entryName, "evil"));
        var extract = temp.CreateDirectory("extract");

        var exception = Assert.ThrowsExactly<InvalidDataException>(() => UpdateHelper.ExtractUpdatePackage(zip, extract));

        StringAssert.Contains(exception.Message, "unsafe path");
        Assert.IsEmpty(Directory.EnumerateFileSystemEntries(extract));
        Assert.IsFalse(File.Exists(Path.Combine(temp.Path, "evil.dll")));
    }

    [TestMethod]
    public void ExtractUpdatePackage_rejects_a_package_without_a_root_executable()
    {
        using var temp = new TempDirectory();
        var zip = temp.CreateZip("update.zip", ("Library.dll", "lib"), ("sub/TrayToolbar.exe", "exe"));
        var extract = temp.CreateDirectory("extract");

        var exception = Assert.ThrowsExactly<InvalidDataException>(() => UpdateHelper.ExtractUpdatePackage(zip, extract));

        StringAssert.Contains(exception.Message, "outside the archive root");
        Assert.IsEmpty(Directory.EnumerateFileSystemEntries(extract));
    }

    [TestMethod]
    public void ExtractUpdatePackage_rejects_a_package_with_only_libraries()
    {
        using var temp = new TempDirectory();
        var zip = temp.CreateZip("update.zip", ("Library.dll", "lib"));
        var extract = temp.CreateDirectory("extract");

        var exception = Assert.ThrowsExactly<InvalidDataException>(() => UpdateHelper.ExtractUpdatePackage(zip, extract));

        StringAssert.Contains(exception.Message, "does not contain the expected updater executable");
    }

    [TestMethod]
    public void EnsureTrustedLibraries_verifies_every_library_against_the_library_policy()
    {
        using var scope = new ConfigHelperStateScope();
        var verifier = new FakeUpdateSignatureVerifier(TrustedResult);
        ConfigHelper.UpdateSignatureVerifier = verifier;
        using var temp = new TempDirectory();
        var updaterPath = temp.WriteFile(@"extract\TrayToolbar.exe", "exe");
        var library = temp.WriteFile(@"extract\TrayToolbar.Core.dll", "lib");
        var resources = temp.WriteFile(@"extract\de\TrayToolbar.Core.resources.dll", "de");
        temp.WriteFile(@"extract\TrayToolbar.deps.json", "{}");

        UpdateHelper.EnsureTrustedLibraries(Path.Combine(temp.Path, "extract"), updaterPath);

        CollectionAssert.AreEquivalent(new[] { library, resources }, verifier.VerifiedFiles.Select(v => v.Path).ToArray());
        Assert.IsTrue(verifier.VerifiedFiles.All(v => v.Policy == UpdateSignerPolicy.Libraries));
    }

    [TestMethod]
    public void EnsureTrustedLibraries_rejects_a_library_that_fails_verification()
    {
        using var scope = new ConfigHelperStateScope();
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(
            (path, _) => path.EndsWith("Evil.dll") ? InvalidSignatureResult : TrustedResult);
        using var temp = new TempDirectory();
        var updaterPath = temp.WriteFile(@"extract\TrayToolbar.exe", "exe");
        temp.WriteFile(@"extract\TrayToolbar.Core.dll", "lib");
        temp.WriteFile(@"extract\Evil.dll", "evil");

        var exception = Assert.ThrowsExactly<InvalidDataException>(
            () => UpdateHelper.EnsureTrustedLibraries(Path.Combine(temp.Path, "extract"), updaterPath));

        StringAssert.Contains(exception.Message, "could not be verified");
    }

    [TestMethod]
    public void Library_policy_accepts_SignPath_and_Microsoft_publishers()
    {
        CollectionAssert.AreEquivalent(
            new[] { "SignPath Foundation", "Microsoft Corporation" },
            UpdateSignerPolicy.Libraries.AcceptedPublisherNames.ToArray());
        CollectionAssert.AreEquivalent(new[] { "SignPath Foundation" }, UpdateSignerPolicy.Default.AcceptedPublisherNames.ToArray());
    }

    [TestMethod]
    public void CleanupStaleUpdateDirectories_removes_old_update_folders_and_keeps_recent_ones()
    {
        using var temp = new TempDirectory();
        var previous = UpdateHelper.UpdatesRootDirectory;
        UpdateHelper.UpdatesRootDirectory = temp.Path;
        try
        {
            var stale = temp.CreateDirectory("1.8.4-20260901000000-abc");
            temp.WriteFile(@"1.8.4-20260901000000-abc\extract\TrayToolbar.exe", "old");
            Directory.SetCreationTimeUtc(stale, DateTime.UtcNow.AddDays(-2));
            var recent = temp.CreateDirectory("1.9.0-20261006000000-def");

            UpdateHelper.CleanupStaleUpdateDirectories();

            Assert.IsFalse(Directory.Exists(stale));
            Assert.IsTrue(Directory.Exists(recent));
        }
        finally
        {
            UpdateHelper.UpdatesRootDirectory = previous;
        }
    }

    [TestMethod]
    public void CleanupStaleUpdateDirectories_ignores_a_missing_updates_folder()
    {
        var previous = UpdateHelper.UpdatesRootDirectory;
        UpdateHelper.UpdatesRootDirectory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-Missing-{Guid.NewGuid():N}");
        try
        {
            UpdateHelper.CleanupStaleUpdateDirectories();
        }
        finally
        {
            UpdateHelper.UpdatesRootDirectory = previous;
        }
    }

    sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"TrayToolbar-UpdateHelperTests-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public string CreateDirectory(string relativePath)
        {
            var directory = System.IO.Path.Combine(Path, relativePath);
            Directory.CreateDirectory(directory);
            return directory;
        }

        public string WriteFile(string relativePath, string content)
        {
            var file = System.IO.Path.Combine(Path, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file)!);
            File.WriteAllText(file, content);
            return file;
        }

        public string CreateZip(string name, params (string EntryName, string Content)[] entries)
        {
            var zip = System.IO.Path.Combine(Path, name);
            using var archive = ZipFile.Open(zip, ZipArchiveMode.Create);
            foreach (var (entryName, content) in entries)
            {
                using var writer = new StreamWriter(archive.CreateEntry(entryName).Open());
                writer.Write(content);
            }
            return zip;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    sealed class FakeUpdateSignatureVerifier(Func<string, UpdateSignerPolicy, UpdateSignatureVerificationResult> verify) : IUpdateSignatureVerifier
    {
        public FakeUpdateSignatureVerifier(UpdateSignatureVerificationResult result) : this((_, _) => result) { }

        public List<(string Path, UpdateSignerPolicy Policy)> VerifiedFiles { get; } = [];

        public UpdateSignatureVerificationResult VerifyForUpdate(string filePath) => Verify(filePath, UpdateSignerPolicy.Default);

        public UpdateSignatureVerificationResult Verify(string filePath, UpdateSignerPolicy policy)
        {
            VerifiedFiles.Add((filePath, policy));
            return verify(filePath, policy);
        }
    }
}