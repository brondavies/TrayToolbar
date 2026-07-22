# Changelog

All notable changes to TrayToolbar will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and release tags follow the repository convention `v<Version>` using the version declared in `src/TrayToolbar/TrayToolbar.csproj`.

For narrative release summaries, packaging notes, and upgrade context that is easier to read than a terse diff, see [docs/release-notes.md](docs/release-notes.md).

## [Unreleased]

## [1.8.0] - 2026-07-22

`v1.7.2` through `v1.7.8` were tagged but never published. Their release builds failed in turn on SignPath policy loading, on the repository having no GitHub rulesets, on no ruleset applying to the release tag ref, on the signing request submission returning `400 Bad Request` while a concurrent master push build held the same artifact name, and on the signed artifact upload colliding with the unsigned artifact uploaded for SignPath. 1.8.0 is the first published release of this work.

### Added

- Added a Sponsors section to `README.md` crediting SignPath.io for free Windows code signing and SignPath Foundation for the certificate.
- Added SignPath GitHub Actions integration hooks so CI can submit the portable `win-arm64` and `win-x64` zip artifacts for code signing and download signed replacements before publishing.

### Changed

- GitHub CI now skips signing on `pull_request` runs, signs non-PR portable artifacts when the repository SignPath configuration is present, and requires SignPath configuration before publishing tagged releases.

### Security

- Tagged GitHub Releases now publish the SignPath-signed portable assets produced by CI instead of the unsigned packaging outputs.
- TrayToolbar now refuses to launch or install a staged automatic update unless the extracted `TrayToolbar.exe` passes Windows Authenticode validation and matches the configured TrayToolbar signer policy.

### Fixed

- Fixed `build.ps1`'s `dotnet msbuild` fallback so the local portable packaging parity script works on machines where standalone `msbuild.exe` is not on `PATH`.
- Removed the unsupported `allow_bypass_actors` key from the SignPath GitHub policy, which the SignPath policy loader rejected and which failed the `v1.7.2` release signing step.
- Reduced the SignPath branch ruleset policy to the force-push protection that both the `master` branch ruleset and the release tag ruleset can enforce, because release builds run on tag refs and GitHub tag rulesets cannot enforce pull request rules.
- Raised the SignPath signing request wait timeout from the ten minute default to one hour, so release builds do not time out while the signing request waits for manual approval.
- Disabled SignPath signing on `pull_request` builds entirely, so pull request validation no longer submits test signing requests.
- Serialized the builds that submit signing requests, so a release tag build no longer submits while the master push build for the same commit is still signing identically named artifacts.
- Archived and explicitly named the final portable release artifacts, so uploading them no longer conflicts with the unsigned artifacts uploaded for SignPath earlier in the same run. With `archive: false`, `actions/upload-artifact` names the artifact after the file and ignores the explicit name, which made both uploads claim the same artifact name.

## [1.7.1] - 2026-04-22

### Added

- Added `TrayToolbar.Benchmarks`, a BenchmarkDotNet-based benchmark project for folder scanning, file filtering, and menu-building performance work.
- Added contributor docs and templates to match the current build and release workflow.

### Changed

- Updated portable packaging so local builds produce the same `win-arm64` and `win-x64` zip assets used for releases.
- Routed update and release toast actions back through TrayToolbar before launch.

### Fixed

- Improved `.lnk` launching so executable shortcuts keep their saved arguments and working directory, and safe non-executable targets open directly.
- Reused the same shortcut-target resolution for folder-link submenus.

### Security

- Tightened GitHub release URL validation so lookalike `/releases` paths are rejected.

### Migration notes

- `CHANGELOG.md` remains the canonical release history, and `docs/release-notes.md` stays the short narrative summary.
- Contributor validation centers on `dotnet restore`, `dotnet test`, `dotnet format`, and `dotnet build` from `src\`, plus `./build.ps1` from the repository root.

### Breaking changes

- None.

## [1.7.0] - 2026-04-22

### Added

- Added built-in Windows toast notifications for update alerts without the previous notification dependency.
- Added one-click updating from the settings surface and update notifications.
- Added contributor-facing documentation, security guidance, and issue templates to support ongoing development.
- Added automated coverage for configuration handling, folder scanning, startup behavior, and update logic.

### Changed

- Reduced bundle size by replacing the previous notification dependency with the current Windows toast implementation.
- Standardized portable release asset names on `TrayToolbar-win-arm64-portable-<version>.zip` and `TrayToolbar-win-x64-portable-<version>.zip`.

### Security

- Hardened the self-update flow by validating release metadata, asset names, download URLs, content types, and SHA-256 digests before launching the staged updater.

### Migration notes

- Portable builds continue to require the .NET Desktop Runtime 8.
- Existing portable configurations are migrated from `<app folder>\TrayToolbar.json` to `%LOCALAPPDATA%\TrayToolbar\TrayToolbarConfig.json` when the new config file does not already exist.

### Breaking changes

- None.

[Unreleased]: https://github.com/brondavies/TrayToolbar/compare/v1.8.0...HEAD
[1.8.0]: https://github.com/brondavies/TrayToolbar/compare/v1.7.1...v1.8.0
[1.7.1]: https://github.com/brondavies/TrayToolbar/compare/v1.7.0...v1.7.1
[1.7.0]: https://github.com/brondavies/TrayToolbar/compare/v1.6.2...v1.7.0
