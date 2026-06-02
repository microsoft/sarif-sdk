# Tool-referenced CWE coverage — methodology and slim briefs

Two slim CWE briefs (`cwe-catalog-brief.shared.md` and `cwe-catalog-brief.observed.md`) ship under `docs/ai/` alongside this methodology document. See **How to use this** below for the tier-escalation pattern.

> **Status.** Reference material for AI-assisted SARIF producers. Not part
> of the Sarif.Multitool product surface; lives under `docs/ai/` for
> discoverability and reproducibility.

## Why this exists

The full MITRE CWE catalog (v4.20 = **969 entries**) is too large to inject
into LLM context cost-effectively, and most of it describes CWEs no
production scanner attempts to detect — hardware register zeroization,
mainframe COBOL idioms, deprecated entries, research-stage 1000-series
additions. AI agents that ingest the catalog to perform best-effort CWE
classification (e.g. for findings produced by tools without a built-in CWE
taxonomy, or for novel findings outside any tool's catalog) waste budget
on entries that will never match a realistic finding.

We need an **empirically-grounded** filter — the subset of CWE identifiers
that the security-tooling industry has visibly invested in detection
logic for. If multiple production scanners ship a rule tagged CWE-N,
CWE-N is a credible classification target. This is a practical lower
bound, not an exhaustive list of every CWE a human auditor might
reasonably assign.

A second consideration: **MITRE maturity status is not detection
maturity.** Many CWEs that production scanners actively detect are still
marked `Incomplete` by MITRE — CWE-918 (SSRF), CWE-862 (Missing
Authorization), CWE-208 (Observable Timing Discrepancy), and others. We
deliberately do NOT filter on MITRE status. The signal we trust is "did a
real scanner ship a rule for this?"

## What we did

### Tools surveyed (10 total)

| Tier | Tool | Status | Source | Method |
|---|---|---|---|---|
| OSS | github/codeql | ✅ complete | clone of `github/codeql` | `external/cwe/cwe-NNN` refs in `.ql`/`.qhelp` |
| OSS | semgrep/semgrep-rules | ✅ complete | clone of `semgrep/semgrep-rules` | `cwe:` YAML metadata in rules |
| OSS | PyCQA/bandit | ✅ complete | clone of `PyCQA/bandit` | `class Cwe:` enum + plugin docstring usage |
| OSS | securego/gosec | ✅ complete | clone of `securego/gosec` | `CWE: "CWE-N"` struct literals |
| Proprietary | Snyk Code | ✅ complete | public docs (19 per-language pages) | direct fetch + table parse |
| Proprietary | Veracode SAST | ✅ complete | public CWE coverage doc | direct fetch + table parse |
| Proprietary | SonarQube | ⚠️ partial | public Rules API (next.sonarqube.com) | rule counts confirmed; CWE IDs gated |
| Proprietary | Fortify SCA | ❌ blocked | vulncat.fortify.com | category listing parsed (8 kingdoms, 1,686 weaknesses); CWE IDs on gated detail pages |
| Proprietary | Checkmarx | ❌ blocked | documentation.checkmarx.com | all docs require login |
| Proprietary | Coverity / Black Duck | ❌ blocked | documentation.blackduck.com | Zoomin portal fully gated |

### Reproducing the OSS mining

Each OSS scanner exposes its CWE tags differently. What to look for:

| Tool | Where the CWE tags live |
|---|---|
| `github/codeql` | `external/cwe/cwe-NNN` references (zero-padded) inside `.ql` and `.qhelp` files |
| `semgrep/semgrep-rules` | `cwe:` YAML metadata block in each rule file, listing `"CWE-N: name"` entries |
| `PyCQA/bandit` | `class Cwe:` enum in `bandit/core/issue.py` defining named constants, plus per-plugin docstring references |
| `securego/gosec` | `CWE: "CWE-N"` struct literals in `.go` rule source |

For each tool: scan all matching files with a pattern that captures the
numeric CWE id, deduplicate, attach `tool: <name>` provenance, union
across tools. The shape used in `per-tool-coverage.json` is:

```jsonc
{
  "cwe": 78,
  "tools": ["bandit", "codeql", "gosec", "semgrep", "snyk-code", "veracode-sast"],
  "tool_count": 6
}
```

### Reproducing the proprietary research

- **Snyk Code**: 19 per-language Markdown pages under
  `docs.snyk.io/scan-fix-and-prevent/scan-with-snyk/snyk-code/snyk-code-security-rules/`.
  Each has a `## Rules` table with a CWE column. Direct fetch + table
  parse; no authentication required.
- **Veracode SAST**: single page `docs.veracode.com/r/c_review_cwe`
  organizes Veracode's internal flaw categories with CWE coverage
  per category. Direct fetch + table parse.
- **SonarQube**: rule count is verifiable via the anonymous Rules API
  (`next.sonarqube.com/sonarqube/api/rules/search?types=VULNERABILITY`)
  but `securityStandards` (the CWE-mapping field) is withheld from
  unauthenticated responses — count only.
- **Fortify / Checkmarx / Coverity**: documentation portals require
  login or render CWE IDs only on gated detail pages; treat as
  unverifiable from public sources today.

### Refresh procedure

1. Re-extract per-tool CWE identifiers from fresh OSS clones using the
   patterns above (CWE coverage tends to grow ~5–20 entries per major
   tool release).
2. Re-fetch the two public proprietary pages (Snyk Code, Veracode);
   record verified CWE IDs per tool.
3. Recompute the union as `union(codeql, semgrep, bandit, gosec, snyk-code, veracode-sast)`,
   preserving per-CWE provenance in `per-tool-coverage.json`.
4. Intersect the union with the current MITRE brief
   (`src/Sarif/Taxonomies/CweTaxonomy.brief.md`) to regenerate both
   slim briefs (`shared.md` filters `tool_count >= 4`; `observed.md`
   filters `tool_count >= 1`).
5. Note any new CWE IDs present in scanner output but absent from MITRE
   (CWE-version skew — see Caveats below).

## What we found

### Headline numbers

| Metric | Value |
|---|---|
| Tools fully inventoried | 6 (4 OSS + 2 proprietary verified) |
| Tools partial / blocked | 4 (1 partial + 3 blocked) |
| OSS-only union | 347 unique CWEs |
| Verified proprietary union (Snyk + Veracode) | 216 unique CWEs |
| **Master union (6 tools)** | **396 unique CWEs** |
| MITRE catalog (4.20) total | 969 entries |
| Slim brief — `cwe-catalog-brief.shared.md` (≥4 tools, after MITRE 4.20 intersection) | 55 of 969 (~5.7%) |
| Slim brief — `cwe-catalog-brief.observed.md` (≥1 tool, after MITRE 4.20 intersection) | 384 of 969 (~39.6%) |
| CWE IDs in union but absent from MITRE 4.20 | 12 (deprecated; see below) |

### Tool-count distribution

How many tools reference each CWE in the union:

| # tools | # CWEs |
|---|---|
| 6 | 6 |
| 5 | 13 |
| 4 | 37 |
| 3 | 46 |
| 2 | 95 |
| 1 | 199 |

The 6-tool tier ("universal SAST set") and the long 1-tool tail tell us
two useful things:

1. The **4+ tools tier — 56 CWEs** (55 after MITRE 4.20 intersection;
   CWE-16 is deprecated) — is the **`shared` subset**: identifiers
   that multiple independent scanner authors agreed warrant a
   dedicated rule (injection, deserialization, path traversal,
   hardcoded credentials, weak crypto, etc.). These are the
   highest-confidence classification targets and form the default
   field guide for AI classification contexts where tokens are
   tight.
2. The full **≥1-tool union — 384 of 969** — is the **`observed`
   subset**: every CWE the surveyed industry has tagged at least
   once. Use this as the escalation reference when a finding's
   weakness doesn't fit any `shared` entry.
3. The long 1-tool tail is largely CodeQL-specific quality rules
   (CodeQL alone contributes 292 CWEs) — useful breadth, but lower
   priority for subset injection if budget is tight.

**Why the ≥4-of-6 threshold for `shared`.** Four independent tool
teams converging on the same CWE identifier is the cleanest empirical
signal we have that the CWE is well-defined enough to operationalize.
Below that threshold, you see meaningful per-tool quirks — pet
abstractions, terminology drift, naming inconsistencies. We picked the
threshold *after* looking at the distribution; it cleanly separates a
plateau of multi-tool agreement from the long single-tool tail. It's
not magic and you can re-derive a different tier from
`per-tool-coverage.json` with one filter if your needs differ.

### Per-tool contribution

| Tool | CWEs contributed |
|---|---|
| codeql | 292 |
| veracode-sast | 172 |
| semgrep | 169 |
| snyk-code | 109 |
| bandit | 23 |
| gosec | 11 |

### Productized-skill sanity check (operational validation)

The 6-tool union was cross-checked against an internal AI-SARIF-producer
project that ships 7 dedicated CWE investigation skills. All 7 are
covered by ≥1 OSS tool, confirming the union is a superset of at least
one production AI-assisted scanner's operational priorities:

| Skill CWE | Tools |
|---|---|
| CWE-78 (Command Injection) | bandit, codeql, gosec, semgrep, snyk-code, veracode-sast |
| CWE-208 (Observable Timing Discrepancy) | codeql, snyk-code |
| CWE-209 (Error Disclosure) | codeql, semgrep, snyk-code, veracode-sast |
| CWE-494 (Download Integrity) | bandit, codeql |
| CWE-502 (Deserialization) | bandit, codeql, gosec, semgrep, snyk-code, veracode-sast |
| CWE-862 (Missing Authorization) | codeql, semgrep, snyk-code, veracode-sast |
| CWE-918 (SSRF) | codeql, gosec, semgrep, snyk-code, veracode-sast |

## How to use this

Two slim briefs ship alongside this methodology, differing only in
threshold:

| Brief | Threshold | Rows | When to use |
|---|---|---|---|
| `cwe-catalog-brief.shared.md` | ≥4 of 6 tools | 55 | **Default** field guide for AI classification contexts. High-confidence "industry-converged" subset. ~16 KB. |
| `cwe-catalog-brief.observed.md` | ≥1 of 6 tools | 384 | **Escalation** reference. Broader empirical "what scanners tag" set, for findings whose weakness doesn't fit any `shared` entry. ~93 KB. |
| `CweTaxonomy.brief.md` (in `src/Sarif/Taxonomies/`) | n/a | 969 | **Last-resort** fallback. Full MITRE 4.20. |

`per-tool-coverage.json` carries machine-readable provenance for the
full 396-CWE union (which scanners tag which CWE). Programmatic
consumers can derive any other threshold (≥2, ≥3, ≥5, single-tool
filter, etc.) from this file in one filter.

Recommended consumption pattern (tier-escalation):

1. **Default-inject `shared.md`** as the CWE field guide when an AI
   agent is classifying findings (e.g. assigning `result.taxa[]`
   entries that reference the MITRE CWE taxonomy). ~16 KB fits
   comfortably in any modern context.
2. **Add a self-awareness gate.** Instruct the classifier: "If no
   CWE in this list materially fits the observed weakness — not
   'kinda close', but actually fits as a name an auditor would use —
   escalate to `observed.md`." Without this prompt, LLMs tend to
   over-confidently pick the nearest-sounding entry.
3. **Escalate to `observed.md`** (≥1 tool, broader 384) when (2)
   triggers. If still no fit, escalate again to the full
   `CweTaxonomy.brief.md` (969 entries).
4. **Use `per-tool-coverage.json` as a confidence/priority signal**,
   not as severity. A CWE referenced by 5–6 tools is well-understood
   across the industry; a CWE referenced by 1 tool is more
   idiosyncratic.
5. **Prefer specific Base or Variant CWEs over Pillar or Class CWEs**
   when the finding evidence supports the specificity. Pillar CWEs
   (e.g. CWE-664 *Improper Control of a Resource Through its
   Lifetime*) are documentation organizing categories — they classify
   the *kind* of weakness but typically don't help a downstream
   consumer triage a real finding.
6. **Do not invent CWE IDs that aren't in any of the three tiers.**
   See the "outside the brief" subsection below.

### When a finding's CWE is outside the briefs

The briefs are strict intersections with MITRE 4.20. Some finding paths
land outside even the broader `observed` set:

- **The CWE is valid in MITRE but not in the brief** (no surveyed tool
  has shipped a rule for it yet). Use it anyway — the brief is a
  signal, not a whitelist. Consider this a candidate for a future
  refresh cycle.
- **The CWE is deprecated** (see CWE-version skew below). Prefer
  MITRE's replacement CWE when known; preserve the original scanner-
  provided CWE as provenance in `result.properties` if it's part of
  the source-of-truth chain.
- **The CWE was added after MITRE 4.20**. Use it; flag for refresh.
- **No CWE applies**. Don't force one. Emit the finding without a CWE
  taxon (or with a `ruleId` that doesn't claim CWE mapping).

## Caveats

### CWE-version skew (the 12 missing IDs)

Scanners ship rules tagged with CWE identifiers that have since been
deprecated or merged upstream. Twelve such IDs appear in the union but
are absent from MITRE 4.20's published catalog:

```
16, 251, 264, 265, 310, 320, 398, 399, 485, 557, 730, 840
```

Most are deprecated "Pillar Weaknesses" (e.g. CWE-310 *Cryptographic
Issues* → replaced by more specific CWE-326 / 327 / 330; CWE-264
*Permissions, Privileges, and Access Controls* → replaced by CWE-269 /
732 / 862). Tool authors haven't re-tagged historical rules. These IDs
won't render in either slim brief (both are strict intersections with
MITRE 4.20), but consumers should know they exist in scanner output
and treat them as soft synonyms for their replacements.

### Proprietary mining has gaps

- **SonarQube**: rule count is verifiable via the unauthenticated Rules
  API (1,293 VULNERABILITY-type rules; 1,727 rules tagged with `cwe`),
  but the `securityStandards` field that lists per-rule CWE IDs is
  withheld from anonymous responses. The IDs exist; they're just not
  publicly fetchable without a login.
- **Fortify SCA**: Vulncat.fortify.com renders kingdom/category listing
  pages publicly (8 kingdoms enumerated, ~1,686 weaknesses total) but
  detail pages — where CWE IDs live — return HTTP 400 to anonymous
  requests. Web Archive returned no useful cached copies. Vendor claims
  800+ CWE coverage; not externally verifiable.
- **Checkmarx**: all product documentation requires login. Marketing
  materials claim 800+ CWE coverage; no public per-rule enumeration
  exists. CX-Flow and KICS (their open-source IaC scanner) are separate
  products.
- **Coverity / Black Duck**: documentation portal (`documentation.
  blackduck.com`, Zoomin) is fully gated. Coverity's checker taxonomy
  (`TAINTED_DATA`, `INTEGER_OVERFLOW`, ...) differs from CWE; CWE-to-
  checker mappings exist in product but are visible only to licensees.

Adding any of these four would likely **broaden** the union (especially
Fortify and Checkmarx, both of which claim significantly more coverage
than the verified set). Treat the 384-CWE `observed` brief as a
**lower bound** on the industry-detected CWE space, and the 55-CWE
`shared` brief as the highest-confidence subset within that lower bound.

### Author-assigned CWE mappings

CWE identifiers in tool rule metadata are assigned by tool authors, not
externally audited. Veracode's own docs note that they prefer the most
general applicable CWE (e.g. CWE-80 instead of more specific XSS
children). Different tools tag the same vulnerability class differently;
the union is the union of *taggings*, not of *vulnerability semantics*.

### What this is NOT

- Not a ranking of CWE severity or prevalence.
- Not a guarantee of detection capability (a tool tagging a rule with
  CWE-N proves intent, not effectiveness).
- Not a substitute for a real rule-level taxonomy when one is available
  (e.g. CodeQL's QL pack metadata, Semgrep's rule registry).

## Why not just use MITRE's `Top 25` or `OWASP Top 10`?

Both are excellent guidance lists but neither serves the use case:

- **CWE Top 25** is ~25 entries. Far too narrow; would exclude e.g.
  CWE-209 (error disclosure), CWE-862 (missing authorization), CWE-918
  (SSRF) in some years.
- **OWASP Top 10** is a category list, not a CWE list. Each category maps
  to dozens of CWEs.

The empirical "what do tools actually detect" filter sits between these
authoritative-but-narrow lists and the full 969-entry catalog. It's the
right granularity for AI classification: broad enough to cover real
findings, focused enough to fit in a context window.

---

*Methodology last edited: 2026-05-31. Data snapshot generated:
2026-05-31. To regenerate, follow the refresh procedure above. Scripts
intentionally not vendored — better to author fresh tooling against the
tool layouts in effect at refresh time than to carry yesterday's code.*
