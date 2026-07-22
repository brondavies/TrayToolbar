using TrayToolbar.Services;

namespace TrayToolbar.Tests;

[TestClass]
public class UpdateHelperTests
{
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
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(
            UpdateSignatureVerificationResult.Success(
                "WinVerifyTrust accepted the staged updater.",
                "CN=Brontech, LLC",
                "Brontech, LLC",
                "ABC123"));

        var operationDirectory = CreateTempDirectory();
        var updaterPath = Path.Combine(operationDirectory, "TrayToolbar.exe");
        File.WriteAllText(updaterPath, "signed updater placeholder");

        try
        {
            UpdateHelper.StartVerifiedUpdater(updaterPath, @"C:\Installed\TrayToolbar.exe", operationDirectory);

            Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
            Assert.AreEqual(updaterPath, processLauncher.StartedProcesses[0].FileName);
            Assert.IsTrue(Directory.Exists(operationDirectory));
        }
        finally
        {
            if (Directory.Exists(operationDirectory))
            {
                Directory.Delete(operationDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public void StartVerifiedUpdater_rejects_invalid_signature_and_cleans_operation_directory()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(
            UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.InvalidSignature,
                "The staged update has an invalid or tampered Authenticode signature.",
                "WinVerifyTrust reported TRUST_E_BAD_DIGEST."));

        var operationDirectory = CreateTempDirectory();
        var updaterPath = Path.Combine(operationDirectory, "TrayToolbar.exe");
        File.WriteAllText(updaterPath, "unsigned updater placeholder");

        var exception = AssertThrows<InvalidDataException>(
            () => UpdateHelper.StartVerifiedUpdater(updaterPath, @"C:\Installed\TrayToolbar.exe", operationDirectory));

        StringAssert.Contains(exception.Message, "could not be verified");
        StringAssert.Contains(exception.Message, "invalid or tampered Authenticode signature");
        Assert.AreEqual(0, processLauncher.StartedProcesses.Count);
        Assert.IsFalse(Directory.Exists(operationDirectory));
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

        var operationDirectory = CreateTempDirectory();
        var updaterPath = Path.Combine(operationDirectory, "TrayToolbar.exe");
        File.WriteAllText(updaterPath, "unexpected publisher updater placeholder");

        try
        {
            var exception = AssertThrows<InvalidDataException>(
                () => UpdateHelper.StartVerifiedUpdater(updaterPath, @"C:\Installed\TrayToolbar.exe", operationDirectory));

            StringAssert.Contains(exception.Message, "unexpected publisher");
            Assert.AreEqual(0, processLauncher.StartedProcesses.Count);
        }
        finally
        {
            if (Directory.Exists(operationDirectory))
            {
                Directory.Delete(operationDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public void ApplyVerifiedUpdate_rejects_untrusted_staged_executable_before_copy()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(
            UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.InvalidSignature,
                "The staged update has an invalid or tampered Authenticode signature.",
                "WinVerifyTrust reported TRUST_E_BAD_DIGEST."));

        var directory = CreateTempDirectory();
        var stagedExe = Path.Combine(directory, "TrayToolbar.exe");
        var installedExe = Path.Combine(directory, "Installed-TrayToolbar.exe");
        File.WriteAllText(stagedExe, "new version");
        File.WriteAllText(installedExe, "old version");

        try
        {
            _ = AssertThrows<InvalidDataException>(() => UpdateHelper.ApplyVerifiedUpdate(stagedExe, installedExe, () => { }));

            Assert.AreEqual("old version", File.ReadAllText(installedExe));
            Assert.AreEqual(0, processLauncher.StartedProcesses.Count);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void ApplyVerifiedUpdate_copies_verified_staged_executable_and_restarts_target()
    {
        using var scope = new ConfigHelperStateScope();
        var processLauncher = new FakeProcessLauncher();
        ConfigHelper.ProcessLauncher = processLauncher;
        ConfigHelper.UpdateSignatureVerifier = new FakeUpdateSignatureVerifier(
            UpdateSignatureVerificationResult.Success(
                "WinVerifyTrust accepted the staged updater.",
                "CN=Brontech, LLC",
                "Brontech, LLC",
                "ABC123"));

        var directory = CreateTempDirectory();
        var stagedExe = Path.Combine(directory, "TrayToolbar.exe");
        var installedExe = Path.Combine(directory, "Installed-TrayToolbar.exe");
        File.WriteAllText(stagedExe, "new version");
        File.WriteAllText(installedExe, "old version");

        try
        {
            var updated = UpdateHelper.ApplyVerifiedUpdate(stagedExe, installedExe, () => { });

            Assert.IsTrue(updated);
            Assert.AreEqual("new version", File.ReadAllText(installedExe));
            Assert.AreEqual(1, processLauncher.StartedProcesses.Count);
            Assert.AreEqual(installedExe, processLauncher.StartedProcesses[0].FileName);
            CollectionAssert.AreEqual(new[] { "--show", "--newversion" }, processLauncher.StartedProcesses[0].ArgumentList.ToArray());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"TrayToolbar-UpdateHelperTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException ex)
        {
            return ex;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
        throw new InvalidOperationException("Assert.Fail should have thrown.");
    }

    sealed class FakeUpdateSignatureVerifier(UpdateSignatureVerificationResult result) : IUpdateSignatureVerifier
    {
        public UpdateSignatureVerificationResult VerifyForUpdate(string filePath) => result;
    }
}
