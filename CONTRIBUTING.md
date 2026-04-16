# Contributing to TrayToolbar

Thanks for helping improve TrayToolbar.
This repository builds a Windows desktop app, so the most useful contributions are small, well-tested changes that match the current .NET 8 and Windows workflow.

## Before you start

TrayToolbar is a Windows Forms app targeting `net8.0-windows`.
Contributor validation is centered on Windows and the .NET 8 SDK.

Recommended local setup:

- Windows 11 for the primary contributor workflow
- .NET 8 SDK
- .NET Desktop Runtime 8
- Visual Studio 2022 17.8+ or the `dotnet` CLI

Helpful references:

- User-facing overview: [`README.md`](README.md)
- Contributor-focused build and troubleshooting details: [`docs/developer-guide.md`](docs/developer-guide.md)
- Update and release trust boundaries: [`docs/update-security.md`](docs/update-security.md)

## Repository layout

Important paths:

- Solution: `src/TrayToolbar.sln`
- App project: `src/TrayToolbar/TrayToolbar.csproj`
- Test project: `src/TrayToolbar.Tests/TrayToolbar.Tests.csproj`

## Development workflow

From the `src\` directory, use the current validation commands:

- `dotnet format .\TrayToolbar.sln`
- `dotnet test .\TrayToolbar.sln`
- `dotnet build .\TrayToolbar.sln`

From the repository root, the preferred release build is:

- `./build.ps1`

That script restores and publishes the app project, writes `publish/TrayToolbar.exe`, and creates the portable release zip files at the repository root.

## Making changes

Please keep changes:

- small and focused
- consistent with the existing naming and code style
- limited to the files needed for the task

Avoid broad refactors unless the change clearly requires them.
If your change updates contributor workflow, release packaging, runtime expectations, or trust-boundary behavior, update the related docs in the same pull request.

That usually means reviewing one or more of:

- `README.md`
- `AGENTS.md`
- `docs/developer-guide.md`
- `docs/update-security.md`

## Files and folders that should usually stay untouched

Do not commit generated or published output from these paths:

- `bin/`
- `obj/`
- `publish/`
- `artifacts/`

Also avoid changing `*.Designer.cs` and `*.resx` files unless your work actually needs a UI or resource update.

## Testing and validation

Before opening a pull request:

1. Run `dotnet test .\TrayToolbar.sln` from `src\`.
2. Run `dotnet format .\TrayToolbar.sln` from `src\`.
3. If your change affects packaging or update behavior, run `./build.ps1` from the repository root.
4. If your change affects update assets, release packaging, or launch policy, make sure the related tests and `docs/update-security.md` stay in sync.

If you changed translations or UI layout, please also do a quick manual pass through the affected settings or tray menu surfaces on Windows.

## Pull request tips

A good pull request usually includes:

- a short explanation of the problem being solved
- the smallest reasonable fix
- notes about any user-visible behavior changes
- screenshots when the UI changed
- test coverage or reproduction notes when a bug is fixed

If you are changing behavior that affects configuration, updates, or diagnostics, include enough detail for reviewers to understand the expected before/after behavior.

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
