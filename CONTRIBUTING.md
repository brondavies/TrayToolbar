# Contributing to TrayToolbar

Thanks for helping improve TrayToolbar.
This repository builds a Windows desktop app, so the most useful contributions are small, well-tested changes that stay aligned with the current .NET 8, Windows 11, and portable-release workflow.

## Before you start

TrayToolbar is a Windows Forms app targeting `net8.0-windows`.
Recommended contributor setup:

- Windows 11
- .NET 8 SDK
- .NET Desktop Runtime 8
- Visual Studio 2022 17.8+ or the `dotnet` CLI

Helpful references:

- User-facing overview: [`README.md`](README.md)
- Canonical release history: [`CHANGELOG.md`](CHANGELOG.md)
- Supplemental release notes: [`docs/release-notes.md`](docs/release-notes.md)
- Build, packaging, configuration, CLI, localization, and troubleshooting details: [`docs/developer-guide.md`](docs/developer-guide.md)
- Update and launch trust boundaries: [`docs/update-security.md`](docs/update-security.md)
- Security reporting guidance: [`SECURITY.md`](SECURITY.md)

## Repository layout

Important paths:

- Solution: `src/TrayToolbar.sln`
- App project: `src/TrayToolbar/TrayToolbar.csproj`
- Test project: `src/TrayToolbar.Tests/TrayToolbar.Tests.csproj`
- Benchmark project: `src/TrayToolbar.Benchmarks/TrayToolbar.Benchmarks.csproj`

## Command matrix

Use these commands as the authoritative contributor workflow.

| Purpose | Run from | Command | Notes |
| --- | --- | --- | --- |
| Restore | `src\` | `dotnet restore .\TrayToolbar.sln` | Restores app, tests, and benchmark dependencies. |
| Build | `src\` | `dotnet build .\TrayToolbar.sln` | Standard validation build. |
| Test | `src\` | `dotnet test .\TrayToolbar.sln` | Required before merging. |
| Format | `src\` | `dotnet format .\TrayToolbar.sln` | Run before sending changes out. |
| Package | repo root | `./build.ps1` | Local parity script for CI release packaging. |
| Benchmark (optional) | `src\` | `dotnet run -c Release --project .\TrayToolbar.Benchmarks\TrayToolbar.Benchmarks.csproj --filter *` | Run from an unplugged-from-debugger Release build only. |

Expected portable release outputs from `build.ps1` and CI:

- `TrayToolbar-win-arm64-portable-<version>.zip`
- `TrayToolbar-win-x64-portable-<version>.zip`

`build.ps1` also writes `publish/TrayToolbar.exe` as the last local publish output.

## Release and versioning discipline

- The version source of truth is `<Version>` in `src/TrayToolbar/TrayToolbar.csproj`.
- Release tags should use `v<Version>` and match the project version exactly.
- `CHANGELOG.md` is the canonical release history.
- `docs/release-notes.md` remains the supplemental narrative release summary.
- The GitHub Actions workflow in `.github/workflows/dotnet-desktop.yml` is the canonical producer of release artifacts.
- `build.ps1` remains the local parity and troubleshooting path.

Current workflow behavior:

- `push` / `pull_request` on `master`: restore, test, format verification, and build portable zip artifacts as workflow artifacts
- `workflow_dispatch`: same packaging flow for branch or tag dry runs
- `push` of `v*.*.*` tags: same packaging flow plus GitHub Release asset publication

If a release is intended to be update-visible, publish it as a stable GitHub Release with the expected portable asset names.
`UpdateLogic` validates those names and the GitHub Releases URL surface.

## Reproducible-build note

This repository currently aims for **functional parity** between local and CI builds rather than byte-for-byte artifact reproducibility.
Use the .NET 8 SDK, Release configuration, and the documented commands above when comparing outputs.

In practical terms:

- use the same major SDK family as CI (`8.0.x`)
- prefer `build.ps1` for packaging parity checks
- do not rename or reshape release assets without updating code, tests, and docs together

## Making changes

Please keep changes:

- small and focused
- consistent with the existing naming and code style
- limited to the files needed for the task

Avoid broad refactors unless the change clearly requires them.
If your change updates contributor workflow, release packaging, runtime expectations, or trust-boundary behavior, update the related docs in the same pull request.

That usually means reviewing one or more of:

- `README.md`
- `CHANGELOG.md`
- `docs/release-notes.md`
- `docs/developer-guide.md`
- `docs/update-security.md`
- `SECURITY.md`

## Files and folders that should usually stay untouched

Do not commit generated or published output from these paths:

- `bin/`
- `obj/`
- `publish/`
- `artifacts/`

Also avoid changing `*.Designer.cs` and `*.resx` files unless your work actually needs a UI or resource update.

## Testing and validation expectations

Before opening a pull request:

1. Run `dotnet test .\TrayToolbar.sln` from `src\`.
2. Run `dotnet format .\TrayToolbar.sln` from `src\`.
3. Run `dotnet build .\TrayToolbar.sln` from `src\`.
4. If your change affects packaging, update behavior, or release assets, run `./build.ps1` from the repository root.
5. If your change affects launch policy, update UX, or release trust boundaries, review `docs/update-security.md`, `SECURITY.md`, and the related tests together.

If you changed translations or UI layout, do a quick manual pass through the affected settings or tray-menu surfaces on Windows.

## Pull requests, labels, and newcomer guidance

- Pull requests use [`.github/PULL_REQUEST_TEMPLATE.md`](.github/PULL_REQUEST_TEMPLATE.md).
- Label meanings live in [`docs/labels.md`](docs/labels.md).
- Starter-task expectations live in [`docs/good-first-issue.md`](docs/good-first-issue.md).

Recommended issue label defaults:

- bug reports: `bug`, `triage`
- feature requests: `enhancement`, `triage`

## Reporting bugs and requesting features

Please use the repository issue templates when possible.
For the fastest triage, include:

- TrayToolbar version
- architecture (`x64` or `arm64`)
- Windows version/build
- install source or whether you built from source
- exact reproduction steps
- expected versus actual behavior
- logs, screenshots, or a sanitized config snippet when relevant

For security-sensitive problems, do **not** open a public issue.
Use the instructions in [`SECURITY.md`](SECURITY.md) instead.
