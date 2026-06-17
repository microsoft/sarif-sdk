// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Memory smoke: 1000× addResults in-process must not grow RSS unboundedly.
 * Run with `node --expose-gc test/memory-smoke.js`.
 *
 * This is the regression guard for the original complaint — node consumers
 * paying CLR working-set per emit-verb call. In-process there is no
 * subprocess and the per-call allocation is O(payload).
 */

import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { emitRun, emitResults } from '../dist/index.js';

if (typeof global.gc !== 'function') {
  console.error('Run with --expose-gc');
  process.exit(2);
}

const ITERATIONS = 1000;
const RSS_GROWTH_LIMIT_MB = 25; // generous; real growth should be ~0

const tmp = mkdtempSync(join(tmpdir(), 'sarif-mem-'));
const out = join(tmp, 'mem.sarif');

await emitRun({
  output: out,
  run: { tool: { driver: { name: 'mem-smoke' } } },
  env: () => undefined,
});

global.gc();
const rss0 = process.memoryUsage().rss;

const result = {
  ruleId: 'CWE-79/dom-xss',
  message: { text: 'x' },
  locations: [
    { physicalLocation: { artifactLocation: { uri: 'a.ts', uriBaseId: 'SRCROOT' }, region: { startLine: 1 } } },
  ],
};

for (let i = 0; i < ITERATIONS; i++) {
  // eslint-disable-next-line no-await-in-loop
  await emitResults({ output: out, results: result });
}

global.gc();
const rss1 = process.memoryUsage().rss;
const growthMb = (rss1 - rss0) / (1024 * 1024);

rmSync(tmp, { recursive: true, force: true });

console.log(
  `${ITERATIONS} addResults calls: RSS ${Math.round(rss0 / 1024 / 1024)}MB → ${Math.round(rss1 / 1024 / 1024)}MB (Δ ${growthMb.toFixed(1)}MB)`,
);

if (growthMb > RSS_GROWTH_LIMIT_MB) {
  console.error(`RSS grew ${growthMb.toFixed(1)}MB > ${RSS_GROWTH_LIMIT_MB}MB limit`);
  process.exit(1);
}
console.log('OK');
