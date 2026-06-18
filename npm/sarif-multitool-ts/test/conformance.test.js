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

function runDotnet(args, cwd, env) {
  const r = spawnSync(
    'dotnet',
    ['run', '--project', MULTITOOL_PROJ, '-c', 'Release', '-f', 'net8.0', '--no-build', '--', ...args],
    { encoding: 'utf8', cwd, env },
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
 * Replaces volatile fields with sentinels so structural diff is meaningful.
 *
 * Volatile:
 *   - endTimeUtc → "<TIMESTAMP>"
 *   - any file:// URI under the temp dir → "<TMPDIR>/..."
 *   - $schema (Newtonsoft model uses a different default URI form)
 *
 * No substantive fields are stripped: run.columnKind (both chains emit
 * 'utf16CodeUnits') and result.partialFingerprints.primaryLocationLineHash
 * (both chains stamp the identical rolling hash for GitHub-hosted runs) are
 * now produced on both sides and so are enforced by the deep-diff.
 */
function normalize(obj, tmpRoot) {
  const tmpUri = pathToFileURL(tmpRoot).toString().replace(/\/?$/, '/');
  const json = JSON.stringify(obj);
  const replaced = json
    .replaceAll(tmpUri, '<TMPDIR>/')
    .replace(/"\$schema":"[^"]*"/g, '"$schema":"<SCHEMA>"')
    .replace(/"endTimeUtc":"[^"]*"/g, '"endTimeUtc":"<TIMESTAMP>"');
  return JSON.parse(replaced);
}

// CI-context env vars read by emit-run's AdoPipelineContext / GitHubActionsContext
// detectors. The .NET multitool reads these from the *process* environment, so
// they are scrubbed from the spawned process and re-supplied per scenario; that
// keeps both chains seeing identical detection inputs even when the conformance
// harness itself runs inside an ADO or GitHub Actions pipeline.
const CI_ENV_KEYS = [
  'TF_BUILD',
  'SYSTEM_COLLECTIONURI',
  'SYSTEM_TEAMPROJECTID',
  'BUILD_DEFINITIONID',
  'SYSTEM_DEFINITIONID',
  'BUILD_DEFINITIONNAME',
  'BUILD_BUILDID',
  'SYSTEM_PHASEID',
  'SYSTEM_JOBID',
  'SYSTEM_PHASENAME',
  'SYSTEM_JOBNAME',
  'BUILD_SOURCEBRANCH',
  'BUILD_REPOSITORY_URI',
  'BUILD_SOURCEVERSION',
  'GITHUB_ACTIONS',
  'GITHUB_SERVER_URL',
  'GITHUB_REPOSITORY',
  'GITHUB_SHA',
  'GITHUB_REF',
];

/** Builds the env for the spawned .NET process: process env minus CI keys, plus the scenario's. */
function dotnetEnv(scenarioEnv) {
  const env = { ...process.env };
  for (const k of CI_ENV_KEYS) delete env[k];
  return { ...env, ...scenarioEnv };
}

const GITHUB_HEADER = (srcRoot) => ({
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
});

/**
 * Each scenario is an end-to-end fixture run through both chains. `env` feeds
 * both the TS env getter and the scrubbed .NET process env so CI-context
 * detection is exercised identically on both sides.
 */
const SCENARIOS = [
  {
    name: 'GitHub run, single CWE result',
    env: {},
    build(tmp) {
      mkdirSync(join(tmp, 'src'), { recursive: true });
      writeFileSync(
        join(tmp, 'src', 'app.ts'),
        'const a = 1;\nconst b = unsafe(input);\nconst c = 3;\n',
        'utf8',
      );
      return {
        runHeader: GITHUB_HEADER(pathToFileURL(tmp).toString().replace(/\/?$/, '/')),
        results: {
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
        },
      };
    },
  },
  {
    name: 'GitHub run, multiple results incl. a NOVEL ruleId',
    env: {},
    build(tmp) {
      mkdirSync(join(tmp, 'src'), { recursive: true });
      writeFileSync(
        join(tmp, 'src', 'app.ts'),
        'const q = `SELECT * FROM t WHERE id=${id}`;\nconst token = "AKIA-hardcoded";\nrun(q, token);\n',
        'utf8',
      );
      return {
        runHeader: GITHUB_HEADER(pathToFileURL(tmp).toString().replace(/\/?$/, '/')),
        // A taxonomy-bearing result (CWE-89, enriched + sub-id-collapsed) and a
        // NOVEL result (flat id, no CWE enrichment) in one batch — exercises
        // multi-result descriptor registration parity against .NET.
        results: [
          {
            ruleId: 'CWE-89/sql-injection',
            message: { text: 'Tainted input flows into a SQL string.' },
            locations: [
              {
                physicalLocation: {
                  artifactLocation: { uri: 'src/app.ts', uriBaseId: 'SRCROOT' },
                  region: { startLine: 1 },
                },
              },
            ],
          },
          {
            ruleId: 'NOVEL-hardcoded-credential',
            message: { text: 'A credential literal is checked into source.' },
            locations: [
              {
                physicalLocation: {
                  artifactLocation: { uri: 'src/app.ts', uriBaseId: 'SRCROOT' },
                  region: { startLine: 2 },
                },
              },
            ],
          },
        ],
      };
    },
  },
];

async function runTsChain(tmp, scenario, data) {
  const out = join(tmp, 'ts.sarif');
  await emitRun({
    output: out,
    run: structuredClone(data.runHeader),
    env: (name) => scenario.env[name],
  });
  await emitResults({ output: out, results: structuredClone(data.results) });
  await emitFinalize({ output: out, keepWip: true });
  return JSON.parse(readFileSync(out, 'utf8'));
}

function runDotnetChain(tmp, scenario, data) {
  const out = join(tmp, 'net.sarif');
  const runJson = join(tmp, 'run.json');
  const resJson = join(tmp, 'res.json');
  const env = dotnetEnv(scenario.env);
  writeFileSync(runJson, JSON.stringify(data.runHeader), 'utf8');
  writeFileSync(resJson, JSON.stringify(data.results), 'utf8');
  runDotnet(['emit-run', out, '--input', runJson, '--force-overwrite'], tmp, env);
  runDotnet(['add-results', out, '--input', resJson], tmp, env);
  runDotnet(['emit-finalize', out, '--keep-wip'], tmp, env);
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

  for (const scenario of SCENARIOS) {
    await t.test(scenario.name, async () => {
      const tmp = mkdtempSync(join(tmpdir(), 'sarif-conf-'));
      try {
        const data = scenario.build(tmp);
        const tsLog = normalize(await runTsChain(tmp, scenario, data), tmp);
        const netLog = normalize(runDotnetChain(tmp, scenario, data), tmp);

        // Headline assertions first so a failure message is actionable even if
        // the deep-diff is large.
        assert.equal(tsLog.runs.length, netLog.runs.length, 'run count');
        assert.equal(
          tsLog.runs[0].results?.length ?? 0,
          netLog.runs[0].results?.length ?? 0,
          'result count',
        );

        // Full structural diff. This surfaces key-ordering and default-value
        // discrepancies between the Newtonsoft contract resolver and
        // JSON.stringify; when it fires, the fix belongs in
        // io.ts#serializeSarifLog or the normalize() helper above (if the
        // discrepancy is genuinely cosmetic).
        assert.deepEqual(tsLog, netLog);
      } finally {
        rmSync(tmp, { recursive: true, force: true });
      }
    });
  }
});
