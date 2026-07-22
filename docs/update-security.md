# Update and execution security

This document describes the trust boundaries for TrayToolbar's self-update flow and the current execution path TrayToolbar uses when it starts files, folders, shortcuts, URLs, or notification actions.

It serves two purposes:

- document the current behavior contributors are expected to preserve
- make the update package contract explicit so release packaging and runtime validation stay in sync

## Trust model summary

TrayToolbar treats these as separate areas:

1. **Release metadata trust**: determine whether a newer stable release exists
2. **Release asset trust**: determine whether the downloaded archive matches the expected release contract
3. **Execution behavior**: describe how TrayToolbar initiates launches from menus, shortcuts, and notifications

For automatic updates, release asset trust now has two fail-closed stages:

- GitHub release metadata plus SHA-256 archive validation
- Authenticode validation of the staged `TrayToolbar.exe` against the configured TrayToolbar signer policy

## Release metadata source

Release metadata is fetched from GitHub's machine-readable REST API endpoint:

- `https://api.github.com/repos/brondavies/TrayToolbar/releases/latest`

TrayToolbar expects the response to be a published **latest stable** release.
GitHub's `releases/latest` behavior excludes drafts and prereleases, which matches the app's current stable-only update model.

Relevant implementation:

- `src/TrayToolbar/Services/GitHubReleaseClient.cs`
- `src/TrayToolbar/Models/Release.cs`

TrayToolbar currently consumes these fields:

- `tag_name`
- `html_url`
- `prerelease`
- `assets[*].name`
- `assets[*].browser_download_url`
- `assets[*].content_type`
- `assets[*].digest`

## Update eligibility rules

Update availability is determined by `src/TrayToolbar/UpdateLogic.cs`.

Current rules:

- the release must contain a parseable `tag_name`
- the release must **not** be marked `prerelease`
- the release version must differ from the running app version
- the release page URL must be an HTTPS URL on `github.com`
- the release path must be exactly `/brondavies/TrayToolbar/releases` or a descendant of that path

TrayToolbar compares versions using `System.Version` after trimming a leading `v`.

This means:

- `v1.6.2` and `1.6.2` are treated equivalently for comparison
- malformed version strings are rejected
- lookalike paths such as `/brondavies/TrayToolbar/releases-malicious/...` are rejected
- prerelease behavior is explicit rather than inferred from a redirect or page shape

## Release asset contract

For a supported portable update, the asset contract is:

- architecture `arm64` asset name:
  - `TrayToolbar-win-arm64-portable-<version>.zip`
- architecture `x64` asset name:
  - `TrayToolbar-win-x64-portable-<version>.zip`

TrayToolbar validates all of the following before it will use a downloaded asset:

- the expected architecture-specific asset exists exactly once in the release metadata
- the asset name matches the expected naming convention
- the asset file name ends in `.zip`
- the asset download URL matches the expected GitHub Releases download URL for the requested version
- the asset `content_type` is a supported zip type
- the asset `digest` is present and formatted as `sha256:<64 hex chars>`

Relevant implementation:

- `src/TrayToolbar/UpdateLogic.cs`
- `src/TrayToolbar/Models/UpdatePackage.cs`

## Download, extraction, and execution flow

The update installer stages each update in an isolated temp directory under:

- `%TEMP%\TrayToolbar\Updates\<version>-<timestamp>-<guid>`

Current behavior:

1. download the expected zip asset into the isolated directory
2. compute the SHA-256 hash of the downloaded zip
3. compare it to the GitHub-provided asset digest
4. open the archive and validate basic archive bounds
5. extract only the expected root updater executable:
   - `TrayToolbar.exe`
6. run Windows Authenticode validation (`WinVerifyTrust`) on the staged `TrayToolbar.exe` with trust UI disabled
7. require the signer identity to match TrayToolbar's configured publisher allow-list
8. start that extracted executable with:
   - `--update <path to installed TrayToolbar.exe>`

TrayToolbar intentionally does **not** trust arbitrary extracted file names for execution.
It only executes the expected updater executable after digest verification and Authenticode verification both succeed.

Relevant implementation:

- `src/TrayToolbar/UpdateHelper.cs`

### Archive handling notes

TrayToolbar currently applies these lightweight safety checks before extraction:

- maximum entry count threshold
- maximum total uncompressed size threshold
- exactly one executable entry named `TrayToolbar.exe` at the archive root

Failure paths attempt to delete the staged temp directory when practical.

### Authenticode validation policy

The staged updater executable must pass Windows code-signing trust evaluation and match TrayToolbar's signer policy.

Current runtime requirements:

- `WinVerifyTrust` must succeed for the staged `TrayToolbar.exe`
- trust UI is disabled so update verification stays non-interactive
- the signer certificate identity must match `UpdateSignerPolicy.Default` in `src/TrayToolbar/Services/AuthenticodeUpdateSignatureVerifier.cs`
- thumbprint pinning is supported by that policy object but is currently left empty so the initial runtime policy is publisher-based

Current allow-list shape:

- accepted publisher name: `SignPath Foundation`
- accepted subject distinguished names: none pinned yet
- accepted signer thumbprints: none pinned yet

If the signing certificate changes in a way that affects the signer subject or if thumbprint pinning is introduced, update `UpdateSignerPolicy.Default` before publishing the next release.

Practical consequences:

- tagged GitHub CI releases that publish SignPath-signed portable zips remain valid update artifacts
- `pull_request` workflow artifacts are intentionally unsigned and are therefore **not** valid automatic-update artifacts
- local `build.ps1` outputs are also not valid automatic-update artifacts unless they are signed with an allowed TrayToolbar publisher identity

## Update application behavior

When the staged updater starts with `--update`, it:

- validates the target executable path shape
- re-runs Authenticode validation on the running staged updater executable before installation
- broadcasts the settings-form exit message
- retries copying the staged updater executable over the target executable
- restarts the target executable with:
  - `--show --newversion`

Relevant implementation:

- `src/TrayToolbar/Program.cs`
- `src/TrayToolbar/UpdateHelper.cs`

## Launches initiated by TrayToolbar

`Program.Launch(...)` currently hands the supplied target to the Windows shell by starting a `ProcessStartInfo` with `UseShellExecute = true`.

In practice, TrayToolbar can forward launches for:

- file paths
- directory paths
- shortcut files such as `.lnk` and `.url`
- URLs, including GitHub Releases links surfaced by the update UX

### Toast activation boundary

Windows toast body clicks and action buttons route back through TrayToolbar foreground activation before `Program.Launch(...)` is called.

That means update and release links shown in notifications are initiated by TrayToolbar rather than by embedding a direct protocol launch in the toast payload.

Relevant implementation:

- `src/TrayToolbar/NotificationsHelper.cs`
- `src/TrayToolbar/Program.cs`

## Code signing status

TrayToolbar's GitHub CI workflow submits the portable release archives to SignPath and publishes signed artifacts.

TrayToolbar enforces Authenticode signature validation during update in addition to the existing GitHub metadata and SHA-256 checks.

Current position:

- GitHub release metadata and asset SHA-256 digests are verified
- tagged GitHub CI builds can publish SignPath-signed portable release archives
- the staged `TrayToolbar.exe` must pass `WinVerifyTrust` and match the configured TrayToolbar publisher allow-list before TrayToolbar launches or installs it
- `pull_request` validation builds intentionally skip signing so signing credentials are not exposed to untrusted changes, and those unsigned artifacts are not update-valid
- local unsigned portable builds are also not update-valid

## Contributor checklist when release packaging or execution behavior changes

If release packaging or execution behavior changes, contributors must update **all** of the following together:

- `build.ps1` release artifact output
- `.github/workflows/dotnet-desktop.yml`
- `src/TrayToolbar/UpdateLogic.cs`
- `src/TrayToolbar/UpdateHelper.cs`
- `src/TrayToolbar/Program.cs`
- any affected files in `src/TrayToolbar/Services/`, especially `AuthenticodeUpdateSignatureVerifier.cs`
- update-related and execution-related tests in `src/TrayToolbar.Tests/`
- `CHANGELOG.md`
- this document
- any related user-facing notes in `README.md`, `docs/release-notes.md`, or `docs/developer-guide.md`

At minimum, re-check:

- asset names
- architecture mapping
- whether the archive still contains a root `TrayToolbar.exe`
- whether GitHub still provides the required digest field for the published asset
- whether the signer identity in `UpdateSignerPolicy.Default` still matches the certificate used by SignPath
- whether any pinned thumbprints in `UpdateSignerPolicy.Default` need rotation before the next release
- whether execution-behavior documentation still matches the code paths used by menu clicks, notifications, and update links

## Known limits

Current hardening is intentionally small and pragmatic for a portable Windows desktop app.

What is enforced now:

- explicit GitHub API metadata source
- explicit stable-release eligibility rules
- release-path validation that rejects lookalike prefixes
- asset naming and URL contract validation
- SHA-256 verification against GitHub asset metadata
- Authenticode validation of the staged updater executable before launch and before copy-over-install
- isolated temp staging

What is not enforced yet:

- a separate published checksum manifest in the repository release assets