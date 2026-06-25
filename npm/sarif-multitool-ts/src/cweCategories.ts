// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * MITRE CWE *Category* recognition. A Category (e.g. `CWE-16` "Configuration")
 * is an organizational grouping of weaknesses, never a valid `result.ruleId`
 * mapping target; naming one in place of the specific Weakness under it is the
 * most common mis-mapping an AI producer makes.
 *
 * Categories do NOT appear in `CweTaxonomy.sarif` (which carries only
 * Weaknesses), so they cannot be recovered from the taxonomy's
 * `cwe/abstraction` field — they live in their own `CweCategories.json`.
 * Ported from src/Sarif/Taxonomies/CweCategories.cs (the same data, generated in
 * the same run as the taxonomy, so the two never drift).
 */

import { readFileSync } from 'node:fs';
import { assetPath } from './assets.js';

interface CategoriesDocument {
  version: string;
  comment?: string;
  categories: Record<string, string>;
}

/**
 * Parses the CWE number out of a canonical CWE id (`CWE-89`, any case, leading
 * zeros tolerated) or an AI ruleId carrying a sub-id (`CWE-89/kql-injection`).
 * The `CWE-` prefix is required: a bare number, the `NOVEL-` form, and anything
 * else yield `undefined`. Mirrors CweSecuritySeverity.TryGetCweNumber.
 */
function toCweNumber(cweId: string): number | undefined {
  if (typeof cweId !== 'string') return undefined;

  let token = cweId.trim();
  if (!/^CWE-/i.test(token)) return undefined;
  token = token.slice(4);

  const slash = token.indexOf('/');
  if (slash >= 0) token = token.slice(0, slash);

  return /^[0-9]+$/.test(token) ? Number.parseInt(token, 10) : undefined;
}

let cachedByNumber: Map<number, string> | undefined;

function categoriesByNumber(): Map<number, string> {
  if (!cachedByNumber) {
    cachedByNumber = new Map();
    for (const [id, name] of Object.entries(getCweCategories())) {
      const number = toCweNumber(id);
      if (number !== undefined) cachedByNumber.set(number, name);
    }
  }
  return cachedByNumber;
}

/**
 * Returns the full id → name map of every MITRE CWE Category, as shipped
 * (`{ "CWE-16": "Configuration", ... }`). Mirrors CweCategories.CategoryNamesByCwe.
 */
export function getCweCategories(): Record<string, string> {
  const doc = JSON.parse(
    readFileSync(assetPath('assets', 'CweCategories.json'), 'utf8'),
  ) as CategoriesDocument;
  return doc.categories;
}

/**
 * Determines whether a CWE identifier names a MITRE Category. Accepts any form
 * the SDK accepts: a canonical id (`CWE-16`, any case) or an AI ruleId carrying
 * a sub-id (`CWE-16/insecure-default`). Mirrors CweCategories.IsCategory.
 */
export function isCweCategory(cweId: string): boolean {
  const number = toCweNumber(cweId);
  return number !== undefined && categoriesByNumber().has(number);
}

/**
 * Looks up the MITRE Category name for a CWE identifier, or `undefined` when the
 * id does not resolve to a known Category. Mirrors CweCategories.TryGetCategoryName.
 */
export function getCweCategoryName(cweId: string): string | undefined {
  const number = toCweNumber(cweId);
  return number === undefined ? undefined : categoriesByNumber().get(number);
}
