// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Unit coverage for `validateFinalizedLog` — the engine behind
 * `emit-finalize --validate`. Asserts the direct library API surface: a
 * conformant whole-log validates clean, and the AI-profile constraints
 * (ruleId grammar, required ai/origin, message.markdown) are each reported.
 *
 * Runs against the built dist/. Run via `npm run test:unit`.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { validateFinalizedLog } from '../dist/index.js';

function conformantLog() {
  return {
    version: '2.1.0',
    runs: [
      {
        tool: { driver: { name: 'ai-scanner' } },
        versionControlProvenance: [{ repositoryUri: 'https://github.com/contoso/widgets' }],
        properties: { 'ai/origin': 'generated' },
        results: [
          {
            ruleId: 'CWE-79',
            message: { text: 'XSS', markdown: '**XSS**' },
            locations: [
              {
                physicalLocation: {
                  artifactLocation: { uri: 'src/app.ts' },
                  region: { startLine: 1 },
                },
              },
            ],
          },
        ],
      },
    ],
  };
}

test('a conformant finalized log validates clean', () => {
  const r = validateFinalizedLog(conformantLog());
  assert.equal(r.valid, true, r.errors.join('\n'));
  assert.deepEqual(r.errors, []);
});

test('a non-grammar ruleId is reported', () => {
  const log = conformantLog();
  log.runs[0].results[0].ruleId = 'not-a-valid-id';
  const r = validateFinalizedLog(log);
  assert.equal(r.valid, false);
  assert.ok(
    r.errors.some((e) => e.includes('/runs/0/results/0/ruleId')),
    r.errors.join('\n'),
  );
});

test('a missing ai/origin is reported', () => {
  const log = conformantLog();
  delete log.runs[0].properties;
  const r = validateFinalizedLog(log);
  assert.equal(r.valid, false);
  assert.ok(
    r.errors.some((e) => e.includes('properties')),
    r.errors.join('\n'),
  );
});

test('a run missing versionControlProvenance is reported', () => {
  const log = conformantLog();
  delete log.runs[0].versionControlProvenance;
  const r = validateFinalizedLog(log);
  assert.equal(r.valid, false);
  assert.ok(
    r.errors.some((e) => e.includes('versionControlProvenance')),
    r.errors.join('\n'),
  );
});

test('a repo-less run stamped unpublishable validates without versionControlProvenance', () => {
  const log = conformantLog();
  delete log.runs[0].versionControlProvenance;
  log.runs[0].properties.unpublishable = true;
  const r = validateFinalizedLog(log);
  assert.equal(r.valid, true, r.errors.join('\n'));
  assert.deepEqual(r.errors, []);
});

test('a result without message.markdown is reported', () => {
  const log = conformantLog();
  log.runs[0].results[0].message = { text: 'XSS' };
  const r = validateFinalizedLog(log);
  assert.equal(r.valid, false);
  assert.ok(
    r.errors.some((e) => e.includes('/runs/0/results/0/message')),
    r.errors.join('\n'),
  );
});

test('the bare CWE collapsed id form is accepted', () => {
  const log = conformantLog();
  log.runs[0].results[0].ruleId = 'CWE-89';
  assert.equal(validateFinalizedLog(log).valid, true);
});

test('the NOVEL escape-hatch ruleId is accepted', () => {
  const log = conformantLog();
  log.runs[0].results[0].ruleId = 'NOVEL-llm-leak';
  assert.equal(validateFinalizedLog(log).valid, true);
});

test('a non-conformant outcome carries structured details parallel to errors', () => {
  const log = conformantLog();
  log.runs[0].results[0].ruleId = 'not-a-valid-id';
  const r = validateFinalizedLog(log);
  assert.equal(r.valid, false);
  assert.equal(r.details.length, r.errors.length);
  assert.ok(
    r.details.every((d) => typeof d.keyword === 'string' && typeof d.instancePath === 'string'),
    JSON.stringify(r.details),
  );
  assert.ok(
    r.details.some((d) => d.instancePath.includes('/runs/0/results/0/ruleId')),
    JSON.stringify(r.details),
  );
});
