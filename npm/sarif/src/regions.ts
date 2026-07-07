// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * NewLineIndex + FileRegionsCache: line/column ↔ char-offset resolution and
 * snippet / context-region extraction.
 *
 * Ported from:
 *   src/Sarif/NewLineIndex.cs
 *   src/Sarif/FileRegionsCache.cs (text-region subset)
 */

import { readFileSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import type { Region } from './sarif.js';
import { computeRollingHashes } from './rollingHash.js';

const NEWLINE_CHARS = new Set(['\n', '\r']);
const BIG_SNIPPET_LENGTH = 512;
const SMALL_SNIPPET_LENGTH = 128;

// ---------------------------------------------------------------------------
// NewLineIndex
// ---------------------------------------------------------------------------

/**
 * Index of newline start locations so a char offset can cheaply turn into a
 * 1-based line/column and vice versa. Mirrors NewLineIndex.cs.
 */
export class NewLineIndex {
  readonly text: string;
  // lineOffsetStarts[n] is the char offset at which (1-based) line n+1 begins.
  private readonly lineOffsetStarts: number[];

  constructor(text: string) {
    this.text = text;
    const starts: number[] = [0];
    for (let i = 0; i < text.length; i++) {
      const c = text[i];
      // Detect \r, \n, but NOT the \r in \r\n (the following \n handles it).
      if (NEWLINE_CHARS.has(c)) {
        if (c !== '\r' || i + 1 >= text.length || text[i + 1] !== '\n') {
          starts.push(i + 1);
        }
      }
    }
    this.lineOffsetStarts = starts;
  }

  get maximumLineNumber(): number {
    return this.lineOffsetStarts.length;
  }

  /** Char offset at which the given 1-based line begins. */
  startOffsetForLine(lineNumber: number): number {
    if (lineNumber <= 0 || lineNumber > this.maximumLineNumber + 1) {
      throw new RangeError(`Line number ${lineNumber} is out of range.`);
    }
    if (lineNumber === this.maximumLineNumber + 1) return this.text.length;
    return this.lineOffsetStarts[lineNumber - 1];
  }

  /** 1-based {line, column} for the given char offset. */
  offsetInfo(offset: number): { lineNumber: number; columnNumber: number } {
    if (offset < 0) throw new RangeError('Offset cannot be negative.');
    // Binary-search for the greatest start ≤ offset.
    let lo = 0,
      hi = this.lineOffsetStarts.length - 1,
      idx = 0;
    while (lo <= hi) {
      const mid = (lo + hi) >> 1;
      const v = this.lineOffsetStarts[mid];
      if (v === offset) {
        idx = mid;
        break;
      }
      if (v < offset) {
        idx = mid;
        lo = mid + 1;
      } else {
        hi = mid - 1;
      }
    }
    const startOffset = this.lineOffsetStarts[idx];
    return { lineNumber: idx + 1, columnNumber: offset - startOffset + 1 };
  }
}

// ---------------------------------------------------------------------------
// FileRegionsCache (text-region subset)
// ---------------------------------------------------------------------------

export class FileRegionsCache {
  private readonly textCache = new Map<string, string | null>();
  private readonly indexCache = new Map<string, NewLineIndex | null>();
  private readonly hashCache = new Map<string, string>();
  private readonly rollingHashCache = new Map<string, Map<number, string> | null>();

  private localPathFor(absoluteUri: string): string | undefined {
    try {
      const u = new URL(absoluteUri);
      if (u.protocol === 'file:') return fileURLToPath(u);
    } catch {
      /* not a URL */
    }
    return undefined;
  }

  /** Returns file text or null if unreadable. */
  getText(absoluteUri: string): string | null {
    if (this.textCache.has(absoluteUri)) return this.textCache.get(absoluteUri)!;
    const path = this.localPathFor(absoluteUri);
    let text: string | null = null;
    if (path) {
      try {
        text = readFileSync(path, 'utf8');
      } catch {
        text = null;
      }
    }
    this.textCache.set(absoluteUri, text);
    return text;
  }

  getNewLineIndex(absoluteUri: string): NewLineIndex | null {
    if (this.indexCache.has(absoluteUri)) return this.indexCache.get(absoluteUri)!;
    const text = this.getText(absoluteUri);
    const idx = text !== null ? new NewLineIndex(text) : null;
    this.indexCache.set(absoluteUri, idx);
    return idx;
  }

  /** sha-256 over the raw bytes of the file. */
  getSha256(absoluteUri: string): string | undefined {
    if (this.hashCache.has(absoluteUri)) return this.hashCache.get(absoluteUri);
    const path = this.localPathFor(absoluteUri);
    if (!path) return undefined;
    let buf: Buffer;
    try {
      buf = readFileSync(path);
    } catch {
      return undefined;
    }
    // .NET HashUtilities emits uppercase hex; match it for byte parity.
    const hash = createHash('sha256').update(buf).digest('hex').toUpperCase();
    this.hashCache.set(absoluteUri, hash);
    return hash;
  }

  /**
   * CodeQL-style per-line rolling-hash fingerprints for the file, keyed by
   * 1-based line number, or null if the file is unreadable. Mirrors
   * InsertOptionalDataVisitor.GetRollingHashes.
   */
  getRollingHashes(absoluteUri: string): Map<number, string> | null {
    if (this.rollingHashCache.has(absoluteUri)) return this.rollingHashCache.get(absoluteUri)!;
    const text = this.getText(absoluteUri);
    const hashes = text !== null ? computeRollingHashes(text) : null;
    this.rollingHashCache.set(absoluteUri, hashes);
    return hashes;
  }

  /**
   * Returns a copy of `inputRegion` with all text-related properties
   * populated (line/col ↔ charOffset/charLength) and optionally a snippet.
   * Mirrors FileRegionsCache.PopulateTextRegionProperties.
   *
   * `overwriteExistingData=false` (the default) throws on author/source
   * coordinate divergence; `true` replaces the authored value.
   */
  populateTextRegionProperties(
    inputRegion: Region,
    absoluteUri: string,
    populateSnippet: boolean,
    overwriteExistingData = false,
  ): Region {
    if (!inputRegion || isBinaryRegion(inputRegion)) return inputRegion;

    const idx = this.getNewLineIndex(absoluteUri);
    if (!idx) return inputRegion;
    const fileText = idx.text;

    const region: Region = { ...inputRegion };

    if (!region.startLine || region.startLine === 0) {
      populateFromCharOffsetAndLength(idx, region, overwriteExistingData);
    } else {
      populateFromStartAndEndProperties(idx, region, fileText, overwriteExistingData);
    }

    if (
      populateSnippet &&
      (region.charOffset ?? -1) >= 0 &&
      (region.charLength ?? -1) >= 0 &&
      (region.charOffset ?? 0) + (region.charLength ?? 0) <= fileText.length
    ) {
      region.snippet ??= {};
      const snippetText = fileText.substring(
        region.charOffset!,
        region.charOffset! + region.charLength!,
      );
      if (region.snippet.text === undefined) {
        region.snippet.text = snippetText;
      }
    }

    return region;
  }

  /**
   * Builds a context region that spans roughly one line above and one line
   * below the input region (or a ±128-char window if that overflows 512
   * chars). Mirrors FileRegionsCache.ConstructMultilineContextSnippet.
   */
  constructMultilineContextSnippet(inputRegion: Region, absoluteUri: string): Region | undefined {
    if (!inputRegion || isBinaryRegion(inputRegion)) return undefined;

    const idx = this.getNewLineIndex(absoluteUri);
    if (!idx) return undefined;
    const fileText = idx.text;

    const original = this.populateTextRegionProperties(inputRegion, absoluteUri, true);

    if ((original.charLength ?? 0) >= BIG_SNIPPET_LENGTH) {
      // A context region must be a proper superset of the region (SARIF §3.29.5,
      // enforced by SARIF1008). The region already meets the snippet cap, so no
      // larger in-cap snippet exists; omit the context region rather than return
      // one identical to the region.
      return undefined;
    }

    const maxLine = idx.maximumLineNumber;
    const startLine = (inputRegion.startLine ?? 1) === 1 ? 1 : (inputRegion.startLine ?? 1) - 1;
    const endLineIn = inputRegion.endLine ?? inputRegion.startLine ?? 1;
    const endLine = endLineIn === maxLine ? maxLine : endLineIn + 1;

    let context = this.populateTextRegionProperties({ startLine, endLine }, absoluteUri, true);

    if (
      (original.charLength ?? 0) <= (context.charLength ?? 0) &&
      (context.charLength ?? 0) <= BIG_SNIPPET_LENGTH
    ) {
      return context;
    }

    const charOffset = Math.max(0, (original.charOffset ?? 0) - SMALL_SNIPPET_LENGTH);
    const charLength = Math.min(BIG_SNIPPET_LENGTH, fileText.length - charOffset);
    context = this.populateTextRegionProperties({ charOffset, charLength }, absoluteUri, true);

    // The capped char-offset window does not always contain the region (a region
    // longer than the window's reach past its start runs off the end). Emit the
    // context region only when it is a genuine proper superset of the region;
    // otherwise omit it (SARIF §3.29.5 / SARIF1008).
    return isProperSupersetOf(context, original) ? context : undefined;
  }
}

// ---------------------------------------------------------------------------
// Region coordinate population (mirrors private helpers in FileRegionsCache)
// ---------------------------------------------------------------------------

function isBinaryRegion(r: Region): boolean {
  return r.byteOffset !== undefined && r.byteOffset >= 0;
}

// ---------------------------------------------------------------------------
// Proper-superset test (mirrors Region.IsProperSupersetOf)
// ---------------------------------------------------------------------------

/**
 * True when `sup` strictly contains `sub`: every position covered by `sub` lies
 * within `sup` and the two spans are not identical. Each populated coordinate
 * dimension (line/column, char offset) must independently hold. Mirrors
 * Region.IsProperSupersetOf.
 */
function isProperSupersetOf(sup: Region, sub: Region): boolean {
  if (
    isLineColumnBasedRegion(sup) &&
    isLineColumnBasedRegion(sub) &&
    !isLineColumnProperSupersetOf(sup, sub)
  ) {
    return false;
  }

  if (
    isOffsetBasedRegion(sup) &&
    isOffsetBasedRegion(sub) &&
    !isOffsetProperSupersetOf(sup, sub)
  ) {
    return false;
  }

  return true;
}

function isLineColumnBasedRegion(r: Region): boolean {
  return (r.startLine ?? 0) >= 1;
}

function isOffsetBasedRegion(r: Region): boolean {
  return (r.charOffset ?? -1) >= 0;
}

function isLineColumnProperSupersetOf(sup: Region, sub: Region): boolean {
  const supStartLine = sup.startLine ?? 0;
  const supEndLine = sup.endLine ?? supStartLine;
  const supStartColumn = sup.startColumn ?? 1;
  const supEndColumn = sup.endColumn ?? Number.MAX_SAFE_INTEGER;
  const subStartLine = sub.startLine ?? 0;
  const subEndLine = sub.endLine ?? subStartLine;
  const subStartColumn = sub.startColumn ?? 1;
  const subEndColumn = sub.endColumn ?? Number.MAX_SAFE_INTEGER;

  if (supStartLine > subStartLine || supEndLine < subEndLine) return false;
  if (supStartLine === subStartLine && supStartColumn > subStartColumn) return false;
  if (supEndLine === subEndLine && supEndColumn < subEndColumn) return false;

  // Identical span is not a proper superset.
  return !(
    supStartLine === subStartLine &&
    supEndLine === subEndLine &&
    supStartColumn === subStartColumn &&
    supEndColumn === subEndColumn
  );
}

function isOffsetProperSupersetOf(sup: Region, sub: Region): boolean {
  const supOffset = sup.charOffset ?? 0;
  const supLength = sup.charLength ?? 0;
  const subOffset = sub.charOffset ?? 0;
  const subLength = sub.charLength ?? 0;

  if (supOffset > subOffset) return false;
  if (supOffset + supLength < subOffset + subLength) return false;

  // Same start and no greater length is not a proper superset.
  return !(supOffset === subOffset && supLength <= subLength);
}

function reconcile(
  overwrite: boolean,
  propertyName: string,
  computed: number,
  authored: number,
): number {
  if (authored === computed) return authored;
  if (overwrite) return computed;
  throw new Error(
    `The input region specifies '${propertyName}' = ${authored}, but the value computed from the source file is ${computed}. Authored region coordinates must exactly match the source text; omit a coordinate to have the SDK compute it, or set overwriteExistingData to recompute (overwrite) it.`,
  );
}

function populateFromCharOffsetAndLength(
  idx: NewLineIndex,
  region: Region,
  overwrite: boolean,
): void {
  const charOffset = region.charOffset ?? 0;
  const charLength = region.charLength ?? 0;

  const start = idx.offsetInfo(charOffset);
  const end = idx.offsetInfo(charOffset + charLength);

  if (!region.startLine) region.startLine = start.lineNumber;
  if (!region.startColumn) region.startColumn = start.columnNumber;
  if (!region.endLine) region.endLine = end.lineNumber;
  if (!region.endColumn) region.endColumn = end.columnNumber;

  region.startLine = reconcile(overwrite, 'startLine', start.lineNumber, region.startLine);
  region.startColumn = reconcile(overwrite, 'startColumn', start.columnNumber, region.startColumn);
  region.endLine = reconcile(overwrite, 'endLine', end.lineNumber, region.endLine);
  region.endColumn = reconcile(overwrite, 'endColumn', end.columnNumber, region.endColumn);
}

function populateFromStartAndEndProperties(
  idx: NewLineIndex,
  region: Region,
  fileText: string,
  overwrite: boolean,
): void {
  // Order is significant; each step relies on coordinates populated earlier.
  region.endLine = region.endLine && region.endLine !== 0 ? region.endLine : region.startLine;
  region.startColumn = region.startColumn && region.startColumn !== 0 ? region.startColumn : 1;

  if (!region.endColumn || region.endColumn === 0) {
    // No explicit end column: scan to the line terminator.
    const lineStart = idx.startOffsetForLine(region.endLine!);
    let p = lineStart;
    while (p < fileText.length && !NEWLINE_CHARS.has(fileText[p])) p++;
    region.endColumn = p - lineStart + 1;
  }

  // charOffset
  const startLineOffset = idx.startOffsetForLine(region.startLine!);
  const computedOffset = startLineOffset + region.startColumn! - 1;
  if (region.charOffset === undefined || region.charOffset === 0 || region.charOffset === -1) {
    region.charOffset = computedOffset;
  }
  region.charOffset = reconcile(overwrite, 'charOffset', computedOffset, region.charOffset);

  // charLength
  const endLineOffset = idx.startOffsetForLine(region.endLine!);
  const computedLength = endLineOffset - region.charOffset + region.endColumn! - 1;
  if (region.charLength === undefined || region.charLength === 0) {
    region.charLength = computedLength;
  }
  region.charLength = reconcile(overwrite, 'charLength', computedLength, region.charLength);

  if (region.charOffset + region.charLength > fileText.length && !overwrite) {
    throw new Error(
      `The input region's character span (charOffset ${region.charOffset} + charLength ${region.charLength} = ${region.charOffset + region.charLength}) extends beyond the source file length (${fileText.length}). Authored region coordinates must lie within the source text; omit them to have the SDK compute the span, or set overwriteExistingData to recompute it.`,
    );
  }
}
