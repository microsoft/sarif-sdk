---
name: emit-sarif
description: Serialize AI-detected security findings as SARIF v2.1.0 conforming to the AI-generated-findings profile, using the Sarif.Multitool emit verbs.
metadata:
  author: sarif-sdk-maintainers
  version: "1.0.0"
  category: security
  packages:
    - "Sarif.Multitool >= 5.0.3"
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

- **`Sarif.Multitool` ≥ 5.0.3.** Recommended invocation: `dotnet dnx Sarif.Multitool --yes -- <verb> ...` (zero-install, version-resolved at first run; requires .NET 10+). Fall back to a global install with `dotnet tool install --global Sarif.Multitool` if `dotnet dnx` is unavailable.
- The current commit SHA, branch, repository URI, and a local source-root path.
- The normative profile doc: [`docs/ai/generating-sarif.md`](../../docs/ai/generating-sarif.md). Cross-reference it for every property you populate — do not invent vocabulary.

## Method

The skill uses these multitool verbs: `emit-run` → `add-result` / `add-invocation` (per finding or scan phase) → `add-notification-reporting-descriptor` / `add-rule-reporting-descriptor` (optional descriptor catalogs) → `emit-finalize --validate`. Each verb either appends to an event log (`<output>.wip.jsonl`) or replays the log into a finished SARIF file.

This staged design lets you build a run incrementally: hold one finding in working memory at a time, write it, move on. The final file is produced atomically by `emit-finalize`.

### Step 1 — Initialize the run

Construct a SARIF `Run` JSON object — the same partial-Run shape consumed by `SarifEventReplayer` — and pipe it to `emit-run`. The verb accepts the run header via `--input <path>` or stdin, exactly like `add-result` / `add-invocation`. There is no flag-based form; if a field belongs on `run.*` in the final SARIF, place it on the JSON you supply here.

```powershell
$runHeader = [ordered]@{
  tool = @{
    driver = [ordered]@{
      name             = "{{SCANNER_NAME}}"
      semanticVersion  = "{{SCANNER_SEMVER}}"
      informationUri   = "{{SCANNER_INFO_URI}}"
      organization     = "{{ORGANIZATION}}"
    }
  }
  versionControlProvenance = @(
    [ordered]@{
      repositoryUri = "{{REPO_URI}}"
      revisionId    = "{{COMMIT_SHA}}"
      branch        = "{{BRANCH}}"
      mappedTo      = @{ uriBaseId = "SRCROOT" }
    }
    # Add more entries as needed — submodules, additional checkouts,
    # cross-repo references. Attach a `properties` bag to any entry
    # (e.g., `properties.skills = @("xss-detector", "sql-tainter")`) to
    # document scanner/skill provenance for that source.
  )
  originalUriBaseIds = @{
    SRCROOT = @{ uri = "file:///{{LOCAL_SOURCE_ROOT}}" }
  }
  automationDetails = [ordered]@{
    guid = "{{NEW_GUID}}"
  }
  properties = @{
    "ai/origin" = "{{AI_ORIGIN}}"
  }
} | ConvertTo-Json -Depth 32

# Option A: pipe via stdin (matches add-result / add-invocation).
$runHeader | dotnet dnx Sarif.Multitool --yes -- emit-run "{{OUTPUT_PATH}}"

# Option B: write to a file and reference it.
$runHeader | Set-Content run-header.json
dotnet dnx Sarif.Multitool --yes -- emit-run "{{OUTPUT_PATH}}" --input run-header.json
```

Inputs:

| Placeholder | Required | Notes |
|---|---|---|
| `{{OUTPUT_PATH}}` | yes | Final SARIF path, e.g. `out/myscanner-<sha-short>.sarif`. Staged event log is written alongside as `<output>.wip.jsonl`. |
| `{{SCANNER_NAME}}` | yes | `run.tool.driver.name`. Keep stable across model upgrades — it is the producer identity. |
| `{{SCANNER_SEMVER}}` | yes | SemVer 2.0 string for `run.tool.driver.semanticVersion`. |
| `{{AI_ORIGIN}}` | yes | One of `generated`, `annotated`, `synthesized`. See `generating-sarif.md § AI Origin Declaration`. |
| `{{REPO_URI}}` / `{{COMMIT_SHA}}` / `{{BRANCH}}` | yes | Populates the first `run.versionControlProvenance` entry. Required by rule AI1004. Add additional entries — each with its own `properties` bag if useful — to document submodules, additional checkouts, or per-source scanner/skill provenance. |
| `{{LOCAL_SOURCE_ROOT}}` | yes for snippet/hash enrichment | A `file://` URI that the SDK can read to compute snippets and artifact hashes during `emit-finalize`. Rewritten to a portable URI in the finalize step. |
| `{{NEW_GUID}}` | yes | A fresh RFC 4122 GUID for `run.automationDetails.guid`. Required by rule AI2005. |

The verb validates a small set of profile-essential fields at receipt: `tool.driver.name` is required and must be a non-empty string; `tool.driver.informationUri` and `versionControlProvenance[].repositoryUri` must be `https`; `originalUriBaseIds["SRCROOT"].uri` must be `https` or `file`, and a `file:` source root must resolve to a directory that exists on disk when the run header is received, so `emit-finalize` can enrich result locations against an observable checkout; GUIDs must be canonical 8-4-4-4-12 strings; `ai/origin` must be one of `generated`, `annotated`, `synthesized`. Anything else the SARIF schema accepts on a partial `Run` is appended to the `.wip.jsonl` run-header event unchanged; note that `emit-finalize` materializes a typed `SarifLog` from that event log, so fields outside the SDK's typed `Run` model are dropped at finalize. Durable custom data should live in SARIF `properties` bags, which the typed model preserves.

When the `TF_BUILD=True` environment indicates an Azure DevOps pipeline, `emit-run` stamps `automationDetails.id` plus the four `azuredevops/pipeline/build/*` properties required by GHAzDO ingestion. If your JSON supplies any of those fields, the values must match what the env detects, otherwise the verb fails with a conflict diagnostic — pick one source of truth.

### Step 2 — Append each result

For each finding, construct a complete SARIF `result` JSON object that conforms to `docs/ai/generating-sarif.md § Result Structure`, then append it:

```powershell
# Option A: write the result to a JSON file, then point at it
'@{ ... your result JSON ... }' | Set-Content result-001.json
dotnet dnx Sarif.Multitool --yes -- add-result "{{OUTPUT_PATH}}" --input result-001.json

# Option B: pipe the result JSON via stdin
Get-Content result-001.json | dotnet dnx Sarif.Multitool --yes -- add-result "{{OUTPUT_PATH}}"
```

The result JSON must include at minimum: `ruleId` (with sub-ID per AI1012, e.g. `CWE-78/api-handler`), `level`, `message.text`, `message.markdown` (AI1005), and at least one `locations[].physicalLocation` with a `region.startLine`. For security findings, also populate the `ai/*` keys the profile recommends — `ai/exploitability` and `ai/attackerPosition` (AI2014, and `ai/evidence` per AI2015 when present). These `ai/*` keys are SHOULD, not MUST, and AI2014's set is all-or-nothing: emit the whole group or none.

**Vocabulary discipline:** only the eight `ai/*` keys defined in the profile are valid. Do not invent additional `ai/*` keys; place tool-specific data under a tool-named namespace instead (e.g. `myscanner/confidence`).

### Step 3 — Append invocations (optional but recommended)

Use `add-invocation` to record one or more `Invocation` objects (`startTimeUtc`, `endTimeUtc`, `executionSuccessful`, `exitCode`, `commandLine`, `arguments`, `workingDirectory`, `environmentVariables`, properties bag, …). The replayer appends invocations to `run.invocations[]` in event order.

Notifications travel inline on the invocation payload. Place each in the invocation's `toolExecutionNotifications` (execution narrative) or `toolConfigurationNotifications` (configuration feedback) array — the array selects placement. Descriptor ids name the concern only (e.g. `DECISION`, `DATA-ACCESS-DENIED`) — no `AI/`, `EXEC/`, `CFG/`, or `<toolName>/` prefix. Every inline notification requires a producer-supplied `timeUtc`. See `docs/ai/generating-sarif.md § Execution Narrative & Configuration Feedback` for descriptor inventory and required shape.

```powershell
Get-Content invocation.json | dotnet dnx Sarif.Multitool --yes -- add-invocation "{{OUTPUT_PATH}}"
```

### Step 4 — Register reporting descriptors (optional)

Two verbs append `reportingDescriptor` objects to the run's tool-driver catalogs. Both are producer-authored and validated at receipt against the same overlay schemas served by `get-schema`.

- `add-notification-reporting-descriptor` appends a descriptor to `run.tool.driver.notifications[]` — the catalog that gives stable metadata (id, name, message strings) for the inline notifications recorded in Step 3.
- `add-rule-reporting-descriptor` appends a descriptor with a `NOVEL-<kebab-sub-id>` id to `run.tool.driver.rules[]` — for novel rules the producer defines. Taxonomy/CWE rule descriptors are injected by the SDK at finalize and must not be supplied here.

```powershell
Get-Content notification-descriptor.json | dotnet dnx Sarif.Multitool --yes -- add-notification-reporting-descriptor "{{OUTPUT_PATH}}"
Get-Content rule-descriptor.json         | dotnet dnx Sarif.Multitool --yes -- add-rule-reporting-descriptor "{{OUTPUT_PATH}}"
```

### Step 5 — Finalize and validate

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
2. The file passes the [validate-sarif skill](../validate-sarif/SKILL.md) at full profile depth.
3. The file is consumable by the SDK object model (`SarifLog.Load`) without exceptions.

Any of these failing means the producer drifted from the profile. The validation skill's "Known Drift Patterns" catalog enumerates the most common drift modes — consult it when finalize fails.

## Reference example

A complete reference SARIF file conforming to the AI profile is at [`docs/ai/example.sarif`](../../docs/ai/example.sarif). The CWE-driven taxonomy sample at [`src/Sarif/Taxonomies/CweGhasSample.sarif`](../../src/Sarif/Taxonomies/CweGhasSample.sarif), generated by [`CweGenerateSample.ps1`](../../src/Sarif/Taxonomies/CweGenerateSample.ps1), is the canonical SDK-generated sample and demonstrates the same emit-verb sequence in PowerShell.

## Escalation

- **Multitool unavailable** — Install .NET 10+ for `dotnet dnx`, or `dotnet tool install --global Sarif.Multitool`. Do **not** attempt to hand-author SARIF JSON: the SDK's emit verbs handle enrichment, validation, and consistency in ways that are difficult to replicate by hand. If you genuinely have no .NET environment, the profile doc is the source of truth — read it carefully — but expect to invest significant effort to match SDK output.
- **`emit-finalize --validate` reports persistent errors** — Inspect the validation output (rule ID, message). Cross-reference the rule ID with `docs/ValidationRules.md` and the AI rule list in the profile doc. If a rule appears wrong (false positive against a correct construct), file an issue against the SDK — do not silence the rule.
- **Source root not available locally** — Omit `originalUriBaseIds["SRCROOT"]` from the run header JSON and drop `--srcroot` from `emit-finalize`. Snippets and artifact hashes will be empty; `--embed-text-files` will have no effect. The resulting log is still profile-conformant but less rich for consumers.
