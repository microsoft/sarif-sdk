// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * SARIF serialization helpers + atomic file writer.
 *
 * Ported from src/Sarif/Emit/AtomicSarifWriter.cs and the Newtonsoft-parity
 * serialization conventions used throughout the .NET SDK
 * (NullValueHandling.Ignore, two-space indent).
 */

import { promises as fs, existsSync, mkdirSync } from 'node:fs';
import { dirname, basename, resolve as resolvePath, join as joinPath } from 'node:path';
import { randomUUID } from 'node:crypto';
import type { SarifLog } from './sarif.js';

/**
 * Atomically writes a file by staging to a sibling temp file in the same
 * directory and renaming over the destination. Mirrors AtomicSarifWriter.Write.
 */
export async function atomicWrite(destinationPath: string, content: string): Promise<void> {
  if (!destinationPath) throw new Error('Destination path must be supplied.');

  const full = resolvePath(destinationPath);
  const dir = dirname(full);
  if (!existsSync(dir)) mkdirSync(dir, { recursive: true });

  const stagingPath = joinPath(dir, `${basename(full)}.${randomUUID().replace(/-/g, '')}.tmp`);

  try {
    const handle = await fs.open(stagingPath, 'wx');
    try {
      await handle.writeFile(content, 'utf8');
      await handle.sync().catch(() => {});
    } finally {
      await handle.close();
    }
    await fs.rename(stagingPath, full);
  } catch (err) {
    await fs.unlink(stagingPath).catch(() => {});
    throw err;
  }
}

/**
 * Recursively strips keys whose value is `null` or `undefined`, matching
 * Newtonsoft `NullValueHandling.Ignore`. Arrays keep null entries (Newtonsoft
 * does too); only object properties are pruned.
 */
export function stripNulls<T>(value: T): T {
  if (Array.isArray(value)) {
    return value.map((v) => stripNulls(v)) as unknown as T;
  }
  if (value !== null && typeof value === 'object') {
    const out: Record<string, unknown> = {};
    for (const [k, v] of Object.entries(value as Record<string, unknown>)) {
      if (v === null || v === undefined) continue;
      out[k] = stripNulls(v);
    }
    return out as T;
  }
  return value;
}

/** Serializes a SarifLog with two-space indent and null-stripping. */
export function serializeSarifLog(log: SarifLog, prettyPrint = true): string {
  const cleaned = stripNulls(log);
  return prettyPrint
    ? JSON.stringify(cleaned, undefined, 2) + '\n'
    : JSON.stringify(cleaned) + '\n';
}
