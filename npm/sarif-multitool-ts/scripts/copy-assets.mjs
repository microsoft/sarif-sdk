// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Copies bundled assets from their C# source-of-truth locations into the
// package's assets/, schemas/, and skills/ directories. Runs as `prebuild`
// so the published package is self-contained without duplicating ~4 MB in git.

import { mkdirSync, copyFileSync, readdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const pkg = join(here, '..');
const repo = join(pkg, '..', '..');

function cp(src, dstDir, dstName) {
  mkdirSync(dstDir, { recursive: true });
  copyFileSync(src, join(dstDir, dstName ?? src.split(/[/\\]/).pop()));
}

// CWE taxonomy + curated severity table.
cp(join(repo, 'src/Sarif/Taxonomies/CweTaxonomy.sarif'), join(pkg, 'assets'));
cp(join(repo, 'src/Sarif/Taxonomies/CweSecuritySeverity.json'), join(pkg, 'assets'));

// CWE Category id -> name map. Categories are organizational groupings, never a
// valid result.ruleId mapping target; they do not appear in CweTaxonomy.sarif
// (Weaknesses only), so they ship as their own file for isCweCategory().
cp(join(repo, 'src/Sarif/Taxonomies/CweCategories.json'), join(pkg, 'assets'));

// Canonical SARIF 2.1.0 document schema, bundled for offline whole-log
// validation (emit-finalize --validate resolves the ai-sarif-log overlay's
// $ref to it). It lives under assets/, not schemas/: schemas/ is the
// verb-addressable set, and a non-verb schema there would fail the get-schema
// parity gate.
cp(
  join(repo, 'src/Sarif/Schemata/sarif-2.1.0.json'),
  join(pkg, 'assets'),
  'sarif-2.1.0.schema.json',
);

// Emit-verb input schemas.
const schemaSrc = join(repo, 'src/Sarif.Multitool.Library/GetSchema');
for (const f of readdirSync(schemaSrc)) {
  if (f.endsWith('.schema.json')) cp(join(schemaSrc, f), join(pkg, 'schemas'));
}

// Skills this package implements.
for (const s of ['emit-sarif', 'validate-sarif']) {
  cp(join(repo, 'skills', s, 'SKILL.md'), join(pkg, 'skills'), `${s}.SKILL.md`);
}

console.log('copy-assets: assets/, schemas/, skills/ staged from repo source-of-truth.');
