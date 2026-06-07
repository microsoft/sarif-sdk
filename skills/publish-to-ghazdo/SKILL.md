---
name: publish-to-ghazdo
description: Uploads a finalized SARIF file to GitHub Advanced Security for Azure DevOps (GHAzDO) using the Sarif.Multitool publish-to-ghazdo verb.
metadata:
  author: sarif-sdk-maintainers
  version: "1.0.0"
  category: sarif
  severity: medium
  packages:
    - "Sarif.Multitool >= 5.0.3"
  triggers:
    - "publish sarif"
    - "upload sarif"
    - "ghazdo"
    - "advanced security azure devops"
    - "sarif ingestion"
---

# Publish SARIF to GHAzDO

## Context

GitHub Advanced Security for Azure DevOps (GHAzDO) ingests SARIF through the Advanced
Security alert API. The [`publish-to-ghazdo`](../../src/Sarif.Multitool.Library/PublishToGhazdo/PublishToGhazdoCommand.cs)
verb uploads a finalized SARIF file to that endpoint: it derives the Azure DevOps target from the
run's version-control provenance, compresses the body, and posts it under a caller-supplied bearer
secret.

Publish a SARIF only after it is **finalized** — its first run must carry an Azure DevOps
`repositoryUri` and `revisionId` under `versionControlProvenance` (see
[`emit-sarif`](../emit-sarif/SKILL.md) and the `emit-finalize` verb). The target
organization, project, and repository are read from that provenance, so an unfinalized file cannot
be published.

## Security model

This is the security-critical part of the skill. Follow it exactly.

- **The secret never appears on the command line.** Place the secret in an environment variable and
  name that variable with `--token-env-var` (default `GHAZDO_TOKEN`). The verb reads the value from
  the environment; it is never an argument, and it is never printed — not in dry-run output, not in
  error messages.
- **Two secret kinds, two schemes (auto-detected).** An Entra access token is a JSON Web Token and
  is sent as `Authorization: Bearer <token>`. An Azure DevOps personal access token (PAT) is opaque
  and is sent as `Authorization: Basic base64(":" + PAT)`. The verb detects which by shape; you do
  not configure the scheme.
- **`dev.azure.com` only.** The target must be `https://dev.azure.com/<org>/<project>/_git/<repo>`.
  Legacy `<org>.visualstudio.com`, SSH, and credential-bearing repository URLs are rejected.
- **PAT scope.** A PAT must carry **Advanced Security (Read & Write)**. An Entra token must be issued
  for the Azure DevOps resource (`499b84ac-1321-427f-aa17-267ca6975798`).

## Prerequisites

- **`Sarif.Multitool` ≥ 5.0.3.** Recommended invocation: `dotnet dnx Sarif.Multitool --yes -- publish-to-ghazdo ...`
  (zero-install, requires .NET 10+). Fall back to `dotnet tool install --global Sarif.Multitool`,
  then invoke as `sarif publish-to-ghazdo ...`.
- A **finalized** SARIF file whose first run carries an Azure DevOps `repositoryUri`.
- A bearer secret (PAT or Entra access token) stored in an environment variable.

## Detection

### Inputs

| Parameter | Required | Description |
|---|---|---|
| `{{SARIF_FILE}}` | Yes | Path to the finalized `.sarif` file to upload |
| `{{TOKEN_ENV_VAR}}` | No | Name of the environment variable holding the secret. Default `GHAZDO_TOKEN` |
| `{{API_VERSION}}` | No | Advanced Security ingestion API version. Default `7.2-preview.1` |

### Step 1 — Set the secret in the environment

Set the variable in the current session only. Do not write the secret to a file, a script, or shell
history. Use your platform's mechanism for assigning a value without echoing it.

```powershell
# PowerShell — paste the secret when prompted; it is not echoed to history.
$env:GHAZDO_TOKEN = (Read-Host -AsSecureString | ForEach-Object { [System.Net.NetworkCredential]::new('', $_).Password })
```

```bash
# bash — read without echo.
read -rs GHAZDO_TOKEN && export GHAZDO_TOKEN
```

### Step 2 — Dry run (no network)

Always dry-run first. This resolves the target, reports the auth scheme that would be used, and
prints the request shape **without contacting the server and without printing the secret**.

```powershell
dotnet dnx Sarif.Multitool --yes -- publish-to-ghazdo "{{SARIF_FILE}}" --dry-run
```

Confirm the reported `org/project/repo`, the `auth scheme` (`Bearer` for an Entra token, `Basic` for
a PAT), and the candidate POST URLs. If the target is wrong, the SARIF was not finalized against the
intended Azure DevOps repository — re-finalize it; do not hand-edit the URL.

### Step 3 — Publish

```powershell
dotnet dnx Sarif.Multitool --yes -- publish-to-ghazdo "{{SARIF_FILE}}"
```

To name a different environment variable or API version:

```powershell
dotnet dnx Sarif.Multitool --yes -- publish-to-ghazdo "{{SARIF_FILE}}" --token-env-var "{{TOKEN_ENV_VAR}}" --api-version "{{API_VERSION}}"
```

The verb posts to `advsec.dev.azure.com` and falls back to `dev.azure.com` on a 404. Exit code `0`
means the server accepted the upload (HTTP < 400); a non-zero exit code with `error: publish failed
with HTTP <code>` means it did not.

**If `dotnet dnx` is not available:** install the global tool and use the `sarif` command name:

```powershell
dotnet tool install --global Sarif.Multitool
sarif publish-to-ghazdo "{{SARIF_FILE}}"
```

## Edge Cases

1. **No provenance** — The run carries no `versionControlProvenance[0].repositoryUri`. The verb fails
   closed. Finalize the SARIF (`emit-finalize`) before publishing.
2. **Non-Azure-DevOps target** — The repository is GitHub or a legacy `visualstudio.com` host. The
   verb rejects it; publish supports `dev.azure.com` only.
3. **Secret unset** — The named environment variable is empty or missing. The verb fails closed
   before any network call. (Dry-run still prints the request shape and reports the scheme as
   undetermined.)
4. **404 on both hosts** — The org/project/repo path does not resolve, or the repository is not
   onboarded to Advanced Security. Verify the target and PAT scope.
5. **401/403** — The secret is invalid, expired, or lacks **Advanced Security (Read & Write)**.

## Validation

After running this skill:

1. The secret never appeared in any printed command, log line, or error message.
2. The dry-run target matched the intended Azure DevOps repository before the live publish.
3. A successful publish returned exit code `0`; a rejected one surfaced the HTTP status and a
   non-zero exit code.

## Escalation

- **Multitool not available** — Neither `dotnet dnx` nor a global install succeeded. Report the gap;
  the SARIF cannot be published from this environment.
- **Repeated HTTP 4xx** — After confirming target and scope, capture the response body the verb
  prints (it does not contain the secret) and escalate to the repository's Advanced Security
  administrator.
