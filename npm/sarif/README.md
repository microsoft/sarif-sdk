# @microsoft/sarif

SARIF SDK for Node.js — the base layer that
[`@microsoft/sarif-multitool-ts`](https://www.npmjs.com/package/@microsoft/sarif-multitool-ts)
and third-party SARIF producers build on. Native TypeScript; **no CLR
dependency.**

## What's here

| | Ported from |
|---|---|
| **Open-typed SARIF 2.1.0 object model** — every node carries `properties?` + `[extra: string]: unknown` so caller-supplied data round-trips verbatim | the SARIF schema (subset) |
| `FileRegionsCache`, `NewLineIndex` — line/col ↔ charOffset, snippet + contextRegion extraction, sha-256 | `src/Sarif/FileRegionsCache.cs`, `NewLineIndex.cs` |
| `AIRuleIdConvention` — `CWE-<n>/<sub>` \| `NOVEL-<sub>` grammar | `src/Sarif/Emit/AIRuleIdConvention.cs` |
| `tryDerivePortableRoot`, `isGitHubHostedRun` — repo-URI → GitHub/ADO permalink root | `src/Sarif.Multitool.Library/Emit/VcpPortableRoot.cs` |
| `insertOptionalData`, `tryReconstructAbsoluteUri` — region/hash enrichment visitor | `src/Sarif/Visitors/InsertOptionalDataVisitor.cs` |
| `atomicWrite`, `stripNulls`, `serializeSarifLog` — Newtonsoft-parity I/O | `src/Sarif/Emit/AtomicSarifWriter.cs` |
| `rewriteRelativeLinks`, `extractFrontmatterDescription` — SKILL.md link pinning | `src/Sarif.Multitool.Library/GetSkill/GetSkillCommand.cs` |

This package is a **port**; the C# under `src/Sarif/` is normative. When they
disagree, this package has a bug.

## Usage

```ts
import { FileRegionsCache, AIRuleIdConvention, serializeSarifLog,
         type SarifLog, type Result } from '@microsoft/sarif';
```

For the multitool verbs (`emit-*`, `get-*`, `validate`) and the `sarif` CLI,
install [`@microsoft/sarif-multitool-ts`](https://www.npmjs.com/package/@microsoft/sarif-multitool-ts)
instead — it re-exports everything here.

## License

MIT
