// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * CWE taxonomy enrichment + curated security-severity priors.
 *
 * Ported from:
 *   src/Sarif/Taxonomies/CweTaxonomyEnricher.cs
 *   src/Sarif/Taxonomies/CweSecuritySeverity.cs (lookup subset)
 *   EmitFinalizeCommand.ApplyAISecuritySeverity / ApplyGitHubCweTags /
 *   CollapseResultRuleSubIds
 *
 * The taxonomy SARIF and the curated severity table are shipped as package
 * assets (../assets/CweTaxonomy.sarif, ../assets/CweSecuritySeverity.json)
 * copied verbatim from src/Sarif/Taxonomies/ at build time.
 */

import { readFileSync } from 'node:fs';
import type { Run, ReportingDescriptor, MultiformatMessageString } from '@microsoft/sarif';
import { assetPath } from './assets.js';

const SECURITY_SEVERITY_PROP = 'security-severity';
const TAGS_PROP = 'tags';
const SECURITY_TAG = 'security';
const CWE_TAG_PREFIX = 'external/cwe/cwe-';
const NOVEL_PREFIX = 'NOVEL-';
const DEFAULT_SECURITY_SEVERITY = 5.0;
const CWE_HELP_URI = (n: number) => `https://cwe.mitre.org/data/definitions/${n}.html`;
const CWE_ID_PATTERN = /^\s*[Cc][Ww][Ee]-(\d+)\s*$/;

// Default statuses: Stable | Draft | Incomplete (excludes Deprecated). See
// CweTaxonomy.DefaultStatuses for rationale.
const DEFAULT_STATUSES = new Set(['Stable', 'Draft', 'Incomplete']);

// ---------------------------------------------------------------------------
// Curated security-severity table
// ---------------------------------------------------------------------------

let severityByCwe: Map<number, number> | undefined;

function loadSeverityTable(): Map<number, number> {
  if (severityByCwe) return severityByCwe;
  const raw = JSON.parse(
    readFileSync(assetPath('assets', 'CweSecuritySeverity.json'), 'utf8'),
  ) as Record<string, number>;
  const m = new Map<number, number>();
  for (const [k, v] of Object.entries(raw)) {
    const num = tryGetCweNumber(k);
    if (num !== undefined) m.set(num, v);
  }
  severityByCwe = m;
  return m;
}

/** Extracts the CWE number from `CWE-<n>`, `CWE-<n>/<sub>`, or bare `<n>`. */
export function tryGetCweNumber(id: string | undefined): number | undefined {
  if (!id) return undefined;
  const bare = /^\d+$/.exec(id.trim());
  if (bare) return Number.parseInt(bare[0], 10);
  const m = /^CWE-(\d+)(\/|$)/i.exec(id.trim());
  return m ? Number.parseInt(m[1], 10) : undefined;
}

export function tryGetSecuritySeverity(cweNumber: number): number | undefined {
  return loadSeverityTable().get(cweNumber);
}

/** Formats a security-severity value for the property bag (one decimal). */
export function formatSecuritySeverity(value: number): string {
  return value.toFixed(1);
}

// ---------------------------------------------------------------------------
// Taxonomy lookup (lazily-loaded, status-filtered)
// ---------------------------------------------------------------------------

let taxaById: Map<string, ReportingDescriptor> | undefined;

function loadTaxa(): Map<string, ReportingDescriptor> {
  if (taxaById) return taxaById;
  const log = JSON.parse(readFileSync(assetPath('assets', 'CweTaxonomy.sarif'), 'utf8')) as {
    runs?: Array<{ taxonomies?: Array<{ taxa?: ReportingDescriptor[] }> }>;
  };
  const m = new Map<string, ReportingDescriptor>();
  for (const run of log.runs ?? []) {
    for (const taxonomy of run.taxonomies ?? []) {
      for (const t of taxonomy.taxa ?? []) {
        if (!t?.id) continue;
        const status = (t.properties as Record<string, unknown> | undefined)?.['cwe/status'];
        if (typeof status === 'string' && !DEFAULT_STATUSES.has(status)) continue;
        m.set(t.id, t);
      }
    }
  }
  taxaById = m;
  return m;
}

// Unfiltered (every status, including Deprecated) view of the embedded taxonomy, keyed by CWE
// number. Ported from CweTaxonomy.WeaknessNumbers / CweTaxonomy.NameByWeaknessNumber: membership
// and name resolution for isKnownWeakness/tryGetName are evaluated across every maturity status —
// a deprecated Weakness is still a Weakness with a name.
let allTaxaByCweNumber: Map<number, ReportingDescriptor> | undefined;

function loadAllTaxaByCweNumber(): Map<number, ReportingDescriptor> {
  if (allTaxaByCweNumber) return allTaxaByCweNumber;
  const log = JSON.parse(readFileSync(assetPath('assets', 'CweTaxonomy.sarif'), 'utf8')) as {
    runs?: Array<{ taxonomies?: Array<{ taxa?: ReportingDescriptor[] }> }>;
  };
  const m = new Map<number, ReportingDescriptor>();
  for (const run of log.runs ?? []) {
    for (const taxonomy of run.taxonomies ?? []) {
      for (const t of taxonomy.taxa ?? []) {
        const num = tryGetCweNumber(t?.id);
        if (num !== undefined) m.set(num, t);
      }
    }
  }
  allTaxaByCweNumber = m;
  return m;
}

/**
 * Determines whether a CWE identifier names a known MITRE Weakness — the only abstraction that
 * is a valid `result.ruleId` mapping target. Returns `false` for a Category, a View, a withdrawn
 * id, a typo, or any non-CWE token. Ported from `CweTaxonomy.IsKnownWeakness`.
 */
export function isKnownWeakness(cweId: string | undefined): boolean {
  const num = tryGetCweNumber(cweId);
  return num !== undefined && loadAllTaxaByCweNumber().has(num);
}

/**
 * Resolves the canonical MITRE `name` for a CWE identifier that names a known Weakness, across
 * every maturity status. Ported from `CweTaxonomy.TryGetName`.
 */
export function tryGetName(cweId: string | undefined): string | undefined {
  const num = tryGetCweNumber(cweId);
  if (num === undefined) return undefined;
  const name = loadAllTaxaByCweNumber().get(num)?.name;
  return name || undefined;
}

function isEmptyMsg(m: MultiformatMessageString | undefined): boolean {
  return !m || (!m.text && !m.markdown);
}

function deriveShortDescription(full: string): string {
  // First sentence (up to first period followed by space/end), trimmed.
  const m = /^(.*?\.)(\s|$)/.exec(full);
  return (m ? m[1] : full).trim();
}

// ---------------------------------------------------------------------------
// Enrichment passes (mutate run in place; never overwrite producer fields)
// ---------------------------------------------------------------------------

/** CweTaxonomyEnricher.Enrich port. */
export function enrichRunWithCweTaxonomy(run: Run): number {
  const taxa = loadTaxa();
  let modified = 0;

  const components = [run.tool?.driver, ...(run.tool?.extensions ?? [])];
  for (const comp of components) {
    for (const rule of comp?.rules ?? []) {
      if (tryEnrichDescriptor(rule, taxa)) modified++;
    }
  }
  return modified;
}

function tryEnrichDescriptor(
  rule: ReportingDescriptor,
  taxa: Map<string, ReportingDescriptor>,
): boolean {
  if (!rule?.id) return false;
  const m = CWE_ID_PATTERN.exec(rule.id);
  if (!m) return false;
  const canonical = `CWE-${m[1]}`;
  const taxon = taxa.get(canonical);
  if (!taxon) return false;

  let changed = false;

  if (!rule.name && taxon.name) {
    rule.name = taxon.name;
    changed = true;
  }
  if (isEmptyMsg(rule.shortDescription) && !isEmptyMsg(taxon.shortDescription)) {
    rule.shortDescription = { ...taxon.shortDescription! };
    changed = true;
  }
  if (isEmptyMsg(rule.fullDescription) && !isEmptyMsg(taxon.fullDescription)) {
    rule.fullDescription = { ...taxon.fullDescription! };
    changed = true;
  }
  if (isEmptyMsg(rule.shortDescription) && rule.fullDescription?.text) {
    rule.shortDescription = { text: deriveShortDescription(rule.fullDescription.text) };
    changed = true;
  }
  if (!rule.helpUri) {
    rule.helpUri = taxon.helpUri ?? CWE_HELP_URI(Number.parseInt(m[1], 10));
    changed = true;
  }
  if (isEmptyMsg(rule.help) && !isEmptyMsg(taxon.help)) {
    rule.help = { ...taxon.help! };
    changed = true;
  }
  const taxonSev = (taxon.properties as Record<string, unknown> | undefined)?.[
    SECURITY_SEVERITY_PROP
  ];
  if (
    !(rule.properties && SECURITY_SEVERITY_PROP in rule.properties) &&
    typeof taxonSev === 'string' &&
    taxonSev
  ) {
    rule.properties = { ...rule.properties, [SECURITY_SEVERITY_PROP]: taxonSev };
    changed = true;
  }

  return changed;
}

/**
 * Guarantees every descriptor for a CWE Weakness used as a rule id carries a non-empty `name` —
 * the property GHAzDO ingestion requires (GHAzDO2012, SARIF1001, SARIF2012) — even when full
 * taxonomy enrichment is suppressed (`noCweEnrichment`). `name` is resolved from the embedded
 * taxonomy; the canonical CWE id is used as a last-resort floor only if that lookup finds
 * nothing. A CWE that is not a known Weakness (a Category, View, withdrawn id, or typo) is left
 * nameless on purpose so it fails loudly at validation instead of being normalized into a
 * publishable-looking descriptor. Ported from `EmitFinalizeCommand.EnsureCweRuleDescriptorNames`.
 */
export function ensureCweRuleDescriptorNames(run: Run): number {
  const rules = run.tool?.driver?.rules;
  if (!rules || rules.length === 0) return 0;

  let modified = 0;
  for (const rule of rules) {
    if (!rule?.id) continue;
    if (rule.name) continue;
    if (!isKnownWeakness(rule.id)) continue;

    rule.name = tryGetName(rule.id) ?? rule.id;
    modified++;
  }
  return modified;
}

/** EmitFinalizeCommand.ApplyAISecuritySeverity port. */
export function applyAISecuritySeverity(run: Run): number {
  const rules = run.tool?.driver?.rules;
  if (!rules || rules.length === 0) return 0;

  let stamped = 0;
  for (const rule of rules) {
    if (!rule?.id) continue;
    if (rule.properties && SECURITY_SEVERITY_PROP in rule.properties) continue;

    const cweNumber = tryGetCweNumber(rule.id);
    const isCwe = CWE_ID_PATTERN.test(rule.id);
    const isNovel = rule.id.startsWith(NOVEL_PREFIX);
    if (!isCwe && !isNovel) continue;

    const curated = isCwe && cweNumber !== undefined ? tryGetSecuritySeverity(cweNumber) : undefined;
    const value = curated ?? DEFAULT_SECURITY_SEVERITY;

    rule.properties = {
      ...rule.properties,
      [SECURITY_SEVERITY_PROP]: formatSecuritySeverity(value),
    };
    stamped++;
  }
  return stamped;
}

/** EmitFinalizeCommand.ApplyGitHubCweTags port. */
export function applyGitHubCweTags(run: Run): number {
  const rules = run.tool?.driver?.rules;
  if (!rules || rules.length === 0) return 0;

  let stamped = 0;
  for (const rule of rules) {
    if (!rule?.id) continue;

    let cweTag: string | undefined;
    const cweNumber = tryGetCweNumber(rule.id);
    if (CWE_ID_PATTERN.test(rule.id) && cweNumber !== undefined) {
      cweTag = CWE_TAG_PREFIX + cweNumber;
    } else if (!rule.id.startsWith(NOVEL_PREFIX)) {
      continue;
    }

    const existing = (rule.properties?.[TAGS_PROP] as string[] | undefined) ?? [];
    const tags = [...existing];
    let changed = false;
    if (!tags.includes(SECURITY_TAG)) {
      tags.push(SECURITY_TAG);
      changed = true;
    }
    if (cweTag && !tags.includes(cweTag)) {
      tags.push(cweTag);
      changed = true;
    }
    if (changed) {
      rule.properties = { ...rule.properties, [TAGS_PROP]: tags };
      stamped++;
    }
  }
  return stamped;
}

/** EmitFinalizeCommand.CollapseResultRuleSubIds port. */
export function collapseResultRuleSubIds(run: Run): number {
  const results = run.results;
  const rules = run.tool?.driver?.rules;
  if (!results || results.length === 0 || !rules) return 0;

  let collapsed = 0;
  for (const result of results) {
    if (!result) continue;
    const descriptor =
      result.ruleIndex !== undefined && result.ruleIndex >= 0
        ? rules[result.ruleIndex]
        : undefined;
    const descriptorId = descriptor?.id;
    if (!descriptorId) continue;

    const prefix = descriptorId + '/';
    let changed = false;
    if (result.ruleId?.startsWith(prefix)) {
      result.ruleId = descriptorId;
      changed = true;
    }
    if (result.rule?.id?.startsWith(prefix)) {
      result.rule.id = descriptorId;
      changed = true;
    }
    if (changed) collapsed++;
  }
  return collapsed;
}
