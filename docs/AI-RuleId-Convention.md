# AI rule-id convention

The SARIF SDK's AI-authoring emit chain (`multitool emit-init-run` → JSONL append → `multitool emit-finalize`, plus anything that flows results through `SarifEventReplayer`) enforces a convention on every `result.ruleId` that it accepts. This document explains the two accepted shapes, why they exist, and how the convention is enforced.

## TL;DR

Every result MUST have a `ruleId` that takes one of these two shapes:

| Shape | Form | Example |
|---|---|---|
| CWE sub-id | `CWE-<number>/<sub-id>` | `CWE-89/kql-injection-from-config` |
| NOVEL escape hatch | `NOVEL-<sub-id>` | `NOVEL-prompt-injection-via-system-message` |

If an AI tool emits a result whose `ruleId` is anything else — bare `CWE-89`, missing entirely, `cwe-89/foo` (lowercase), `CWE-89/a--b` (consecutive hyphens), `NOVEL-foo/bar` (slash inside a NOVEL- form), a non-CWE taxonomy like `CVE-…` or `OWASP-…`, etc. — `emit-finalize` exits with a non-zero status and prints an AI-consumable error envelope on stderr listing every offending id at once.

## Why

A SARIF result without a sub-classifier — a bare `ruleId: "CWE-89"` — is a flag that the producer did not actually think about *which kind* of CWE-89 finding it just emitted. AI-produced findings should be specific about the sub-pattern they observed; the taxonomy entry is necessary but rarely sufficient. The `AI1012` validation rule encodes exactly this expectation: any well-shaped AI finding either has a slash-bearing `ruleId` or extends its descriptor id with a slash-separated sub-id.

The two-shape contract serves two distinct producer cases:

1. **The finding maps to a CWE entry.** Use the sub-id form (`CWE-<number>/<sub-id>`). The replayer registers a descriptor with the *base id* (e.g., `CWE-89`) per [SARIF §3.49.3 (descriptor ids are base-only)](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317644), keeps the full hierarchical form on `result.ruleId` per [§3.52.4](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317727), and the CWE taxonomy enricher hydrates the base descriptor with MITRE metadata. The AI gets enriched output for free while staying honest about which sub-pattern it observed. AI findings are always CWE-based; other taxonomies (CVE, OWASP) are not accepted on this path.

2. **The finding doesn't fit any CWE entry.** Use the NOVEL escape hatch (`NOVEL-<sub-id>`). This keeps non-CWE findings emittable without forcing the AI to pretend a CWE applies. NOVEL- is intentionally *flat* — no slash, no hierarchy — to signal "I looked, I did not find a fit, this is new ground". If the AI can connect the finding back to a CWE entry, it MUST use shape #1 instead.

`SarifLogger` consumers do not flow through this convention — it is specific to the AI-authoring emit verb path.

## Grammar

```
result.ruleId  ::= cweSubId | novelEscape

cweSubId       ::= base "/" subId
base           ::= "CWE-" [0-9]+                            ; e.g., CWE-89, CWE-327
subId          ::= [a-z0-9]+ ("-" [a-z0-9]+)*               ; lowercase-alphanumeric kebab,
                                                             ; single hyphens, no leading/
                                                             ; trailing/consecutive hyphen

novelEscape    ::= "NOVEL-" novelSubId
novelSubId     ::= [a-z0-9]+ ("-" [a-z0-9]+)*               ; same kebab grammar as subId
```

AI findings are always CWE-based; CVE, OWASP, and other taxonomies are not accepted. The NOVEL- form is exclusive and the prefixes are disjoint: a `ruleId` starting with `NOVEL-` is validated only against the NOVEL grammar, and a `CWE-…` id is validated only against the CWE grammar. `NOVEL-foo/bar` is rejected — the NOVEL form is flat (no slash), and `NOVEL-foo` is not a valid CWE base either.

## Accepted examples

| Input | Why it passes |
|---|---|
| `CWE-89/kql-injection-from-config` | Sub-id form; base `CWE-89` enriched, sub-id descriptive |
| `CWE-79/dom-xss-via-sanitizer-bypass` | Sub-id form |
| `CWE-327/md5-usage` | Sub-id may contain digits |
| `CWE-89/2nd-order-injection` | A token may start with a digit |
| `NOVEL-prompt-injection-via-system-message` | NOVEL escape hatch |
| `NOVEL-x509-bypass` | NOVEL sub-id may contain digits |
| `NOVEL-look-ma-i-hallucinated-outside-of-mitre` | NOVEL escape hatch |

## Rejected examples

| Input | Why it fails |
|---|---|
| *(missing)* | Empty / null ruleId is rejected |
| `CWE-89` | Bare taxonomy id — no sub-id |
| `cwe-89/foo` | Base must be uppercase `CWE-` |
| `CWE-x/foo` | Base number must be numeric |
| `CWE-89/` | Sub-id is empty |
| `CWE-89/Foo` | Sub-id must be lowercase |
| `CWE-89/a--b` | Consecutive hyphens not allowed |
| `CWE-89/a-` | Trailing hyphen not allowed |
| `CWE-89/-a` | Leading hyphen not allowed |
| `CWE-89/a/b` | Sub-id contains a slash |
| `CWE-89/a b` | Sub-id contains whitespace |
| `CVE-2021-12345/exploit-via-file-upload` | Not a CWE — AI rules are CWE-only |
| `OWASP-A01-2021/broken-access-control` | Not a CWE — AI rules are CWE-only |
| `NOVEL` | Missing `-<sub-id>` |
| `NOVEL-` | Empty sub-id after `NOVEL-` |
| `NOVEL-foo/bar` | NOVEL- form is flat — no slash allowed |
| `NOVEL-a--b` | Consecutive hyphens not allowed |
| `NOVEL-Foo` | Sub-id must be lowercase |
| `novel-foo` | NOVEL- prefix must be uppercase |
| `MY-CUSTOM-RULE` | No `/` and not NOVEL-prefixed |

## Enforcement

Today the convention is enforced at `emit-finalize` time by [`AIRuleIdConvention.ThrowIfAnyUnacceptable`](../src/Sarif/Emit/AIRuleIdConvention.cs), invoked from `SarifEventReplayer.RegisterDescriptorsFromResults`. The validator collects every offender in a single pass and throws `AIRuleIdConventionException` once with the full list, so an AI orchestrator can correct all violations in one round-trip rather than discovering them one at a time.

When `emit-finalize` catches the exception it writes the exception message to stderr verbatim — the message is already AI-shaped:

```
error AI-RULEID-001: 2 results did not conform to the AI ruleId convention:
  - 'CWE-89'
  - 'my-custom-rule'

Every AI-emitted result.ruleId MUST take one of two shapes:
  1. Taxonomy sub-id  CWE-<number>/<sub-id>
     e.g., 'CWE-89/kql-injection-from-config'
     Use this whenever the finding maps to a CWE entry.
     The base id (CWE-89) drives descriptor enrichment; the sub-id
     is your AI-chosen sub-classifier and keeps AI1012 silent.
  2. NOVEL escape hatch  NOVEL-<sub-id>
     e.g., 'NOVEL-prompt-injection-via-system-message'
     Use this ONLY when no CWE entry fits. The NOVEL- form is
     flat (no slash). If the finding maps to a CWE entry,
     use shape #1 instead.

Retry the emit after correcting every offender above.
See docs/AI-RuleId-Convention.md for full guidance.
```

A future per-result emit verb will reject the same way at append time, giving AI orchestrators per-call feedback. The validator and exception type stay the same.
