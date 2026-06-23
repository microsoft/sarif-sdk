// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Given, When, Then } from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import { existsSync, writeFileSync, mkdirSync, readFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import {
  emitRun,
  emitResults,
  emitInvocations,
  emitRuleDescriptors,
  emitNotificationDescriptors,
  emitFinalize,
  readEvents,
  replay,
  wipPathFor,
} from '../../dist/index.js';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function get(obj, path) {
  return path.split('.').reduce((o, k) => (o == null ? undefined : o[k]), obj);
}

function makeResult(ruleId, relPath = 'src/app.ts', line = 1, extra = {}) {
  return {
    ruleId,
    message: { text: 'finding' },
    locations: [
      {
        physicalLocation: {
          artifactLocation: { uri: relPath, uriBaseId: 'SRCROOT' },
          region: { startLine: line },
        },
      },
    ],
    ...extra,
  };
}

function makeInvocation(extra = {}) {
  return {
    executionSuccessful: true,
    commandLine: 'ai-scanner --scan',
    workingDirectory: { uriBaseId: 'SRCROOT' },
    ...extra,
  };
}

// ---------------------------------------------------------------------------
// Given
// ---------------------------------------------------------------------------

Given('a clean working directory', function () {
  this.init();
});

Given('a run header with tool.driver.name {string}', function (name) {
  this.runHeader = { tool: { driver: { name } } };
});

Given('a run header without tool.driver.name', function () {
  this.runHeader = { tool: { driver: {} } };
});

Given('a run header that is actually a full SARIF log', function () {
  this.runHeader = { version: '2.1.0', runs: [] };
});

Given('the run header carries properties bag key {string} = {string}', function (k, v) {
  this.runHeader.properties ??= {};
  this.runHeader.properties[k] = v;
});

Given('the run header carries unknown top-level key {string} = {string}', function (k, v) {
  this.runHeader[k] = v;
});

Given('an open event log for tool {string}', async function (name) {
  if (!this.tmp) this.init();
  this.runHeader = { tool: { driver: { name } } };
  await emitRun({ output: this.output, run: this.runHeader, env: this.env });
});

Given(
  'an open event log for tool {string} with a GitHub source root and VCP',
  async function (name) {
    if (!this.tmp) this.init();
    this.runHeader = this.finalizableRunHeader(true);
    this.runHeader.tool.driver.name = name;
    await emitRun({ output: this.output, run: this.runHeader, env: this.env });
  },
);

Given(
  'an open event log for tool {string} with a GitHub source root, VCP, and ai\\/origin {string}',
  async function (name, origin) {
    if (!this.tmp) this.init();
    this.runHeader = this.finalizableRunHeader(true);
    this.runHeader.tool.driver.name = name;
    this.runHeader.properties = { 'ai/origin': origin };
    await emitRun({ output: this.output, run: this.runHeader, env: this.env });
  },
);

Given(
  'an open event log for tool {string} with a source root but no VCP',
  async function (name) {
    if (!this.tmp) this.init();
    this.runHeader = this.finalizableRunHeader(false);
    this.runHeader.tool.driver.name = name;
    await emitRun({
      output: this.output,
      run: this.runHeader,
      forceOverwrite: true,
      env: this.env,
    });
  },
);

Given('emit-run has already been invoked', async function () {
  await emitRun({ output: this.output, run: this.runHeader, env: this.env });
});

Given('a fixture source file {string} with content:', function (relPath, content) {
  const full = join(this.tmp, relPath);
  mkdirSync(dirname(full), { recursive: true });
  writeFileSync(full, content, 'utf8');
});

Given(
  'emit-notification-descriptors has appended id {string}',
  async function (id) {
    const r = await emitNotificationDescriptors({
      output: this.output,
      descriptors: { id },
    });
    assert.equal(r.appended, 1);
  },
);

// ---------------------------------------------------------------------------
// When — emit-run
// ---------------------------------------------------------------------------

When('emit-run is invoked', async function () {
  this.lastError = undefined;
  await emitRun({ output: this.output, run: this.runHeader, env: this.env });
});

When('emit-run is invoked with force-overwrite', async function () {
  this.lastError = undefined;
  await emitRun({
    output: this.output,
    run: this.runHeader,
    forceOverwrite: true,
    env: this.env,
  });
});

When('emit-run is invoked expecting failure', async function () {
  try {
    await emitRun({ output: this.output, run: this.runHeader, env: this.env });
    assert.fail('emit-run was expected to fail but succeeded');
  } catch (err) {
    this.lastError = err;
  }
});

// ---------------------------------------------------------------------------
// When — emit-results
// ---------------------------------------------------------------------------

When(
  'emit-results is invoked with a single result whose ruleId is {string}',
  async function (ruleId) {
    this.lastOutcome = await emitResults({ output: this.output, results: makeResult(ruleId) });
  },
);

When('emit-results is invoked with a batch of ruleIds:', async function (table) {
  const ids = table.raw().map((r) => r[0]);
  this.lastOutcome = await emitResults({
    output: this.output,
    results: ids.map((id) => makeResult(id)),
  });
});

When(
  'emit-results is invoked with a result at {string} line {int}',
  async function (relPath, line) {
    this.lastOutcome = await emitResults({
      output: this.output,
      results: makeResult('CWE-79/dom-xss', relPath, line),
    });
  },
);

When(
  'emit-results is invoked with a result at {string} line {int} with ruleId {string}',
  async function (relPath, line, ruleId) {
    this.lastOutcome = await emitResults({
      output: this.output,
      results: makeResult(ruleId, relPath, line),
    });
  },
);

When(
  'emit-results is invoked with a conformant AI result at {string} line {int} with ruleId {string}',
  async function (relPath, line, ruleId) {
    this.lastOutcome = await emitResults({
      output: this.output,
      results: makeResult(ruleId, relPath, line, {
        message: { text: 'finding', markdown: '**finding**' },
      }),
    });
  },
);

When(
  'emit-results is invoked with a result at {string} line {int} carrying properties bag key {string} = {string}',
  async function (relPath, line, k, v) {
    this.lastOutcome = await emitResults({
      output: this.output,
      results: makeResult('CWE-79/dom-xss', relPath, line, { properties: { [k]: v } }),
    });
  },
);

// ---------------------------------------------------------------------------
// When — emit-invocations
// ---------------------------------------------------------------------------

When(
  'emit-invocations is invoked with a single valid invocation lacking endTimeUtc',
  async function () {
    this.lastOutcome = await emitInvocations({
      output: this.output,
      invocations: makeInvocation(),
    });
  },
);

When(
  'emit-invocations is invoked with a batch of {int} valid invocations lacking endTimeUtc',
  async function (n) {
    this.lastOutcome = await emitInvocations({
      output: this.output,
      invocations: Array.from({ length: n }, () => makeInvocation()),
    });
  },
);

When(
  'emit-invocations is invoked with a single invocation missing {string}',
  async function (field) {
    const inv = makeInvocation();
    delete inv[field];
    this.lastOutcome = await emitInvocations({ output: this.output, invocations: inv });
  },
);

When(
  'emit-invocations is invoked with a single invocation whose workingDirectory has neither uri nor uriBaseId',
  async function () {
    this.lastOutcome = await emitInvocations({
      output: this.output,
      invocations: makeInvocation({ workingDirectory: {} }),
    });
  },
);

When(
  'emit-invocations is invoked with a single valid invocation carrying unknown key {string} = {string}',
  async function (k, v) {
    this.lastOutcome = await emitInvocations({
      output: this.output,
      invocations: makeInvocation({ [k]: v }),
    });
  },
);

// ---------------------------------------------------------------------------
// When — add-*-reporting-descriptors
// ---------------------------------------------------------------------------

When('emit-rule-descriptors is invoked with id {string}', async function (id) {
  this.lastOutcome = await emitRuleDescriptors({
    output: this.output,
    descriptors: { id },
  });
});

When('emit-notification-descriptors is invoked with id {string}', async function (id) {
  this.lastOutcome = await emitNotificationDescriptors({
    output: this.output,
    descriptors: { id },
  });
});

When('emit-notification-descriptors is invoked with ids:', async function (table) {
  const ids = table.raw().map((r) => r[0]);
  this.lastOutcome = await emitNotificationDescriptors({
    output: this.output,
    descriptors: ids.map((id) => ({ id })),
  });
});

// ---------------------------------------------------------------------------
// When — replay / finalize
// ---------------------------------------------------------------------------

When('the event log is replayed', async function () {
  this.replayedLog = await replay(wipPathFor(this.output));
});

When('emit-finalize is invoked', async function () {
  const r = await emitFinalize({ output: this.output, keepWip: true });
  this.finalizedLog = r.log;
});

When('emit-finalize is invoked with validation', async function () {
  const r = await emitFinalize({ output: this.output, keepWip: true, validate: true });
  this.finalizedLog = r.log;
  this.validation = r.validation;
});

When('emit-finalize is invoked expecting failure', async function () {
  try {
    await emitFinalize({ output: this.output, keepWip: true });
    assert.fail('emit-finalize was expected to fail but succeeded');
  } catch (err) {
    this.lastError = err;
  }
});

// ---------------------------------------------------------------------------
// Then — event log
// ---------------------------------------------------------------------------

Then('a .wip.jsonl event log exists', function () {
  assert.ok(existsSync(wipPathFor(this.output)));
});

Then('no .wip.jsonl event log exists', function () {
  assert.ok(!existsSync(wipPathFor(this.output)));
});

Then('the log contains exactly {int} {string} event(s)', async function (n, kind) {
  const events = await readEvents(wipPathFor(this.output));
  assert.equal(events.filter((e) => e.kind === kind).length, n);
});

Then('the run-header payload field {string} equals {string}', async function (path, expected) {
  const events = await readEvents(wipPathFor(this.output));
  const header = events.find((e) => e.kind === 'run-header');
  assert.equal(get(header.payload, path), expected);
});

Then('the appended invocation carries a non-empty {string}', async function (field) {
  const events = await readEvents(wipPathFor(this.output));
  const inv = events.find((e) => e.kind === 'invocation');
  assert.ok(inv && typeof inv.payload[field] === 'string' && inv.payload[field].length > 0);
});

// ---------------------------------------------------------------------------
// Then — batch outcome
// ---------------------------------------------------------------------------

Then('the outcome reports {int} appended and {int} rejected', function (appended, rejected) {
  assert.equal(this.lastOutcome.appended, appended);
  assert.equal(this.lastOutcome.rejected.length, rejected);
});

Then('rejected element {int} carries errorCode {string}', function (idx, code) {
  const e = this.lastOutcome.rejected.find((r) => r.index === idx);
  assert.ok(e, `no rejected element at index ${idx}`);
  assert.equal(e.errorCode, code);
});

Then('rejected element {int} message mentions {string}', function (idx, fragment) {
  const e = this.lastOutcome.rejected.find((r) => r.index === idx);
  assert.ok(e, `no rejected element at index ${idx}`);
  assert.ok(e.message.includes(fragment), `'${e.message}' does not contain '${fragment}'`);
});

// ---------------------------------------------------------------------------
// Then — failure
// ---------------------------------------------------------------------------

Then('the failure message mentions {string}', function (fragment) {
  assert.ok(this.lastError, 'expected a failure but none was recorded');
  assert.ok(
    this.lastError.message.includes(fragment),
    `'${this.lastError.message}' does not contain '${fragment}'`,
  );
});

// ---------------------------------------------------------------------------
// Then — replayed run
// ---------------------------------------------------------------------------

Then('the replayed run field {string} equals {string}', function (path, expected) {
  assert.equal(get(this.replayedLog.runs[0], path), expected);
});

Then('the replayed invocation {int} field {string} equals {string}', function (i, path, expected) {
  assert.equal(get(this.replayedLog.runs[0].invocations[i], path), expected);
});

// ---------------------------------------------------------------------------
// Then — finalized SARIF
// ---------------------------------------------------------------------------

function finalizedRun(world) {
  return world.finalizedLog.runs[0];
}

Then('the finalized SARIF conforms to the AI whole-log schema', function () {
  assert.ok(this.validation, 'emit-finalize was not invoked with validation');
  assert.ok(
    this.validation.valid,
    `expected conformance but got errors:\n  ${this.validation.errors.join('\n  ')}`,
  );
  assert.deepEqual(this.validation.errors, []);
});

Then('the finalized SARIF does not conform to the AI whole-log schema', function () {
  assert.ok(this.validation, 'emit-finalize was not invoked with validation');
  assert.equal(this.validation.valid, false, 'expected a non-conformant log');
  assert.ok(this.validation.errors.length > 0, 'expected at least one validation error');
});

Then('a validation error mentions {string}', function (fragment) {
  assert.ok(this.validation, 'emit-finalize was not invoked with validation');
  assert.ok(
    this.validation.errors.some((e) => e.includes(fragment)),
    `no validation error mentions '${fragment}':\n  ${this.validation.errors.join('\n  ')}`,
  );
});

Then('the finalized SARIF result {int} field {string} equals {string}', function (i, path, expected) {
  assert.equal(get(finalizedRun(this).results[i], path), expected);
});

Then('the finalized SARIF result {int} ruleId equals {string}', function (i, expected) {
  assert.equal(finalizedRun(this).results[i].ruleId, expected);
});

Then('the finalized SARIF result {int} region has a non-negative {string}', function (i, field) {
  const v = finalizedRun(this).results[i].locations[0].physicalLocation.region[field];
  assert.ok(typeof v === 'number' && v >= 0, `region.${field}=${v}`);
});

Then('the finalized SARIF result {int} region has a positive {string}', function (i, field) {
  const v = finalizedRun(this).results[i].locations[0].physicalLocation.region[field];
  assert.ok(typeof v === 'number' && v > 0, `region.${field}=${v}`);
});

Then('the finalized SARIF result {int} region snippet text equals {string}', function (i, expected) {
  assert.equal(
    finalizedRun(this).results[i].locations[0].physicalLocation.region.snippet.text,
    expected,
  );
});

Then('the finalized SARIF result {int} has a contextRegion', function (i) {
  assert.ok(finalizedRun(this).results[i].locations[0].physicalLocation.contextRegion);
});

Then('the contextRegion snippet text contains {string}', function (fragment) {
  const text =
    finalizedRun(this).results[0].locations[0].physicalLocation.contextRegion.snippet.text;
  assert.ok(text.includes(fragment), `'${text}' does not contain '${fragment}'`);
});

Then('the finalized SARIF artifact for {string} carries a {string} hash', function (relPath, alg) {
  const a = finalizedRun(this).artifacts.find((x) => x.location.uri === relPath);
  assert.ok(a && a.hashes && typeof a.hashes[alg] === 'string' && a.hashes[alg].length > 0);
});

Then(
  'the finalized SARIF originalUriBaseIds {string} uri starts with {string}',
  function (baseId, prefix) {
    assert.ok(finalizedRun(this).originalUriBaseIds[baseId].uri.startsWith(prefix));
  },
);

Then(
  'the finalized SARIF originalUriBaseIds {string} uri contains {string}',
  function (baseId, fragment) {
    assert.ok(finalizedRun(this).originalUriBaseIds[baseId].uri.includes(fragment));
  },
);

Then(
  'the finalized SARIF result {int} artifactLocation uriBaseId equals {string}',
  function (i, expected) {
    assert.equal(
      finalizedRun(this).results[i].locations[0].physicalLocation.artifactLocation.uriBaseId,
      expected,
    );
  },
);

Then(
  'the finalized SARIF result {int} artifactLocation uri equals {string}',
  function (i, expected) {
    assert.equal(
      finalizedRun(this).results[i].locations[0].physicalLocation.artifactLocation.uri,
      expected,
    );
  },
);

Then('the finalized SARIF contains no {string} anywhere', function (fragment) {
  const text = readFileSync(this.output, 'utf8');
  assert.ok(!text.includes(fragment), `output SARIF still contains '${fragment}'`);
});

Then('the finalized SARIF rule {string} has a non-empty {string}', function (id, field) {
  const rule = finalizedRun(this).tool.driver.rules.find((r) => r.id === id);
  assert.ok(rule && rule[field], `rule ${id}.${field} is empty`);
});

Then('the finalized SARIF rule {string} carries property {string}', function (id, prop) {
  const rule = finalizedRun(this).tool.driver.rules.find((r) => r.id === id);
  assert.ok(rule?.properties && prop in rule.properties);
});

Then(
  'the finalized SARIF rule {string} carries property {string} equal to {string}',
  function (id, prop, expected) {
    const rule = finalizedRun(this).tool.driver.rules.find((r) => r.id === id);
    assert.equal(rule?.properties?.[prop], expected);
  },
);

Then('the finalized SARIF rule {string} tags include {string}', function (id, tag) {
  const rule = finalizedRun(this).tool.driver.rules.find((r) => r.id === id);
  assert.ok(Array.isArray(rule?.properties?.tags) && rule.properties.tags.includes(tag));
});

Then(
  'the finalized SARIF rule {string} has helpUri starting with {string}',
  function (id, prefix) {
    const rule = finalizedRun(this).tool.driver.rules.find((r) => r.id === id);
    assert.ok(rule?.helpUri?.startsWith(prefix));
  },
);
