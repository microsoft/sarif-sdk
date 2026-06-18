// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * CodeQL-style rolling-hash partial fingerprints (`primaryLocationLineHash`).
 *
 * The normative source for this algorithm is github/codeql-action's
 * fingerprints.ts (https://github.com/github/codeql-action, src/fingerprints.ts);
 * src/Sarif/HashUtilities.cs is itself a port of it, and src/Sarif/Numeric/Long.cs
 * exists only to re-emulate JS 64-bit `Long` arithmetic in the CLR. This module
 * returns to that source: it reproduces the same per-line `"<hex>:<count>"`
 * fingerprints byte-for-byte using native BigInt mod-2^64 arithmetic, so no
 * `Long` emulation (or npm `long` dependency) is needed.
 *
 * GitHub's raw code-scanning SARIF upload API does NOT backfill
 * partialFingerprints, so a GitHub-hosted producer must stamp this itself to
 * avoid duplicate alerts across scans. See InsertOptionalDataVisitor.cs.
 */

const TAB = 0x09;
const SPACE = 0x20;
const LF = 0x0a;
const CR = 0x0d;
const EOF = 65535;
const BLOCK_SIZE = 100;

// 64-bit two's-complement ring. BigInt `& MASK` reduces any (incl. negative)
// intermediate to its low 64 bits, exactly matching the CLR `Long` wraparound
// and `ToUnsigned()` the C# performs before `ToString(16)`.
const MASK = (1n << 64n) - 1n;
const MOD = 37n;

function computeFirstMod(): bigint {
  let firstMod = 1n;
  for (let i = 0; i < BLOCK_SIZE; i++) {
    firstMod = (firstMod * MOD) & MASK;
  }
  return firstMod;
}

/**
 * Computes the rolling-hash fingerprint for every line of `fileText`, keyed by
 * 1-based line number, with values of the form `"<hex>:<ordinal>"` where the
 * ordinal disambiguates lines whose hash collides (identical normalized
 * content). Mirrors HashUtilities.RollingHash.
 */
export function computeRollingHashes(fileText: string): Map<number, string> {
  const rollingHashes = new Map<number, string>();
  if (fileText == null) return rollingHashes;

  // A rolling view into the input.
  const window = new Array<number>(BLOCK_SIZE).fill(0);
  const lineNumbers = new Array<number>(BLOCK_SIZE).fill(-1);

  let hashRaw = 0n;
  const firstMod = computeFirstMod();

  let index = 0;
  let lineNumber = 0;
  let lineStart = true;
  let prevCR = false;

  const hashCounts = new Map<string, number>();

  const outputHash = (): void => {
    const hashValue = (hashRaw & MASK).toString(16);
    const count = (hashCounts.get(hashValue) ?? 0) + 1;
    hashCounts.set(hashValue, count);
    rollingHashes.set(lineNumbers[index], `${hashValue}:${count}`);
    lineNumbers[index] = -1;
  };

  const updateHash = (current: number): void => {
    const begin = window[index];
    window[index] = current;
    hashRaw = (MOD * hashRaw + BigInt(current) - firstMod * BigInt(begin)) & MASK;
    index = (index + 1) % BLOCK_SIZE;
  };

  const processCharacter = (current: number): void => {
    // Skip tabs, spaces, and line feeds that come directly after a carriage return.
    if (current === SPACE || current === TAB || (prevCR && current === LF)) {
      prevCR = false;
      return;
    }
    // Replace CR with LF. (\u2028 / \u2029 are intentionally not handled,
    // matching the reference implementation.)
    if (current === CR) {
      current = LF;
      prevCR = true;
    } else {
      prevCR = false;
    }
    if (lineNumbers[index] !== -1) {
      outputHash();
    }
    if (lineStart) {
      lineStart = false;
      lineNumber++;
      lineNumbers[index] = lineNumber;
    }
    if (current === LF) {
      lineStart = true;
    }
    updateHash(current);
  };

  // `charCodeAt` yields UTF-16 code units, matching the C# `char` iteration the
  // reference algorithm is defined over.
  for (let i = 0; i < fileText.length; i++) {
    processCharacter(fileText.charCodeAt(i));
  }

  processCharacter(EOF);

  // Flush the remaining lines.
  for (let i = 0; i < BLOCK_SIZE; i++) {
    if (lineNumbers[index] !== -1) {
      outputHash();
    }
    updateHash(0);
  }

  return rollingHashes;
}
