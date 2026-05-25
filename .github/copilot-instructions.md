# Copilot / Agent Instructions for `microsoft/sarif-sdk`

Short, durable conventions. If you're an agent contributing to this repo, read
these first — they capture the code-review notes that come up most often.

## ReleaseHistory.md style

`ReleaseHistory.md` is **release notes**, not a PR description. Each bullet is
a single self-contained sentence (or two, if a `BRK` requires it). The PR
description carries the narrative — motivation, design alternatives,
audit trail. The release-notes bullet carries only the consumer-facing
truth: what changed and the smallest set of facts a downstream consumer
needs to act on it.

Concretely:

- One bullet per change. Lead with `NEW:` / `BRK:` / `BUG:` / `BUGFIX:`.
- Public surface or behavior change only. Internal refactors and test
  additions belong in the PR, not in `ReleaseHistory.md`.
- Mention concrete names — types, members, CLI flags, rule IDs — not
  prose paraphrases.
- For a flag or verb: state what it does in one clause, then list the
  inputs/outputs it touches. No three-state detection paragraphs.
- For a `BRK`: state the rename or signature change and what callers
  must do. Don't re-litigate the motivation.
- Link rule IDs to their numeric form (`GHAzDO1019`, `AI1010`, etc.) so
  cross-references in the changelog stay greppable.

Calibration: read the bullets above the `UNRELEASED` section that you're
adding to. If your bullet is more than ~3× the length of its neighbors,
you're writing a PR description. Trim or split.

## House idioms (the recurring code-review notes)

- **No `[Theory]` / `[InlineData]`.** When the same behavior needs to be
  exercised over multiple inputs, write multiple `[Fact]` methods that
  delegate to a shared private helper. This is a strong style preference
  across the repo.
- **`GHAzDO` casing is canonical** (not `GHAZDO`, not `Ghazdo`). The
  visual structure `GH` + `Az` + `DO` is preserved deliberately to read
  as "GitHub Advanced Security for Azure DevOps". Every type, file,
  namespace, resource key, and prose mention uses exactly `GHAzDO`. The
  only places lowercase/uppercase variants appear are inside doc strings
  that demonstrate `--rule-kind` case-insensitive parsing.
- **PowerShell parameters are case-insensitive by default.** Don't
  invent ceremony around that — `-GHAzDO`, `-ghazdo`, `-GHAZDO` all
  bind to the same `[switch]`.
- **AI ruleId convention (`AI-RULEID-001`).** Every AI-emitted
  `result.ruleId` must take taxonomy sub-id form (`CWE-89/kql-injection`)
  or the NOVEL escape hatch (`NOVEL-<sub-id>`); the `NOVEL-` form does
  not accept a slash. `AI1012` stays silent on conformant ids.
- **GHAzDO automationDetails contract.** When a producer fills
  `run.automationDetails`, it must satisfy GHAzDO1019 (the four
  `azuredevops/pipeline/build/*` property keys) and GHAzDO1020 (`id`
  starts with `azuredevops/pipeline/build/`). The env-driven
  `AdoPipelineContext` path in `emit-init-run` produces a compliant
  shape automatically when `TF_BUILD=True`.
- **Sample fixture convention.** Every taxonomy ships a
  `<Tax>GenerateSample.ps1` that produces a deterministic
  `<Tax>Sample.sarif`, and a `<Tax>GeneratedSampleTests.cs` that gates
  byte-identical regen. Variants of a sample (e.g. the GHAzDO companion)
  are produced by parameters on the same script, each gated by its own
  `[Fact]`.
- **Side effects after detection, never before.** Verbs that detect
  context from env / args (e.g. `AdoPipelineContext.TryDetect`) must
  validate inputs and fail the partial-input case before touching the
  filesystem. A half-stamped SARIF is worse than a clean refusal.
- **Internals are test-visible via `InternalsVisibleTo`.** Declared in
  `src/Shared/CommonAssemblyInfo.cs`. New `internal const` env-var name
  fields or test-only helpers are reachable from the standard test
  assemblies without needing to be made `public`.

## Commit / PR mechanics

- Commit messages: imperative subject line, blank line, body that
  matches PR-description register (longer is fine).
- Include `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
  as a trailer on every commit you author.
- PR titles mirror the commit subject. Don't add ticket numbers — the
  repo doesn't use them.
- PR descriptions are where the long narrative goes (env-var tables,
  three-state detection prose, design tradeoffs). Don't duplicate that
  in `ReleaseHistory.md`.
