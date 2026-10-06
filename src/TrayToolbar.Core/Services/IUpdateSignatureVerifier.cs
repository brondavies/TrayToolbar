namespace TrayToolbar.Services;

internal interface IUpdateSignatureVerifier
{
    /// <summary>
    /// Verifies the staged updater executable against the TrayToolbar signer policy
    /// </summary>
    UpdateSignatureVerificationResult VerifyForUpdate(string filePath);

    /// <summary>
    /// Verifies any file in an update package against the given signer policy
    /// </summary>
    UpdateSignatureVerificationResult Verify(string filePath, UpdateSignerPolicy policy);
}