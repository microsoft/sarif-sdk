// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Direct unit coverage for insertOptionalData.ts: the uriBaseId-chain
 * resolution in tryReconstructAbsoluteUri and the end-to-end enrichment
 * (snippet population, sha-256 hashing, and artifacts[] back-references).
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';
import { tryReconstructAbsoluteUri, insertOptionalData, computeRollingHashes } from '../dist/index.js';

test('tryReconstructAbsoluteUri returns an already-absolute uri unchanged', () => {
  assert.equal(
    tryReconstructAbsoluteUri({ uri: 'https://example.com/a' }, undefined),
    'https://example.com/a',
  );
});

test('tryReconstructAbsoluteUri composes a single uriBaseId', () => {
  assert.equal(
    tryReconstructAbsoluteUri(
      { uri: 'src/a.ts', uriBaseId: 'SRCROOT' },
      { SRCROOT: { uri: 'file:///repo/' } },
    ),
    'file:///repo/src/a.ts',
  );
});

test('tryReconstructAbsoluteUri walks a nested uriBaseId chain', () => {
  assert.equal(
    tryReconstructAbsoluteUri(
      { uri: 'a.ts', uriBaseId: 'SRC' },
      { SRC: { uri: 'sub/', uriBaseId: 'REPO' }, REPO: { uri: 'file:///r/' } },
    ),
    'file:///r/sub/a.ts',
  );
});

test('tryReconstructAbsoluteUri returns undefined when a base is missing', () => {
  assert.equal(tryReconstructAbsoluteUri({ uri: 'a.ts', uriBaseId: 'NOPE' }, {}), undefined);
});

test('insertOptionalData populates snippet, hashes, and artifacts[]', () => {
  const dir = mkdtempSync(join(tmpdir(), 'sarif-iod-'));
  try {
    writeFileSync(join(dir, 'a.ts'), 'alpha\nbeta\ngamma\n', 'utf8');
    const srcRoot = pathToFileURL(dir).toString().replace(/\/?$/, '/');
    const run = {
      tool: { driver: { name: 't' } },
      originalUriBaseIds: { SRCROOT: { uri: srcRoot } },
      results: [
        {
          ruleId: 'CWE-79/x',
          locations: [
            {
              physicalLocation: {
                artifactLocation: { uri: 'a.ts', uriBaseId: 'SRCROOT' },
                region: { startLine: 2 },
              },
            },
          ],
        },
      ],
    };

    insertOptionalData(run, {
      hashes: true,
      regionSnippets: true,
      contextRegionSnippets: true,
      comprehensiveRegionProperties: true,
    });

    const physicalLocation = run.results[0].locations[0].physicalLocation;
    assert.equal(physicalLocation.region.snippet.text, 'beta');
    assert.equal(typeof physicalLocation.artifactLocation.index, 'number');

    const artifact = (run.artifacts ?? []).find((a) => a.location?.uri === 'a.ts');
    assert.ok(artifact, 'expected an artifacts[] entry for a.ts');
    assert.match(artifact.hashes['sha-256'], /^[0-9A-F]{64}$/);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test('insertOptionalData stamps primaryLocationLineHash when rollingHashFingerprints is set', () => {
  const dir = mkdtempSync(join(tmpdir(), 'sarif-iod-'));
  try {
    const fileText = 'const a = 1;\nconst b = unsafe(input);\nconst c = 3;\n';
    writeFileSync(join(dir, 'app.ts'), fileText, 'utf8');
    const srcRoot = pathToFileURL(dir).toString().replace(/\/?$/, '/');
    const run = {
      tool: { driver: { name: 't' } },
      originalUriBaseIds: { SRCROOT: { uri: srcRoot } },
      results: [
        {
          ruleId: 'CWE-79/x',
          locations: [
            {
              physicalLocation: {
                artifactLocation: { uri: 'app.ts', uriBaseId: 'SRCROOT' },
                region: { startLine: 2 },
              },
            },
          ],
        },
      ],
    };

    insertOptionalData(run, {
      hashes: false,
      regionSnippets: false,
      contextRegionSnippets: false,
      comprehensiveRegionProperties: false,
      rollingHashFingerprints: true,
    });

    const expected = computeRollingHashes(fileText).get(2);
    assert.ok(expected, 'expected a rolling hash for line 2');
    assert.equal(run.results[0].partialFingerprints.primaryLocationLineHash, expected);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test('insertOptionalData anchors the line hash to line 1 when the region has no startLine', () => {
  const dir = mkdtempSync(join(tmpdir(), 'sarif-iod-'));
  try {
    const fileText = 'alpha\nbeta\ngamma\n';
    writeFileSync(join(dir, 'a.ts'), fileText, 'utf8');
    const srcRoot = pathToFileURL(dir).toString().replace(/\/?$/, '/');
    const run = {
      tool: { driver: { name: 't' } },
      originalUriBaseIds: { SRCROOT: { uri: srcRoot } },
      results: [
        {
          ruleId: 'CWE-79/x',
          locations: [
            {
              physicalLocation: {
                artifactLocation: { uri: 'a.ts', uriBaseId: 'SRCROOT' },
              },
            },
          ],
        },
      ],
    };

    insertOptionalData(run, {
      hashes: false,
      regionSnippets: false,
      contextRegionSnippets: false,
      comprehensiveRegionProperties: false,
      rollingHashFingerprints: true,
    });

    assert.equal(
      run.results[0].partialFingerprints.primaryLocationLineHash,
      computeRollingHashes(fileText).get(1),
    );
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test('insertOptionalData preserves an existing primaryLocationLineHash', () => {
  const dir = mkdtempSync(join(tmpdir(), 'sarif-iod-'));
  try {
    writeFileSync(join(dir, 'a.ts'), 'alpha\nbeta\ngamma\n', 'utf8');
    const srcRoot = pathToFileURL(dir).toString().replace(/\/?$/, '/');
    const run = {
      tool: { driver: { name: 't' } },
      originalUriBaseIds: { SRCROOT: { uri: srcRoot } },
      results: [
        {
          ruleId: 'CWE-79/x',
          partialFingerprints: { primaryLocationLineHash: 'preexisting:1' },
          locations: [
            {
              physicalLocation: {
                artifactLocation: { uri: 'a.ts', uriBaseId: 'SRCROOT' },
                region: { startLine: 2 },
              },
            },
          ],
        },
      ],
    };

    insertOptionalData(run, {
      hashes: false,
      regionSnippets: false,
      contextRegionSnippets: false,
      comprehensiveRegionProperties: false,
      rollingHashFingerprints: true,
    });

    assert.equal(run.results[0].partialFingerprints.primaryLocationLineHash, 'preexisting:1');
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test('insertOptionalData does not stamp a line hash when rollingHashFingerprints is unset', () => {
  const dir = mkdtempSync(join(tmpdir(), 'sarif-iod-'));
  try {
    writeFileSync(join(dir, 'a.ts'), 'alpha\nbeta\ngamma\n', 'utf8');
    const srcRoot = pathToFileURL(dir).toString().replace(/\/?$/, '/');
    const run = {
      tool: { driver: { name: 't' } },
      originalUriBaseIds: { SRCROOT: { uri: srcRoot } },
      results: [
        {
          ruleId: 'CWE-79/x',
          locations: [
            {
              physicalLocation: {
                artifactLocation: { uri: 'a.ts', uriBaseId: 'SRCROOT' },
                region: { startLine: 2 },
              },
            },
          ],
        },
      ],
    };

    insertOptionalData(run, {
      hashes: true,
      regionSnippets: true,
      contextRegionSnippets: true,
      comprehensiveRegionProperties: true,
    });

    assert.equal(run.results[0].partialFingerprints, undefined);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});
