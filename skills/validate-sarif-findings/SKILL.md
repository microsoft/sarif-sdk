---
name: validate-sarif-findings
description: Validates SARIF files against the SARIF 2.1.0 schema and the AI-generated-findings profile rules shipped by Sarif.Multitool.
metadata:
  author: sarif-sdk-maintainers
  version: "1.0.0"
  category: sarif
  severity: medium
  packages:
    - "Sarif.Multitool >= 5.0.0"
  triggers:
    - "validate sarif"
    - "check sarif"
    - "sarif validation"
    - "sarif lint"
    - "ai profile validation"
---

# Validate SARIF Findings

## Context

SARIF files produced by AI security agents must conform to two layers of correctness:

1. **Schema layer** — Valid SARIF 2.1.0 per the OASIS standard. Structural issues like misplaced properties, missing required fields, or type mismatches. `Sarif.Multitool validate` checks this.

2. **AI profile layer** — The conventions defined in [`docs/ai/generating-sarif.md`](../../docs/ai/generating-sarif.md). These include closed vocabularies for `ai/exploitability` and `ai/origin`, all-or-nothing consistency rules, required keys per result, and structural evidence backing. The multitool's AI rule pack (AI1003–AI2019) covers most of this when run with `--rule-kind "Sarif;AI"`; this skill rounds out the remaining profile-level checks.

Both layers matter. A structurally valid SARIF file with `ai/exploitability: "unconfirmed"` will pass schema validation but fail downstream consumers that expect the `{demonstrated, poc, theoretical}` vocabulary. Conversely, a file with correct `ai/*` keys but `contextRegion` nested inside `region` (instead of `physicalLocation`) is structurally broken.

**Why validate early:** every SARIF file that reaches a result store, dashboard, or remediation agent with profile violations creates silent data quality debt. Validate at production time — immediately after the producing agent writes the file — to catch issues before they propagate.

## Prerequisites

- **`Sarif.Multitool` ≥ 5.0.0.** Recommended invocation: `dotnet dnx Sarif.Multitool --yes -- validate ...` (zero-install, requires .NET 10+). Fall back to `dotnet tool install --global Sarif.Multitool` if `dotnet dnx` is unavailable.
- The normative profile document at [`docs/ai/generating-sarif.md`](../../docs/ai/generating-sarif.md). This skill references it but embeds the rules for offline execution.
- The full validation rule catalog at [`docs/ValidationRules.md`](../../docs/ValidationRules.md).

## Detection

### Inputs

| Parameter | Required | Description |
|---|---|---|
| `{{SARIF_FILE}}` | Yes | Path to a single `.sarif` file to validate |
| `{{SARIF_DIR}}` | No | Path to a directory; validates all `*.sarif` files found recursively |
| `{{PROFILE}}` | No | `full` (default) or `schema-only`. `schema-only` skips AI profile checks |

If both `{{SARIF_FILE}}` and `{{SARIF_DIR}}` are provided, validate the single file. If neither is provided, ask the operator.

### Step 1 — Schema + SDK AI Validation (Sarif.Multitool)

Run the multitool with `--rule-kind "Sarif;AI"` to activate **both** standard SARIF rules and the SDK's AI profile rule pack in one pass:

```powershell
dotnet dnx Sarif.Multitool --yes -- validate "{{SARIF_FILE}}" --rule-kind "Sarif;AI" --level "Note;Warning;Error"
```

**CRITICAL: Always use `--rule-kind "Sarif;AI"`.** Without `AI` in the list, the AI rule pack (AI1003–AI2019) is opt-in and won't run. Without `Sarif`, you skip the standard rules (SARIF1xxx / SARIF2xxx / JSON1xxx) that catch structural drift the AI rules don't cover.

Capture all output. Each line with `note`, `warning`, or `error` is a finding. The following AI rules are implemented:

| Rule | Name | Level | What it checks |
|---|---|---|---|
| AI1003 | ProvideRequiredRegionProperties | error | Region has `startLine`; should have all four coordinates |
| AI1004 | ProvideVersionControlProvenance | error | VCS provenance with `repositoryUri` + `revisionId` |
| AI1005 | ProvideMessageMarkdown | error | `result.message.markdown` present |
| AI1006 | ProvideAIOrigin | error | `ai/origin` ∈ {generated, annotated, synthesized} |
| AI1010 | ProvideEvidenceBackingUri | error | `sarif:` URIs in evidence resolve within the log |
| AI1011 | RedactedRunMarker | error | `ai/redacted` is `true` or absent, never `false` |
| AI1012 | ProvideRuleSubId | error | Rule descriptors include sub-IDs |
| AI1013 | ProvideNotificationAssociatedRule | error | `notification.associatedRule` resolves to a valid rule |
| AI2003 | ProvideSemanticVersion | warning | `tool.driver.semanticVersion` present |
| AI2005 | ProvideAutomationDetails | warning | `automationDetails.guid` present |
| AI2010 | ProvideResultRank | note | `result.rank` in 0.0–100.0 |
| AI2011 | DoNotPersistFingerprints | note | No fingerprints/partialFingerprints |
| AI2012 | ProvideAIHandoff | note | `ai/handoff` present |
| AI2014 | ProvideExploitability | warning | `ai/exploitability` ∈ {demonstrated, poc, theoretical}; all-or-nothing |
| AI2015 | ProvideAttackerPosition | warning | `ai/attackerPosition` present; all-or-nothing |
| AI2016 | ProvideEvidenceBacking | warning | Demonstrated evidence entries have backing |
| AI2017 | ProvideNotificationDescriptor | warning | Notification descriptors resolve |
| AI2018 | ProvideLearningSignalArtifact | note | Learning signal artifact has attachment role |
| AI2019 | ProvideNotificationTimestamp | note | Notifications include `timeUtc` |

General SARIF rules that commonly fire on AI-generated output:

| Rule | What it catches |
|---|---|
| JSON1008 | Property value out of range (e.g., `startLine=0` — must be ≥1) |
| SARIF1007 | Region missing required `startLine` or `startColumn` |
| SARIF1009 | `threadFlowLocation.index` references missing `threadFlowLocations` array |
| SARIF2002 | Message strings should use markdown |
| SARIF2009 | Non-conventional rule IDs |
| SARIF2012 | Rules missing `helpUri` |

See [`docs/ValidationRules.md`](../../docs/ValidationRules.md) for the full catalog.

**If `dotnet dnx` is not available:** Fall back to the global tool: `dotnet tool install --global Sarif.Multitool` then run:

```powershell
sarif validate "{{SARIF_FILE}}" --rule-kind "Sarif;AI" --level "Note;Warning;Error"
```

If neither dotnet approach works, report:

> Sarif.Multitool is not available. Install .NET 10+ for `dotnet dnx`, or run: `dotnet tool install --global Sarif.Multitool`

and skip to Step 2 (AI profile checks can still run independently). **Note:** Step 2 MUST include a `startLine >= 1` check (rule `JSON1008` equivalent) since schema validation was skipped — a `startLine: 0` that passes AI checks but fails schema validation is a silent defect.

### Step 2 — AI Profile Validation

Parse the SARIF JSON and check each rule below. These correspond to the rules in [`docs/ai/generating-sarif.md § Appendix: Validation Rules`](../../docs/ai/generating-sarif.md#appendix-validation-rules).

If `{{PROFILE}}` is `schema-only`, skip this step entirely.

#### Run-level checks

| Rule | ID | Level | Check |
|---|---|---|---|
| ProvideAIOrigin | AI1006 | error | `run.properties["ai/origin"]` exists and is one of: `generated`, `annotated`, `synthesized` |
| ProvideVersionControlProvenance | AI1004 | error | `run.versionControlProvenance` has ≥1 entry with both `repositoryUri` and `revisionId` |
| ProvideAutomationDetails | AI2005 | warning | `run.automationDetails.guid` is present and non-empty |
| ProvideSemanticVersion | AI2003 | warning | `run.tool.driver.semanticVersion` is present |
| ProvideAIHandoff | AI2012 | note | `run.properties["ai/handoff"]` is present |
| RedactedRunMarker | AI1011 | error | If `ai/redacted` is present, it is `true` (never `false`). If `true`, `ai/fullLogLocation` MAY be present. If `ai/redacted` is absent, `ai/fullLogLocation` MUST be absent |

#### Result-level checks (for each result)

| Rule | ID | Level | Check |
|---|---|---|---|
| ProvideExploitability | AI2014 | warning | `result.properties["ai/exploitability"]` is one of: `demonstrated`, `poc`, `theoretical`. **Any other value (including `unconfirmed`, `unknown`, `none`) is a violation.** |
| ProvideAttackerPosition | AI2015 | warning | `result.properties["ai/attackerPosition"]` is present. Vocabulary is open but recommended values are: `unauthenticated-network`, `adjacent-network`, `authenticated-user`, `local-host`, `configuration`, `physical`, `harness-only`, `unclear` |
| ProvideMessageMarkdown | AI1005 | error | `result.message.markdown` is present and non-empty |
| ProvideResultRank | AI2010 | note | `result.rank` is present and in range 0.0–100.0 |
| ProvideRequiredRegionProperties | AI1003 | error | Every `region` object has `startLine`. `startColumn`, `endLine`, `endColumn` SHOULD be present |
| DoNotPersistFingerprints | AI2011 | note | `result.fingerprints` and `result.partialFingerprints` SHOULD be empty or absent |
| ProvideCodeSnippets | SARIF2010 | warning | `region` objects SHOULD include `snippet` |
| ProvideContextRegion | SARIF2011 | note | `physicalLocation` objects SHOULD include `contextRegion` (as a sibling of `region`, NOT nested inside `region`) |

#### All-or-nothing consistency checks

| Rule | ID | Level | Check |
|---|---|---|---|
| ExploitabilityConsistency | AI2014 | warning | If ANY result has `ai/exploitability`, ALL results MUST have it |
| AttackerPositionConsistency | AI2015 | warning | If ANY result has `ai/attackerPosition`, ALL results MUST have it |

#### Evidence checks

| Rule | ID | Level | Check |
|---|---|---|---|
| ProvideEvidenceBacking | AI2016 | warning | If `ai/evidence[]` entry has `strength: "demonstrated"`, `backing` SHOULD be non-empty. If `ai/exploitability` is `demonstrated`, at least one `ai/evidence[]` entry SHOULD be `demonstrated` with non-empty `backing` |
| ProvideEvidenceBackingUri | AI1010 | error | Every `sarif:` URI in `ai/evidence[].backing` SHALL resolve within the log |

#### Key count check

| Rule | ID | Level | Check |
|---|---|---|---|
| RestrictedAIKeyVocabulary | AI-PROFILE | error | Every `ai/*` key MUST be drawn from this fixed inventory of eight names: `ai/origin`, `ai/exploitability`, `ai/attackerPosition`, `ai/evidence`, `ai/nearestCwe`, `ai/handoff`, `ai/redacted`, `ai/fullLogLocation`. Presence is conditional per category (e.g., `ai/redacted` only when applicable). Any `ai/*` key not in this list is a violation. Report unexpected keys by name |

#### Notification checks

| Rule | ID | Level | Check |
|---|---|---|---|
| NotificationDescriptorResolvable | AI2017 | warning | Every `notification.descriptor` in `toolExecutionNotifications` or `toolConfigurationNotifications` SHOULD resolve to a `reportingDescriptor` in `tool.driver.notifications[]` or an extension's `notifications[]` via `index` or `guid` (§3.52.3). If `descriptor.id` is present, it SHALL match the resolved descriptor's `id` |
| NotificationAssociatedRuleResolvable | AI1013 | error | If `notification.associatedRule` is present, it SHALL resolve to a valid rule in `tool.driver.rules[]` or an extension's `rules[]` via `index` or `guid` |
| LearningSignalArtifactResolvable | AI2018 | note | A notification with `descriptor.id` of `LEARNING-SIGNAL` SHOULD include a `locations[]` entry whose `physicalLocation.artifactLocation.index` resolves to a valid artifact in `run.artifacts[]` with `roles` containing `"attachment"` |
| NotificationTimestampPresent | AI2019 | note | Notifications SHOULD include `timeUtc` to enable execution timeline reconstruction |

### Step 3 — Report

Produce a structured report. Group findings by severity:

```
## SARIF Validation Report

**File:** {{SARIF_FILE}}
**Schema validation:** ✅ PASS | ❌ N errors, M warnings
**AI profile validation:** ✅ PASS | ❌ N errors, M warnings, K notes

### Errors (must fix)
- AI2014: result[0] — ai/exploitability value "unconfirmed" is not in {demonstrated, poc, theoretical}
- ...

### Warnings (should fix)
- ...

### Notes (consider)
- ...

### Summary
| Layer | Errors | Warnings | Notes |
|---|---|---|---|
| Schema (Multitool) | 0 | 0 | — |
| AI Profile | 0 | 0 | 0 |
| **Total** | **0** | **0** | **0** |
```

## Edge Cases

1. **Multiple runs in one log** — Validate each `run` independently. Report run index in each finding.
2. **Redacted vs full log pairs** — If `ai/redacted: true`, expect fewer keys (no `ai/handoff`, `ai/evidence`, tool-namespace keys in redacted copy). Validate the redacted log against the redacted subset of rules. If `ai/fullLogLocation` is present, note it but do not attempt to fetch the full log.
3. **Empty results array** — Valid SARIF. All-or-nothing rules are vacuously satisfied. Note "0 results — nothing to profile-check."
4. **Non-AI SARIF** — If `ai/origin` is absent from all runs, report AI1006 as an error but skip result-level AI checks (the file is not claiming to be AI-produced).
5. **Very large files** — Parse with streaming if >50MB. The multitool handles this; for profile checks, read results incrementally if memory is a concern.

## Known Drift Patterns

AI agents generating SARIF systematically drift from the standard in predictable ways. This catalog captures observed patterns — use it both for validation (catching drift) and for improving the [`emit-sarif-findings`](../emit-sarif-findings/SKILL.md) skill (preventing drift).

| # | Pattern | What the agent does | What the standard requires | Rule(s) |
|---|---------|--------------------|-----------------------------|---------|
| 1 | **contextRegion misplacement** | Nests `contextRegion` as a child of `region` | `contextRegion` is a sibling property on `physicalLocation`, same level as `region` | JSON1005 (schema) |
| 2 | **Invented exploitability values** | Uses `unconfirmed`, `unknown`, `low`, `medium`, `high`, or other freeform strings | Closed vocabulary: `demonstrated`, `poc`, `theoretical` only | AI2014 |
| 3 | **Missing `message.markdown`** | Provides `message.text` only | Both `text` and `markdown` are required; `markdown` carries the structured narrative | AI1005 |
| 4 | **`ai/origin` at result level** | Places `ai/origin` on `result.properties` | `ai/origin` is a **run-level** property only (`run.properties`) | AI1006 |
| 5 | **Partial ai/* key coverage** | Emits 4–6 of the 8 keys, typically missing `ai/evidence`, `ai/redacted`, `ai/fullLogLocation` | All 8 keys must be accounted for across the run (some are conditional, e.g., `ai/redacted` only on redacted logs) | AI-PROFILE |
| 6 | **`rank` as string** | Emits `"rank": "65"` (string) | `rank` is a number (0.0–100.0), not a string | JSON1005 (schema) |
| 7 | **Missing `versionControlProvenance`** | Omits entirely or provides `repositoryUri` without `revisionId` | At least one entry with both `repositoryUri` and `revisionId` | AI1004 |
| 8 | **Invented `ai/*` keys** | Adds `ai/confidence`, `ai/severity`, `ai/model`, etc. | Exactly 8 defined keys under `ai/` namespace; tool-specific data goes under tool namespace | AI-PROFILE |
| 9 | **`kind` omitted** | Relies on default | Explicit `kind: "fail"` for vulnerability findings | Schema best practice |
| 10 | **All-or-nothing violation** | Some results have `ai/exploitability`, others don't | If any result declares it, every result must | AI2014 consistency |
| 11 | **Execution narrative in `ai/handoff`** | Puts dead-end analysis, model selection, and confidence self-assessment in `ai/handoff` | Execution narrative belongs in `toolExecutionNotifications`; `ai/handoff` is for remediation context only | AI2012 (ai/handoff scope) |
| 12 | **Configuration gaps as prose** | Describes data access or permission issues in `ai/handoff` or `message.text` | Configuration gaps belong in `toolConfigurationNotifications` (use `add-notification --config`) | — |
| 13 | **Missing notification descriptors** | Emits notifications without registering descriptors in `tool.driver.notifications[]` | Notification descriptors must be registered for the `descriptor.id` to resolve | AI2017 |
| 14 | **Editorializing notification ids** | Uses `AI/EXEC/DECISION`, `<toolName>/EXEC/...`, or other prefixed ids | Notification descriptor ids name the concern only (e.g. `DECISION`, `DATA-ACCESS-DENIED`); the array (`toolExecutionNotifications` vs `toolConfigurationNotifications`) encodes the kind, `tool.driver.name` encodes the emitter | — |
| 15 | **Zero-based line numbers** | Emits `startLine: 0` or other 0-based coordinates | SARIF line numbers are 1-based (`startLine` ≥ 1). 0 is invalid per JSON schema | JSON1008 |
| 16 | **threadFlowLocation index dangling** | Uses `threadFlow.locations[].index` to reference `runs[].threadFlowLocations` but never populates that top-level array | Either populate `runs[].threadFlowLocations[]` or use inline `location` objects on each threadFlowLocation | SARIF1009 |
| 17 | **Missing `ai/attackerPosition`** | Omits attacker position entirely or on some results | Must appear on every result if present on any (all-or-nothing). Use `"unclear"` if genuinely unknown | AI2015 |
| 18 | **Missing rule `helpUri`** | Emits `rules[]` without `helpUri` | Every rule should include `helpUri` linking to documentation (CWE URL, internal doc, etc.) | SARIF2012 |
| 19 | **Non-conventional rule IDs** | Uses tool-specific prefixes like `ACME-CPP-001` | Rule IDs should follow conventional patterns; CWE-based IDs preferred for interoperability | SARIF2009 |

**How drift happens:** LLMs generate SARIF from training data that includes pre-standard drafts, partial examples, and SARIF from non-AI tools. The `emit-sarif-findings` skill instructs them correctly, but agents hallucinate "reasonable" values that aren't in the vocabulary, or place properties at plausible-but-wrong locations in the object graph. Schema validation catches structural drift (#1, #6); AI profile rules catch semantic drift (#2, #4, #5, #8, #10). Both layers are needed.

**Feedback loop:** When this skill finds a new drift pattern not in this catalog, add it. When the `emit-sarif-findings` skill is updated to prevent a pattern, note that in the catalog but keep the validation rule — prevention at generation time reduces but never eliminates drift.

## Validation

After running this skill:

1. Every error-level finding should be actionable — the producing agent can fix it.
2. The report should be self-contained (no need to cross-reference the normative doc).
3. Zero false positives on well-formed files — validate against [`docs/ai/example.sarif`](../../docs/ai/example.sarif) as a smoke test.

## Escalation

- **Multitool not available** — Neither `dotnet dnx` nor global tool install succeeded. Report the gap, run AI profile checks only (Step 2), note that schema validation was skipped.
- **SARIF file is not valid JSON** — Report parse error and stop; no further validation is possible.
- **Unknown `ai/*` keys found** — Report them by name; they may be legitimate tool-specific extensions using an `ai/` prefix incorrectly, or they may indicate a profile evolution this skill doesn't know about yet. Recommend checking whether [`docs/ai/generating-sarif.md`](../../docs/ai/generating-sarif.md) has been updated.
