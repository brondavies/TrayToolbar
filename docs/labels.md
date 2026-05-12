# Label taxonomy

Use labels to communicate both the *type* of work and the *next triage action*.

## Core triage labels

| Label | Meaning | Typical use |
| --- | --- | --- |
| `triage` | Newly filed and not yet fully categorized | Default label on new issues from templates |
| `bug` | Confirmed defect or regression | Reproducible runtime, update, launch, or UI problems |
| `enhancement` | Requested improvement | Feature requests or UX improvements |
| `documentation` | Docs-only work | README, contributor docs, release notes, security docs |
| `tests` | Test-only or test-heavy work | Coverage expansion, deterministic seams, benchmark scaffolding |
| `release` | Packaging, workflow, or versioning work | GitHub Actions, release assets, changelog, version tags |
| `security` | Security-sensitive hardening or reporting follow-up | Trust-boundary changes, update integrity, launch policy |

## Contributor-friendly labels

| Label | Meaning | Guidance |
| --- | --- | --- |
| `good first issue` | Small, well-bounded task for new contributors | Should include reproduction details, expected outcome, and likely files |
| `help wanted` | Maintainers welcome outside contribution | Good fit for independent work after scope is understood |

## Suggested combinations

- New defect report: `bug`, `triage`
- New feature request: `enhancement`, `triage`
- Small docs cleanup: `documentation`, `good first issue`
- Test gap or benchmark follow-up: `tests`, `help wanted`
- Packaging or workflow parity work: `release`, `triage`
- Launch-policy or update-boundary changes: `security`, `release`

## Triage notes

- Remove `triage` once the issue has an owner, confirmed scope, or a clear “not planned” decision.
- Reserve `good first issue` for tasks with low ambiguity and minimal repo-specific setup.
- When a change touches release packaging or trust boundaries, cross-link the related sections in `CHANGELOG.md`, `docs/release-notes.md`, and `docs/update-security.md`.
