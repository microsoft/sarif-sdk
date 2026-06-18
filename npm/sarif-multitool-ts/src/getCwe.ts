// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `get-cwe`: returns the embedded MITRE CWE taxonomy in SARIF form, optionally
 * filtered by status, plus the curated security-severity table.
 *
 * Ported from src/Sarif.Multitool.Library/GetCwe/GetCweCommand.cs and
 * src/Sarif/Taxonomies/CweTaxonomy.cs (the load + status-filter subset).
 */

import { readFileSync } from 'node:fs';
import type { SarifLog, ReportingDescriptor } from '@microsoft/sarif';
import { assetPath } from './assets.js';

export type CweStatus = 'Stable' | 'Draft' | 'Incomplete' | 'Deprecated' | 'Obsolete';

/**
 * The default set of CWE statuses for read and enrichment operations:
 * Stable | Draft | Incomplete. See CweTaxonomy.DefaultStatuses for rationale
 * (notably: SSRF/CWE-918 is Incomplete; most household-name CWEs are Draft).
 */
export const DEFAULT_CWE_STATUSES: ReadonlySet<CweStatus> = new Set([
  'Stable',
  'Draft',
  'Incomplete',
]);

/**
 * Loads the embedded CWE taxonomy SARIF and filters its taxa to the given
 * statuses. Mirrors CweTaxonomy.Load.
 */
export function getCweTaxonomy(
  statuses: ReadonlySet<CweStatus> = DEFAULT_CWE_STATUSES,
): SarifLog {
  const log = JSON.parse(readFileSync(assetPath('assets', 'CweTaxonomy.sarif'), 'utf8')) as SarifLog;
  for (const run of log.runs ?? []) {
    for (const taxonomy of run.taxonomies ?? []) {
      if (!taxonomy.taxa) continue;
      taxonomy.taxa = taxonomy.taxa.filter((t: ReportingDescriptor) => {
        const status = (t.properties as Record<string, unknown> | undefined)?.['cwe/status'];
        return typeof status !== 'string' || statuses.has(status as CweStatus);
      });
    }
  }
  return log;
}

/** Returns the curated CWE → security-severity table (CweSecuritySeverity.json). */
export function getCweSecuritySeverityTable(): Record<string, number> {
  return JSON.parse(
    readFileSync(assetPath('assets', 'CweSecuritySeverity.json'), 'utf8'),
  ) as Record<string, number>;
}
