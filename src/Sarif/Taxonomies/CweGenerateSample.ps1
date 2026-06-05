# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

<#
.SYNOPSIS
    Emits CweSample.sarif (default) or CweGHAzDoSample.sarif (-GHAzDO) — a
    deterministic, fully-enriched SARIF fixture that exercises the multitool
    emit chain end-to-end and passes the validator with zero Errors,
    zero Warnings, and zero Notes under the relevant rule-kinds.

.DESCRIPTION
    Convention: every taxonomy that ships with this SDK includes a
    <Taxonomy>GenerateSample.ps1 alongside its data, and that script
    produces a checked-in <Taxonomy>Sample.sarif fixture next to itself.
    CI re-runs the script and asserts the working tree stays byte-identical.

    Two variants, one script:
      * Default (no switch) writes CweSample.sarif and validates with
        --rule-kind Sarif;AI. This is the "AI scanner running anywhere"
        shape — no ADO-pipeline identity is claimed.
      * -GHAzDO writes CweGHAzDoSample.sarif and validates with
        --rule-kind Sarif;AI;GHAzDO. This is the "AI scanner running
        inside an Azure DevOps pipeline" shape — the GHAzDO ingestion
        contract for automationDetails.id + the four
        azuredevops/pipeline/build/* properties is satisfied. The script
        sets the ADO predefined environment variables (TF_BUILD,
        SYSTEM_COLLECTIONURI, …) to deterministic constants for the
        duration of emit-run; the multitool's AdoPipelineContext
        detector reads them and stamps the automationDetails. The env
        vars are cleared in a finally block so they don't leak to other
        steps in the same shell.

    Pipeline:

      1. emit-run — opens .wip.jsonl with rich run-header metadata:
           tool.driver { name, version, semanticVersion, informationUri }
           run.versionControlProvenance[0]
           run.originalUriBaseIds.SRCROOT (local file:// so the
             InsertOptionalDataVisitor can read SampleCode.cs at finalize)
           run.automationDetails { guid, correlationGuid }   (fixed)
           run.properties.ai/origin = "generated"

      2. add-result × 7 — each Result is a fully-formed SARIF object piped
         in as JSON. Per-result payload carries:
           message.text + message.markdown
           rank (numeric, derived from level)
           properties.ai/exploitability (spread across the AI2014 vocab)
           properties.ai/attackerPosition (spread across a vocab demo)
           locations[].physicalLocation { artifactLocation, region }

      3. add-invocation × 1 — a fully-formed invocation (executionSuccessful +
         commandLine + a real workingDirectory + arguments + preset endTimeUtc)
         carrying one toolExecutionNotification INLINE with a producer-supplied
         timeUtc (the verb requires it and never stamps it) so the fixture's
         bytes are stable across re-runs. (SARIF has no run-level notifications
         array, so notifications ride inside the invocation that owns them.)

      4. emit-finalize --embed-text-files
         Enrichment runs against the local SRCROOT (snippets, hashes,
         contextRegion, charOffset). --embed-text-files inlines
         SampleCode.cs into run.artifacts[].contents.text so the fixture
         is self-contained (clears SARIF2013). emit-finalize then
         deconstructs the local SRCROOT into a portable repository root
         derived from versionControlProvenance AFTER enrichment: a
         github.com/<owner>/<repo>/blob/<revisionId>/ commit permalink,
         or — for an Azure DevOps repositoryUri — the commit-less
         dev.azure.com/<org>/<project>/_git/<repo>/ repository root. The
         shipped artifact anchors at a stable, host-independent URL.

      5. Post-finalize JSON patches that the multitool emit verbs do not
         currently model as first-class flags:
           a. Register tool.driver.notifications[0] = AnalyzeComplete
              and have the notification reference it (clears AI2017).
           b. Set run.properties.ai/handoff to a brief, plausible string
              (clears AI2012).
           c. Set the NOVEL- descriptor's name + helpUri so it carries a
              Pascal-case identifier per SARIF §3.49.7 (clears SARIF2012
              on the NOVEL rule).

      6. Validate the produced SARIF with --rule-kind Sarif;AI. Because
         this fixture's run carries the ai/origin property, AI-aware
         validation rules (SARIF2002, SARIF2009, SARIF2014, SARIF2015)
         self-suppress per their AI-origin contract on
         SarifValidationSkimmerBase. The fixture is required to ship
         with 0 errors, 0 warnings, and 0 notes.

    Determinism notes:
      * SampleCode.cs is pinned to LF via .gitattributes so snippets,
        contextRegion text, and the artifact sha-256 are byte-identical
        across Windows / Linux / macOS checkouts.
      * automationDetails GUIDs, ai/handoff text, and notification.timeUtc
        are all fixed constants (no Guid.NewGuid, no DateTime.UtcNow).
      * versionControlProvenance (repositoryUri, revisionId, branch) is
        resolved as a coherent unit: from the live git working tree by
        default (full reconstruction when this taxonomy is copied into
        another repo and run there), or pinned to the canonical fixture
        triple under -Deterministic. The two are never mixed — a live
        repositoryUri paired with a pinned commit would name a commit that
        does not exist in that repository.
      * The CWE IDs span CweTaxonomy.DefaultStatuses (Stable, Draft,
        Incomplete) so the fixture doubles as a smoke test that the
        default loadout covers real-world ruleset surface area. See
        CweReadme.md for the measurement behind that default.

.PARAMETER Configuration
    Build configuration whose multitool binary to invoke. Release or
    Debug. Defaults to Release.

.PARAMETER GHAzDO
    When set, produces CweGHAzDoSample.sarif (the GHAzDO ingestion
    variant) instead of CweSample.sarif. ADO predefined env vars are
    populated for the duration of emit-run, AdoPipelineContext
    stamps the automationDetails, and validation runs with rule-kind
    Sarif;AI;GHAzDO. Default (switch absent) preserves the original
    CweSample.sarif emission unchanged.

.PARAMETER Deterministic
    Pins versionControlProvenance to the canonical fixture triple
    (repositoryUri https://github.com/microsoft/sarif-sdk, the frozen
    v4.5.0 revisionId, branch refs/heads/main) so the checked-in
    CweSample.sarif / CweGHAzDoSample.sarif regenerate byte-identically
    on any machine, commit, or fork. The byte-gate test passes this.
    Mutually exclusive with -RevisionId / -Branch.

.PARAMETER RevisionId
    Overrides the live-derived commit (git rev-parse HEAD) in default
    mode — e.g. to anchor provenance at a specific commit. Pair with
    -Branch when HEAD is detached. Ignored under -Deterministic.

.PARAMETER Branch
    Overrides the live-derived branch (git rev-parse --abbrev-ref HEAD)
    in default mode; a bare name is normalized to refs/heads/<name>.
    Ignored under -Deterministic.

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1 -GHAzDO

.EXAMPLE
    # Reproduce the checked-in fixtures byte-for-byte (what CI runs):
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1 -Deterministic
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [switch]$GHAzDO,

    [switch]$Deterministic,

    [string]$RevisionId = '',

    [string]$Branch = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot  = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')).Path
$multitool = Join-Path $repoRoot "bld/bin/AnyCPU_$Configuration/Sarif.Multitool/net8.0/Sarif.Multitool.dll"

if (-not (Test-Path $multitool)) {
    throw "Sarif.Multitool.dll not found at '$multitool'. Build the SDK in $Configuration configuration first (e.g. dotnet build src\Sarif.Multitool\Sarif.Multitool.csproj -c $Configuration)."
}

$sampleBaseName = if ($GHAzDO) { 'CweGHAzDoSample' } else { 'CweSample' }
$outPath = Join-Path $PSScriptRoot ($sampleBaseName + '.sarif')
$wipPath = "$outPath.wip.jsonl"

# Validation rule-kinds. The GHAzDO variant adds the GHAzDO ruleset so the
# fixture is required to satisfy the ADO ingestion contract on top of the
# Sarif+AI baseline. Self-suppression for ai/origin runs lives on the rule
# implementations, not here.
$validateRuleKind = if ($GHAzDO) { 'Sarif;AI;GHAzDO' } else { 'Sarif;AI' }

# Deterministic ADO pipeline-context env values used only when -GHAzDO. The
# AdoPipelineContext detector reads these in emit-run and stamps
# run.automationDetails.id plus the four azuredevops/pipeline/build/* keys
# that GHAzDO1019/1020 validate. Values chosen so the resulting fixture is
# stable across machines.
# The fallback env vars (SYSTEM_DEFINITIONID / SYSTEM_JOBID / SYSTEM_JOBNAME)
# are also set here because ADO agents inject all of them, and
# AdoPipelineContext.TryDetect rejects the run when a primary (e.g.
# BUILD_DEFINITIONID) disagrees with its fallback (SYSTEM_DEFINITIONID).
# Without overriding the fallbacks, this script crashes when re-run inside
# a real ADO pipeline (the agent's SYSTEM_DEFINITIONID is the genuine
# pipeline id, which disagrees with the fixed 1234 we stamp on the primary).
$adoEnv = [ordered]@{
    'TF_BUILD'             = 'True'
    'SYSTEM_COLLECTIONURI' = 'https://dev.azure.com/example-org/'
    'SYSTEM_TEAMPROJECTID' = '00000000-0000-0000-0000-000000000001'
    'BUILD_DEFINITIONID'   = '1234'
    'SYSTEM_DEFINITIONID'  = '1234'
    'BUILD_DEFINITIONNAME' = 'CweSamplerScanner CI'
    'BUILD_BUILDID'        = '98765'
    'SYSTEM_PHASEID'       = '00000000-0000-0000-0000-000000000002'
    'SYSTEM_JOBID'         = '00000000-0000-0000-0000-000000000002'
    'SYSTEM_PHASENAME'     = 'Build'
    'SYSTEM_JOBNAME'       = 'Build'
    # The three VCP-augmenting vars (BUILD_REPOSITORY_URI,
    # BUILD_SOURCEVERSION, BUILD_SOURCEBRANCH) are assigned post-resolution
    # from the resolved provenance triple, below — AdoPipelineContext reads
    # them and verifies field-by-field agreement against the supplied VCP
    # entry; any mismatch fails emit-run.
    # GitHub Actions gate + identity vars. Cleared in every script
    # invocation so that GitHubActionsContext.TryDetect returns None: this
    # script supplies a deterministic VCP and stamps ADO identity
    # (-GHAzDO variant only); inheriting ambient GHA env from the host CI
    # runner would let GitHubActionsContext detect a conflicting
    # revisionId (the runner's real GITHUB_SHA) and abort emit-run.
    'GITHUB_ACTIONS'       = $null
    'GITHUB_SERVER_URL'    = $null
    'GITHUB_REPOSITORY'    = $null
    'GITHUB_SHA'           = $null
    'GITHUB_REF_NAME'      = $null
    'GITHUB_REF'           = $null
}

function Set-AdoEnv {
    param([System.Collections.IDictionary]$envMap)
    $saved = [ordered]@{}
    foreach ($name in $envMap.Keys) {
        $saved[$name] = [System.Environment]::GetEnvironmentVariable($name)
        [System.Environment]::SetEnvironmentVariable($name, $envMap[$name])
    }
    return $saved
}

function Restore-AdoEnv {
    param([System.Collections.IDictionary]$saved)
    if ($null -eq $saved) { return }
    foreach ($name in $saved.Keys) {
        [System.Environment]::SetEnvironmentVariable($name, $saved[$name])
    }
}

# Env vars to clear in default mode (a developer with TF_BUILD already in
# their shell would otherwise accidentally stamp CweSample.sarif via
# AdoPipelineContext auto-detect).
$adoEnvCleared = [ordered]@{}
foreach ($name in $adoEnv.Keys) {
    $adoEnvCleared[$name] = $null
}

# Resolve the versionControlProvenance triple (repositoryUri, revisionId,
# branch) ATOMICALLY: either every field comes from the live git working tree
# (full reconstruction — what a consumer who copies this taxonomy into their
# own repo and runs it gets) or every field comes from the canonical pin (the
# checked-in fixture's frozen contract). The two are never mixed — a live
# repositoryUri paired with a canonical commit would name a commit that does
# not exist in that repository.
#
# emit-run (for the -GHAzDO variant) stamps BUILD_REPOSITORY_URI /
# BUILD_SOURCEVERSION / BUILD_SOURCEBRANCH from these same resolved values;
# AdoPipelineContext verifies them field-by-field against the supplied VCP
# entry and fails emit-run on any disagreement, so all three must agree.
# emit-finalize binds the local SRCROOT to a portable root derived from this
# triple (github blob permalink, or the commit-less ADO repository root).

# The canonical pin — a coherent triple anchored on a real, immutable
# sarif-sdk commit (the v4.5.0 release). The checked-in fixtures are frozen
# to these values so the byte-gate regenerates identically on any machine,
# commit, or fork; -Deterministic selects it. The revisionId must resolve to
# a real blob on github.com for the default fixture's SRCROOT permalink to be
# clickable; when the sample source (SampleCode.cs) changes, advance this pin
# to a commit that carries the new content.
$canonicalRepositoryUri = 'https://github.com/microsoft/sarif-sdk'
$canonicalRevisionId    = '84f83c813bcf52ae2c0fd7ff2963e2fa2a2efac7'
$canonicalBranch        = 'refs/heads/main'

function ConvertTo-RefsHeads {
    param([string]$name)
    $n = $name.Trim()
    if ($n -like 'refs/*') { return $n }
    return "refs/heads/$n"
}

if ($Deterministic) {
    if ($RevisionId -or $Branch) {
        throw "-Deterministic pins the canonical provenance triple and cannot be combined with -RevisionId/-Branch. Drop -Deterministic to override individual live-derived fields."
    }
    $vcpRepoUri = $canonicalRepositoryUri
    $revisionId = $canonicalRevisionId
    $vcpBranch  = $canonicalBranch
}
else {
    $insideGit = $false
    try { $insideGit = ((& git -C $repoRoot rev-parse --is-inside-work-tree 2>$null) -eq 'true') } catch { }

    if ($insideGit) {
        # Full live reconstruction. Every field resolves from this one git
        # context; we never backfill a missing field from the canonical pin
        # (that is exactly the incoherent-provenance bug this guards against),
        # we fail with an actionable message instead.
        $originUrl = $null
        try { $originUrl = (& git -C $repoRoot remote get-url origin 2>$null).Trim() } catch { }
        if (-not $originUrl) {
            throw "The git repository at '$repoRoot' has no 'origin' remote, so repositoryUri cannot be resolved. Add an origin remote, or pass -Deterministic to emit the checked-in fixture."
        }
        $vcpRepoUri = ($originUrl -replace '\.git$','').TrimEnd('/')

        if ($RevisionId) {
            $revisionId = $RevisionId.Trim()
        }
        else {
            $revisionId = $null
            try { $revisionId = (& git -C $repoRoot rev-parse HEAD 2>$null).Trim() } catch { }
            if (-not $revisionId) {
                throw "Could not resolve HEAD at '$repoRoot' (an unborn branch has no commit). Commit at least once, pass -RevisionId <sha>, or pass -Deterministic."
            }
        }

        if ($Branch) {
            $vcpBranch = ConvertTo-RefsHeads $Branch
        }
        else {
            $abbrev = $null
            try { $abbrev = (& git -C $repoRoot rev-parse --abbrev-ref HEAD 2>$null).Trim() } catch { }
            if (-not $abbrev -or $abbrev -eq 'HEAD') {
                throw "Detached HEAD at '$repoRoot' has no branch to record. Pass -Branch <name>, or pass -Deterministic."
            }
            $vcpBranch = ConvertTo-RefsHeads $abbrev
        }
    }
    else {
        # No git context (e.g. an extracted source tarball). Fall back to the
        # canonical pin WHOLESALE so the triple stays coherent; explicit
        # overrides still layer onto it for callers who know their target.
        $vcpRepoUri = $canonicalRepositoryUri
        $revisionId = if ($RevisionId) { $RevisionId.Trim() } else { $canonicalRevisionId }
        $vcpBranch  = if ($Branch) { ConvertTo-RefsHeads $Branch } else { $canonicalBranch }
    }
}

# A bare run inside the canonical sarif-sdk repo (no -Deterministic) stamps
# the developer's live HEAD and would dirty the checked-in fixture. That is
# intentional for consumers copying this taxonomy elsewhere, but surprising
# in-repo — guide the developer to the deterministic path.
if (-not $Deterministic -and $vcpRepoUri -eq $canonicalRepositoryUri -and $revisionId -ne $canonicalRevisionId) {
    Write-Warning "Generating with live provenance (revisionId $revisionId). To reproduce the checked-in fixture byte-for-byte, re-run with -Deterministic."
}

# Stamp the three VCP-augmenting ADO env vars from the resolved triple so the
# -GHAzDO variant's AdoPipelineContext agreement check passes (it compares
# these against the supplied VCP entry and fails emit-run on mismatch).
$adoEnv['BUILD_REPOSITORY_URI'] = $vcpRepoUri
$adoEnv['BUILD_SOURCEVERSION']  = $revisionId
$adoEnv['BUILD_SOURCEBRANCH']   = $vcpBranch

# Local SRCROOT for enrichment; emit-finalize deconstructs it into the portable
# repository root (derived from versionControlProvenance) after enrichment.
# Cross-platform file:// construction: [System.Uri]$path returns a relative
# Uri on Linux/macOS (Unix paths lack a scheme), and .AbsoluteUri on a
# relative Uri yields $null — which then null-refs on .EndsWith. Build the
# URI textually so it works identically on Windows and Unix.
$repoRootSlash = $repoRoot.Replace('\', '/')
if (-not $repoRootSlash.StartsWith('/')) { $repoRootSlash = "/$repoRootSlash" }
if (-not $repoRootSlash.EndsWith('/'))   { $repoRootSlash = "$repoRootSlash/" }
$localSrcRootUri = "file://$repoRootSlash"

# Repo-relative artifact path; emit-finalize binds it to the SRCROOT base,
# which resolves to a github.com/<owner>/<repo>/blob/<sha>/ commit permalink
# or, for an Azure DevOps repositoryUri, the dev.azure.com/<org>/<project>/
# _git/<repo>/ repository root.
$sampleFileRepoRelative = 'src/Sarif/Taxonomies/SampleCode.cs'
$sampleFileOnDisk       = Join-Path $PSScriptRoot 'SampleCode.cs'

# Fixed for determinism — the sample is checked in; per-run GUID drift would
# constantly dirty the working tree.
$automationGuid            = 'a7ad9ab8-1234-5678-9abc-def012345678'
$automationCorrelationGuid = '660f3001-34a8-46c5-8ad5-14b9682470ba'

Write-Host "[1/6] Opening run -> $outPath"

# JSON-payload contract: construct a SARIF Run object and pipe it to
# emit-run via stdin, matching the other emit verbs (add-result,
# add-invocation, add-notification-reporting-descriptor,
# add-rule-reporting-descriptor). The Run object can carry rich run-header
# shapes (multiple VCP entries, properties bags, etc.).
$runHeader = [ordered]@{
    tool = [ordered]@{
        driver = [ordered]@{
            name             = 'CweSamplerScanner'
            version          = '0.1.0'
            semanticVersion  = '0.1.0'
            informationUri   = 'https://cwesamplerscanner.example.com/'
            organization     = 'Example Scanner Authority'
        }
    }
    versionControlProvenance = @(
        [ordered]@{
            repositoryUri = $vcpRepoUri
            revisionId    = $revisionId
            branch        = $vcpBranch
            mappedTo      = [ordered]@{ uriBaseId = 'SRCROOT' }
        }
    )
    originalUriBaseIds = [ordered]@{
        SRCROOT = [ordered]@{ uri = $localSrcRootUri }
    }
    automationDetails = [ordered]@{
        guid            = $automationGuid
        correlationGuid = $automationCorrelationGuid
    }
    properties = [ordered]@{
        'ai/origin' = 'generated'
    }
}

$runHeaderJson = $runHeader | ConvertTo-Json -Depth 32 -Compress

$initArgs = @(
    $multitool, 'emit-run', $outPath,
    '--force-overwrite'
)

# Stamp pipeline identity (-GHAzDO variant) or explicitly clear the ADO env
# vars (default variant) for the lifetime of emit-run. Either way the
# script's behavior is independent of the caller's shell state.
$envToApply = if ($GHAzDO) { $adoEnv } else { $adoEnvCleared }
$savedEnv = $null
try {
    $savedEnv = Set-AdoEnv $envToApply
    $runHeaderJson | & dotnet @initArgs | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "emit-run failed (exit $LASTEXITCODE)." }
} finally {
    Restore-AdoEnv $savedEnv
}

# Each finding. Vocabulary spread across AI2014 (ai/exploitability) and a
# small ai/attackerPosition vocab so the fixture demonstrates the full range
# rather than monoculture values.
$events = @(
    @{ kind='result'; cwe='CWE-79/unescaped-view-input';            level='warning'; status='Stable';     rank=60; exploit='theoretical';  attacker='unauthenticated-remote'; startLine=18; endLine=18; msg='Possible XSS via unescaped user input in view template.'; mdAdd='Untrusted request data flows directly into a server-rendered HTML response without per-context escaping. Escape on output via the templating engine''s context-aware helper.' }
    @{ kind='result'; cwe='CWE-89/string-concat-query';             level='error';   status='Stable';     rank=90; exploit='demonstrated'; attacker='unauthenticated-remote'; startLine=22; endLine=22; msg='SQL query built via string concatenation; parameterize.'; mdAdd='User-controlled input is concatenated into a SQL statement. Use a parameterized query (`SqlCommand` + `Parameters.AddWithValue`) so the input is sent out of band.' }
    @{ kind='result'; cwe='CWE-22/untrusted-path-no-canon';         level='error';   status='Stable';     rank=85; exploit='poc';          attacker='authenticated-remote';   startLine=26; endLine=26; msg='Path constructed from untrusted input without canonicalization.'; mdAdd='Use `Path.GetFullPath` to canonicalize the candidate path and reject any result that escapes the allow-listed root directory.' }
    @{ kind='result'; cwe='CWE-798/embedded-credential';            level='error';   status='Draft';      rank=95; exploit='demonstrated'; attacker='source-disclosure';      startLine=30; endLine=30; msg='Hard-coded credential in production code path.'; mdAdd='Replace the embedded literal with a managed-identity acquisition (or Key Vault retrieval) and rotate the leaked credential immediately.' }
    @{ kind='result'; cwe='CWE-1220/missing-tenant-scope';          level='warning'; status='Incomplete'; rank=55; exploit='theoretical';  attacker='authenticated-remote';   startLine=34; endLine=34; msg='Authorization check missing tenant-scoped granularity.'; mdAdd='Per-resource authorization MUST include the caller''s tenant in the policy decision; cross-tenant data leakage is otherwise possible.' }
    @{ kind='result'; cwe='CWE-79/dom-xss-via-sanitizer-bypass';    level='error';   status='Stable';     rank=80; exploit='poc';          attacker='unauthenticated-remote'; startLine=39; endLine=39; msg='DOM XSS: untrusted value reaches innerHTML after a sanitizer that does not escape the current context.'; mdAdd='Second sub-id under CWE-79; shares the base descriptor with the first finding. The DOM sink (`innerHTML`) requires HTML-context escaping; URL- or attribute-context escaping is insufficient.' }
    @{ kind='result'; cwe='NOVEL-prompt-injection-via-system-message'; level='error'; status='(novel)';  rank=70; exploit='demonstrated'; attacker='unauthenticated-remote'; startLine=44; endLine=44; msg='Untrusted content reaches a system-role prompt at runtime.'; mdAdd='Untrusted content is concatenated into a system-role prompt at runtime, letting an attacker override tool-use policy. No CWE entry fits — emitted under the NOVEL- escape hatch.' }
)

Write-Host "[2/6] Appending $($events.Count) Result events + 1 Invocation"

$sampleLines = [System.IO.File]::ReadAllLines($sampleFileOnDisk)

function Get-FirstNonWhitespaceColumn {
    param([string]$line)
    if ([string]::IsNullOrEmpty($line)) { return 1 }
    for ($i = 0; $i -lt $line.Length; $i++) {
        if (-not [char]::IsWhiteSpace($line[$i])) { return $i + 1 }
    }
    return 1
}

foreach ($e in $events) {
    $startLineText = if ($e.startLine -ge 1 -and $e.startLine -le $sampleLines.Length) { $sampleLines[$e.startLine - 1] } else { '' }
    $startColumn = Get-FirstNonWhitespaceColumn $startLineText

    # [ordered] throughout: ConvertTo-Json preserves PowerShell hashtable key order
    # only for [ordered]@{}. Bare @{} uses .NET Hashtable whose enumeration order
    # varies across process startups (different hash seed per process), which would
    # alternate JSON property order between script runs / platforms and break the
    # determinism gate. See PR #2926 for the original diagnosis.
    $payload = [ordered]@{
        ruleId  = $e.cwe
        level   = $e.level
        rank    = $e.rank
        message = [ordered]@{
            text     = $e.msg
            markdown = "**$($e.cwe)** — $($e.msg)`n`n$($e.mdAdd)"
        }
        locations = @([ordered]@{
            physicalLocation = [ordered]@{
                artifactLocation = [ordered]@{ uri = $sampleFileRepoRelative; uriBaseId = 'SRCROOT' }
                region           = [ordered]@{ startLine = $e.startLine; startColumn = $startColumn; endLine = $e.endLine }
            }
        })
        properties = [ordered]@{
            'ai/exploitability'    = $e.exploit
            'ai/attackerPosition'  = $e.attacker
        }
    }
    $payloadJson = $payload | ConvertTo-Json -Compress -Depth 12
    $payloadJson | & dotnet $multitool add-result $outPath | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "add-result failed for ruleId '$($e.cwe)' (exit $LASTEXITCODE)." }
}

# A single fully-formed invocation carries the run's notification INLINE on
# toolExecutionNotifications. SARIF has no run-level notifications array, so a
# notification travels inside the invocation that owns it (and parallel processes
# are each modeled by their own invocation).
# workingDirectory is a REAL repo-relative directory (under SRCROOT) so enrichment
# resolves it to an actual path; after emit-finalize rewrites SRCROOT to the hosted
# GitHub URL it resolves there. endTimeUtc is preset so the verb leaves it alone (it
# only auto-stamps endTimeUtc when the producer omits it). The notification timeUtc
# is producer-supplied (the verb requires it and never stamps it). arguments[]
# accompanies commandLine per SARIF 3.20.4. All presets keep the fixture's bytes
# stable across re-runs.
$invocationPayload = [ordered]@{
    executionSuccessful = $true
    commandLine         = 'cwe-sampler-scanner analyze ./SampleCode.cs'
    arguments           = @('analyze', './SampleCode.cs')
    workingDirectory    = [ordered]@{ uri = 'src/Sarif/Taxonomies/'; uriBaseId = 'SRCROOT' }
    endTimeUtc          = '2024-01-01T00:00:00.000Z'
    toolExecutionNotifications = @([ordered]@{
        level   = 'note'
        message = [ordered]@{
            text     = "Analyzed $($events.Count) findings across 1 file."
            markdown = "Analyzed **$($events.Count)** findings across **1** file."
        }
        timeUtc = '2024-01-01T00:00:00.000Z'
    })
}
$invocationJson = $invocationPayload | ConvertTo-Json -Compress -Depth 8
$invocationJson | & dotnet $multitool add-invocation $outPath | Out-Null
if ($LASTEXITCODE -ne 0) { throw "add-invocation failed (exit $LASTEXITCODE)." }

Write-Host "[3/6] Finalizing (--embed-text-files)"
$finalizeArgs = @(
    $multitool, 'emit-finalize', $outPath,
    '--embed-text-files'
)
& dotnet @finalizeArgs | Out-Host
if ($LASTEXITCODE -ne 0) { throw "emit-finalize failed (exit $LASTEXITCODE)." }

# ---------------------------------------------------------------------------
# Post-finalize JSON patches the emit verbs do not currently model:
#   * notification descriptor registration + reference  (AI2017)
#   * ai/handoff on run.properties                       (AI2012)
#   * NOVEL- descriptor name + helpUri                   (SARIF2012)
# Doing this here keeps the SDK's verb surface lean; if any of these get
# enough usage to warrant flags, promote them.
# ---------------------------------------------------------------------------
Write-Host "[4/6] Applying post-finalize JSON patches (notification descriptor, ai/handoff, NOVEL name)"

$novelRuleName     = 'PromptInjectionViaSystemMessage'
$novelRuleHelpUri  = 'https://cwesamplerscanner.example.com/rules/PromptInjectionViaSystemMessage'
$notificationDescriptorId      = 'NOTIF0001'
$notificationDescriptorName    = 'AnalyzeComplete'
$notificationDescriptorHelpUri = 'https://cwesamplerscanner.example.com/notifications/AnalyzeComplete'

# Read indented (the emit-finalize default); rewrite with the same formatting
# so the patch keeps the file byte-stable on re-runs.
$rawJson = Get-Content -LiteralPath $outPath -Raw
$doc = $rawJson | ConvertFrom-Json
$run = $doc.runs[0]

# Notification descriptor registration.
$driver = $run.tool.driver
$notifDescriptor = [pscustomobject]@{
    id              = $notificationDescriptorId
    name            = $notificationDescriptorName
    shortDescription = [pscustomobject]@{ text = 'Analysis run completed.' }
    fullDescription  = [pscustomobject]@{ text = 'Emitted when the scanner finishes analyzing its inputs. Carries the count of findings reported.' }
    helpUri         = $notificationDescriptorHelpUri
    defaultConfiguration = [pscustomobject]@{ level = 'note' }
}
$driver | Add-Member -NotePropertyName 'notifications' -NotePropertyValue @($notifDescriptor) -Force

# Notification reference.
$notification = $run.invocations[0].toolExecutionNotifications[0]
$notification | Add-Member -NotePropertyName 'descriptor' -NotePropertyValue ([pscustomobject]@{ id = $notificationDescriptorId; index = 0 }) -Force

# ai/handoff on run.properties (string body — AI2012 checks .TryGetProperty<string>).
$handoffText = 'Analysis complete. 7 findings reported (5 error, 2 warning). No tool errors; no further passes recommended.'
if (-not $run.PSObject.Properties['properties']) {
    $run | Add-Member -NotePropertyName 'properties' -NotePropertyValue ([pscustomobject]@{}) -Force
}
$run.properties | Add-Member -NotePropertyName 'ai/handoff' -NotePropertyValue $handoffText -Force

# NOVEL- descriptor name + helpUri so it satisfies SARIF §3.49.7 Pascal-case
# and AI consumers have a stable identifier + a home for further reading.
foreach ($rule in $driver.rules) {
    if ($rule.id -eq 'NOVEL-prompt-injection-via-system-message') {
        $rule | Add-Member -NotePropertyName 'name'    -NotePropertyValue $novelRuleName    -Force
        $rule | Add-Member -NotePropertyName 'helpUri' -NotePropertyValue $novelRuleHelpUri -Force
    }
}

# tool.driver.fullName — GHAzDO1018 requires a human-readable driver fullName
# distinct from name. Only the -GHAzDO variant ships it so CweSample.sarif
# (the bare AI shape) stays byte-stable; emit-run has no first-class
# flag for fullName so we patch it post-finalize like the items above.
if ($GHAzDO) {
    $driver | Add-Member -NotePropertyName 'fullName' -NotePropertyValue 'CWE Sampler Scanner' -Force
}

# Write back. The SARIF was indented; preserve indentation. ConvertTo-Json
# in PowerShell 7 uses 2-space indent by default; the emit-finalize JsonTextWriter
# defaults to Indented = 2-space as well. Compare on hash, not diff.
$json = $doc | ConvertTo-Json -Depth 64
[System.IO.File]::WriteAllText($outPath, $json, [System.Text.UTF8Encoding]::new($false))

# ---------------------------------------------------------------------------
# Validate. CweSample.sarif MUST pass with 0 errors, 0 warnings, and 0 notes
# under --rule-kind Sarif;AI. The run carries ai/origin = "generated" so the
# AI-aware style rules (SARIF2002, SARIF2009, SARIF2014, SARIF2015) self-
# suppress; the fixture is also constructed to satisfy the remaining
# correctness-class rules (snippets, hashes, provenance, etc.).
# ---------------------------------------------------------------------------
Write-Host "[5/6] Validating $sampleBaseName.sarif (--rule-kind $validateRuleKind)"
$validateReport = Join-Path $PSScriptRoot ($sampleBaseName + '.validate-report.sarif')
& dotnet $multitool validate $outPath `
    --rule-kind $validateRuleKind `
    --level 'Error;Warning;Note' `
    --output $validateReport `
    --log 'ForceOverwrite' `
    --quiet 2>&1 | Out-Null
# Validate returns non-zero exit on Error-level findings; treat that as
# information, not a script failure, so we can report the diagnostics summary.

if (-not (Test-Path $validateReport)) {
    throw "Validator did not produce a report at '$validateReport'."
}

$report = Get-Content -LiteralPath $validateReport -Raw | ConvertFrom-Json
$reportRun = $report.runs[0]
$reportResults = @()
if ($reportRun.PSObject.Properties['results']) { $reportResults = @($reportRun.results) }

function Get-ResultLevel {
    param($r)
    if ($null -eq $r) { return 'warning' }
    if (-not $r.PSObject.Properties['level']) { return 'warning' }
    $v = $r.level
    if ([string]::IsNullOrEmpty($v)) { return 'warning' }
    return $v
}

$errors   = @($reportResults | Where-Object { (Get-ResultLevel $_) -eq 'error' })
$warnings = @($reportResults | Where-Object { (Get-ResultLevel $_) -eq 'warning' })
$notes    = @($reportResults | Where-Object { (Get-ResultLevel $_) -eq 'note' })

Write-Host ""
Write-Host "Validator summary: $($errors.Count) error(s), $($warnings.Count) warning(s), $($notes.Count) note(s)"
if ($notes.Count -gt 0) {
    $byRule = $notes | Group-Object -Property ruleId | Sort-Object Name
    foreach ($g in $byRule) {
        Write-Host ("  note: {0,-12} x{1}" -f $g.Name, $g.Count)
    }
}

if ($errors.Count -gt 0 -or $warnings.Count -gt 0 -or $notes.Count -gt 0) {
    if ($errors.Count -gt 0)   { Write-Warning ("Error rules: "   + (($errors   | Group-Object ruleId | ForEach-Object { $_.Name }) -join ', ')) }
    if ($warnings.Count -gt 0) { Write-Warning ("Warning rules: " + (($warnings | Group-Object ruleId | ForEach-Object { $_.Name }) -join ', ')) }
    if ($notes.Count -gt 0)    { Write-Warning ("Note rules: "    + (($notes    | Group-Object ruleId | ForEach-Object { $_.Name }) -join ', ')) }
    Write-Warning "See '$validateReport' for details."
    exit 1
}

# Clean up only on success — leave the report behind for forensics on failure.
Remove-Item -LiteralPath $validateReport -Force -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
# Smoke summary
# ---------------------------------------------------------------------------
Write-Host "[6/6] Summary"
$rules = $driver.rules

function Get-OptionalProperty {
    param($obj, [string]$name)
    if ($null -eq $obj) { return $null }
    $prop = $obj.PSObject.Properties[$name]
    if ($null -eq $prop) { return $null }
    return $prop.Value
}

$toolName    = Get-OptionalProperty $driver 'name'
$toolVersion = Get-OptionalProperty $driver 'version'
$toolInfoUri = Get-OptionalProperty $driver 'informationUri'
$artifacts   = Get-OptionalProperty $run 'artifacts'
$srcRootUri  = Get-OptionalProperty (Get-OptionalProperty $run.originalUriBaseIds 'SRCROOT') 'uri'

Write-Host ""
Write-Host "Sample SARIF: $outPath"
$variant = if ($GHAzDO) { 'GHAzDO ingestion (Sarif+AI+GHAzDO)' } else { 'AI-shape (Sarif+AI)' }
Write-Host "Variant:      $variant"
$toolLine = if ([string]::IsNullOrEmpty($toolName)) { '(missing - is your multitool DLL current?)' } else { $toolName }
if (-not [string]::IsNullOrEmpty($toolVersion)) { $toolLine = "$toolLine $toolVersion" }
Write-Host "Tool:         $toolLine"
Write-Host "Info URI:     $toolInfoUri"
Write-Host "SRCROOT URI:  $srcRootUri"
Write-Host "Results:      $($run.results.Count)"
Write-Host "Rules:        $($rules.Count)"
if ($null -ne $artifacts) {
    $hashedCount = @($artifacts | Where-Object { $null -ne (Get-OptionalProperty $_ 'hashes') }).Count
    $embeddedCount = @($artifacts | Where-Object {
        $c = Get-OptionalProperty $_ 'contents'
        ($null -ne $c) -and ($null -ne (Get-OptionalProperty $c 'text'))
    }).Count
    Write-Host "Artifacts:    $($artifacts.Count) ($hashedCount with sha-256, $embeddedCount with embedded text)"
}
Write-Host ""

foreach ($rule in $rules) {
    $name    = Get-OptionalProperty $rule 'name'
    $helpUri = Get-OptionalProperty $rule 'helpUri'
    $isNovel = $rule.id -like 'NOVEL-*'
    $hasName    = -not [string]::IsNullOrEmpty($name)
    $hasHelpUri = -not [string]::IsNullOrEmpty($helpUri)
    $marker = if ($hasName -and $hasHelpUri) { '[OK]' } else { '[!!]' }
    $shown  = if ($hasName) { $name } else { '(no name)' }
    Write-Host ("{0} {1,-50} {2}" -f $marker, $rule.id, $shown)
}

Write-Host ""
Write-Host "CweSample.sarif: 0 errors, 0 warnings, 0 notes."
