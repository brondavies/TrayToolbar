# COMReference Replacement Plan

## Objective

Replace the build-time `<COMReference Include="IWshRuntimeLibrary">` in `src/TrayToolbar/TrayToolbar.csproj` with a stable, repo-controlled interop assembly or package approach so CLI tooling such as `dotnet format` no longer depends on `ResolveComReference`.

## Current state

- The project currently uses a COM reference for `IWshRuntimeLibrary` in `src/TrayToolbar/TrayToolbar.csproj`.
- The only known source usage is in `src/TrayToolbar/Extensions/ExtensionMethods.cs`:
  - `new WshShell()`
  - `(IWshShortcut)shell.CreateShortcut(shortcutPath)`
  - `link.TargetPath`
- The project already uses `EmbedInteropTypes=true`, which is helpful to preserve.

## Recommended approach

Use a pre-generated interop assembly (`Interop.IWshRuntimeLibrary.dll`) created once with `tlbimp.exe`, store it in a repo-controlled location such as `src/TrayToolbar/lib/`, and reference it as a normal managed assembly.

This is preferred over:

- keeping `<COMReference>`: still triggers build-time COM resolution
- switching to `<COMFileReference>`: still goes through `ResolveComReference`
- depending on an unvetted third-party public NuGet package

## Implementation steps

### 1. Generate a stable wrapper assembly

From a Visual Studio Developer PowerShell or Developer Command Prompt, generate the wrapper from the Windows Script Host type library.

Suggested command:

`tlbimp "%WINDIR%\System32\wshom.ocx" /out:"src\TrayToolbar\lib\Interop.IWshRuntimeLibrary.dll" /namespace:IWshRuntimeLibrary /machine:Agnostic`

Notes:

- Keep the namespace as `IWshRuntimeLibrary` so existing C# code does not need to change.
- Generate the wrapper once and treat it as a source-controlled binary artifact.
- If needed later, move this into an internal NuGet package, but start with a checked-in DLL for simplicity.

### 2. Replace the project reference

Remove the existing `<COMReference>` item from `src/TrayToolbar/TrayToolbar.csproj`.

Add a managed reference instead:

- `Include="Interop.IWshRuntimeLibrary"`
- `HintPath="lib\Interop.IWshRuntimeLibrary.dll"`
- `EmbedInteropTypes="true"`
- `Private="false"`

Target outcome:

- no build-time COM wrapper generation
- no dependency on registry-based COM resolution during formatting/build analysis
- no code changes required for existing `using IWshRuntimeLibrary;`

### 3. Keep the runtime expectation explicit

Document that this change removes the build-time COM dependency, but runtime still depends on Windows Script Host COM being available on the machine.

Add a short note to one of:

- `README.md`
- `AGENTS.md`
- a future developer guide

Suggested note topics:

- why the interop DLL is checked in
- how it was generated
- when it should be regenerated

### 4. Validate the build and tooling behavior

Run the smallest relevant validation first:

1. `dotnet format .\TrayToolbar.sln`
2. `dotnet build .\TrayToolbar.sln`
3. `dotnet test .\TrayToolbar.sln` (if test projects are added later)

Expected result:

- `dotnet format` should no longer emit the `ResolveComReference` / `MSB4803` warning for this project.

### 5. Optional future refinement

If reuse across repositories becomes desirable, wrap `Interop.IWshRuntimeLibrary.dll` in an internal NuGet package and replace the file-based reference with a package reference.

Use this only if the team already has:

- an internal package feed
- versioning expectations for shared interop assets
- a maintenance path for publishing package updates

## Risks and mitigations

### Risk: generated wrapper differs from current embedded interop

Mitigation:

- preserve the `IWshRuntimeLibrary` namespace
- keep `EmbedInteropTypes=true`
- build and test shortcut resolution on a Windows 11 machine

### Risk: runtime COM availability is confused with build-time COM availability

Mitigation:

- document clearly that the change removes build-time COM resolution only
- retain Windows-specific validation for shortcut target resolution

### Risk: wrapper regeneration becomes tribal knowledge

Mitigation:

- document the exact `tlbimp.exe` command in the repo
- keep the generated DLL in a predictable path such as `src/TrayToolbar/lib/`

## Definition of done

- `TrayToolbar.csproj` no longer contains `<COMReference Include="IWshRuntimeLibrary">`
- The project references a repo-controlled interop DLL or internal package instead
- Existing shortcut resolution code still compiles without API changes
- `dotnet format .\TrayToolbar.sln` no longer reports the `ResolveComReference` / `MSB4803` warning for this dependency
- The generation/maintenance process is documented for future contributors
