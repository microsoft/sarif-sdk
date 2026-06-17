---
name: publish-to-ghas
description: Uploads a finalized SARIF file to GitHub Advanced Security (GHAS) code scanning using the Sarif.Multitool publish-to-ghas verb, deriving the GitHub target, commit, and ref from the run's version-control provenance.
metadata:
  author: sarif-sdk-maintainers
  version: "2.0.0"
  category: sarif
  severity: medium
  packages:
    - "Sarif.Multitool >= 5.1.0"
  triggers:
    - "publish sarif"
    - "upload sarif"
    - "ghas"
    - "github advanced security"
    - "code scanning"
    - "sarif ingestion"
---

# Publish SARIF to GHAS (GitHub code scanning)

## Context

GitHub Advanced Security (GHAS) ingests SARIF through the GitHub code-scanning SARIF API
([`POST /repos/{owner}/{repo}/code-scanning/sarifs`](https://docs.github.com/en/rest/code-scanning/code-scanning#upload-an-analysis-as-sarif-data)).
The [`publish-to-ghas`](../../src/Sarif.Multitool.Library/PublishToGhas/PublishToGhasCommand.cs) verb
uploads a finalized SARIF file to that endpoint: it derives the GitHub `owner/repo`, the `commit_sha`,
and the `ref` from the run's version-control provenance, gzip-compresses and base64-encodes the body
into the JSON payload, and posts it under a caller-supplied bearer token. The verb is the companion of
[`publish-to-ghazdo`](../publish-to-ghazdo/SKILL.md) for Azure DevOps targets, and shares its
log-level refusal of an unpublishable (`--no-repo`) log.

Publish a SARIF only after it is **finalized** — its first run must carry a GitHub `repositoryUri`,
`revisionId`, and branch under `versionControlProvenance` (see [`emit-sarif`](../emit-sarif/SKILL.md)
and the `emit-finalize` verb). The target `owner/repo`, the `commit_sha`, and the `ref` are read from
that provenance, so an unfinalized file cannot be published.

### Why finalize matters for GHAS specifically

`emit-finalize` stamps the metadata GHAS needs to treat a finding as a *security* alert:

- **`security-severity`** on each AI rule (curated per-CWE prior, or a neutral medium default for
  uncurated content). GitHub buckets the alert critical/high/medium/low from this value.
- **`tags`** on each AI rule — `["security", "external/cwe/cwe-<n>"]` for a CWE rule, `["security"]`
  for a `NOVEL-` rule. **GitHub ignores `security-severity` unless the `security` tag is present**,
  so an unfinalized file uploads as plain code-scanning results with no security severity.

These tags are emitted only for GitHub-hosted runs; an Azure DevOps-hosted SARIF will not carry them
(publish that one with [`publish-to-ghazdo`](../publish-to-ghazdo/SKILL.md) instead).

## Security model

This is the security-critical part of the skill. Follow it exactly.

- **The token never appears on the command line.** Place the token in an environment variable and name
  that variable with `--token-env-var` (default `GH_TOKEN`). The verb reads the value from the
  environment; it is never an argument, and it is never printed — not in dry-run output, not in error
  messages, and it is redacted from any server response body or exception text.
- **Always sent as `Authorization: Bearer`.** A classic or fine-grained GitHub personal access token
  carrying **`security_events`** write (`repo` also grants it for private repositories) is sent as a
  bearer token. Less scope yields HTTP 403.
- **GitHub hosts only.** The target must resolve to `github.com` or a `<slug>.ghe.com` data-residency
  host from `versionControlProvenance`; the API host is `api.github.com` or `api.<slug>.ghe.com`
  respectively. A `dev.azure.com` (GHAzDO) target, a legacy host, and credential-bearing repository
  URLs are rejected. A GitHub SSH/scp clone URL is normalized to its https identity.

## Prerequisites

- **`Sarif.Multitool` ≥ 5.1.0.** Recommended invocation: `dotnet dnx Sarif.Multitool --yes -- publish-to-ghas ...`
  (zero-install, requires .NET 10+). Fall back to `dotnet tool install --global Sarif.Multitool`,
  then invoke as `sarif publish-to-ghas ...`.
- A **finalized** SARIF whose first run carries a GitHub `repositoryUri`, `revisionId`, and a
  `refs/heads/...` branch.
- A GitHub personal access token with `security_events` write, stored in an environment variable.

## Detection

### Inputs

| Parameter | Required | Description |
|---|---|---|
| `{{SARIF_FILE}}` | Yes | Path to the finalized `.sarif` file to upload |
| `{{TOKEN_ENV_VAR}}` | No | Name of the environment variable holding the token. Default `GH_TOKEN` |

### Step 1 — Set the token in the environment

Set the variable in the current session only. Do not write the token to a file, a script, or shell
history. Use your platform's mechanism for assigning a value without echoing it.

```powershell
# PowerShell — paste the token when prompted; it is not echoed to history.
$env:GH_TOKEN = (Read-Host -AsSecureString | ForEach-Object { [System.Net.NetworkCredential]::new('', $_).Password })
```

```bash
# bash — read without echo.
read -rs GH_TOKEN && export GH_TOKEN
```

### Step 2 — Validate pre-flight (no network)

GHAS ingestion-blocking defects are detectable offline. A clean validation under the GHAS rule
kinds is the offline equivalent of upload acceptance — run it before the round-trip.

```powershell
dotnet dnx Sarif.Multitool --yes -- validate "{{SARIF_FILE}}" --rule-kind "Sarif;AI;Ghas"
```

Resolve any **error**-level findings before publishing. In particular, GHAS will not surface a
security severity unless each AI rule carries the `security` tag — re-finalize (`emit-finalize`) the
SARIF rather than hand-editing tags.

### Step 3 — Dry run (no network)

Always dry-run first. This resolves the GitHub target, the `ref`, and the `commit_sha` from the
SARIF's provenance and prints the request shape **without contacting the server and without printing
the token**.

```powershell
dotnet dnx Sarif.Multitool --yes -- publish-to-ghas "{{SARIF_FILE}}" --dry-run
```

Confirm the reported `owner/repo`, `ref`, and `commit_sha`. If the target is wrong, the SARIF was not
finalized against the intended GitHub repository — re-finalize it; do not hand-edit these values.

### Step 4 — Publish

```powershell
dotnet dnx Sarif.Multitool --yes -- publish-to-ghas "{{SARIF_FILE}}"
```

To name a different environment variable:

```powershell
dotnet dnx Sarif.Multitool --yes -- publish-to-ghas "{{SARIF_FILE}}" --token-env-var "{{TOKEN_ENV_VAR}}"
```

The verb posts to `api.github.com` (or `api.<slug>.ghe.com`). Exit code `0` means the server accepted
the upload (HTTP 2xx — GitHub answers `202 Accepted`); a non-zero exit code with `error: publish
failed with HTTP <code>` means it did not. The verb prints the analysis response body (with the token
redacted) — its `url` is the analysis status endpoint, which you can poll with `gh api` until
`"processing_status": "complete"`.

**If `dotnet dnx` is not available:** install the global tool and use the `sarif` command name:

```powershell
dotnet tool install --global Sarif.Multitool
sarif publish-to-ghas "{{SARIF_FILE}}"
```

## Edge Cases

1. **No provenance** — The run carries no `versionControlProvenance[0].repositoryUri`. The verb fails
   closed. Finalize the SARIF (`emit-finalize`) before publishing.
2. **Unpublishable (repo-less) log** — Any run carries `properties.unpublishable = true`, stamped by
   `emit-finalize --no-repo` for a scan outside version control. The verb refuses the whole file up
   front (publishing ingests every run): a non-version-controlled scan has no repository or commit to
   anchor alerts to. Publish a log whose runs were all finalized against a checked-out repository
   (without `--no-repo`); split a merged log first if only some runs are unpublishable.
3. **Non-GitHub target** — The repository is `dev.azure.com` or a legacy host. The verb rejects it;
   use `publish-to-ghazdo`.
4. **Missing `revisionId` or `branch`** — GHAS anchors an analysis to a commit and a fully-qualified
   ref (`refs/heads/...`). The verb fails closed if either is absent; re-finalize with both present.
5. **`commit_sha` not in the repo** — GitHub rejects an analysis whose commit it cannot find. Publish
   against a SARIF finalized at a commit that has been pushed.
6. **HTTP 403** — The token lacks `security_events` (or `repo` for a private repo), or GHAS / code
   scanning is not enabled on the repository.
7. **HTTP 404** — `owner/repo` does not resolve, or the token cannot see it.
8. **No security severity on alerts** — The alerts appear but without critical/high/medium/low. The
   rules are missing the `security` tag; re-finalize so `emit-finalize` stamps it (Step 2 catches
   this offline).

## Validation

After running this skill:

1. The token never appeared in any printed command, log line, or error message.
2. The GHAS-ruleset validation (`--rule-kind "Sarif;AI;Ghas"`) was clean before the live publish.
3. The dry-run `owner/repo` + `ref` + `commit_sha` matched the intended GitHub repository.
4. A successful publish returned exit code `0` and the analysis reached `processing_status: complete`.
5. The resulting alerts carry a security severity (critical/high/medium/low) — confirming the
   `security` tag and `security-severity` survived finalize.

## Escalation

- **Multitool not available** — Neither `dotnet dnx` nor a global install succeeded. Report the gap;
  the SARIF cannot be published from this environment.
- **Repeated 4xx / non-`complete` status** — After confirming target, ref, commit, and token scope,
  capture the analysis `errors` body the verb prints (it contains no secret) and escalate to the
  repository's Advanced Security administrator.
