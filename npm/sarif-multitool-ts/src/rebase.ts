// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Rewrites absolute local file paths in a run into relative URIs plus
 * portable, per-repository uriBaseIds derived from versionControlProvenance.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/EmitFinalizeRebaseVisitor.cs.
 */

import type { Run, ArtifactLocation, VersionControlDetails } from '@microsoft/sarif';
import { tryDerivePortableRoot } from '@microsoft/sarif';
import { tryReconstructAbsoluteUri } from '@microsoft/sarif';
import { SOURCE_ROOT_BASE_ID } from './emitRun.js';

const MAX_BASE_CHAIN_DEPTH = 64;

interface RepoRoot {
  vcd: VersionControlDetails;
  sourceBaseId: string;
  localRoot: string; // absolute file:// URI with trailing slash
  portableRoot: string;
  revisionWebUrl: string;
  leaf: string;
  outputBaseId: string;
}

export interface RebaseResult {
  success: boolean;
  errors: string[];
}

/** Rebases the run in place. Returns errors (success ↔ errors.length === 0). */
export function rebaseRun(run: Run, noRepo = false): RebaseResult {
  if (noRepo) return rebaseRunRepoless(run);

  const errors: string[] = [];
  const referencedBaseIds = new Set<string>();
  const leakUris: string[] = [];

  const planResult = buildPlan(run, errors);
  if (!planResult) return { success: false, errors };
  const plan = planResult;

  const sourceToOutput = new Map<string, string>();
  for (const r of plan) sourceToOutput.set(r.sourceBaseId, r.outputBaseId);

  // Snapshot input bases so mutating live locations during the pass cannot
  // corrupt resolution.
  const inputBases = snapshotBases(run.originalUriBaseIds);
  run.originalUriBaseIds = undefined;

  // Walk every artifactLocation in the run.
  walkArtifactLocations(run, (al) =>
    rewriteLocation(al, plan, inputBases, sourceToOutput, referencedBaseIds, leakUris),
  );

  // Bind each VCP entry's mappedTo to its minted output base.
  for (const root of plan) {
    root.vcd.mappedTo = { uriBaseId: root.outputBaseId };
    referencedBaseIds.add(root.outputBaseId);
  }

  run.originalUriBaseIds = buildOutputBases(plan, inputBases);

  verifyNoLeak(run, referencedBaseIds, leakUris, errors);

  return { success: errors.length === 0, errors };
}

/**
 * Rebases a run finalized with --no-repo: the scan asserts it has no version control, so there is
 * no portable root to rebase onto. Locations are leak-checked but never rewritten, and each base's
 * transient local file:// root is elided so the finalized log carries no machine-specific path.
 *
 * Ported from EmitFinalizeRebaseVisitor.VisitRunRepoless.
 */
function rebaseRunRepoless(run: Run): RebaseResult {
  const errors: string[] = [];
  const referencedBaseIds = new Set<string>();
  const leakUris: string[] = [];

  // --no-repo asserts the scan has no version control. A run that nonetheless declares
  // versionControlProvenance is a contradictory signal; refuse rather than ship a log that
  // claims both.
  if (run.versionControlProvenance && run.versionControlProvenance.length > 0) {
    errors.push(
      '--no-repo was specified, but run.versionControlProvenance declares one or more repositories. Remove --no-repo to rebase artifact locations to portable, per-repository roots derived from versionControlProvenance, or remove the provenance to finalize as a repo-less scan.',
    );
    return { success: false, errors };
  }

  const cycleErr = validateNoBaseCycles(run.originalUriBaseIds);
  if (cycleErr) {
    errors.push(cycleErr);
    return { success: false, errors };
  }

  // A repo-less location must be expressed relative to a declared base. A rooted local path
  // (absolute file://, or a rooted relative shape) cannot be made portable without a repository
  // root, so it is a leak the final assertion rejects.
  walkArtifactLocations(run, (al) => {
    if (al.uriBaseId) referencedBaseIds.add(al.uriBaseId);
    if (al.uri && isRootedLocalShape(al.uri)) leakUris.push(al.uri);
  });

  // Drop the transient local file:// root from each base, keeping the base symbol (and any
  // description / parent chain) so locations remain <BASE>-relative.
  if (run.originalUriBaseIds) {
    for (const entry of Object.values(run.originalUriBaseIds)) {
      if (entry?.uri && entry.uri.startsWith('file:')) {
        entry.uri = undefined;
      }
    }
  }

  verifyNoLeak(run, referencedBaseIds, leakUris, errors);

  return { success: errors.length === 0, errors };
}

function buildPlan(run: Run, errors: string[]): RepoRoot[] | undefined {
  const vcp = run.versionControlProvenance;
  if (!vcp || vcp.length === 0) {
    errors.push(
      'emit-finalize requires run.versionControlProvenance to declare at least one repository, each carrying mappedTo, repositoryUri, and revisionId.',
    );
    return undefined;
  }

  const cycleErr = validateNoBaseCycles(run.originalUriBaseIds);
  if (cycleErr) {
    errors.push(cycleErr);
    return undefined;
  }

  const roots: RepoRoot[] = [];
  const seenLocalRoots = new Map<string, number>();

  for (let i = 0; i < vcp.length; i++) {
    const vcd = vcp[i];
    const where = `versionControlProvenance[${i}]`;

    if (!vcd) {
      errors.push(`${where} must be a version-control-details object.`);
      return undefined;
    }
    if (!vcd.mappedTo?.uriBaseId) {
      errors.push(
        `${where}.mappedTo must declare a uriBaseId that binds the repository root to an originalUriBaseIds entry.${describeMappedToFix(run)} See rule SARIF2007 (ExpressPathsRelativeToRepoRoot).`,
      );
      return undefined;
    }
    if (vcd.mappedTo.uri) {
      errors.push(
        `${where}.mappedTo must carry only a uriBaseId; the repository root's local path is declared on the named originalUriBaseIds entry, not inline on mappedTo.`,
      );
      return undefined;
    }
    if (!vcd.repositoryUri) {
      errors.push(`${where}.repositoryUri is required so a portable root can be derived.`);
      return undefined;
    }
    if (!vcd.revisionId) {
      errors.push(
        `${where}.revisionId must be a non-empty commit identifier so a portable root can be derived.`,
      );
      return undefined;
    }

    const localRoot = resolveLocalRoot(run.originalUriBaseIds, vcd.mappedTo.uriBaseId);
    if (!localRoot || !localRoot.startsWith('file:')) {
      errors.push(
        `${where}.mappedTo (uriBaseId '${vcd.mappedTo.uriBaseId}') must resolve to an absolute local file:// path through originalUriBaseIds. For example, originalUriBaseIds['${vcd.mappedTo.uriBaseId}'].uri = "file:///path/to/checkout/". emit-finalize resolves result regions and snippets against this on-disk checkout, then rebases the local prefix to a portable per-repository root derived from versionControlProvenance (repositoryUri + revisionId), so the local path is not retained in the finalized SARIF.`,
      );
      return undefined;
    }

    const localRootSlashed = ensureTrailingSlash(localRoot);
    const localKey = localRootSlashed.toLowerCase();
    if (seenLocalRoots.has(localKey)) {
      errors.push(
        `${where}.mappedTo resolves to the same local root as versionControlProvenance[${seenLocalRoots.get(localKey)}]; each repository must map to a distinct root.`,
      );
      return undefined;
    }
    seenLocalRoots.set(localKey, i);

    const derived = tryDerivePortableRoot(vcd.repositoryUri, vcd.revisionId);
    if (!derived.ok) {
      errors.push(`${where}: ${derived.error}`);
      return undefined;
    }

    // Ship the canonical https identity.
    if (vcd.repositoryUri !== derived.value.canonicalRepositoryUri) {
      vcd.repositoryUri = derived.value.canonicalRepositoryUri;
    }

    roots.push({
      vcd,
      sourceBaseId: vcd.mappedTo.uriBaseId,
      localRoot: localRootSlashed,
      portableRoot: derived.value.portableRoot,
      revisionWebUrl: derived.value.revisionWebUrl,
      leaf: derived.value.leaf,
      outputBaseId: '', // assigned below
    });
  }

  assignOutputBaseIds(roots);
  return roots;
}

function assignOutputBaseIds(roots: RepoRoot[]): void {
  if (roots.length === 1) {
    roots[0].outputBaseId = SOURCE_ROOT_BASE_ID;
    return;
  }
  const used = new Set<string>();
  for (const root of roots) {
    const base = `${SOURCE_ROOT_BASE_ID}_${upperSnake(root.leaf)}`;
    let candidate = base;
    let n = 2;
    while (used.has(candidate)) candidate = `${base}_${n++}`;
    used.add(candidate);
    root.outputBaseId = candidate;
  }
}

function upperSnake(value: string): string {
  let out = '';
  let lastUnder = false;
  for (const c of value) {
    if (/[A-Za-z0-9]/.test(c)) {
      out += c.toUpperCase();
      lastUnder = false;
    } else if (!lastUnder && out.length > 0) {
      out += '_';
      lastUnder = true;
    }
  }
  out = out.replace(/_+$/, '');
  return out.length === 0 ? 'REPO' : out;
}

// ---------------------------------------------------------------------------
// Rewriting
// ---------------------------------------------------------------------------

function rewriteLocation(
  al: ArtifactLocation,
  plan: RepoRoot[],
  inputBases: Record<string, ArtifactLocation>,
  sourceToOutput: Map<string, string>,
  referencedBaseIds: Set<string>,
  leakUris: string[],
): void {
  if (!al.uri) {
    // Base-only reference — remap to the minted output base if known.
    if (al.uriBaseId) {
      const out = sourceToOutput.get(al.uriBaseId);
      if (out) {
        al.uriBaseId = out;
        referencedBaseIds.add(out);
      } else {
        referencedBaseIds.add(al.uriBaseId);
      }
    }
    return;
  }

  const abs = tryReconstructAbsoluteUri(al, inputBases);
  if (!abs) {
    if (al.uriBaseId) referencedBaseIds.add(al.uriBaseId);
    if (isRootedLocalShape(al.uri)) leakUris.push(al.uri);
    return;
  }

  const owner = longestPrefixOwner(abs, plan);
  if (owner) {
    al.uri = makeRelative(owner.localRoot, abs);
    al.uriBaseId = owner.outputBaseId;
    referencedBaseIds.add(owner.outputBaseId);
    return;
  }

  if (abs.startsWith('file:')) {
    leakUris.push(abs);
    return;
  }

  // Already portable: inline as absolute, no base.
  al.uri = abs;
  al.uriBaseId = undefined;
}

function longestPrefixOwner(abs: string, plan: RepoRoot[]): RepoRoot | undefined {
  let best: RepoRoot | undefined;
  let bestLen = -1;
  for (const root of plan) {
    if (isBaseOf(root.localRoot, abs) && root.localRoot.length > bestLen) {
      best = root;
      bestLen = root.localRoot.length;
    }
  }
  return best;
}

function isBaseOf(base: string, target: string): boolean {
  // Case-insensitive on Windows file: URIs; case-sensitive otherwise. We
  // approximate by comparing lowercased for file: scheme.
  if (base.startsWith('file:')) {
    return target.toLowerCase().startsWith(base.toLowerCase());
  }
  return target.startsWith(base);
}

function makeRelative(base: string, abs: string): string {
  return abs.slice(base.length);
}

function ensureTrailingSlash(uri: string): string {
  return uri.endsWith('/') ? uri : uri + '/';
}

function isRootedLocalShape(uri: string): boolean {
  if (uri.startsWith('file:')) return true;
  if (uri.length === 0) return false;
  if (uri[0] === '/' || uri[0] === '\\') return true;
  return uri.length >= 2 && uri[1] === ':' && /[A-Za-z]/.test(uri[0]);
}

// ---------------------------------------------------------------------------
// Output bases / verification
// ---------------------------------------------------------------------------

function buildOutputBases(
  plan: RepoRoot[],
  inputBases: Record<string, ArtifactLocation>,
): Record<string, ArtifactLocation> {
  const out: Record<string, ArtifactLocation> = {};
  for (const root of plan) {
    const source = inputBases[root.sourceBaseId];
    const baseEntry: ArtifactLocation = source ? { ...source } : {};
    baseEntry.uri = root.portableRoot;
    baseEntry.uriBaseId = undefined;
    const shortRev =
      root.vcd.revisionId && root.vcd.revisionId.length > 7
        ? root.vcd.revisionId.slice(0, 7)
        : root.vcd.revisionId;
    baseEntry.description ??= {
      text: `Source root mapped to [${root.leaf}@${shortRev}](${root.revisionWebUrl}).`,
    };
    out[root.outputBaseId] = baseEntry;
  }
  return out;
}

function verifyNoLeak(
  run: Run,
  referencedBaseIds: Set<string>,
  leakUris: string[],
  errors: string[],
): void {
  for (const baseId of referencedBaseIds) {
    if (!run.originalUriBaseIds || !(baseId in run.originalUriBaseIds)) {
      errors.push(`After rebasing, a location still references undefined uriBaseId '${baseId}'.`);
    }
  }
  for (const leak of leakUris) {
    errors.push(
      `Local file path '${leak}' could not be attributed to a declared repository root; finalize will not ship a machine-specific path.`,
    );
  }
  if (run.originalUriBaseIds) {
    for (const [k, v] of Object.entries(run.originalUriBaseIds)) {
      if (v?.uri && v.uri.startsWith('file:')) {
        errors.push(`originalUriBaseIds['${k}'] still anchors at a local path '${v.uri}'.`);
      }
    }
  }
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function snapshotBases(
  bases: Record<string, ArtifactLocation> | undefined,
): Record<string, ArtifactLocation> {
  const copy: Record<string, ArtifactLocation> = {};
  if (bases) {
    for (const [k, v] of Object.entries(bases)) {
      copy[k] = v ? { ...v } : ({} as ArtifactLocation);
    }
  }
  return copy;
}

function validateNoBaseCycles(
  bases: Record<string, ArtifactLocation> | undefined,
): string | undefined {
  if (!bases) return undefined;
  for (const startKey of Object.keys(bases)) {
    const seen = new Set<string>();
    let key: string | undefined = startKey;
    let depth = 0;
    while (key) {
      if (seen.has(key)) {
        return `originalUriBaseIds contains a cyclic uriBaseId chain involving '${key}'.`;
      }
      seen.add(key);
      if (++depth > MAX_BASE_CHAIN_DEPTH) {
        return 'originalUriBaseIds uriBaseId chain exceeds the maximum supported depth.';
      }
      const al: ArtifactLocation | undefined = bases[key];
      if (!al) break;
      key = al.uriBaseId;
    }
  }
  return undefined;
}

function resolveLocalRoot(
  bases: Record<string, ArtifactLocation> | undefined,
  baseId: string,
): string | undefined {
  if (!bases || !baseId || !bases[baseId]) return undefined;
  return tryReconstructAbsoluteUri(bases[baseId], bases);
}

function describeMappedToFix(run: Run): string {
  const bases = run.originalUriBaseIds;
  if (bases && SOURCE_ROOT_BASE_ID in bases) {
    return ` Your originalUriBaseIds already declares '${SOURCE_ROOT_BASE_ID}', so bind to it: "mappedTo": { "uriBaseId": "${SOURCE_ROOT_BASE_ID}" }.`;
  }
  if (bases && Object.keys(bases).length > 0) {
    const declared = Object.keys(bases)
      .map((k) => `'${k}'`)
      .join(', ');
    return ` Your originalUriBaseIds declares ${declared}; set mappedTo.uriBaseId to the entry for the repository root (conventionally '${SOURCE_ROOT_BASE_ID}').`;
  }
  return ` Declare an originalUriBaseIds entry for the repository root (conventionally '${SOURCE_ROOT_BASE_ID}') and bind to it: "mappedTo": { "uriBaseId": "${SOURCE_ROOT_BASE_ID}" }.`;
}

/** Visits every ArtifactLocation in the run that the rebase pass should touch. */
function walkArtifactLocations(run: Run, visit: (al: ArtifactLocation) => void): void {
  const visitLoc = (l: { physicalLocation?: { artifactLocation?: ArtifactLocation } } | undefined) => {
    const al = l?.physicalLocation?.artifactLocation;
    if (al) visit(al);
  };

  for (const a of run.artifacts ?? []) {
    if (a?.location) visit(a.location);
  }
  for (const r of run.results ?? []) {
    if (r.analysisTarget) visit(r.analysisTarget);
    for (const l of r.locations ?? []) visitLoc(l);
    for (const l of r.relatedLocations ?? []) visitLoc(l);
  }
  for (const inv of run.invocations ?? []) {
    if (inv.workingDirectory) visit(inv.workingDirectory);
    for (const n of inv.toolExecutionNotifications ?? []) {
      for (const l of n.locations ?? []) visitLoc(l);
    }
    for (const n of inv.toolConfigurationNotifications ?? []) {
      for (const l of n.locations ?? []) visitLoc(l);
    }
  }
}
