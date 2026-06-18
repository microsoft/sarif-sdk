// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Resolves paths to bundled package assets relative to the compiled module,
 * so the same code works whether the package is consumed from dist/ or via a
 * file: dev link.
 */

import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join as joinPath } from 'node:path';

const PKG_ROOT = joinPath(dirname(fileURLToPath(import.meta.url)), '..');

/** Returns an absolute path under the package root: assetPath('schemas', 'x.json'). */
export function assetPath(...segments: string[]): string {
  return joinPath(PKG_ROOT, ...segments);
}

let cachedVersion: string | undefined;

/** Reads the package.json version (post-stamp at publish time). */
export function packageVersion(): string {
  if (cachedVersion) return cachedVersion;
  const pkg = JSON.parse(readFileSync(joinPath(PKG_ROOT, 'package.json'), 'utf8')) as {
    version: string;
  };
  cachedVersion = pkg.version;
  return cachedVersion;
}
