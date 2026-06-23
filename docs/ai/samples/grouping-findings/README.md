# GroupingSample.sarif

A deterministic, generated worked example of the **two-tier finding-grouping
convention** — how a *synthesized* finding (one assembled from other findings)
links to the raw findings it consolidates, and how each raw finding links back.
See [`../../grouping-findings.md`](../../grouping-findings.md) for the full design.

## What it shows

An unauthenticated SSRF assembled from two raw findings:

| | `ai/origin` | Results |
|---|---|---|
| `runs[0]` | `generated` | `[0]` CWE-306 missing-auth-check, `[1]` CWE-918 unvalidated-fetch |
| `runs[1]` | `synthesized` | `[0]` CWE-918 ssrf-via-unvalidated-fetch (the cluster) |

## How the cross-links are encoded

The cluster cross-link lives **entirely in `relatedLocations[]`**;
`result.locations[]` stays the detection's own physical sites and never
references a clustering parent.

Each cross-link is one `relatedLocation` that carries:

- a **`sarif:` URI** (`sarif:/runs/<r>/results/<i>`) in
  `physicalLocation.artifactLocation.uri` — the **address** of the linked result;
- a **`locationRelationship`** whose `kinds` is `includes` (synthesized → member)
  or `isIncludedBy` (member → cluster) — the **type** of the edge.

Because the kind and the `sarif:` pointer ride on the same related-location, the
relationship's required `target` resolves to that location's **own `id`** (a
self-reference). This keeps the link out of `result.locations[]` while still
using SARIF's well-known relationship kinds.

```jsonc
// runs[0].results[0] (a raw finding) — points back at its cluster
"relatedLocations": [
  {
    "id": 1,
    "physicalLocation": { "artifactLocation": { "uri": "sarif:/runs/1/results/0" } },
    "message": { "text": "Grouping parent: the SSRF cluster that includes this finding." },
    "relationships": [ { "target": 1, "kinds": [ "isIncludedBy" ] } ]
  }
]
```

The links are reciprocal by construction (**AI1015**), flow synthesized →
generated (**AI2020**), and every `sarif:` pointer resolves against the assembled
log (**SARIF1013**).

## Regenerating

The sample is produced by the official multitool emit chain, never hand-authored:

```powershell
# build the multitool first, e.g. dotnet build src\Sarif.Multitool\Sarif.Multitool.csproj -c Release
pwsh docs\ai\samples\grouping-findings\GroupingGenerateSample.ps1
```

`GroupingGeneratedSampleTests.cs` gates byte-identical regeneration, so any change
to the convention must flow through `GroupingGenerateSample.ps1` and this file
must be regenerated — a hand-edit will fail the test.
