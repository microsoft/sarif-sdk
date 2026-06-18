// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Detects Azure DevOps and GitHub Actions execution contexts from environment
 * variables and surfaces the automationDetails / versionControlProvenance
 * fields the emit-run verb stamps onto the run header.
 *
 * Ported from:
 *   src/Sarif.Multitool.Library/Emit/AdoPipelineContext.cs
 *   src/Sarif.Multitool.Library/Emit/GitHubActionsContext.cs
 *   src/Sarif.Multitool.Library/Emit/VcpFieldNames.cs
 */

export type EnvGetter = (name: string) => string | undefined;

export const VcpFieldNames = {
  RepositoryUri: 'repositoryUri',
  RevisionId: 'revisionId',
  Branch: 'branch',
} as const;

export type DetectionState = 'none' | 'partial' | 'complete';

export interface Detection<T> {
  state: DetectionState;
  context?: T;
  errorMessage?: string;
}

const REVISION_ID = /^[0-9a-fA-F]{7,40}$/;
const GUID_D = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
const GUID_EMPTY = '00000000-0000-0000-0000-000000000000';

function isTrueLike(raw: string | undefined): boolean {
  if (!raw) return false;
  const t = raw.trim();
  return t.toLowerCase() === 'true' || t === '1';
}

function firstNonBlank(a: string | undefined, b: string | undefined): string | undefined {
  return a && a.trim() ? a : b && b.trim() ? b : undefined;
}

function formatPartialError(label: string, present: string[], problems: string[]): string {
  const lines: string[] = [
    `${label} context is partially configured. Either populate every required variable or clear them all.`,
  ];
  if (present.length > 0) {
    lines.push('Present:');
    for (const p of present) lines.push(`  ${p}`);
  }
  lines.push('Problems:');
  for (const p of problems) lines.push(`  ${p}`);
  return lines.join('\n');
}

// ---------------------------------------------------------------------------
// Azure DevOps pipeline context
// ---------------------------------------------------------------------------

export interface AdoPipelineContext {
  organizationName: string;
  projectId: string;
  buildDefinitionId: number;
  buildDefinitionName: string;
  buildId: number;
  phaseId: string;
  phaseName: string;
  branchRef: string;
  repositoryUri?: string;
  revisionId?: string;
}

const ADO = {
  TfBuild: 'TF_BUILD',
  CollectionUri: 'SYSTEM_COLLECTIONURI',
  TeamProjectId: 'SYSTEM_TEAMPROJECTID',
  BuildDefIdPrimary: 'BUILD_DEFINITIONID',
  BuildDefIdFallback: 'SYSTEM_DEFINITIONID',
  BuildDefName: 'BUILD_DEFINITIONNAME',
  BuildId: 'BUILD_BUILDID',
  PhaseIdPrimary: 'SYSTEM_PHASEID',
  PhaseIdFallback: 'SYSTEM_JOBID',
  PhaseNamePrimary: 'SYSTEM_PHASENAME',
  PhaseNameFallback: 'SYSTEM_JOBNAME',
  SourceBranch: 'BUILD_SOURCEBRANCH',
  RepositoryUri: 'BUILD_REPOSITORY_URI',
  SourceVersion: 'BUILD_SOURCEVERSION',
} as const;

export const AdoProps = {
  BuildDefinitionId: 'azuredevops/pipeline/build/buildDefinitionId',
  BuildDefinitionName: 'azuredevops/pipeline/build/buildDefinitionName',
  PhaseId: 'azuredevops/pipeline/build/phaseId',
  PhaseName: 'azuredevops/pipeline/build/phaseName',
  AutomationIdPrefix: 'azuredevops/pipeline/build/',
} as const;

export function detectAdoPipeline(env: EnvGetter): Detection<AdoPipelineContext> {
  if (!isTrueLike(env(ADO.TfBuild))) return { state: 'none' };

  const collectionUri = env(ADO.CollectionUri);
  const teamProjectId = env(ADO.TeamProjectId);
  const buildDefIdPrimary = env(ADO.BuildDefIdPrimary);
  const buildDefIdFallback = env(ADO.BuildDefIdFallback);
  const buildDefName = env(ADO.BuildDefName);
  const buildId = env(ADO.BuildId);
  const phaseIdPrimary = env(ADO.PhaseIdPrimary);
  const phaseIdFallback = env(ADO.PhaseIdFallback);
  const phaseNamePrimary = env(ADO.PhaseNamePrimary);
  const phaseNameFallback = env(ADO.PhaseNameFallback);
  const sourceBranch = env(ADO.SourceBranch);
  const repositoryUri = env(ADO.RepositoryUri);
  const sourceVersion = env(ADO.SourceVersion);

  const buildDefIdRaw = firstNonBlank(buildDefIdPrimary, buildDefIdFallback);
  const phaseIdRaw = firstNonBlank(phaseIdPrimary, phaseIdFallback);
  const phaseNameRaw = firstNonBlank(phaseNamePrimary, phaseNameFallback);

  // All unset → None (TF_BUILD without other vars is not a typical agent state,
  // but the test-clear pattern is simpler if "all unset" is silent).
  if (
    !collectionUri?.trim() &&
    !teamProjectId?.trim() &&
    !buildDefIdRaw &&
    !buildDefName?.trim() &&
    !buildId?.trim() &&
    !phaseIdRaw &&
    !phaseNameRaw &&
    !sourceBranch?.trim()
  ) {
    return { state: 'none' };
  }

  const present: string[] = [];
  const problems: string[] = [];

  const organizationName = parseOrganizationName(collectionUri, ADO.CollectionUri, present, problems);
  const projectId = parseRequiredGuid(teamProjectId, ADO.TeamProjectId, present, problems);
  const buildDefinitionId = parseRequiredPositiveInt(
    buildDefIdPrimary,
    buildDefIdFallback,
    ADO.BuildDefIdPrimary,
    ADO.BuildDefIdFallback,
    present,
    problems,
  );
  const buildDefinitionName = readRequiredString(buildDefName, ADO.BuildDefName, present, problems);
  const buildIdValue = parseRequiredPositiveInt(buildId, undefined, ADO.BuildId, undefined, present, problems);
  const phaseId = parseRequiredGuid(
    phaseIdRaw,
    phaseIdPrimary?.trim() ? ADO.PhaseIdPrimary : ADO.PhaseIdFallback,
    present,
    problems,
  );
  const phaseName = readRequiredString(
    phaseNameRaw,
    phaseNamePrimary?.trim() ? ADO.PhaseNamePrimary : ADO.PhaseNameFallback,
    present,
    problems,
  );
  const branchRef = readRequiredString(sourceBranch, ADO.SourceBranch, present, problems);

  // Optional: absence is no signal; malformed presence adds to problems.
  const repoUriValue = readOptionalAbsoluteHttpUri(repositoryUri, ADO.RepositoryUri, present, problems);
  const revisionIdValue = readOptionalRevisionId(sourceVersion, ADO.SourceVersion, present, problems);

  if (problems.length > 0) {
    return { state: 'partial', errorMessage: formatPartialError('ADO pipeline', present, problems) };
  }

  return {
    state: 'complete',
    context: {
      organizationName: organizationName!,
      projectId: projectId!,
      buildDefinitionId: buildDefinitionId!,
      buildDefinitionName: buildDefinitionName!,
      buildId: buildIdValue!,
      phaseId: phaseId!,
      phaseName: phaseName!,
      branchRef: branchRef!,
      repositoryUri: repoUriValue,
      revisionId: revisionIdValue,
    },
  };
}

export function adoCanonicalAutomationId(c: AdoPipelineContext): string {
  return `${AdoProps.AutomationIdPrefix}${c.organizationName}/${c.projectId}/${c.buildDefinitionId}/${c.phaseId}/${c.branchRef}/${c.buildId}`;
}

export function adoPipelinePropertyValues(c: AdoPipelineContext): Array<[string, string]> {
  return [
    [AdoProps.BuildDefinitionId, String(c.buildDefinitionId)],
    [AdoProps.BuildDefinitionName, c.buildDefinitionName],
    [AdoProps.PhaseId, c.phaseId],
    [AdoProps.PhaseName, c.phaseName],
  ];
}

// SYSTEM_COLLECTIONURI accepted forms:
//   https://dev.azure.com/<org>/
//   https://<org>.visualstudio.com/
//   https://vsrm.dev.azure.com/<org>/
//   https://<org>.vsrm.visualstudio.com/
function parseOrganizationName(
  raw: string | undefined,
  envName: string,
  present: string[],
  problems: string[],
): string | undefined {
  if (!raw?.trim()) {
    problems.push(`${envName} is required but not set`);
    return undefined;
  }
  present.push(envName);
  let parsed: URL;
  try {
    parsed = new URL(raw.trim());
  } catch {
    problems.push(`${envName}='${raw}' is not a valid absolute URI`);
    return undefined;
  }
  const host = parsed.hostname.toLowerCase();
  const firstSegment = parsed.pathname.replace(/^\/+|\/+$/g, '').split('/')[0] ?? '';

  if (host === 'dev.azure.com' || host === 'vsrm.dev.azure.com') {
    if (!firstSegment) {
      problems.push(`${envName}='${raw}' is missing the organization path segment`);
      return undefined;
    }
    return firstSegment;
  }
  if (host.endsWith('.visualstudio.com')) {
    let sub = host.slice(0, -'.visualstudio.com'.length);
    if (sub.endsWith('.vsrm')) sub = sub.slice(0, -'.vsrm'.length);
    if (!sub) {
      problems.push(`${envName}='${raw}' is missing the organization subdomain`);
      return undefined;
    }
    return sub;
  }
  problems.push(
    `${envName}='${raw}' uses an unrecognized host (expected dev.azure.com/<org>, <org>.visualstudio.com, vsrm.dev.azure.com/<org>, or <org>.vsrm.visualstudio.com)`,
  );
  return undefined;
}

function parseRequiredGuid(
  raw: string | undefined,
  envName: string,
  present: string[],
  problems: string[],
): string | undefined {
  if (!raw?.trim()) {
    problems.push(`${envName} is required but not set`);
    return undefined;
  }
  present.push(envName);
  const t = raw.trim();
  if (!GUID_D.test(t)) {
    problems.push(`${envName}='${raw}' is not a valid GUID (expected canonical 8-4-4-4-12 hex form)`);
    return undefined;
  }
  if (t === GUID_EMPTY) {
    problems.push(`${envName}='${raw}' must not be Guid.Empty`);
    return undefined;
  }
  return t;
}

function parseRequiredPositiveInt(
  primaryRaw: string | undefined,
  fallbackRaw: string | undefined,
  primaryEnv: string,
  fallbackEnv: string | undefined,
  present: string[],
  problems: string[],
): number | undefined {
  const raw = firstNonBlank(primaryRaw, fallbackRaw);
  const sourceEnv = primaryRaw?.trim() ? primaryEnv : fallbackEnv ?? primaryEnv;
  if (!raw) {
    problems.push(`${primaryEnv} is required but not set`);
    return undefined;
  }
  present.push(sourceEnv);
  const t = raw.trim();
  if (!/^-?\d+$/.test(t)) {
    problems.push(`${sourceEnv}='${raw}' is not a valid integer`);
    return undefined;
  }
  const parsed = Number.parseInt(t, 10);
  if (parsed <= 0) {
    problems.push(`${sourceEnv}='${raw}' must be a positive integer`);
    return undefined;
  }
  // Primary/fallback agreement check (build def id only; phase/job exempted).
  if (
    fallbackEnv &&
    primaryRaw?.trim() &&
    fallbackRaw?.trim() &&
    /^-?\d+$/.test(fallbackRaw.trim()) &&
    Number.parseInt(fallbackRaw.trim(), 10) !== parsed
  ) {
    problems.push(
      `${primaryEnv}='${primaryRaw}' disagrees with ${fallbackEnv}='${fallbackRaw}' (both name the same pipeline identifier and must match)`,
    );
    return undefined;
  }
  return parsed;
}

function readRequiredString(
  raw: string | undefined,
  envName: string,
  present: string[],
  problems: string[],
): string | undefined {
  if (!raw?.trim()) {
    problems.push(`${envName} is required but not set`);
    return undefined;
  }
  present.push(envName);
  return raw.trim();
}

function readOptionalAbsoluteHttpUri(
  raw: string | undefined,
  envName: string,
  present: string[],
  problems: string[],
): string | undefined {
  if (!raw?.trim()) return undefined;
  present.push(envName);
  let parsed: URL;
  try {
    parsed = new URL(raw.trim());
  } catch {
    // Don't echo the raw value: it can carry a PAT or user:password@ credential.
    problems.push(`${envName} is not a valid absolute http(s) URI`);
    return undefined;
  }
  if (parsed.protocol !== 'http:' && parsed.protocol !== 'https:') {
    problems.push(`${envName} is not a valid absolute http(s) URI`);
    return undefined;
  }
  // Strip userinfo at this boundary so the stamped repositoryUri is a clean
  // identity that the credential-rejecting portable-root derivation accepts.
  parsed.username = '';
  parsed.password = '';
  return parsed.toString();
}

function readOptionalRevisionId(
  raw: string | undefined,
  envName: string,
  present: string[],
  problems: string[],
): string | undefined {
  if (!raw?.trim()) return undefined;
  present.push(envName);
  const t = raw.trim();
  if (!REVISION_ID.test(t)) {
    problems.push(`${envName}='${raw}' is not a valid revision id (expected 7-40 hex chars)`);
    return undefined;
  }
  return t;
}

// ---------------------------------------------------------------------------
// GitHub Actions context (VCP-scoped only)
// ---------------------------------------------------------------------------

export interface GitHubActionsContext {
  repositoryUri?: string;
  revisionId?: string;
  branchRef?: string;
}

const GHA = {
  Sentinel: 'GITHUB_ACTIONS',
  ServerUrl: 'GITHUB_SERVER_URL',
  Repository: 'GITHUB_REPOSITORY',
  Sha: 'GITHUB_SHA',
  Ref: 'GITHUB_REF',
} as const;

export function detectGitHubActions(env: EnvGetter): Detection<GitHubActionsContext> {
  if (!isTrueLike(env(GHA.Sentinel))) return { state: 'none' };

  const serverUrl = env(GHA.ServerUrl);
  const repository = env(GHA.Repository);
  const sha = env(GHA.Sha);
  const refValue = env(GHA.Ref);

  const present: string[] = [];
  const problems: string[] = [];

  const repositoryUri = composeRepositoryUri(serverUrl, repository, present, problems);
  const revisionId = readOptionalRevisionId(sha, GHA.Sha, present, problems);
  const branchRef = refValue?.trim() ? (present.push(GHA.Ref), refValue.trim()) : undefined;

  if (problems.length > 0) {
    return { state: 'partial', errorMessage: formatPartialError('GitHub Actions', present, problems) };
  }

  return { state: 'complete', context: { repositoryUri, revisionId, branchRef } };
}

function composeRepositoryUri(
  rawServer: string | undefined,
  rawRepo: string | undefined,
  present: string[],
  problems: string[],
): string | undefined {
  const serverPresent = !!rawServer?.trim();
  const repoPresent = !!rawRepo?.trim();
  if (!serverPresent && !repoPresent) return undefined;
  if (serverPresent) present.push(GHA.ServerUrl);
  if (repoPresent) present.push(GHA.Repository);
  if (!serverPresent || !repoPresent) {
    problems.push(
      `${GHA.ServerUrl} and ${GHA.Repository} must both be set to derive a repository URI; got ${GHA.ServerUrl}='${rawServer ?? ''}', ${GHA.Repository}='${rawRepo ?? ''}'`,
    );
    return undefined;
  }
  const composed = `${rawServer!.trim().replace(/\/+$/, '')}/${rawRepo!.trim().replace(/^\/+/, '')}`;
  let parsed: URL;
  try {
    parsed = new URL(composed);
  } catch {
    problems.push(
      `${GHA.ServerUrl}='${rawServer}' and ${GHA.Repository}='${rawRepo}' do not compose a valid absolute http(s) URI`,
    );
    return undefined;
  }
  if (parsed.protocol !== 'http:' && parsed.protocol !== 'https:') {
    problems.push(
      `${GHA.ServerUrl}='${rawServer}' and ${GHA.Repository}='${rawRepo}' do not compose a valid absolute http(s) URI`,
    );
    return undefined;
  }
  return parsed.toString();
}
