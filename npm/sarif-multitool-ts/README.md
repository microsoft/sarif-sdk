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

`emit-finalize --validate` checks the finalized SARIF against
`ai-sarif-log.schema.json` — the AI emit profile overlay on the canonical
SARIF 2.1.0 document schema. Both schemas are bundled, so validation runs
fully offline. On a conforming log the CLI prints a one-line summary to stdout
and exits 0; on a non-conforming log it writes a count header plus concise
per-error detail (`keyword @ instancePath — message`, capped at 20) to stderr —
the channel a CI pipeline reliably captures — persists the complete set of
findings to `<output>.validate-report.sarif`, and exits 1. The SARIF file is
written either way — validation is a report, not a gate on the write.

```sh
npx @microsoft/sarif-multitool-ts emit-finalize scan.sarif --validate
```

Library callers read the verdict off the outcome (or validate an
already-finalized log directly):

```js
import { emitFinalize, validateFinalizedLog } from '@microsoft/sarif-multitool-ts';

const { validation } = await emitFinalize({ output: 'scan.sarif', validate: true });
if (validation && !validation.valid) console.error(validation.errors.join('\n'));

const { valid, errors } = validateFinalizedLog(log);
```

This is JSON-Schema validation: it enforces the whole-log AI contract —
required `versionControlProvenance`, `properties[ai/origin]`, the result
`ruleId` grammar, and the per-run GHAzDO `automationDetails` shape. It does
**not** run the .NET analyzer rule set; for the full `Sarif;AI` rule pass keep
a .NET step in your test/CI environment:

```sh
dotnet tool install -g Sarif.Multitool   # once
sarif validate --rule-kind "Sarif;AI" scan.sarif
```

## Schemas

The AI-content JSON Schemas ship in the package and are named for the SARIF
object each constrains: `ai-run`, `ai-result`, `ai-invocation`,
`ai-rule-reporting-descriptor`, `ai-notification-reporting-descriptor`, and
`ai-sarif-log` (the finalized whole-log contract `emit-finalize` produces).

**Primary path — import the file directly.** Each schema is exported under
`./schemas/*`, so Node consumers resolve it without going through a verb:

```js
import sarifLogSchema from '@microsoft/sarif-multitool-ts/schemas/ai-sarif-log.schema.json' with { type: 'json' };
// or, CommonJS:
// const sarifLogSchema = require('@microsoft/sarif-multitool-ts/schemas/ai-sarif-log.schema.json');
```

This is the robust path: it depends only on the published file name, not on the
verb→schema lookup.

**Fallback — resolve by verb.** `get-schema <verb>` (CLI) and `getSchema(verb)`
(library) return the same schema text keyed by the verb that produces or
consumes it — handy when you already hold a verb name:

```sh
npx @microsoft/sarif-multitool-ts get-schema emit-finalize   # → ai-sarif-log.schema.json
```

The verb→schema map mirrors the .NET multitool and is held in sync by a CI
drift gate (`npm run test:conformance`).

## Known gaps vs. the .NET tool (v1)

| Gap | Effect | Workaround |
|---|---|---|
| `--validate` (analyzer rules) | TS `--validate` is JSON-Schema only — it does not run the full `Sarif;AI` analyzer rule set | `.NET sarif validate --rule-kind "Sarif;AI"` for the full rule pass |
| `--embed-text-files` | Source bytes not inlined in artifacts | Run `.NET emit-finalize` with the flag if you need self-contained fixtures |

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
