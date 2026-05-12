# Good first issue guidance

TrayToolbar is a Windows-specific .NET desktop application, so the best starter issues are small, focused, and easy to validate locally.

## What makes a good first issue here

A starter issue should:

- touch a small number of files
- have a clear expected result
- avoid complex Windows shell or tray-icon behavior unless heavily documented
- include the exact validation steps needed from `src\`
- mention any docs that must stay in sync, especially `CHANGELOG.md`, `docs/release-notes.md`, and `docs/update-security.md`

## Great first-task examples

- clarify or correct user-facing documentation
- expand deterministic tests around configuration, filtering, or update metadata parsing
- improve issue templates or contributor ergonomics
- tighten CI or release documentation without changing runtime behavior
- small launch-policy follow-ups with existing tests and docs as anchors

## Tasks that are usually *not* good starter issues

- broad UI refactors in `SettingsForm`
- shell-integration changes that need deep Windows behavior knowledge
- large localization sweeps across many resource files
- release or update changes that alter the asset contract without tests

## Maintainer checklist before applying `good first issue`

- [ ] Problem statement is concise and concrete
- [ ] Expected outcome is documented
- [ ] Relevant files are listed
- [ ] Validation commands are listed
- [ ] Any trust-boundary or release-doc sync requirements are called out

## Helpful links for new contributors

- [README.md](../README.md)
- [CONTRIBUTING.md](../CONTRIBUTING.md)
- [docs/developer-guide.md](developer-guide.md)
- [docs/labels.md](labels.md)
- [docs/update-security.md](update-security.md)
