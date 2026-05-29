using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TrayToolbar.Services;

internal enum UpdateSignatureFailureReason
{
    None,
    FileNotFound,
    Unsigned,
    InvalidSignature,
    TrustChainFailure,
    UnexpectedPublisher,
    CertificateReadFailed,
    UnknownError,
}

internal readonly record struct UpdateSignatureVerificationResult(
    bool IsSuccess,
    UpdateSignatureFailureReason FailureReason,
    string UserMessage,
    string DiagnosticMessage,
    string? SignerSubject,
    string? SignerPublisher,
    string? SignerThumbprint)
{
    internal static UpdateSignatureVerificationResult Success(
        string diagnosticMessage,
        string signerSubject,
        string signerPublisher,
        string signerThumbprint)
    {
        return new UpdateSignatureVerificationResult(
            true,
            UpdateSignatureFailureReason.None,
            string.Empty,
            diagnosticMessage,
            signerSubject,
            signerPublisher,
            signerThumbprint);
    }

    internal static UpdateSignatureVerificationResult Failure(
        UpdateSignatureFailureReason failureReason,
        string userMessage,
        string diagnosticMessage,
        string? signerSubject = null,
        string? signerPublisher = null,
        string? signerThumbprint = null)
    {
        return new UpdateSignatureVerificationResult(
            false,
            failureReason,
            userMessage,
            diagnosticMessage,
            signerSubject,
            signerPublisher,
            signerThumbprint);
    }
}

internal sealed class UpdateSignerPolicy
{
    public static UpdateSignerPolicy Default { get; } = new(
        acceptedPublisherNames: ["SignPath Foundation"],
        acceptedSubjectDistinguishedNames: [],
        acceptedSignerThumbprints: []);

    public IReadOnlyCollection<string> AcceptedPublisherNames { get; }
    public IReadOnlyCollection<string> AcceptedSubjectDistinguishedNames { get; }
    public IReadOnlyCollection<string> AcceptedSignerThumbprints { get; }

    internal UpdateSignerPolicy(
        IEnumerable<string> acceptedPublisherNames,
        IEnumerable<string>? acceptedSubjectDistinguishedNames,
        IEnumerable<string>? acceptedSignerThumbprints)
    {
        AcceptedPublisherNames = NormalizeValues(acceptedPublisherNames);
        AcceptedSubjectDistinguishedNames = NormalizeValues(acceptedSubjectDistinguishedNames);
        AcceptedSignerThumbprints = NormalizeThumbprints(acceptedSignerThumbprints);
    }

    internal bool Matches(X509Certificate2 certificate, out string mismatchReason)
    {
        if (AcceptedPublisherNames.Count == 0 && AcceptedSubjectDistinguishedNames.Count == 0)
        {
            mismatchReason = "No accepted publisher identities are configured.";
            return false;
        }

        var publisher = GetPublisherIdentity(certificate);
        var subject = certificate.Subject ?? string.Empty;
        var normalizedThumbprint = NormalizeThumbprint(certificate.Thumbprint);
        var publisherMatch = AcceptedPublisherNames.Contains(publisher, StringComparer.OrdinalIgnoreCase);
        var subjectMatch = AcceptedSubjectDistinguishedNames.Contains(subject, StringComparer.OrdinalIgnoreCase);

        if (!publisherMatch && !subjectMatch)
        {
            mismatchReason = $"Signer publisher '{publisher}' and subject '{subject}' do not match the configured allow-list.";
            return false;
        }

        if (AcceptedSignerThumbprints.Count > 0
            && !AcceptedSignerThumbprints.Contains(normalizedThumbprint, StringComparer.OrdinalIgnoreCase))
        {
            mismatchReason = $"Signer thumbprint '{normalizedThumbprint}' does not match the configured allow-list.";
            return false;
        }

        mismatchReason = string.Empty;
        return true;
    }

    internal static string GetPublisherIdentity(X509Certificate2 certificate)
    {
        var publisher = certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false)?.Trim();
        return string.IsNullOrWhiteSpace(publisher)
            ? certificate.Subject ?? string.Empty
            : publisher;
    }

    internal static string NormalizeThumbprint(string? thumbprint)
    {
        return string.IsNullOrWhiteSpace(thumbprint)
            ? string.Empty
            : thumbprint.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }

    static IReadOnlyCollection<string> NormalizeValues(IEnumerable<string>? values)
    {
        return (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    static string[] NormalizeThumbprints(IEnumerable<string>? values)
    {
        return (values ?? [])
            .Select(NormalizeThumbprint)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

[SupportedOSPlatform("windows")]
internal sealed class AuthenticodeUpdateSignatureVerifier(UpdateSignerPolicy? policy = null) : IUpdateSignatureVerifier
{
    const uint WinTrustUiNone = 2;
    const uint WinTrustRevokeNone = 0;
    const uint WinTrustChoiceFile = 1;
    const uint WinTrustStateActionIgnore = 0;
    const uint WinTrustSaferFlag = 0x00000100;
    const uint WinTrustDisableMd2Md4 = 0x00002000;
    const uint WinTrustUiContextExecute = 0;

    const int TrustSuccess = 0;
    const int TrustErrorNoSignature = unchecked((int)0x800B0100);
    const int TrustErrorProviderUnknown = unchecked((int)0x800B0001);
    const int TrustErrorActionUnknown = unchecked((int)0x800B0002);
    const int TrustErrorSubjectFormUnknown = unchecked((int)0x800B0003);
    const int TrustErrorSubjectNotTrusted = unchecked((int)0x800B0004);
    const int TrustErrorBadDigest = unchecked((int)0x80096010);
    const int TrustErrorExplicitDistrust = unchecked((int)0x800B0111);
    const int CertErrorExpired = unchecked((int)0x800B0101);
    const int CertErrorUntrustedRoot = unchecked((int)0x800B0109);
    const int CertErrorChaining = unchecked((int)0x800B010A);
    const int CertErrorRevoked = unchecked((int)0x800B010C);
    const int CertErrorWrongUsage = unchecked((int)0x800B0110);

    static readonly Guid GenericVerifyV2Action = new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

    readonly UpdateSignerPolicy policy = policy ?? UpdateSignerPolicy.Default;

    public UpdateSignatureVerificationResult VerifyForUpdate(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.FileNotFound,
                "The staged update executable could not be found.",
                $"The staged updater path '{filePath}' does not exist.");
        }

        var status = VerifyAuthenticode(filePath);
        if (status != TrustSuccess)
        {
            return CreateTrustFailure(filePath, status);
        }

        try
        {
            using var signerCertificate = LoadSignerCertificate(filePath);
            var publisher = UpdateSignerPolicy.GetPublisherIdentity(signerCertificate);
            var subject = signerCertificate.Subject ?? string.Empty;
            var thumbprint = UpdateSignerPolicy.NormalizeThumbprint(signerCertificate.Thumbprint);

            if (!policy.Matches(signerCertificate, out var mismatchReason))
            {
                return UpdateSignatureVerificationResult.Failure(
                    UpdateSignatureFailureReason.UnexpectedPublisher,
                    "The staged update was signed by an unexpected publisher.",
                    $"Signer identity mismatch for '{filePath}'. Publisher='{publisher}', Subject='{subject}', Thumbprint='{thumbprint}'. {mismatchReason}",
                    subject,
                    publisher,
                    thumbprint);
            }

            return UpdateSignatureVerificationResult.Success(
                $"WinVerifyTrust accepted '{filePath}'. Publisher='{publisher}', Subject='{subject}', Thumbprint='{thumbprint}'.",
                subject,
                publisher,
                thumbprint);
        }
        catch (CryptographicException ex)
        {
            return UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.CertificateReadFailed,
                "The staged update signer certificate could not be read.",
                $"The signer certificate for '{filePath}' could not be read: {ex.Message}");
        }
    }

    static UpdateSignatureVerificationResult CreateTrustFailure(string filePath, int status)
    {
        var statusHex = $"0x{status:X8}";
        return status switch
        {
            TrustErrorNoSignature => UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.Unsigned,
                "The staged update is not Authenticode-signed.",
                $"WinVerifyTrust reported no Authenticode signature for '{filePath}' ({statusHex})."),

            TrustErrorBadDigest => UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.InvalidSignature,
                "The staged update has an invalid or tampered Authenticode signature.",
                $"WinVerifyTrust reported an invalid Authenticode digest for '{filePath}' ({statusHex})."),

            TrustErrorExplicitDistrust
                or TrustErrorSubjectNotTrusted
                or CertErrorExpired
                or CertErrorUntrustedRoot
                or CertErrorChaining
                or CertErrorRevoked
                or CertErrorWrongUsage => UpdateSignatureVerificationResult.Failure(
                    UpdateSignatureFailureReason.TrustChainFailure,
                    "Windows could not validate the staged update's code-signing certificate.",
                    $"WinVerifyTrust rejected the signer trust chain for '{filePath}' ({statusHex}: {DescribeTrustStatus(status)})."),

            _ => UpdateSignatureVerificationResult.Failure(
                UpdateSignatureFailureReason.UnknownError,
                "Windows could not verify the staged update signature.",
                $"WinVerifyTrust rejected '{filePath}' with status {statusHex}: {DescribeTrustStatus(status)}.")
        };
    }

    static X509Certificate2 LoadSignerCertificate(string filePath)
    {
        using var signerCertificate = X509Certificate.CreateFromSignedFile(filePath);
        return new X509Certificate2(signerCertificate);
    }

    static int VerifyAuthenticode(string filePath)
    {
        var fileInfo = new WinTrustFileInfo
        {
            cbStruct = (uint)Marshal.SizeOf<WinTrustFileInfo>(),
            pcwszFilePath = filePath,
            hFile = IntPtr.Zero,
            pgKnownSubject = IntPtr.Zero,
        };

        var fileInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrustFileInfo>());
        try
        {
            Marshal.StructureToPtr(fileInfo, fileInfoPointer, false);

            var trustData = new WinTrustData
            {
                cbStruct = (uint)Marshal.SizeOf<WinTrustData>(),
                pPolicyCallbackData = IntPtr.Zero,
                pSIPClientData = IntPtr.Zero,
                dwUIChoice = WinTrustUiNone,
                fdwRevocationChecks = WinTrustRevokeNone,
                dwUnionChoice = WinTrustChoiceFile,
                pInfoStruct = fileInfoPointer,
                dwStateAction = WinTrustStateActionIgnore,
                hWVTStateData = IntPtr.Zero,
                pwszURLReference = null,
                dwProvFlags = WinTrustSaferFlag | WinTrustDisableMd2Md4,
                dwUIContext = WinTrustUiContextExecute,
                pSignatureSettings = IntPtr.Zero,
            };

            return WinVerifyTrust(IntPtr.Zero, in GenericVerifyV2Action, ref trustData);
        }
        finally
        {
            Marshal.DestroyStructure<WinTrustFileInfo>(fileInfoPointer);
            Marshal.FreeHGlobal(fileInfoPointer);
        }
    }

    static string DescribeTrustStatus(int status)
    {
        return status switch
        {
            TrustErrorNoSignature => "No Authenticode signature was present.",
            TrustErrorProviderUnknown => "The trust provider is unknown.",
            TrustErrorActionUnknown => "The trust action is unknown.",
            TrustErrorSubjectFormUnknown => "The file format is unsupported for trust verification.",
            TrustErrorSubjectNotTrusted => "The signer is not trusted on this machine.",
            TrustErrorBadDigest => "The embedded signature digest did not match the file contents.",
            TrustErrorExplicitDistrust => "The signer is explicitly distrusted on this machine.",
            CertErrorExpired => "The code-signing certificate is expired and timestamp validation did not rescue it.",
            CertErrorUntrustedRoot => "The signer chain terminates in an untrusted root.",
            CertErrorChaining => "Windows could not build a complete certificate chain.",
            CertErrorRevoked => "The signer certificate has been revoked.",
            CertErrorWrongUsage => "The certificate is not valid for code signing.",
            _ => "Windows returned an unspecified trust failure.",
        };
    }

    [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int WinVerifyTrust(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] in Guid actionId, ref WinTrustData trustData);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WinTrustFileInfo
    {
        public uint cbStruct;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pcwszFilePath;
        public IntPtr hFile;
        public IntPtr pgKnownSubject;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WinTrustData
    {
        public uint cbStruct;
        public IntPtr pPolicyCallbackData;
        public IntPtr pSIPClientData;
        public uint dwUIChoice;
        public uint fdwRevocationChecks;
        public uint dwUnionChoice;
        public IntPtr pInfoStruct;
        public uint dwStateAction;
        public IntPtr hWVTStateData;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pwszURLReference;
        public uint dwProvFlags;
        public uint dwUIContext;
        public IntPtr pSignatureSettings;
    }
}