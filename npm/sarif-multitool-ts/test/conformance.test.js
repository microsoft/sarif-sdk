// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Conformance harness: runs the same emit-chain scenario through the TS
 * library and the .NET multitool, then deep-diffs the output SARIF after
 * normalizing volatile fields (endTimeUtc, absolute temp paths).
 *
 * Skips with a clear message if `dotnet` is not on PATH or the multitool
 * project doesn't build — so `npm test` stays green on a node-only box,
 * but `npm run test:conformance` in CI (where .NET is available) is a
 * hard parity gate.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, rmSync, writeFileSync, readFileSync, mkdirSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { emitRun, emitResults, emitFinalize } from '../dist/index.js';

const REPO_ROOT = resolve(import.meta.dirname, '..', '..', '..');
const MULTITOOL_PROJ = join(REPO_ROOT, 'src', 'Sarif.Multitool', 'Sarif.Multitool.csproj');

function dotnetAvailable() {
  const r = spawnSync('dotnet', ['--version'], { encoding: 'utf8' });
  return r.status === 0;
}

function runDotnet(args, cwd) {
  const r = spawnSync(
    'dotnet',
    ['run', '--project', MULTITOOL_PROJ, '-c', 'Release', '-f', 'net8.0', '--no-build', '--', ...args],
    { encoding: 'utf8', cwd },
  );
  if (r.status !== 0) {
    throw new Error(`dotnet ${args.join(' ')} exited ${r.status}\nstdout: ${r.stdout}\nstderr: ${r.stderr}`);
  }
  return r;
}

function buildDotnetOnce() {
  const r = spawnSync(
    'dotnet',
    ['build', MULTITOOL_PROJ, '-c', 'Release', '-f', 'net8.0', '-v', 'quiet'],
    { encoding: 'utf8' },
  );
  return r.status === 0;
}

/**
 * Replaces volatile fields with sentinels so structural diff is meaningful,
 * and strips known-gap fields (documented in README#Known-gaps) so the
 * deep-diff focuses on substantive parity.
 *
 * Volatile:
 *   - endTimeUtc → "<TIMESTAMP>"
 *   - any file:// URI under the temp dir → "<TMPDIR>/..."
 *   - $schema (Newtonsoft model uses a different default URI form)
 *
 * Known gaps (stripped from BOTH sides):
 *   - run.columnKind — .NET model emits its enum default ('utf16CodeUnits')
 *     even when the producer never set it; the TS port preserves absence.
 *     Cosmetic Newtonsoft default-value emission, not a semantic delta.
 *   - result.partialFingerprints.primaryLocationLineHash — the GitHub-only
 *     rolling-hash dedup hint. Documented v1 gap; remove when ported.
 */
function normalize(obj, tmpRoot) {
  const tmpUri = pathToFileURL(tmpRoot).toString().replace(/\/?$/, '/');
  const json = JSON.stringify(obj);
  const replaced = json
    .replaceAll(tmpUri, '<TMPDIR>/')
    .replace(/"\$schema":"[^"]*"/g, '"$schema":"<SCHEMA>"')
    .replace(/"endTimeUtc":"[^"]*"/g, '"endTimeUtc":"<TIMESTAMP>"');
  const out = JSON.parse(replaced);
  for (const run of out.runs ?? []) {
    delete run.columnKind;
    for (const r of run.results ?? []) {
      if (r.partialFingerprints) {
        delete r.partialFingerprints.primaryLocationLineHash;
        if (Object.keys(r.partialFingerprints).length === 0) delete r.partialFingerprints;
      }
    }
  }
  return out;
}

/** A single end-to-end scenario, parameterized so more fixtures can be added. */
function makeScenario(tmp) {
  mkdirSync(join(tmp, 'src'), { recursive: true });
  writeFileSync(join(tmp, 'src', 'app.ts'), 'const a = 1;\nconst b = unsafe(input);\nconst c = 3;\n', 'utf8');

  const srcRoot = pathToFileURL(tmp).toString().replace(/\/?$/, '/');
  const runHeader = {
    tool: { driver: { name: 'conformance-scanner', version: '1.0.0' } },
    originalUriBaseIds: { SRCROOT: { uri: srcRoot } },
    versionControlProvenance: [
      {
        repositoryUri: 'https://github.com/contoso/widgets',
        revisionId: 'a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0',
        mappedTo: { uriBaseId: 'SRCROOT' },
      },
    ],
    properties: { 'ai/origin': 'generated', 'customer/passthrough': 'kept' },
  };
  const result = {
    ruleId: 'CWE-79/dom-xss',
    message: { text: 'Unsanitized input flows to sink.' },
    locations: [
      {
        physicalLocation: {
          artifactLocation: { uri: 'src/app.ts', uriBaseId: 'SRCROOT' },
          region: { startLine: 2 },
        },
      },
    ],
    properties: { 'customer/score': '0.91' },
  };
  return { runHeader, result };
}

async function runTsChain(tmp, scenario) {
  const out = join(tmp, 'ts.sarif');
  await emitRun({ output: out, run: structuredClone(scenario.runHeader), env: () => undefined });
  await emitResults({ output: out, results: structuredClone(scenario.result) });
  await emitFinalize({ output: out, keepWip: true });
  return JSON.parse(readFileSync(out, 'utf8'));
}

function runDotnetChain(tmp, scenario) {
  const out = join(tmp, 'net.sarif');
  const runJson = join(tmp, 'run.json');
  const resJson = join(tmp, 'res.json');
  writeFileSync(runJson, JSON.stringify(scenario.runHeader), 'utf8');
  writeFileSync(resJson, JSON.stringify(scenario.result), 'utf8');
  runDotnet(['emit-run', out, '--input', runJson, '--force-overwrite'], tmp);
  runDotnet(['add-results', out, '--input', resJson], tmp);
  runDotnet(['emit-finalize', out, '--keep-wip'], tmp);
  return JSON.parse(readFileSync(out, 'utf8'));
}

test('TS emit chain ⇔ .NET emit chain produce equivalent SARIF', async (t) => {
  if (!dotnetAvailable()) {
    t.skip('dotnet not on PATH; conformance harness requires the .NET SDK');
    return;
  }
  if (!buildDotnetOnce()) {
    t.skip('dotnet build of Sarif.Multitool failed; fix the .NET build first');
    return;
  }

  const tmp = mkdtempSync(join(tmpdir(), 'sarif-conf-'));
  try {
    const scenario = makeScenario(tmp);
    const tsLog = normalize(await runTsChain(tmp, scenario), tmp);
    const netLog = normalize(runDotnetChain(tmp, scenario), tmp);

    // Headline assertions first so a failure message is actionable even if
    // the deep-diff is large.
    assert.equal(tsLog.runs.length, netLog.runs.length, 'run count');
    assert.equal(
      tsLog.runs[0].results?.length ?? 0,
      netLog.runs[0].results?.length ?? 0,
      'result count',
    );
    assert.equal(
      tsLog.runs[0].results[0].ruleId,
      netLog.runs[0].results[0].ruleId,
      'result[0].ruleId',
    );
    assert.equal(
      tsLog.runs[0].results[0].locations[0].physicalLocation.region.snippet.text,
      netLog.runs[0].results[0].locations[0].physicalLocation.region.snippet.text,
      'region snippet',
    );
    assert.equal(
      tsLog.runs[0].properties['customer/passthrough'],
      netLog.runs[0].properties['customer/passthrough'],
      'pass-through property bag',
    );

    // Full structural diff. This will surface key-ordering and default-value
    // discrepancies between the Newtonsoft contract resolver and JSON.stringify;
    // when it fires, the fix belongs in eventLog.ts#serializeSarifLog or the
    // normalize() helper above (if the discrepancy is genuinely cosmetic).
    assert.deepEqual(tsLog, netLog);
  } finally {
    rmSync(tmp, { recursive: true, force: true });
  }
});
