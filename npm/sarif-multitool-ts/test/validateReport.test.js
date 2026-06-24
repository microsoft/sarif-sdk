// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Unit coverage for `reportValidation` — the channeling layer behind
 * `emit-finalize --validate`. Asserts the converged contract: a structured JSON
 * receipt (verdict + full, uncapped error set) goes to stdout on every run; on
 * failure a capped human summary also goes to stderr and the complete findings
 * are persisted to a `<output>.validate-report.sarif` file; on success no report
 * is written.
 *
 * Runs against the built dist/. Run via `npm run test:unit`.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { existsSync, mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import process from 'node:process';
import { reportValidation, validationReportPath } from '../dist/index.js';

function capture(fn) {
  const out = [];
  const err = [];
  const origOut = process.stdout.write;
  const origErr = process.stderr.write;
  process.stdout.write = (s) => (out.push(String(s)), true);
  process.stderr.write = (s) => (err.push(String(s)), true);
  let exit;
  try {
    exit = fn();
  } finally {
    process.stdout.write = origOut;
    process.stderr.write = origErr;
  }
  return { exit, stdout: out.join(''), stderr: err.join('') };
}

function withTempOutput(fn) {
  const dir = mkdtempSync(path.join(tmpdir(), 'validate-report-'));
  try {
    return fn(path.join(dir, 'scan.sarif'));
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
}

function nonConformantOutcome(count) {
  const details = [];
  for (let i = 0; i < count; i++) {
    details.push({
      instancePath: `/runs/0/results/${i}/ruleId`,
      message: 'must match pattern',
      keyword: 'pattern',
    });
  }
  return { valid: false, errors: details.map((d) => `${d.instancePath}: ${d.message}`), details };
}

test('a conforming outcome writes a structured receipt to stdout and no report', () => {
  withTempOutput((output) => {
    const { exit, stdout, stderr } = capture(() =>
      reportValidation(output, { valid: true, errors: [], details: [] }),
    );
    assert.equal(exit, 0);
    assert.equal(stderr, '');
    assert.equal(existsSync(validationReportPath(output)), false);

    const receipt = JSON.parse(stdout);
    assert.equal(receipt.conforms, true);
    assert.equal(receipt.profile, 'ai-sarif-log.schema.json');
    assert.equal(receipt.errorCount, 0);
    assert.equal(receipt.reportPath, null);
    assert.deepEqual(receipt.errors, []);
  });
});

test('a failing outcome writes a structured receipt to stdout and a human summary to stderr', () => {
  withTempOutput((output) => {
    const { exit, stdout, stderr } = capture(() => reportValidation(output, nonConformantOutcome(2)));
    assert.equal(exit, 1);

    // stderr: the human summary.
    assert.match(stderr, /does not conform/);
    assert.match(stderr, /Full report:/);
    assert.match(stderr, /\n {2}pattern @ \/runs\/0\/results\/0\/ruleId — must match pattern/);

    // stdout: the structured receipt — parseable, with the full error set.
    const receipt = JSON.parse(stdout);
    assert.equal(receipt.conforms, false);
    assert.equal(receipt.errorCount, 2);
    assert.equal(receipt.errors.length, 2);
    assert.equal(receipt.errors[0].ruleId, 'pattern');
    assert.equal(receipt.errors[0].location, '/runs/0/results/0/ruleId');
    assert.match(receipt.reportPath, /scan\.validate-report\.sarif$/);

    // The human prose never leaks onto stdout.
    assert.doesNotMatch(stdout, /does not conform/);
    assert.doesNotMatch(stdout, /Full report:/);
  });
});

test('a failing outcome persists a valid SARIF report next to the output', () => {
  withTempOutput((output) => {
    capture(() => reportValidation(output, nonConformantOutcome(3)));

    const reportPath = validationReportPath(output);
    assert.equal(existsSync(reportPath), true);

    const report = JSON.parse(readFileSync(reportPath, 'utf8'));
    assert.equal(report.version, '2.1.0');
    assert.equal(report.runs[0].results.length, 3);
    assert.equal(report.runs[0].results[0].ruleId, 'pattern');
    assert.equal(report.runs[0].results[0].level, 'error');
  });
});

test('stderr caps detail at 20 while the stdout receipt carries the full uncapped set', () => {
  withTempOutput((output) => {
    const { stdout, stderr } = capture(() => reportValidation(output, nonConformantOutcome(25)));

    const detailLines = stderr.split('\n').filter((l) => / @ \//.test(l));
    assert.equal(detailLines.length, 20);
    assert.match(stderr, /\.\.\.and 5 more error\(s\)\./);

    // The receipt is never capped — it is the machine-readable channel.
    const receipt = JSON.parse(stdout);
    assert.equal(receipt.errorCount, 25);
    assert.equal(receipt.errors.length, 25);
  });
});
