// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Enforces the SARIF SDK AI-authoring convention for `result.ruleId`.
 * Ported from src/Sarif/Emit/AIRuleIdConvention.cs.
 *
 * Accepted forms:
 *   - `CWE-<number>/<sub-id>` where sub-id is lowercase-alphanumeric kebab-case
 *     e.g. `CWE-89/kql-injection-from-config`
 *   - `NOVEL-<sub-id>` (flat, no slash) for findings with no CWE mapping
 */

import type { Result } from './sarif.js';

const TAXONOMY_SUB_ID = /^CWE-[0-9]+\/[a-z0-9]+(-[a-z0-9]+)*$/;
const NOVEL_PREFIX = /^NOVEL-[a-z0-9]+(-[a-z0-9]+)*$/;
const CWE_NUMBER = /^CWE-([0-9]+)(\/|$)/;
const NOVEL_MARKER = 'NOVEL-';

export class AIRuleIdConventionError extends Error {
  readonly offenders: readonly string[];
  constructor(offenders: readonly string[]) {
    const list = offenders.map((o) => `'${o}'`).join(', ');
    super(
      `result.ruleId must follow the AI ruleId convention: 'CWE-<number>/<lowercase-kebab-sub-id>' or 'NOVEL-<lowercase-kebab-sub-id>'. Offending value(s): ${list}.`,
    );
    this.name = 'AIRuleIdConventionError';
    this.offenders = offenders;
  }
}

export const AIRuleIdConvention = {
  /**
   * Extracts the canonical `CWE-<number>` id (leading-zero-free) from a ruleId,
   * whether bare (`CWE-89`) or sub-id form (`CWE-89/kql-injection`). Returns
   * undefined for NOVEL- ids, null/empty, or anything that does not start
   * with a CWE token.
   */
  tryGetCweId(ruleId: string | undefined): string | undefined {
    if (!ruleId) return undefined;
    const m = CWE_NUMBER.exec(ruleId);
    if (!m) return undefined;
    const n = Number.parseInt(m[1], 10);
    if (!Number.isFinite(n)) return undefined;
    return `CWE-${n}`;
  },

  isNovel(ruleId: string | undefined): boolean {
    return !!ruleId && ruleId.startsWith(NOVEL_MARKER);
  },

  isAcceptable(ruleId: string | undefined): boolean {
    if (!ruleId) return false;
    if (ruleId.startsWith(NOVEL_MARKER)) return NOVEL_PREFIX.test(ruleId);
    return TAXONOMY_SUB_ID.test(ruleId);
  },

  throwIfUnacceptable(ruleId: string | undefined): void {
    if (!this.isAcceptable(ruleId)) {
      throw new AIRuleIdConventionError([ruleId ?? '']);
    }
  },

  throwIfAnyUnacceptable(results: ReadonlyArray<Result> | undefined): void {
    if (!results || results.length === 0) return;
    const offenders: string[] = [];
    for (const r of results) {
      if (!this.isAcceptable(r?.ruleId)) offenders.push(r?.ruleId ?? '');
    }
    if (offenders.length > 0) throw new AIRuleIdConventionError(offenders);
  },
} as const;
