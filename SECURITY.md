# Security Policy

## Supported versions

TrayToolbar is maintained on a best-effort basis.
For security fixes, please assume that only the latest published release is supported.
Older releases may not receive backported fixes.

| Version | Supported |
| --- | --- |
| Latest release | :white_check_mark: Best effort |
| Older releases | :x: No |
| Unreleased local builds / forks | :warning: Case by case |

## Reporting a vulnerability

Please do **not** open a public GitHub issue for suspected security problems.

Preferred reporting path:

1. Use GitHub's private vulnerability reporting for this repository using the **Report a vulnerability** option available from the repository's **Security** tab.
2. If private vulnerability reporting is not enabled or you cannot access it, contact the repository owner privately through the contact options on their GitHub profile: <https://github.com/brondavies>.
3. Include `TrayToolbar security report` in the subject or first line so the report can be triaged quickly.

When possible, include:

- affected TrayToolbar version
- architecture (`x64` or `arm64`)
- Windows version/build
- how the app was installed or built
- clear reproduction steps or a proof of concept
- expected impact
- whether the issue affects update checks, release downloads, launch behavior, configuration handling, or local file/folder scanning

Please share only the minimum data needed to reproduce the issue, and sanitize local paths, usernames, tokens, or other sensitive information before sending it.

## What counts as a security issue here

Examples that are in scope for private reporting include:

- bypasses in update integrity or release asset validation
- flaws in GitHub release metadata handling, digest verification, download staging, or update execution
- direct or indirect paths to unintended code execution through TrayToolbar-controlled behavior
- privilege, path-handling, or trust-boundary issues that could let untrusted data escape the intended local-only model
- vulnerabilities in the app or its shipped dependencies that have a practical security impact
- release provenance or remote launch-policy weaknesses that create a real integrity or spoofing risk

Examples that are usually **not** security reports:

- feature requests or general hardening ideas without a demonstrated vulnerability
- ordinary crashes, usability bugs, or build failures without security impact
- support questions about installation or configuration

## Notes about update integrity and code signing

TrayToolbar currently validates GitHub release metadata and asset digests as part of its update flow.
That contract is documented in [`docs/update-security.md`](docs/update-security.md).

TrayToolbar does **not** currently enforce Authenticode code-signing validation during update.
Reports that demonstrate a practical integrity or impersonation weakness are welcome.
A request to add code signing by itself may be treated as future hardening work rather than an active vulnerability.

## Disclosure and response expectations

This project is maintained on a best-effort basis and does not offer a formal response SLA.
If you report a valid issue privately, please allow reasonable time for investigation and remediation before public disclosure.
If the report is accepted, the preferred path is to fix the issue first and then disclose the details responsibly.
