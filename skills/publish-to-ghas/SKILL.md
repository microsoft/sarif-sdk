---
name: publish-to-ghas
description: Uploads a finalized SARIF file to GitHub Advanced Security (GHAS) code scanning via the GitHub code-scanning SARIF API, deriving the GitHub target from the run's version-control provenance.
metadata:
  author: sarif-sdk-maintainers
  version: "1.0.0"
  category: sarif
  severity: medium
  packages:
    - "Sarif.Multitool >= 5.0.7"
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
Unlike GHAzDO, there is **no `Sarif.Multitool` verb** for this upload — the GitHub host accepts a
gzipped, base64-encoded SARIF body directly. This skill drives that upload with the `gh` CLI so the
SARIF you produced with [`emit-sarif`](../emit-sarif/SKILL.md) and the `emit-finalize` verb shows up
as code-scanning alerts.

Publish a SARIF only after it is **finalized** — its first run must carry a GitHub `repositoryUri`,
`revisionId`, and branch under `versionControlProvenance`. The target `owner/repo`, the `commit_sha`,
and the `ref` are read from that provenance, so an unfinalized file cannot be published.

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

- **The secret never appears on the command line.** Place the token in an environment variable
  (`GH_TOKEN`, which `gh` reads automatically) and let `gh` consume it. It is never an argument and
  never printed. If you are already authenticated via `gh auth login`, you do not need to set a token
  at all — `gh` uses the keyring.
- **`github.com` / `*.ghe.com` only.** The target must resolve to a GitHub host from
  `versionControlProvenance`. A `dev.azure.com` (GHAzDO) target is out of scope for this skill.
- **Token scope.** A fine-grained or classic token must carry **`security_events`** write
  (`repo` also grants it for private repositories). Less scope yields HTTP 403.

## Prerequisites

- **`gh` CLI**, authenticated (`gh auth status`), or a `GH_TOKEN` env var with `security_events`.
- **`Sarif.Multitool` ≥ 5.0.7** for the recommended validate pre-flight. Recommended invocation:
  `dotnet dnx Sarif.Multitool --yes -- ...` (zero-install, .NET 10+), or a global
  `dotnet tool install --global Sarif.Multitool` then `sarif ...`.
- A **finalized** SARIF whose first run carries a GitHub `repositoryUri`, `revisionId`, and a
  `refs/heads/...` branch.

## Detection

### Inputs

| Parameter | Required | Description |
|---|---|---|
| `{{SARIF_FILE}}` | Yes | Path to the finalized `.sarif` file to upload |
| `{{TOKEN_ENV_VAR}}` | No | Name of the environment variable holding the token. Default `GH_TOKEN` |

### Step 1 — Set the token in the environment (skip if `gh auth status` is green)

Set the variable in the current session only. Do not write the token to a file, a script, or shell
history.

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

### Step 3 — Resolve the target from provenance (no network)

The `owner/repo`, `commit_sha`, and `ref` come from the SARIF, not from you. Read them and confirm
they match the repository you intend to publish to.

```powershell
$log = Get-Content "{{SARIF_FILE}}" -Raw | ConvertFrom-Json
$vcp = $log.runs[0].versionControlProvenance[0]
$uri = [Uri]$vcp.repositoryUri
if ($uri.Host -notmatch '(^|\.)github\.com$|\.ghe\.com$') {
    throw "repositoryUri host '$($uri.Host)' is not a GitHub host; use publish-to-ghazdo for dev.azure.com."
}
$ownerRepo = $uri.AbsolutePath.Trim('/') -replace '\.git$',''
$commitSha = $vcp.revisionId
$ref       = $vcp.branch          # expected shape: refs/heads/<branch>
"$ownerRepo  $ref  $commitSha"
```

Confirm the reported `owner/repo`, `ref`, and `commit_sha`. If the target is wrong, the SARIF was not
finalized against the intended GitHub repository — re-finalize it; do not hand-edit these values.

### Step 4 — Publish

GitHub requires the SARIF body **gzipped then base64-encoded**. The `gh api` call below reads
`GH_TOKEN` (or the `gh` keyring) for auth — the token is never on the command line.

```powershell
$bytes = [System.IO.File]::ReadAllBytes((Resolve-Path "{{SARIF_FILE}}"))
$ms = [System.IO.MemoryStream]::new()
$gz = [System.IO.Compression.GZipStream]::new($ms, [System.IO.Compression.CompressionMode]::Compress)
$gz.Write($bytes, 0, $bytes.Length); $gz.Dispose()
$sarifB64 = [Convert]::ToBase64String($ms.ToArray())

gh api "repos/$ownerRepo/code-scanning/sarifs" -X POST `
  -f commit_sha="$commitSha" `
  -f ref="$ref" `
  -f sarif="$sarifB64"
```

A successful call returns `201` with an `{ "id": ..., "url": ... }` body — the URL is the analysis
status endpoint. Poll it until `"processing_status": "complete"`:

```powershell
gh api "repos/$ownerRepo/code-scanning/sarifs/<id>"
```

A non-`complete` status with `errors` means GitHub rejected the analysis; the body names the cause.

## Edge Cases

1. **No provenance** — The run carries no `versionControlProvenance[0].repositoryUri`. Finalize the
   SARIF (`emit-finalize`) before publishing.
2. **Non-GitHub target** — The repository is `dev.azure.com` or a legacy host. Out of scope; use
   `publish-to-ghazdo`.
3. **`ref` is not `refs/heads/...`** — The upload needs a fully-qualified ref. Re-finalize with the
   correct branch; a bare branch name is rejected.
4. **`commit_sha` not in the repo** — GitHub rejects an analysis whose commit it cannot find. Publish
   against a SARIF finalized at a commit that has been pushed.
5. **HTTP 403** — The token lacks `security_events` (or `repo` for a private repo), or GHAS / code
   scanning is not enabled on the repository.
6. **HTTP 404** — `owner/repo` does not resolve, or the token cannot see it.
7. **No security severity on alerts** — The alerts appear but without critical/high/medium/low. The
   rules are missing the `security` tag; re-finalize so `emit-finalize` stamps it (Step 2 catches
   this offline).

## Validation

After running this skill:

1. The token never appeared in any printed command, log line, or error message.
2. The resolved `owner/repo` + `ref` + `commit_sha` matched the intended GitHub repository before the
   upload.
3. The upload returned `201` and the analysis reached `processing_status: complete`.
4. The resulting alerts carry a security severity (critical/high/medium/low) — confirming the
   `security` tag and `security-severity` survived finalize.

## Escalation

- **`gh` not available / not authenticated** — Report the gap; the SARIF cannot be published from
  this environment.
- **Repeated 4xx / non-`complete` status** — After confirming target, ref, commit, and token scope,
  capture the analysis `errors` body (it contains no secret) and escalate to the repository's
  Advanced Security administrator.
