// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Direct unit coverage for the region/snippet primitives in regions.ts.
 * These algorithmic edges (newline detection, the offset binary search, and
 * the authored-coordinate reconcile) were previously exercised only
 * transitively through the multitool Gherkin scenarios.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';
import { createHash } from 'node:crypto';
import { NewLineIndex, FileRegionsCache } from '../dist/index.js';

// A buffer that mixes LF, CRLF, and a bare CR so the terminator handling is
// exercised in one fixture:
//   line 1 "abc"  (offset 0, LF at 3)
//   line 2 "def"  (offset 4, CRLF at 7-8)
//   line 3 "ghi"  (offset 9, bare CR at 12)
//   line 4 "jkl"  (offset 13, no terminator)
const MIXED = 'abc\ndef\r\nghi\rjkl';

function withTempFile(content, fn) {
  const dir = mkdtempSync(join(tmpdir(), 'sarif-regions-'));
  try {
    const p = join(dir, 'src.txt');
    writeFileSync(p, content, 'utf8');
    return fn(pathToFileURL(p).toString());
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
}

test('NewLineIndex counts lines across LF, CRLF, and bare CR', () => {
  assert.equal(new NewLineIndex(MIXED).maximumLineNumber, 4);
});

test('NewLineIndex.startOffsetForLine returns each line start', () => {
  const idx = new NewLineIndex(MIXED);
  assert.equal(idx.startOffsetForLine(1), 0);
  assert.equal(idx.startOffsetForLine(2), 4);
  assert.equal(idx.startOffsetForLine(3), 9);
  assert.equal(idx.startOffsetForLine(4), 13);
  // maximumLineNumber + 1 resolves to end-of-text.
  assert.equal(idx.startOffsetForLine(5), MIXED.length);
});

test('NewLineIndex.startOffsetForLine rejects out-of-range lines', () => {
  const idx = new NewLineIndex(MIXED);
  assert.throws(() => idx.startOffsetForLine(0), RangeError);
  assert.throws(() => idx.startOffsetForLine(6), RangeError);
});

test('NewLineIndex.offsetInfo maps offsets to 1-based line/column', () => {
  const idx = new NewLineIndex(MIXED);
  assert.deepEqual(idx.offsetInfo(0), { lineNumber: 1, columnNumber: 1 });
  assert.deepEqual(idx.offsetInfo(4), { lineNumber: 2, columnNumber: 1 });
  assert.deepEqual(idx.offsetInfo(6), { lineNumber: 2, columnNumber: 3 });
  assert.deepEqual(idx.offsetInfo(9), { lineNumber: 3, columnNumber: 1 });
  assert.deepEqual(idx.offsetInfo(15), { lineNumber: 4, columnNumber: 3 });
});

test('NewLineIndex.offsetInfo rejects a negative offset', () => {
  assert.throws(() => new NewLineIndex(MIXED).offsetInfo(-1), RangeError);
});

const THREE_LINES = 'line1\nline2\nline3\n';

test('populateTextRegionProperties fills the char span and snippet from a start line', () => {
  withTempFile(THREE_LINES, (uri) => {
    const region = new FileRegionsCache().populateTextRegionProperties({ startLine: 2 }, uri, true);
    assert.equal(region.startColumn, 1);
    assert.equal(region.endLine, 2);
    assert.equal(region.endColumn, 6);
    assert.equal(region.charOffset, 6);
    assert.equal(region.charLength, 5);
    assert.equal(region.snippet?.text, 'line2');
  });
});

test('populateTextRegionProperties throws when an authored coordinate diverges from the source', () => {
  withTempFile(THREE_LINES, (uri) => {
    assert.throws(
      () =>
        new FileRegionsCache().populateTextRegionProperties(
          { startLine: 1, charOffset: 999 },
          uri,
          false,
        ),
      /computed from the source file/,
    );
  });
});

test('populateTextRegionProperties overwrites a divergent coordinate when asked', () => {
  withTempFile(THREE_LINES, (uri) => {
    const region = new FileRegionsCache().populateTextRegionProperties(
      { startLine: 1, charOffset: 999 },
      uri,
      true,
      true,
    );
    assert.equal(region.charOffset, 0);
  });
});

test('getSha256 returns uppercase hex matching node crypto', () => {
  const content = 'hash me\n';
  withTempFile(content, (uri) => {
    const expected = createHash('sha256').update(Buffer.from(content)).digest('hex').toUpperCase();
    assert.equal(new FileRegionsCache().getSha256(uri), expected);
  });
});

test('constructMultilineContextSnippet widens to the neighbouring lines', () => {
  withTempFile('a\nbb\nccc\ndddd\n', (uri) => {
    const ctx = new FileRegionsCache().constructMultilineContextSnippet(
      { startLine: 2, endLine: 2 },
      uri,
    );
    assert.ok(ctx);
    // line 2 of 4 → context spans lines 1..3.
    assert.equal(ctx.startLine, 1);
    assert.equal(ctx.endLine, 3);
  });
});
