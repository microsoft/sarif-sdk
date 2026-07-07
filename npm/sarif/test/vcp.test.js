// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Direct unit coverage for the portable-root derivation in vcp.ts: the scp
 * clone-URL parser, host classification (GitHub / Azure DevOps / legacy), and
 * the GitHub-hosted-run default-deny gate. These branch-heavy paths were
 * previously exercised only transitively through emit-finalize rebase
 * scenarios.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  tryValidateRepositoryUri,
  tryDerivePortableRoot,
  isGitHubHostedRun,
  parseAuthorizedHosts,
} from '../dist/index.js';

test('tryDerivePortableRoot mints a GitHub blob root from an https clone URL', () => {
  const r = tryDerivePortableRoot('https://github.com/contoso/widgets', 'abc123');
  assert.ok(r.ok);
  assert.equal(r.value.portableRoot, 'https://github.com/contoso/widgets/blob/abc123/');
  assert.equal(r.value.canonicalRepositoryUri, 'https://github.com/contoso/widgets');
  assert.equal(r.value.leaf, 'widgets');
  assert.equal(r.value.revisionWebUrl, 'https://github.com/contoso/widgets/tree/abc123');
});

test('tryDerivePortableRoot accepts a github scp clone URL and strips the .git suffix', () => {
  const r = tryDerivePortableRoot('git@github.com:contoso/widgets.git', 'deadbeef');
  assert.ok(r.ok);
  assert.equal(r.value.leaf, 'widgets');
  assert.equal(r.value.portableRoot, 'https://github.com/contoso/widgets/blob/deadbeef/');
});

test('tryDerivePortableRoot mints an Azure DevOps _git root', () => {
  const r = tryDerivePortableRoot('https://dev.azure.com/myorg/myproj/_git/myrepo', 'cafe');
  assert.ok(r.ok);
  assert.equal(r.value.leaf, 'myrepo');
  assert.equal(r.value.portableRoot, 'https://dev.azure.com/myorg/myproj/_git/myrepo/');
  assert.equal(
    r.value.revisionWebUrl,
    'https://dev.azure.com/myorg/myproj/_git/myrepo?version=GCcafe',
  );
});

test('tryValidateRepositoryUri rejects an empty repositoryUri', () => {
  assert.equal(tryValidateRepositoryUri('').ok, false);
});

test('tryValidateRepositoryUri rejects an embedded-credential URL', () => {
  assert.equal(tryValidateRepositoryUri('https://user:pass@github.com/o/r').ok, false);
});

test('tryValidateRepositoryUri rejects an ambiguous double-@ scp form', () => {
  assert.equal(tryValidateRepositoryUri('git@@github.com:o/r').ok, false);
});

test('tryValidateRepositoryUri rejects an unsupported host', () => {
  assert.equal(tryValidateRepositoryUri('https://gitlab.com/o/r').ok, false);
});

test('tryValidateRepositoryUri rejects the legacy visualstudio.com host', () => {
  assert.equal(tryValidateRepositoryUri('https://org.visualstudio.com/proj/_git/repo').ok, false);
});

test('isGitHubHostedRun is true when every VCP entry is a GitHub host', () => {
  assert.equal(
    isGitHubHostedRun({ versionControlProvenance: [{ repositoryUri: 'https://github.com/o/r' }] }),
    true,
  );
});

test('isGitHubHostedRun is false when there is no VCP', () => {
  assert.equal(isGitHubHostedRun({}), false);
  assert.equal(isGitHubHostedRun({ versionControlProvenance: [] }), false);
});

test('isGitHubHostedRun is false when any entry is a non-GitHub host', () => {
  assert.equal(
    isGitHubHostedRun({
      versionControlProvenance: [
        { repositoryUri: 'https://github.com/o/r' },
        { repositoryUri: 'https://dev.azure.com/o/p/_git/r' },
      ],
    }),
    false,
  );
});

// --- Enterprise GitHub (GHES) host attestation -------------------------------
// A self-hosted GitHub Enterprise Server uses a custom domain (e.g. Walmart's
// gecgithub01.walmart.com) that is structurally indistinguishable from any
// other host, so it cannot be auto-recognized. When the caller attests a host
// as `ghe`, its permalinks are byte-identical to github.com
// ({host}/{owner}/{repo}/blob/{sha}/), so the existing GitHub derivation path
// applies unchanged.

const GHES_URI = 'https://gecgithub01.walmart.com/AI-INNOVATION-LAB/pawprints';
const GHES_HOSTS = [{ type: 'ghe', host: 'gecgithub01.walmart.com' }];

test('parseAuthorizedHosts parses <hostType>:<host> tokens', () => {
  const r = parseAuthorizedHosts('ghe:githost.contoso.com, ghe:Other.Example.com');
  assert.ok(r.ok);
  assert.deepEqual(r.value, [
    { type: 'ghe', host: 'githost.contoso.com' },
    { type: 'ghe', host: 'other.example.com' },
  ]);
});

test('parseAuthorizedHosts returns an empty list for empty input', () => {
  const r = parseAuthorizedHosts(undefined);
  assert.ok(r.ok);
  assert.deepEqual(r.value, []);
});

test('parseAuthorizedHosts rejects a token missing the host type', () => {
  assert.equal(parseAuthorizedHosts('githost.contoso.com').ok, false);
});

test('parseAuthorizedHosts rejects an unknown host type', () => {
  assert.equal(parseAuthorizedHosts('gitlab:git.contoso.com').ok, false);
});

test('tryDerivePortableRoot mints a GitHub-style blob root for an attested ghe host', () => {
  const r = tryDerivePortableRoot(GHES_URI, 'abc123', { authorizedHosts: GHES_HOSTS });
  assert.ok(r.ok);
  assert.equal(
    r.value.portableRoot,
    'https://gecgithub01.walmart.com/AI-INNOVATION-LAB/pawprints/blob/abc123/',
  );
  assert.equal(r.value.leaf, 'pawprints');
  assert.equal(
    r.value.revisionWebUrl,
    'https://gecgithub01.walmart.com/AI-INNOVATION-LAB/pawprints/tree/abc123',
  );
});

test('tryValidateRepositoryUri accepts an attested ghe host', () => {
  const r = tryValidateRepositoryUri(GHES_URI, { authorizedHosts: GHES_HOSTS });
  assert.ok(r.ok);
  assert.equal(r.value.leaf, 'pawprints');
});

test('ghe-host attestation is case-insensitive on the host', () => {
  const r = tryValidateRepositoryUri(GHES_URI, {
    authorizedHosts: [{ type: 'ghe', host: 'GECGitHub01.Walmart.com' }],
  });
  assert.ok(r.ok);
});

test('tryValidateRepositoryUri still rejects an unattested host (default-deny preserved)', () => {
  assert.equal(tryValidateRepositoryUri(GHES_URI).ok, false);
});

test('isGitHubHostedRun is true when every VCP entry is an attested ghe host', () => {
  assert.equal(
    isGitHubHostedRun(
      { versionControlProvenance: [{ repositoryUri: GHES_URI }] },
      { authorizedHosts: GHES_HOSTS },
    ),
    true,
  );
});
