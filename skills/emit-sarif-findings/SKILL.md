---
name: emit-sarif-findings
description: Serialize AI-detected security findings as SARIF v2.1.0 conforming to the AI-generated-findings profile, using the Sarif.Multitool emit verbs.
metadata:
  author: sarif-sdk-maintainers
  version: "1.0.0"
  category: security
  packages:
    - "Sarif.Multitool >= 5.0.0"
  triggers:
    - "emit SARIF"
    - "write findings to SARIF"
    - "produce SARIF log"
    - "ai/origin"
---

# Emit SARIF Findings

## Context

You have completed a security analysis of a codebase and hold one or more findings in working memory. This skill serializes those findings as a SARIF v2.1.0 log that downstream tooling — result-management systems, triage agents, and autonomous remediation agents — can consume without tool-specific knowledge.

The output contract is the **AI-generated-findings profile** defined in [`docs/ai/generating-sarif.md`](../../docs/ai/generating-sarif.md). That document is the normative reference; this skill is the operational wrapper that uses the `Sarif.Multitool` emit verbs to produce a conformant log.

## When to apply this skill

Apply this skill when an agent is the **originating** detector (not post-processing another tool's SARIF) and needs to persist findings. Signals:

- The agent has enumerated vulnerabilities with file/line locations and CWE classifications.
- A downstream step expects a `.sarif` artifact.
- The orchestrator requests `ai/origin: "generated"` output (or `annotated` / `synthesized`).

## Prerequisites

- **`Sarif.Multitool` ≥ 5.0.0.** Recommended invocation: `dotnet dnx Sarif.Multitool --yes -- <verb> ...` (zero-install, version-resolved at first run; requires .NET 10+). Fall back to a global install with `dotnet tool install --global Sarif.Multitool` if `dotnet dnx` is unavailable.
- The current commit SHA, branch, repository URI, and a local source-root path.
- The normative profile doc: [`docs/ai/generating-sarif.md`](../../docs/ai/generating-sarif.md). Cross-reference it for every property you populate — do not invent vocabulary.

## Method

The skill uses four multitool verbs in sequence: `emit-init-run` → `add-result` (and/or `add-notification`) per finding → `emit-finalize --validate`. Each verb either appends to an event log (`<output>.wip.jsonl`) or replays the log into a finished SARIF file.

This staged design lets you build a run incrementally: hold one finding in working memory at a time, write it, move on. The final file is produced atomically by `emit-finalize`.

### Step 1 — Initialize the run

```powershell
dotnet dnx Sarif.Multitool --yes -- emit-init-run "{{OUTPUT_PATH}}" `
  --tool-driver-name "{{SCANNER_NAME}}" `
  --tool-driver-semantic-version "{{SCANNER_SEMVER}}" `
  --information-uri "{{SCANNER_INFO_URI}}" `
  --organization "{{ORGANIZATION}}" `
  --ai-origin "{{AI_ORIGIN}}" `
  --vcp-repositoryuri "{{REPO_URI}}" `
  --vcp-revisionid "{{COMMIT_SHA}}" `
  --vcp-branch "{{BRANCH}}" `
  --srcroot "file:///{{LOCAL_SOURCE_ROOT}}" `
  --automation-guid "{{NEW_GUID}}"
```

Inputs:

| Placeholder | Required | Notes |
|---|---|---|
| `{{OUTPUT_PATH}}` | yes | Final SARIF path, e.g. `out/myscanner-<sha-short>.sarif`. Staged event log is written alongside as `<output>.wip.jsonl`. |
| `{{SCANNER_NAME}}` | yes | `run.tool.driver.name`. Keep stable across model upgrades — it is the producer identity. |
| `{{SCANNER_SEMVER}}` | yes | SemVer 2.0 string for `run.tool.driver.semanticVersion`. |
| `{{AI_ORIGIN}}` | yes | One of `generated`, `annotated`, `synthesized`. See `generating-sarif.md § AI Origin Declaration`. |
| `{{REPO_URI}}` / `{{COMMIT_SHA}}` / `{{BRANCH}}` | yes | Populates `run.versionControlProvenance[0]`. Required by rule AI1004. |
| `{{LOCAL_SOURCE_ROOT}}` | yes for snippet/hash enrichment | A `file://` URI that the SDK can read to compute snippets and artifact hashes during `emit-finalize`. Rewritten to a portable URI in the finalize step. |
| `{{NEW_GUID}}` | yes | A fresh RFC 4122 GUID for `run.automationDetails.guid`. Required by rule AI2005. |

### Step 2 — Append each result

For each finding, construct a complete SARIF `result` JSON object that conforms to `docs/ai/generating-sarif.md § Result Structure`, then append it:

```powershell
# Option A: write the result to a JSON file, then point at it
'@{ ... your result JSON ... }' | Set-Content result-001.json
dotnet dnx Sarif.Multitool --yes -- add-result "{{OUTPUT_PATH}}" --input result-001.json

# Option B: pipe the result JSON via stdin
Get-Content result-001.json | dotnet dnx Sarif.Multitool --yes -- add-result "{{OUTPUT_PATH}}"
```

The result JSON must include at minimum: `ruleId` (with sub-ID per AI1012, e.g. `CWE-78/api-handler`), `kind: "fail"`, `level`, `message.text`, `message.markdown` (AI1005), at least one `locations[].physicalLocation` with a `region.startLine`, and any `ai/*` property bag entries required by the profile (`ai/exploitability`, `ai/attackerPosition`, `ai/evidence`).

**Vocabulary discipline:** only the eight `ai/*` keys defined in the profile are valid. Do not invent additional `ai/*` keys; place tool-specific data under a tool-named namespace instead (e.g. `myscanner/confidence`).

### Step 3 — Append notifications (optional but recommended)

Use `add-notification` for execution narrative and configuration feedback. Notification descriptor ids name the concern only (e.g. `DECISION`, `DATA-ACCESS-DENIED`) — no `AI/`, `EXEC/`, `CFG/`, or `<toolName>/` prefix. Placement is selected at the verb: the default routes to `toolExecutionNotifications`; pass `--config` (`-c`) to route to `toolConfigurationNotifications` instead.

```powershell
Get-Content notification-001.json | dotnet dnx Sarif.Multitool --yes -- add-notification "{{OUTPUT_PATH}}"
```

See `docs/ai/generating-sarif.md § Execution Narrative & Configuration Feedback` for descriptor inventory and required shape.

### Step 4 — Finalize and validate

```powershell
dotnet dnx Sarif.Multitool --yes -- emit-finalize "{{OUTPUT_PATH}}" `
  --srcroot "{{PORTABLE_SRCROOT_URI}}" `
  --embed-text-files `
  --validate
```

What this does:

1. Replays the `.wip.jsonl` event log into a final SARIF file.
2. Runs `InsertOptionalDataVisitor` against the local source root to populate snippets, context regions, and artifact hashes.
3. Enriches CWE-as-rule-id descriptors from the embedded MITRE CWE taxonomy (omit with `--no-cwe-enrichment` if you've already populated descriptors).
4. Rewrites `originalUriBaseIds["SRCROOT"]` to `{{PORTABLE_SRCROOT_URI}}` (typically `https://github.com/<org>/<repo>/blob/<sha>/`) so the published log anchors at a host-independent location.
5. Embeds text-file artifact contents (`--embed-text-files`). Useful for self-contained AI fixtures and to clear `SARIF2013`.
6. Runs the validator against the output with `--rule-kind Sarif;AI` (`--validate`). Fails non-zero with a summary if any Error-level findings are reported.

If `--validate` reports errors, the produced file is on disk but did not meet the profile. Treat this as a generation defect: fix the offending result or notification, regenerate, and re-finalize.

## Validation

This skill's contract is satisfied when:

1. `emit-finalize --validate` exits with code 0 (no Error-level rule findings under `--rule-kind Sarif;AI`).
2. The file passes the [validate-sarif-findings skill](../validate-sarif-findings/SKILL.md) at full profile depth.
3. The file is consumable by the SDK object model (`SarifLog.Load`) without exceptions.

Any of these failing means the producer drifted from the profile. The validation skill's "Known Drift Patterns" catalog enumerates the most common drift modes — consult it when finalize fails.

## Reference example

A complete reference SARIF file conforming to the AI profile is at [`docs/ai/example.sarif`](../../docs/ai/example.sarif). The CWE-driven taxonomy sample at [`src/Sarif/Taxonomies/CweSample.sarif`](../../src/Sarif/Taxonomies/CweSample.sarif), generated by [`CweGenerateSample.ps1`](../../src/Sarif/Taxonomies/CweGenerateSample.ps1), is the canonical SDK-generated sample and demonstrates the same emit-verb sequence in PowerShell.

## Escalation

- **Multitool unavailable** — Install .NET 10+ for `dotnet dnx`, or `dotnet tool install --global Sarif.Multitool`. Do **not** attempt to hand-author SARIF JSON: the SDK's emit verbs handle enrichment, validation, and consistency in ways that are difficult to replicate by hand. If you genuinely have no .NET environment, the profile doc is the source of truth — read it carefully — but expect to invest significant effort to match SDK output.
- **`emit-finalize --validate` reports persistent errors** — Inspect the validation output (rule ID, message). Cross-reference the rule ID with `docs/ValidationRules.md` and the AI rule list in the profile doc. If a rule appears wrong (false positive against a correct construct), file an issue against the SDK — do not silence the rule.
- **Source root not available locally** — Drop `--srcroot` from `emit-init-run`. Snippets and artifact hashes will be empty; `--embed-text-files` will have no effect. The resulting log is still profile-conformant but less rich for consumers.
