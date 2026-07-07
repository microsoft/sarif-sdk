// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * `emit-run`: creates an append-only SARIF event log (`<output>.wip.jsonl`)
 * seeded with a `run-header` event built from a caller-supplied SARIF Run.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/EmitRunCommand.cs.
 */

import { existsSync, mkdirSync } from 'node:fs';
import { promises as fs } from 'node:fs';
import { dirname, resolve as resolvePath } from 'node:path';
import { fileURLToPath } from 'node:url';
import { appendEvents, SarifEventKinds, wipPathFor } from './eventLog.js';
import {
  detectAdoPipeline,
  detectGitHubActions,
  adoCanonicalAutomationId,
  adoPipelinePropertyValues,
  VcpFieldNames,
  type AdoPipelineContext,
  type GitHubActionsContext,
  type EnvGetter,
} from './ciContext.js';
import { tryValidateRepositoryUri } from '@microsoft/sarif';
import type { AuthorizedHost } from '@microsoft/sarif';
import { EmitVerbError } from './batch.js';

export const SOURCE_ROOT_BASE_ID = 'SRCROOT';
const AI_ORIGIN_VALUES = ['generated', 'annotated', 'synthesized'] as const;
const DOC_URI_SCHEMES = ['https:'] as const;
const BASE_URI_SCHEMES = ['https:', 'file:'] as const;
const GUID_D = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

type Obj = Record<string, unknown>;

export interface EmitRunOptions {
  /** Final SARIF file path. The event log is written to `<output>.wip.jsonl`. */
  output: string;
  /** Partial SARIF Run JSON object (NOT a SARIF log). */
  run: Obj;
  /** Replace an existing .sarif or in-progress .wip.jsonl at the destination. */
  forceOverwrite?: boolean;
  /**
   * Hosts the caller attests are self-hosted VCS instances (`<hostType>:<host>`,
   * parsed by `parseAuthorizedHosts`), so their
   * `versionControlProvenance.repositoryUri` validates against the attested
   * product instead of being rejected as unsupported. Matched
   * case-insensitively.
   */
  authorizedHosts?: readonly AuthorizedHost[];
  /** Test seam; defaults to process.env. */
  env?: EnvGetter;
}

export interface EmitRunOutcome {
  wipPath: string;
  toolName: string;
  /** Non-fatal warnings (e.g. ignored header data). */
  warnings: string[];
}

/**
 * Opens a new SARIF event log seeded with the supplied Run as a `run-header`
 * event. Throws EmitVerbError on validation failure (no file-system side
 * effects on failure). Returns the wip path and tool name on success.
 */
export async function emitRun(opts: EmitRunOptions): Promise<EmitRunOutcome> {
  const env: EnvGetter = opts.env ?? ((n) => process.env[n]);
  const warnings: string[] = [];

  if (!opts.output?.trim()) throw new EmitVerbError('Output SARIF path is required.');

  // Detect CI contexts BEFORE any file-system side effects so a partial
  // failure doesn't leave a half-written or freshly-deleted .wip.jsonl.
  const ado = detectAdoPipeline(env);
  if (ado.state === 'partial') throw new EmitVerbError(ado.errorMessage!);

  const gha = detectGitHubActions(env);
  if (gha.state === 'partial') throw new EmitVerbError(gha.errorMessage!);

  const runObject = opts.run;
  if (!isObj(runObject)) {
    throw new EmitVerbError('Run JSON must be a JSON object.');
  }

  rejectSarifLogShape(runObject);
  validateRunHeader(runObject);

  if (ado.state === 'complete') {
    stampAdoContext(runObject, ado.context!);
  }

  if (ado.state === 'complete' || gha.state === 'complete') {
    const merged = resolveVcpFields(ado.context, gha.context);
    stampVcp(runObject, merged.repositoryUri, merged.revisionId, merged.branch);
  }

  validateVcpRepositoryShapes(runObject, opts.authorizedHosts);

  const outputPath = resolvePath(opts.output);
  const wipPath = wipPathFor(outputPath);

  const sarifExists = existsSync(outputPath);
  const wipExists = existsSync(wipPath);

  if ((sarifExists || wipExists) && !opts.forceOverwrite) {
    const which =
      wipExists && sarifExists
        ? ' (and its .wip.jsonl)'
        : wipExists
          ? ' (.wip.jsonl)'
          : '';
    throw new EmitVerbError(`'${outputPath}'${which} already exists. Pass --force-overwrite to replace.`);
  }

  if (wipExists) await fs.unlink(wipPath);

  const dir = dirname(wipPath);
  if (dir && !existsSync(dir)) mkdirSync(dir, { recursive: true });

  warnOnIgnoredHeaderData(runObject, warnings);

  await appendEvents(wipPath, [{ kind: SarifEventKinds.RunHeader, payload: runObject }]);

  const toolName = ((runObject.tool as Obj | undefined)?.driver as Obj | undefined)?.name as string;
  return { wipPath, toolName, warnings };
}

// ---------------------------------------------------------------------------
// Validation helpers
// ---------------------------------------------------------------------------

function isObj(v: unknown): v is Obj {
  return v !== null && typeof v === 'object' && !Array.isArray(v);
}

function jsonTypeName(v: unknown): string {
  if (v === null) return 'null';
  if (Array.isArray(v)) return 'array';
  return typeof v;
}

function requireOptionalObject(parent: Obj, leafKey: string, path: string): Obj | undefined {
  const token = parent[leafKey];
  if (token === undefined || token === null) return undefined;
  if (!isObj(token)) {
    throw new EmitVerbError(`${path} must be a JSON object, but the payload supplied a ${jsonTypeName(token)}.`);
  }
  return token;
}

function rejectSarifLogShape(runObject: Obj): void {
  if (Array.isArray(runObject.runs) && runObject.version !== undefined) {
    throw new EmitVerbError(
      '--input expects a SARIF Run object, not a SARIF log. Supply runs[0] or construct a Run JSON directly.',
    );
  }
}

function validateRunHeader(runObject: Obj): void {
  const tool = requireOptionalObject(runObject, 'tool', 'tool');
  const driver = tool ? requireOptionalObject(tool, 'driver', 'tool.driver') : undefined;
  const automationDetails = requireOptionalObject(runObject, 'automationDetails', 'automationDetails');
  if (automationDetails) {
    requireOptionalObject(automationDetails, 'properties', 'automationDetails.properties');
  }
  const properties = requireOptionalObject(runObject, 'properties', 'properties');
  const oub = requireOptionalObject(runObject, 'originalUriBaseIds', 'originalUriBaseIds');
  const srcRoot = oub
    ? requireOptionalObject(oub, SOURCE_ROOT_BASE_ID, `originalUriBaseIds["${SOURCE_ROOT_BASE_ID}"]`)
    : undefined;

  const driverName = driver?.name;
  if (typeof driverName !== 'string' || !driverName.trim()) {
    throw new EmitVerbError('tool.driver.name is required and must be a non-empty JSON string.');
  }

  validateOptionalUri(driver?.informationUri, 'tool.driver.informationUri', DOC_URI_SCHEMES);

  const vcp = runObject.versionControlProvenance;
  if (vcp !== undefined && vcp !== null) {
    if (!Array.isArray(vcp)) {
      throw new EmitVerbError(
        `versionControlProvenance must be a JSON array, but the payload supplied a ${jsonTypeName(vcp)}.`,
      );
    }
    for (let i = 0; i < vcp.length; i++) {
      if (!isObj(vcp[i])) {
        throw new EmitVerbError(
          `versionControlProvenance[${i}] must be a JSON object, but the payload supplied a ${jsonTypeName(vcp[i])}.`,
        );
      }
      validateOptionalUri(
        (vcp[i] as Obj).repositoryUri,
        `versionControlProvenance[${i}].repositoryUri`,
        DOC_URI_SCHEMES,
      );
    }
  }

  validateOptionalUri(srcRoot?.uri, `originalUriBaseIds["${SOURCE_ROOT_BASE_ID}"].uri`, BASE_URI_SCHEMES);
  validateSourceRootResolvesOnDisk(srcRoot?.uri);

  validateOptionalGuid(automationDetails?.guid, 'automationDetails.guid');
  validateOptionalGuid(automationDetails?.correlationGuid, 'automationDetails.correlationGuid');

  validateOptionalAiOrigin(properties?.['ai/origin']);
}

function validateOptionalUri(token: unknown, path: string, allowedSchemes: readonly string[]): void {
  if (token === undefined || token === null) return;
  if (typeof token !== 'string') {
    throw new EmitVerbError(`${path} must be a JSON string, but the payload supplied a ${jsonTypeName(token)}.`);
  }
  if (!token.trim()) return;
  let u: URL;
  try {
    u = new URL(token);
  } catch {
    throw new EmitVerbError(`${path} value '${token}' is not a well-formed absolute URI.`);
  }
  if (!allowedSchemes.includes(u.protocol)) {
    const list = allowedSchemes.map((s) => s.slice(0, -1)).join(', ');
    throw new EmitVerbError(
      `${path} value '${token}' uses scheme '${u.protocol.slice(0, -1)}'; expected one of: ${list}.`,
    );
  }
}

function validateSourceRootResolvesOnDisk(token: unknown): void {
  if (typeof token !== 'string' || !token.trim()) return;
  let u: URL;
  try {
    u = new URL(token);
  } catch {
    return;
  }
  if (u.protocol !== 'file:') return;
  const localPath = fileURLToPath(u);
  if (!existsSync(localPath)) {
    throw new EmitVerbError(
      `originalUriBaseIds["${SOURCE_ROOT_BASE_ID}"].uri: '${token}' does not resolve to an existing directory ('${localPath}'). A file: source root must point at an observable checkout when the run header is received.`,
    );
  }
}

function validateOptionalGuid(token: unknown, path: string): void {
  if (token === undefined || token === null) return;
  if (typeof token !== 'string') {
    throw new EmitVerbError(`${path} must be a JSON string, but the payload supplied a ${jsonTypeName(token)}.`);
  }
  if (!token.trim() || !GUID_D.test(token)) {
    throw new EmitVerbError(
      `${path}: '${token}' is not a valid GUID (expected canonical 8-4-4-4-12 hex form, e.g. a7ad9ab8-1234-5678-9abc-def012345678). Omit the field if no value is available.`,
    );
  }
}

function validateOptionalAiOrigin(token: unknown): void {
  if (token === undefined || token === null) return;
  if (typeof token !== 'string') {
    throw new EmitVerbError(
      `properties["ai/origin"] must be a JSON string, but the payload supplied a ${jsonTypeName(token)}.`,
    );
  }
  if (!(AI_ORIGIN_VALUES as readonly string[]).includes(token)) {
    throw new EmitVerbError(
      `properties["ai/origin"]: '${token}' is not valid. Expected exactly one of: ${AI_ORIGIN_VALUES.join(', ')}.`,
    );
  }
}

function validateVcpRepositoryShapes(
  runObject: Obj,
  authorizedHosts?: readonly AuthorizedHost[],
): void {
  const vcp = runObject.versionControlProvenance;
  if (!Array.isArray(vcp)) return;
  for (let i = 0; i < vcp.length; i++) {
    const repoUri = isObj(vcp[i]) ? (vcp[i] as Obj).repositoryUri : undefined;
    if (typeof repoUri !== 'string' || !repoUri) continue;
    const r = tryValidateRepositoryUri(repoUri, { authorizedHosts });
    if (!r.ok) {
      throw new EmitVerbError(`versionControlProvenance[${i}]: ${r.error}`);
    }
  }
}

function warnOnIgnoredHeaderData(runObject: Obj, warnings: string[]): void {
  const dropped: string[] = [];
  if (Array.isArray(runObject.results) && runObject.results.length > 0) {
    dropped.push(`results[${runObject.results.length}]`);
  }
  if (Array.isArray(runObject.invocations) && runObject.invocations.length > 0) {
    dropped.push(`invocations[${runObject.invocations.length}]`);
  }
  if (dropped.length > 0) {
    warnings.push(
      `warning: the run header carries ${dropped.join(', ')}; this data is ignored at replay (results belong in add-results, invocations in add-invocations) and will not appear in the finalized log.`,
    );
  }
}

// ---------------------------------------------------------------------------
// CI-context stamping (probe-before-write; conflict throws)
// ---------------------------------------------------------------------------

function isAbsentOrEmpty(v: unknown): boolean {
  return v === undefined || v === null || (typeof v === 'string' && v.length === 0);
}

function stampAdoContext(runObject: Obj, ctx: AdoPipelineContext): void {
  const canonicalId = adoCanonicalAutomationId(ctx);
  const props = adoPipelinePropertyValues(ctx);

  // Probe-before-write so a conflict on any field leaves the object unchanged.
  const ad = runObject.automationDetails as Obj | undefined;
  if (isObj(ad)) {
    const existingId = ad.id;
    if (existingId !== undefined && existingId !== null) {
      if (typeof existingId !== 'string') {
        throw new EmitVerbError(
          `Supplied automationDetails.id must be a JSON string, but the payload supplied a ${jsonTypeName(existingId)}. Detected ADO pipeline value is '${canonicalId}'; either match it or omit the field.`,
        );
      }
      if (existingId.length !== 0 && existingId !== canonicalId) {
        throw new EmitVerbError(
          `Supplied automationDetails.id '${existingId}' conflicts with detected ADO pipeline value '${canonicalId}'.`,
        );
      }
    }
    const ep = ad.properties as Obj | undefined;
    if (isObj(ep)) {
      for (const [k, v] of props) {
        const existing = ep[k];
        if (existing === undefined || existing === null) continue;
        if (typeof existing !== 'string') {
          throw new EmitVerbError(
            `Supplied automationDetails.properties["${k}"] must be a JSON string, but the payload supplied a ${jsonTypeName(existing)}. Detected ADO pipeline value is '${v}'; either match it or omit the field.`,
          );
        }
        if (existing.length !== 0 && existing !== v) {
          throw new EmitVerbError(
            `Supplied automationDetails.properties["${k}"]='${existing}' conflicts with detected ADO pipeline value '${v}'.`,
          );
        }
      }
    }
  }

  const ensured = (runObject.automationDetails ??= {}) as Obj;
  if (isAbsentOrEmpty(ensured.id)) ensured.id = canonicalId;
  const ensuredProps = (ensured.properties ??= {}) as Obj;
  for (const [k, v] of props) {
    if (isAbsentOrEmpty(ensuredProps[k])) ensuredProps[k] = v;
  }
}

function resolveVcpFields(
  ado: AdoPipelineContext | undefined,
  gha: GitHubActionsContext | undefined,
): { repositoryUri?: string; revisionId?: string; branch?: string } {
  let repositoryUri = ado?.repositoryUri;
  let revisionId = ado?.revisionId;
  let branch = ado?.branchRef;

  if (gha) {
    repositoryUri = mergeField(repositoryUri, gha.repositoryUri, VcpFieldNames.RepositoryUri, true);
    revisionId = mergeField(revisionId, gha.revisionId, VcpFieldNames.RevisionId, false);
    branch = mergeField(branch, gha.branchRef, VcpFieldNames.Branch, false);
  }
  return { repositoryUri, revisionId, branch };
}

function mergeField(
  resolved: string | undefined,
  candidate: string | undefined,
  fieldName: string,
  uriCompare: boolean,
): string | undefined {
  if (!candidate) return resolved;
  if (!resolved) return candidate;
  if (vcpFieldValuesAgree(fieldName, resolved, candidate, uriCompare)) return resolved;
  throw new EmitVerbError(
    `ADO pipeline env says ${fieldName}='${resolved}', GitHub Actions env says ${fieldName}='${candidate}'. Cross-source disagreement; refusing to stamp.`,
  );
}

function vcpFieldValuesAgree(
  fieldName: string,
  supplied: string,
  detected: string,
  uriCompare: boolean,
): boolean {
  if (uriCompare || fieldName === VcpFieldNames.RepositoryUri) {
    try {
      return new URL(supplied).toString() === new URL(detected).toString();
    } catch {
      /* fall through */
    }
  }
  return supplied === detected;
}

function stampVcp(
  runObject: Obj,
  repositoryUri: string | undefined,
  revisionId: string | undefined,
  branch: string | undefined,
): void {
  const fields: Array<[string, string]> = [];
  if (repositoryUri) fields.push([VcpFieldNames.RepositoryUri, repositoryUri]);
  if (revisionId) fields.push([VcpFieldNames.RevisionId, revisionId]);
  if (branch) fields.push([VcpFieldNames.Branch, branch]);
  if (fields.length === 0) return;

  const vcpArray = runObject.versionControlProvenance as Obj[] | undefined;

  if (!vcpArray || vcpArray.length === 0) {
    // Branch/revision without a repository URI cannot bind to a repo downstream.
    if (!repositoryUri) return;
    const entry: Obj = {};
    for (const [k, v] of fields) entry[k] = v;
    stampMappedToIfAbsent(entry, runObject);
    if (!vcpArray) runObject.versionControlProvenance = [entry];
    else vcpArray.push(entry);
    return;
  }

  if (vcpArray.length > 1) return; // multi-entry VCP is caller-authored

  const existing = vcpArray[0];

  // Probe-before-write.
  for (const [k, v] of fields) {
    const e = existing[k];
    if (e === undefined || e === null) continue;
    if (typeof e !== 'string') {
      throw new EmitVerbError(
        `Supplied versionControlProvenance[0].${k} must be a JSON string, but the payload supplied a ${jsonTypeName(e)}. Detected pipeline value is '${v}'; either match it or omit the field.`,
      );
    }
    if (e.length === 0) continue;
    if (!vcpFieldValuesAgree(k, e, v, false)) {
      throw new EmitVerbError(
        `Supplied versionControlProvenance[0].${k}='${e}' conflicts with detected pipeline value '${v}'.`,
      );
    }
  }

  for (const [k, v] of fields) {
    if (isAbsentOrEmpty(existing[k])) existing[k] = v;
  }

  stampMappedToIfAbsent(existing, runObject);
}

function stampMappedToIfAbsent(vcpEntry: Obj, runObject: Obj): void {
  const oub = runObject.originalUriBaseIds as Obj | undefined;
  if (!isObj(oub) || !isObj(oub[SOURCE_ROOT_BASE_ID])) return;
  if (vcpEntry.mappedTo !== undefined && vcpEntry.mappedTo !== null) return;
  vcpEntry.mappedTo = { uriBaseId: SOURCE_ROOT_BASE_ID };
}
