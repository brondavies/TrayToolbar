using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using TrayToolbar.Extensions;
using TrayToolbar.Services;

namespace TrayToolbar.Tests;

/// <summary>
/// These tests exercise the real <c>WinVerifyTrust</c> P/Invoke rather than a fake verifier.
/// A broken interop declaration makes every call fail with an unmapped status, which is the
/// class of bug that silently disabled automatic updates in 1.8.1.
/// </summary>
[TestClass]
public class AuthenticodeUpdateSignatureVerifierTests
{
    [TestMethod]
    public void VerifyForUpdate_reports_missing_file()
    {
        var verifier = new AuthenticodeUpdateSignatureVerifier();

        var result = verifier.VerifyForUpdate(Path.Combine(Path.GetTempPath(), $"TrayToolbar-missing-{Guid.NewGuid():N}.exe"));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(UpdateSignatureFailureReason.FileNotFound, result.FailureReason);
    }

    [TestMethod]
    public void VerifyForUpdate_reports_unsigned_file_as_unsigned_rather_than_unknown_error()
    {
        var verifier = new AuthenticodeUpdateSignatureVerifier();
        var unsignedFile = Path.Combine(Path.GetTempPath(), $"TrayToolbar-unsigned-{Guid.NewGuid():N}.exe");
        File.WriteAllText(unsignedFile, "not a signed portable executable");

        try
        {
            var result = verifier.VerifyForUpdate(unsignedFile);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(
                UpdateSignatureFailureReason.Unsigned,
                result.FailureReason,
                $"WinVerifyTrust returned an unexpected status. {result.DiagnosticMessage}");
        }
        finally
        {
            File.Delete(unsignedFile);
        }
    }

    [TestMethod]
    public void VerifyForUpdate_accepts_a_trusted_binary_signed_by_an_allowed_publisher()
    {
        var signedFile = FindTrustedSignedBinary(out var publisher);
        var verifier = new AuthenticodeUpdateSignatureVerifier(
            new UpdateSignerPolicy([publisher], null, null));

        var result = verifier.VerifyForUpdate(signedFile);

        Assert.IsTrue(result.IsSuccess, $"Expected '{signedFile}' to verify. {result.DiagnosticMessage}");
        Assert.AreEqual(UpdateSignatureFailureReason.None, result.FailureReason);
        Assert.AreEqual(publisher, result.SignerPublisher);
    }

    [TestMethod]
    public void VerifyForUpdate_rejects_a_trusted_binary_from_a_publisher_outside_the_policy()
    {
        var signedFile = FindTrustedSignedBinary(out _);
        var verifier = new AuthenticodeUpdateSignatureVerifier(
            new UpdateSignerPolicy(["TrayToolbar Test Publisher That Does Not Exist"], null, null));

        var result = verifier.VerifyForUpdate(signedFile);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(
            UpdateSignatureFailureReason.UnexpectedPublisher,
            result.FailureReason,
            $"WinVerifyTrust did not reach the signer policy check. {result.DiagnosticMessage}");
    }

    /// <summary>
    /// Returns a locally installed executable that carries a trusted embedded Authenticode
    /// signature, so the test can assert against a real trust evaluation without shipping a
    /// signed fixture. Marks the test inconclusive when the machine has no usable candidate.
    /// </summary>
    static string FindTrustedSignedBinary(out string publisher)
    {
        foreach (var candidate in EnumerateSignedBinaryCandidates())
        {
            if (!candidate.HasValue() || !File.Exists(candidate))
            {
                continue;
            }

            try
            {
                using var certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(candidate));
                publisher = UpdateSignerPolicy.GetPublisherIdentity(certificate);
                if (publisher.HasValue())
                {
                    return candidate;
                }
            }
            catch (CryptographicException)
            {
                // Not embedded-signed (catalog-signed or unsigned); try the next candidate.
            }
        }

        Assert.Inconclusive("No embedded Authenticode-signed executable was found on this machine.");
        throw new InvalidOperationException("Assert.Inconclusive should have thrown.");
    }

    static IEnumerable<string?> EnumerateSignedBinaryCandidates()
    {
        yield return Environment.ProcessPath;

        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (dotnetRoot.HasValue())
        {
            yield return Path.Combine(dotnetRoot, "dotnet.exe");
        }

        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
    }
}
