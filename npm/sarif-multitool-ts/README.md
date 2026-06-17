# @microsoft/sarif-multitool-ts

Native-TypeScript port of the SARIF Multitool **emit verbs** (`emit-run`,
`emit-results`, `emit-invocations`, `emit-rule-descriptors`,
`emit-notification-descriptors`, `emit-finalize`).

In-process library + arg-compatible CLI. **No CLR dependency.**

## Why

`@microsoft/sarif-multitool` is a `spawnSync` shim over a ~70 MB single-file
.NET executable. Node consumers that drive the emit chain (open log → append
findings → finalize) pay CLR cold-start and working-set on every call. This
package reimplements exactly that closure in TypeScript so the emit chain
runs in-process.

## Library

```js
import { emitRun, emitResults, emitInvocations, emitFinalize } from '@microsoft/sarif-multitool-ts';

await emitRun({
  output: 'scan.sarif',
  run: {
    tool: { driver: { name: 'my-ai-scanner', version: '1.0.0' } },
    originalUriBaseIds: { SRCROOT: { uri: 'file:///path/to/checkout/' } },
    versionControlProvenance: [{
      repositoryUri: 'https://github.com/owner/repo',
      revisionId: '<full-sha>',
      mappedTo: { uriBaseId: 'SRCROOT' },
    }],
  },
});

const r = await emitResults({
  output: 'scan.sarif',
  results: [{ ruleId: 'CWE-79/dom-xss', message: { text: '...' }, locations: [...] }],
});
// r = { appended: 1, rejected: [] }

await emitFinalize({ output: 'scan.sarif' });
```

Every `add-*` function is **polymorphic** (single object or array) and
**atomic** (all-or-none; a rejected element appends nothing). The structured
`{ appended, rejected: [{ index, errorCode, message }] }` return matches the
.NET tool's stdout JSON, so an AI orchestrator can correct offending elements
and retry idempotently.

## CLI

```sh
npx @microsoft/sarif-multitool-ts emit-run scan.sarif --input run.json
echo '{"ruleId":"CWE-79/x","message":{"text":"..."}}' | npx @microsoft/sarif-multitool-ts emit-results scan.sarif
npx @microsoft/sarif-multitool-ts emit-finalize scan.sarif
```

Flags are arg-for-arg compatible with `sarif <verb>` for the six emit verbs.

## Pass-through guarantee

The TypeScript object model is intentionally **sparse** — only the fields the
emit verbs read or mutate are typed. Every node is open-typed: any
`properties` bag and any unrecognized top-level key you supply on a Run,
Result, Invocation, or ReportingDescriptor round-trips **verbatim** through
the emit chain into the finalized SARIF. The sparse types are not a schema
and never drop data. See `features/passthrough-property-bags.feature` for
the contract.

## Validation

`emit-finalize --validate` is **deferred** in this package. The emit verbs
perform receipt-time checks only — AI ruleId grammar (`AI1012`), required
fields, batch atomicity, descriptor-id uniqueness — not the full SARIF/AI
analyzer rule set.

**If you adopt this package, keep a .NET validation step in your test/CI
environment** until the AI-content JSON Schema ships:

```sh
dotnet tool install -g Sarif.Multitool   # once
sarif validate --rule-kind "Sarif;AI" scan.sarif
```

This is a **test-time-only** dependency — no CLR in your production path —
and is the backstop that catches anything the sparse port lets through.

## Known gaps vs. the .NET tool (v1)

| Gap | Effect | Workaround |
|---|---|---|
| `--validate` | Stubbed (see above) | `.NET sarif validate` in CI |
| `RollingHashPartialFingerprints` | GitHub-only `primaryLocationLineHash` not stamped | Upload via the `upload-sarif` Action (it backfills), or accept dedup on location alone |
| `--embed-text-files` | Source bytes not inlined in artifacts | Run `.NET emit-finalize` with the flag if you need self-contained fixtures |
| `run.columnKind` default | .NET emits its enum default `utf16CodeUnits` even when unset; TS preserves absence | Cosmetic; set it explicitly in your run header if a consumer requires it |

## Behavioral parity

`npm run test:conformance` runs each fixture through both this library and
`dotnet run --project src/Sarif.Multitool` and deep-diffs the output SARIF
(after normalizing timestamps and absolute temp paths). Any diff fails. Wire
this into your branch CI so a .NET-side change that breaks parity surfaces
immediately.

## Source of truth

This package is a **port**, not a fork. The C# under
`src/Sarif.Multitool.Library/Emit/` and `src/Sarif/Emit/` is normative; every
TS module's header comment names the file(s) it was ported from. When they
disagree, the C# wins and this package has a bug.

## License

MIT — see repository root.
