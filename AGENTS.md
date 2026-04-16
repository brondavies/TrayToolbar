# AGENTS.md

## Project
- Windows Forms app targeting `net8.0-windows`
- Solution: `src/TrayToolbar.sln`
- Project: `src/TrayToolbar/TrayToolbar.csproj`
- Tests: `src/TrayToolbar.Tests/TrayToolbar.Tests.csproj`

## Working rules
- Keep changes small and task-focused.
- Don't use powershell to edit files unless absolutely necessary. Prefer manual edits for code changes, and use powershell only for bulk updates or formatting.
- Preserve existing style and naming; avoid broad refactors unless instructed.
- Do not edit generated/build output in `bin/`, `obj/`, `publish/`, or `artifacts/`.
- Treat `*.Designer.cs` and `*.resx` as UI/resource files; change them only when needed.
- Avoid changing versioning, packaging, or release artifacts unless requested.

## Validation
- Run `dotnet format .\TrayToolbar.sln` from `src\` before committing.
- Preferred repo release build: `./build.ps1`
- If the task is narrow, use the smallest relevant validation step first.
- All unit tests must pass before merging. Run `dotnet test .\TrayToolbar.sln` from `src\`.

## Notes
- This is Windows-specific desktop code; keep fixes compatible with Windows 11.
- Contributor-facing workflow guidance now lives in `CONTRIBUTING.md` and `SECURITY.md`; keep those files in sync if validation or reporting expectations change.
- Update and launch trust-boundary guidance now lives in `docs/update-security.md`; if release packaging changes, keep that document and the update-related tests in sync.