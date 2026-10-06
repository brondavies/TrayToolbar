namespace TrayToolbar.Services;

internal interface IUpdateSignatureVerifier
{
    UpdateSignatureVerificationResult VerifyForUpdate(string filePath);
}