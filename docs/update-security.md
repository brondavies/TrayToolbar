# Update and execution security

This document describes the trust boundaries for TrayToolbar's self-update flow and direct URL launching.

It serves two purposes:

- document the current behavior contributors are expected to preserve
- make the update package contract explicit so release packaging and runtime validation stay in sync

## Trust model summary

TrayToolbar treats these as separate trust decisions:

1. **Release metadata trust**: determine whether a newer stable release exists
2. **Release asset trust**: determine whether the downloaded archive matches the expected release contract
3. **Execution trust**: determine what TrayToolbar is willing to launch directly

## Release metadata source

Release metadata is fetched from GitHub's machine-readable REST API endpoint:

- `https://api.github.com/repos/brondavies/TrayToolbar/releases/latest`

TrayToolbar expects the response to be a published **latest stable** release.
GitHub documents this endpoint as returning the latest published full release, excluding drafts and prereleases.

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
- the release page URL must stay under:
  - `https://github.com/brondavies/TrayToolbar/releases...`

TrayToolbar compares versions using `System.Version` after trimming a leading `v`.

This means:

- `v1.6.2` and `1.6.2` are treated equivalently for comparison
- malformed version strings are rejected
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
6. start that extracted executable with:
   - `--update <path to installed TrayToolbar.exe>`

TrayToolbar intentionally does **not** trust arbitrary extracted file names for execution.
It only executes the expected updater executable after digest verification.

Relevant implementation:

- `src/TrayToolbar/UpdateHelper.cs`

### Archive handling notes

TrayToolbar currently applies these lightweight safety checks before extraction:

- maximum entry count threshold
- maximum total uncompressed size threshold
- exactly one executable entry named `TrayToolbar.exe` at the archive root

Failure paths attempt to delete the staged temp directory when practical.

## Update application behavior

When the staged updater starts with `--update`, it:

- validates the target executable path shape
- broadcasts the settings-form exit message
- retries copying the staged updater executable over the target executable
- restarts the target executable with:
  - `--show --newversion`

Relevant implementation:

- `src/TrayToolbar/Program.cs`
- `src/TrayToolbar/UpdateHelper.cs`

## Direct launch policy

`Program.Launch(string fileName)` currently allows:

- an existing file path
- an existing directory path
- a direct HTTPS release URL that matches:
  - host: `github.com`
  - path prefix: `/brondavies/TrayToolbar/releases`

This intentionally narrows direct remote launching to TrayToolbar release pages.

Important nuance:

- TrayToolbar can still launch local files the user explicitly selected or placed in monitored folders, including Windows shortcut and internet shortcut files, because those are launched as local files rather than as arbitrary raw URL strings.

Relevant implementation:

- `src/TrayToolbar/Program.cs`
- `src/TrayToolbar/UpdateLogic.cs`

## Code signing status

TrayToolbar does **not** currently enforce Authenticode signature validation during update.

Current position:

- GitHub release metadata and asset SHA-256 digests are verified
- Authenticode verification is **not yet implemented**

Recommended future step:

- if the project adopts code signing, add Authenticode validation for the staged updater executable before execution

## Contributor checklist when release packaging changes

If release packaging changes, contributors must update **all** of the following together:

- `build.ps1` release artifact output
- `src/TrayToolbar/UpdateLogic.cs`
- `src/TrayToolbar/UpdateHelper.cs`
- update-related tests in `src/TrayToolbar.Tests/`
- this document
- any user-facing release/update notes in `README.md` or `docs/developer-guide.md`

At minimum, re-check:

- asset names
- architecture mapping
- whether the archive still contains a root `TrayToolbar.exe`
- whether GitHub still provides the required digest field for the published asset

## Known limits

Current hardening is intentionally small and pragmatic for a portable Windows desktop app.

What is enforced now:

- explicit GitHub API metadata source
- explicit stable-release eligibility rules
- asset naming and URL contract validation
- SHA-256 verification against GitHub asset metadata
- isolated temp staging
- narrow direct remote launch policy

What is not enforced yet:

- Authenticode validation
- a separate published checksum manifest in the repository release assets
- trust decisions for arbitrary remote URLs outside the TrayToolbar GitHub Releases surface