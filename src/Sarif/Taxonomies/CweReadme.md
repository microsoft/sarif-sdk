# CWE taxonomy (embedded SDK resources)

This folder ships the [MITRE CWE](https://cwe.mitre.org/) catalog as embedded
SDK resources so SARIF producers and consumers can enrich CWE-shaped rule
descriptors without making a network call to MITRE.

## What's here

| File | Size | Format | Purpose |
|---|---:|---|---|
| `CweTaxonomy.sarif` | 2.81 MB | SARIF 2.1.0 taxonomy log | Full fidelity: `name` (Pascal-case identifier per SARIF §3.49.7), `shortDescription`, `fullDescription`, `helpUri`, verbatim MITRE `help.markdown` (Description + Extended Description + Common Consequences + Potential Mitigations), plus `cwe/title` (the original MITRE long name), `cwe/status`, `cwe/abstraction`, and `cwe/parent` (canonical View 1000 ChildOf) as properties on every taxon. |
| `CweTaxonomy.brief.md` | 240 KB | Markdown table | One row per entry: `ID │ Name │ Abstraction │ Status │ Parent │ Description`. `Name` is the Pascal-case identifier; the long MITRE title lives on the SARIF taxon as `cwe/title`. Sorted numerically. ~60K tokens at the default loadout — sized for AI prompt-context injection. |

Both files contain **every** entry in the upstream MITRE catalog (969 entries
in `cwec_v4.20`) regardless of status. Filtering by status is a **read-time**
concern via [`CweTaxonomy.Load(CweStatus)`](CweTaxonomy.cs) /
`CweTaxonomy.LoadBrief(CweStatus)`, not a file-load concern.

## MITRE 4.20 status distribution

| Status | Count | Notes |
|---|---:|---|
| Stable | 26 | The 12 classic software CWEs (XSS, SQLi, path traversal, RNG, CSRF, UAF, integer overflow, null deref, buffer ops, OS command, untrusted search path, input validation) plus 14 hardware CWEs from CWE 4.0's 2020 expansion. |
| Draft | 432 | XXE, deserialization, hardcoded credentials, broken crypto, out-of-bounds write, command injection variants, etc. |
| Incomplete | 486 | **SSRF (CWE-918)** — an OWASP Top 10 entry since 2021 — sits here. Also: access-control granularity, timing side channels, plaintext password storage, least-privilege violations. |
| Deprecated | 25 | MITRE recommends migration. |
| Obsolete | 0 | Empty bucket today; the enum reserves the slot for forward compatibility. |

## Why the default loadout is `Stable | Draft | Incomplete`

The [`CweTaxonomy.DefaultStatuses`](CweTaxonomy.cs) constant excludes only
**Deprecated** (25 entries; 2.6% of the catalog). This shape was chosen by
measurement, not by assumption.

We scanned two large open-source rulesets and counted how often each CWE is
cited:

| Source | Files scanned | Distinct CWEs cited | Stable | Draft | **Incomplete** | Deprecated | Unknown¹ |
|---|---:|---:|---:|---:|---:|---:|---:|
| [`github/codeql`](https://github.com/github/codeql) (commit Oct 2025) | 13,143 `.ql` / `.qll` | 296 | 11 (3.7%) | 170 (57.4%) | **106 (35.8%)** | 1 | 8 |
| [`semgrep/semgrep-rules`](https://github.com/semgrep/semgrep-rules) (commit Oct 2025) | 2,183 `.yaml` / `.yml` | 169 | 11 (6.5%) | 83 (49.1%) | **70 (41.4%)** | 0 | 5 |
| **Combined distinct** | — | **349** | 12 (3.4%) | 190 (54.4%) | **136 (39.0%)** | 1 (0.3%) | 10 |

¹ "Unknown" = CWE IDs cited by these rulesets that are no longer in MITRE 4.20
(e.g. CWE-16, 264, 310, 320, 399 — real CWEs that MITRE deprecated *and*
removed from the catalog).

**Headline: 39% of CWEs cited by real-world scanners are MITRE-Incomplete.**
A default of `Stable | Draft` would silently exclude all 136 of these from
enrichment.

Proof points beyond SSRF — Semgrep's top-cited CWEs by file count:

| Rank | CWE | Name | Status | Files |
|---:|---|---|---|---:|
| 1 | CWE-798 | UseOfHardCodedCredentials | Draft | 265 |
| 2 | CWE-79 | CrossSiteScripting | Stable | 128 |
| 3 | **CWE-1220** | InsufficientGranularityOfAccessControl | **Incomplete** | **108** |
| 4 | CWE-89 | SQLInjection | Stable | 79 |
| 5 | CWE-319 | CleartextTransmissionOfSensitiveInformation | Draft | 77 |

The third-most-cited CWE across all of Semgrep is Incomplete. This is not an
edge case.

**Excluding Deprecated by default is also intentional.** Across 349 distinct
CWEs cited by these two rulesets, exactly **one** Deprecated CWE appears
(CWE-247, once, in CodeQL). Real consumers have already migrated off
deprecated CWEs, so default-skip-Deprecated doesn't hurt anyone — it just
catches stale rule descriptors that haven't been updated. The enricher leaves
those descriptors' metadata empty so the producer notices and migrates.

Callers that want a complete snapshot pass `CweStatus.All`.

## Sample

[`CweGenerateSample.ps1`](CweGenerateSample.ps1) emits the checked-in
[`CweGhasSample.sarif`](CweGhasSample.sarif) fixture (and, with `-GHAzDO`,
[`CweGHAzDOSample.sarif`](CweGHAzDOSample.sarif)), a fully enriched SARIF log that
exercises the emit chain end-to-end via the multitool `emit-run`,
`emit-results`, `emit-invocations`, and `emit-finalize` verbs. The sample
appends seven Result events (covering the Stable, Draft, Incomplete, and
NOVEL- "no-CWE-fits" cases) plus one Invocation carrying a notification
inline, runs finalize with the
enrichment + validate gates on, and verifies that every `CWE-*` ruleId came
back hydrated with `name`, `shortDescription`, `fullDescription`, `helpUri`,
and the MITRE markdown.

Convention for this repo: every taxonomy that ships alongside the SDK includes
a `<Taxonomy>GenerateSample.ps1` next to its data and a checked-in
`<Taxonomy>Sample.sarif` artifact that the script (re)generates in place, so
reviewers can `git diff` it like any other source artifact and CI asserts it
stays byte-identical to what the emit chain produces.

```pwsh
pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1
```

## Regeneration

```bash
python3 scripts/generate_cwe_taxonomy.py
```

Requires Python 3 (stdlib only — no pip packages). Downloads
`cwec_latest.xml.zip` from MITRE, parses every weakness, sorts by numeric
ID, and writes both artifacts in place under `src/Sarif/Taxonomies/` with
UTF-8 (no BOM) and LF line endings. Re-run when MITRE publishes a new CWE
version and update the version-stamp in this README.

For offline or testing scenarios, point the script at a pre-extracted XML:

```bash
python3 scripts/generate_cwe_taxonomy.py --xml path/to/cwec_v4.20.xml
```

## Licensing

MITRE provides the CWE under a permissive use license that requires
attribution and prohibits modification of CWE itself. We redistribute the
data verbatim with attribution preserved on
`taxonomies[0].organization = "MITRE"` and
`taxonomies[0].informationUri = "https://cwe.mitre.org/"`.

See <https://cwe.mitre.org/about/termsofuse.html>.
