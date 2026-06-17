// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Single source of truth for turning a `versionControlProvenance.repositoryUri`
 * into a portable artifact root. emit-run validates the shape at receipt;
 * emit-finalize mints the root.
 *
 * Ported from src/Sarif.Multitool.Library/Emit/VcpPortableRoot.cs.
 */

import type { Run } from './sarif.js';

const AZURE_DEVOPS_HOST = 'dev.azure.com';
const AZURE_DEVOPS_SSH_HOST = 'ssh.dev.azure.com';
const INVALID_SCP_HOST_CHARS = /[@/\\:?#\s]/;

interface Classification {
  isGitHub: boolean;
  isAzureDevOps: boolean;
  schemeAndServer: string;
  adoPrefix?: string; // <org>/<project>, percent-encoded
  owner?: string;
  repoForUrl: string;
  leaf: string;
  display: string;
}

export interface PortableRoot {
  portableRoot: string;
  canonicalRepositoryUri: string;
  leaf: string;
  revisionWebUrl: string;
}

type Result<T> = { ok: true; value: T } | { ok: false; error: string };

function ok<T>(value: T): Result<T> {
  return { ok: true, value };
}
function err<T>(error: string): Result<T> {
  return { ok: false, error };
}

function isGitHubHost(host: string): boolean {
  const h = host.toLowerCase();
  return h === 'github.com' || h.endsWith('.ghe.com');
}

function isLegacyAzureDevOpsHost(host: string): boolean {
  const h = host.toLowerCase();
  return h === 'visualstudio.com' || h.endsWith('.visualstudio.com');
}

function sanitizeForDisplay(u: URL): string {
  // Never echo credentials or query/fragment in diagnostics.
  return `${u.protocol}//${u.host}${u.pathname}`;
}

function isSingleSafeSegment(rawSegment: string): boolean {
  if (!rawSegment) return false;
  const decoded = decodeURIComponent(rawSegment);
  return (
    decoded.length !== 0 &&
    decoded !== '.' &&
    decoded !== '..' &&
    !decoded.includes('/') &&
    !decoded.includes('\\')
  );
}

function normalizeRepoSegment(rawSegment: string): Result<{ repoForUrl: string; leaf: string }> {
  let repo = rawSegment;
  if (repo.toLowerCase().endsWith('.git')) repo = repo.slice(0, -4);
  const decoded = decodeURIComponent(repo);
  if (
    decoded.length === 0 ||
    decoded === '.' ||
    decoded === '..' ||
    decoded.includes('/') ||
    decoded.includes('\\')
  ) {
    return err('the repository name is empty or is not a single valid path segment.');
  }
  return ok({ repoForUrl: repo, leaf: decoded });
}

// scp syntax: [user@]host:path with no scheme and no slash before the colon.
function tryParseScp(raw: string): { host: string; userInfo: string; path: string } | undefined {
  if (!raw || raw.includes('://')) return undefined;
  const colon = raw.indexOf(':');
  if (colon < 0) return undefined;
  const authority = raw.slice(0, colon);
  if (authority.length === 0 || authority.includes('/')) return undefined;
  let userInfo = '';
  let host: string;
  const at = authority.indexOf('@');
  if (at >= 0) {
    if (authority.indexOf('@', at + 1) >= 0) return undefined; // ambiguous double @
    userInfo = authority.slice(0, at);
    host = authority.slice(at + 1);
    if (userInfo.length === 0) return undefined;
  } else {
    host = authority;
  }
  if (host.length === 0 || INVALID_SCP_HOST_CHARS.test(host)) return undefined;
  const path = raw.slice(colon + 1);
  if (path.includes('@')) return undefined; // colon was inside user:password
  if (path.length === 0) return undefined;
  return { host, userInfo, path };
}

function tryNormalizeToHttps(rawRepositoryUri: string): Result<URL> {
  // Try absolute URL first.
  let absolute: URL | undefined;
  try {
    absolute = new URL(rawRepositoryUri);
  } catch {
    /* maybe scp form */
  }

  if (absolute && absolute.protocol === 'https:') return ok(absolute);

  let host: string, userInfo: string, path: string;

  if (absolute && absolute.protocol === 'ssh:') {
    if (absolute.port) {
      return err('an ssh repositoryUri must use the default port; supply the https clone URL instead.');
    }
    if (absolute.search || absolute.hash) {
      return err(
        'an ssh repositoryUri must be a bare clone URL with no query or fragment; supply the https clone URL instead.',
      );
    }
    host = absolute.hostname;
    userInfo = absolute.username + (absolute.password ? `:${absolute.password}` : '');
    path = absolute.pathname;
  } else {
    const scp = tryParseScp(rawRepositoryUri);
    if (!scp) {
      return err(
        'repositoryUri must be an absolute https URL, or a github-compatible ssh/scp clone URL (git@host:owner/repo).',
      );
    }
    ({ host, userInfo, path } = scp);
  }

  if (userInfo.includes(':')) {
    return err(
      'a repositoryUri carrying credentials (user:password@) cannot be used to derive a portable root.',
    );
  }

  const normalizedPath = path.replace(/\\/g, '/').replace(/^\/+/, '');

  if (
    host.toLowerCase() === AZURE_DEVOPS_SSH_HOST ||
    normalizedPath.toLowerCase().startsWith('v3/')
  ) {
    return err(
      'azure devops ssh repositoryUri normalization is not supported; supply the https clone URL (https://dev.azure.com/<org>/<project>/_git/<repo>).',
    );
  }

  try {
    return ok(new URL(`https://${host}/${normalizedPath}`));
  } catch {
    return err(
      'repositoryUri must be an absolute https URL, or a github-compatible ssh/scp clone URL (git@host:owner/repo).',
    );
  }
}

function classify(rawRepositoryUri: string | undefined): Result<Classification> {
  if (!rawRepositoryUri) {
    return err('repositoryUri is required so a portable root can be derived.');
  }

  const norm = tryNormalizeToHttps(rawRepositoryUri);
  if (!norm.ok) return norm;
  const u = norm.value;

  const display = sanitizeForDisplay(u);

  if (u.port) {
    return err(`repositoryUri '${display}' must use the host's default port.`);
  }
  if (u.search || u.hash) {
    return err(`repositoryUri '${display}' must be a bare repository URL with no query or fragment.`);
  }
  if (u.username || u.password) {
    return err(
      'a repositoryUri must not carry embedded credentials (no account@ or account:password@); supply the clean https repository URL.',
    );
  }

  const segments = u.pathname.split('/').filter((s) => s.length > 0);
  const schemeAndServer = `${u.protocol}//${u.host}`;
  const host = u.hostname;

  if (isGitHubHost(host)) {
    if (segments.length !== 2) {
      return err(`github repositoryUri must take the form https://<host>/<owner>/<repo>; '${display}' did not.`);
    }
    const repo = normalizeRepoSegment(segments[1]);
    if (!repo.ok) return err(`github repositoryUri '${display}': ${repo.error}`);
    return ok({
      isGitHub: true,
      isAzureDevOps: false,
      schemeAndServer,
      owner: segments[0],
      repoForUrl: repo.value.repoForUrl,
      leaf: repo.value.leaf,
      display,
    });
  }

  if (host.toLowerCase() === AZURE_DEVOPS_HOST) {
    if (segments.length !== 4 || segments[2].toLowerCase() !== '_git') {
      return err(
        `azure devops repositoryUri must take the form https://dev.azure.com/<org>/<project>/_git/<repo>; '${display}' did not.`,
      );
    }
    if (!isSingleSafeSegment(segments[0]) || !isSingleSafeSegment(segments[1])) {
      return err(
        `azure devops repositoryUri '${display}': the organization and project must each be a single valid path segment.`,
      );
    }
    const repo = normalizeRepoSegment(segments[3]);
    if (!repo.ok) return err(`azure devops repositoryUri '${display}': ${repo.error}`);
    return ok({
      isGitHub: false,
      isAzureDevOps: true,
      schemeAndServer,
      adoPrefix: `${segments[0]}/${segments[1]}`,
      repoForUrl: repo.value.repoForUrl,
      leaf: repo.value.leaf,
      display,
    });
  }

  if (isLegacyAzureDevOpsHost(host)) {
    return err(
      `the legacy Azure DevOps host form is not supported; supply the repository as https://dev.azure.com/<org>/<project>/_git/<repo>. '${display}' did not.`,
    );
  }

  return err(
    `portable root derivation supports GitHub (github.com or <slug>.ghe.com) and Azure DevOps (dev.azure.com); '${display}' is not a supported host.`,
  );
}

/** Validates that a portable root can be derived (no revisionId required). */
export function tryValidateRepositoryUri(rawRepositoryUri: string): Result<{ leaf: string }> {
  const c = classify(rawRepositoryUri);
  return c.ok ? ok({ leaf: c.value.leaf }) : c;
}

/** Mints the portable root for a repository URI + revision id. */
export function tryDerivePortableRoot(
  rawRepositoryUri: string,
  revisionId: string,
): Result<PortableRoot> {
  const cr = classify(rawRepositoryUri);
  if (!cr.ok) return cr;
  const c = cr.value;

  const canonicalRepositoryUri = c.display;
  const rev = encodeURIComponent(revisionId);

  const portableRoot = c.isAzureDevOps
    ? `${c.schemeAndServer}/${c.adoPrefix}/_git/${c.repoForUrl}/`
    : `${c.schemeAndServer}/${c.owner}/${c.repoForUrl}/blob/${rev}/`;

  const revisionWebUrl = c.isAzureDevOps
    ? `${c.schemeAndServer}/${c.adoPrefix}/_git/${c.repoForUrl}?version=GC${rev}`
    : `${c.schemeAndServer}/${c.owner}/${c.repoForUrl}/tree/${rev}`;

  return ok({ portableRoot, canonicalRepositoryUri, leaf: c.leaf, revisionWebUrl });
}

/**
 * Default-deny: true only when the run carries ≥1 VCP entry and every entry's
 * repositoryUri classifies as a supported GitHub host. Mirrors
 * VcpPortableRoot.IsGitHubHostedRun.
 */
export function isGitHubHostedRun(run: Run): boolean {
  const vcp = run.versionControlProvenance;
  if (!vcp || vcp.length === 0) return false;
  for (const d of vcp) {
    if (!d) return false;
    const c = classify(d.repositoryUri);
    if (!c.ok || !c.value.isGitHub) return false;
  }
  return true;
}
