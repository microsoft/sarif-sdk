// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Subset of InsertOptionalDataVisitor needed by emit-finalize: walks every
 * physical location, populates region snippets / context regions / char
 * spans, and builds `run.artifacts[]` with sha-256 hashes.
 *
 * Ported from src/Sarif/Visitors/InsertOptionalDataVisitor.cs (the
 * Hashes | RegionSnippets | ContextRegionSnippets | ComprehensiveRegionProperties
 * subset; OverwriteExistingData is intentionally NOT set — see
 * EmitFinalizeCommand.cs for rationale).
 */

import type {
  Run,
  Result,
  Location,
  PhysicalLocation,
  ArtifactLocation,
  Artifact,
} from './sarif.js';
import { FileRegionsCache } from './regions.js';

export interface InsertOptionalDataFlags {
  hashes: boolean;
  regionSnippets: boolean;
  contextRegionSnippets: boolean;
  comprehensiveRegionProperties: boolean;
  /**
   * Stamp the GitHub rolling-hash `primaryLocationLineHash` partial
   * fingerprint on each result's primary location. GitHub-only — the raw
   * code-scanning upload API does not backfill it. Off by default; existing
   * values are preserved (OverwriteExistingData is not honored here).
   */
  rollingHashFingerprints?: boolean;
}

/** Partial-fingerprint key GitHub reads for cross-scan result de-duplication. */
export const PRIMARY_LOCATION_LINE_HASH = 'primaryLocationLineHash';

/**
 * Resolves an artifactLocation's `uri` + `uriBaseId` against the run's
 * `originalUriBaseIds` to an absolute URI string, or undefined if it cannot
 * be made absolute. Mirrors ArtifactLocation.TryReconstructAbsoluteUri.
 */
export function tryReconstructAbsoluteUri(
  loc: ArtifactLocation | undefined,
  originalUriBaseIds: Record<string, ArtifactLocation> | undefined,
): string | undefined {
  if (!loc?.uri) return undefined;
  // Already absolute?
  try {
    const u = new URL(loc.uri);
    if (u.protocol) return u.toString();
  } catch {
    /* relative */
  }
  // Walk the uriBaseId chain.
  let baseId = loc.uriBaseId;
  let composed = loc.uri;
  const seen = new Set<string>();
  while (baseId && originalUriBaseIds && !seen.has(baseId)) {
    seen.add(baseId);
    const base = originalUriBaseIds[baseId];
    if (!base?.uri) return undefined;
    composed = base.uri.replace(/\/?$/, '/') + composed;
    try {
      const u = new URL(composed);
      if (u.protocol) return u.toString();
    } catch {
      /* still relative */
    }
    baseId = base.uriBaseId;
  }
  return undefined;
}

/**
 * Walks the run mutating physical-location regions in place and populating
 * `run.artifacts[]`. Returns the same run (mutation, not a copy — every
 * caller-supplied key on every node is preserved per the pass-through
 * guarantee).
 */
export function insertOptionalData(run: Run, flags: InsertOptionalDataFlags): Run {
  const cache = new FileRegionsCache();
  const oub = run.originalUriBaseIds;

  // Scrape file references first so artifacts[] is populated before hashing.
  if (flags.hashes) {
    scrapeFileReferences(run);
  }

  for (const result of run.results ?? []) {
    visitResultLocations(result, (pl) => visitPhysicalLocation(pl, oub, cache, flags));
  }

  if (flags.hashes && run.artifacts) {
    for (const artifact of run.artifacts) {
      const abs = tryReconstructAbsoluteUri(artifact.location, oub);
      if (!abs) continue;
      if (!artifact.hashes) {
        const sha = cache.getSha256(abs);
        if (sha) artifact.hashes = { 'sha-256': sha };
      }
    }
  }

  if (flags.rollingHashFingerprints) {
    for (const result of run.results ?? []) {
      stampPrimaryLocationLineHash(result, oub, cache);
    }
  }

  return run;
}

/**
 * Stamps the rolling-hash `primaryLocationLineHash` on a result's primary
 * location, anchored to its startLine (a result with no region line pertains
 * to the whole file and is fingerprinted from line 1, matching
 * github/codeql-action fingerprints.ts). Mirrors the
 * RollingHashPartialFingerprints branch of InsertOptionalDataVisitor.cs.
 */
function stampPrimaryLocationLineHash(
  result: Result,
  oub: Record<string, ArtifactLocation> | undefined,
  cache: FileRegionsCache,
): void {
  if (result.partialFingerprints?.[PRIMARY_LOCATION_LINE_HASH] !== undefined) return;

  const physicalLocation = result.locations?.[0]?.physicalLocation;
  if (!physicalLocation?.artifactLocation) return;

  let startLine = physicalLocation.region?.startLine ?? 0;
  if (startLine <= 0) startLine = 1;

  const resolvedUri = tryReconstructAbsoluteUri(physicalLocation.artifactLocation, oub);
  if (!resolvedUri) return;

  const lineHash = cache.getRollingHashes(resolvedUri)?.get(startLine);
  if (lineHash === undefined) return;

  result.partialFingerprints ??= {};
  result.partialFingerprints[PRIMARY_LOCATION_LINE_HASH] = lineHash;
}

function visitResultLocations(result: Result, visit: (pl: PhysicalLocation) => void): void {
  const allLocations: (Location | undefined)[] = [
    ...(result.locations ?? []),
    ...(result.relatedLocations ?? []),
  ];
  for (const loc of allLocations) {
    if (loc?.physicalLocation) visit(loc.physicalLocation);
  }
}

function visitPhysicalLocation(
  node: PhysicalLocation,
  oub: Record<string, ArtifactLocation> | undefined,
  cache: FileRegionsCache,
  flags: InsertOptionalDataFlags,
): void {
  if (!node.region || node.region.byteOffset !== undefined) return;
  if (!flags.regionSnippets && !flags.contextRegionSnippets && !flags.comprehensiveRegionProperties) {
    return;
  }

  const resolvedUri = tryReconstructAbsoluteUri(node.artifactLocation, oub);
  if (!resolvedUri) return;

  const expanded = cache.populateTextRegionProperties(
    node.region,
    resolvedUri,
    flags.regionSnippets,
  );

  const originalSnippet = node.region.snippet;

  if (flags.comprehensiveRegionProperties) {
    // Replace the region wholesale (preserving any property bag the caller
    // supplied — populateTextRegionProperties spreads the input).
    node.region = expanded;
  }

  node.region.snippet = originalSnippet ?? expanded.snippet;

  if (flags.contextRegionSnippets && !node.contextRegion) {
    node.contextRegion = cache.constructMultilineContextSnippet(expanded, resolvedUri);
  }
}

/**
 * Builds run.artifacts[] from every artifactLocation referenced by results,
 * deduplicating by (uri, uriBaseId). Mirrors AddFileReferencesVisitor.
 */
function scrapeFileReferences(run: Run): void {
  const artifacts: Artifact[] = run.artifacts ? [...run.artifacts] : [];
  const indexByKey = new Map<string, number>();
  for (let i = 0; i < artifacts.length; i++) {
    const k = artifactKey(artifacts[i].location);
    if (k) indexByKey.set(k, i);
  }

  const register = (al: ArtifactLocation | undefined) => {
    if (!al?.uri) return;
    const k = artifactKey(al);
    if (!k) return;
    let idx = indexByKey.get(k);
    if (idx === undefined) {
      idx = artifacts.length;
      artifacts.push({ location: { uri: al.uri, uriBaseId: al.uriBaseId } });
      indexByKey.set(k, idx);
    }
    // Back-reference into run.artifacts[] (mirrors AddFileReferencesVisitor).
    al.index = idx;
  };

  for (const result of run.results ?? []) {
    visitResultLocations(result, (pl) => register(pl.artifactLocation));
  }

  if (artifacts.length > 0) run.artifacts = artifacts;
}

function artifactKey(al: ArtifactLocation | undefined): string | undefined {
  if (!al?.uri) return undefined;
  return `${al.uriBaseId ?? ''}#${al.uri}`;
}
