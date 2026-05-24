# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

<#
.SYNOPSIS
    Emits CweSample.sarif — a deterministic, fully-enriched SARIF fixture
    that exercises the multitool emit chain end-to-end and passes the
    Sarif+AI validator with zero Errors and zero Warnings.

.DESCRIPTION
    Convention: every taxonomy that ships with this SDK includes a
    <Taxonomy>GenerateSample.ps1 alongside its data, and that script
    produces a checked-in <Taxonomy>Sample.sarif fixture next to itself.
    CI re-runs the script and asserts the working tree stays byte-identical.

    Pipeline:

      1. emit-init-run — opens .wip.jsonl with rich run-header metadata:
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

      3. add-notification × 1 — toolExecutionNotification with preset
         timeUtc so the fixture's bytes are stable across re-runs.

      4. emit-finalize --embed-text-files --srcroot https://github.com/microsoft/sarif-sdk/blob/main/
         Enrichment runs against the local SRCROOT (snippets, hashes,
         contextRegion, charOffset). --embed-text-files inlines
         SampleCode.cs into run.artifacts[].contents.text so the fixture
         is self-contained (clears SARIF2013). --srcroot rewrites
         originalUriBaseIds.SRCROOT.uri to the canonical GitHub URL
         AFTER enrichment so the shipped artifact anchors at a stable,
         host-independent URL.

      5. Post-finalize JSON patches that the multitool emit verbs do not
         currently model as first-class flags:
           a. Register tool.driver.notifications[0] = AnalyzeComplete
              and have the notification reference it (clears AI2017).
           b. Set run.properties.ai/handoff to a brief, plausible string
              (clears AI2012).
           c. Set the NOVEL- descriptor's name + helpUri so it carries a
              Pascal-case identifier per SARIF §3.49.7 (clears SARIF2012
              on the NOVEL rule).

      6. Validate the produced SARIF with --rule-kind Sarif;AI. The
         fixture is required to ship with 0 Errors and 0 Warnings.
         The only acceptable notes are SARIF2002 (recommends
         message.id+arguments over message.text — not valuable for
         AI-emitted fixtures) and SARIF2009 (NOVEL- rule id does not
         match the conventional TOOL2001 form — by design; the NOVEL-
         prefix IS the AI ruleId convention's escape hatch).

    Determinism notes:
      * SampleCode.cs is pinned to LF via .gitattributes so snippets,
        contextRegion text, and the artifact sha-256 are byte-identical
        across Windows / Linux / macOS checkouts.
      * automationDetails GUIDs, ai/handoff text, and notification.timeUtc
        are all fixed constants (no Guid.NewGuid, no DateTime.UtcNow).
      * The CWE IDs span CweTaxonomy.DefaultStatuses (Stable, Draft,
        Incomplete) so the fixture doubles as a smoke test that the
        default loadout covers real-world ruleset surface area. See
        CweReadme.md for the measurement behind that default.

.PARAMETER Configuration
    Build configuration whose multitool binary to invoke. Release or
    Debug. Defaults to Release.

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot  = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')).Path
$multitool = Join-Path $repoRoot "bld/bin/AnyCPU_$Configuration/Sarif.Multitool/net8.0/Sarif.Multitool.dll"

if (-not (Test-Path $multitool)) {
    throw "Sarif.Multitool.dll not found at '$multitool'. Build the SDK in $Configuration configuration first (e.g. dotnet build src\Sarif.Multitool\Sarif.Multitool.csproj -c $Configuration)."
}

$outPath = Join-Path $PSScriptRoot 'CweSample.sarif'
$wipPath = "$outPath.wip.jsonl"

# Local SRCROOT for enrichment; rewritten to the canonical GitHub URL at finalize.
# Cross-platform file:// construction: [System.Uri]$path returns a relative
# Uri on Linux/macOS (Unix paths lack a scheme), and .AbsoluteUri on a
# relative Uri yields $null — which then null-refs on .EndsWith. Build the
# URI textually so it works identically on Windows and Unix.
$repoRootSlash = $repoRoot.Replace('\', '/')
if (-not $repoRootSlash.StartsWith('/')) { $repoRootSlash = "/$repoRootSlash" }
if (-not $repoRootSlash.EndsWith('/'))   { $repoRootSlash = "$repoRootSlash/" }
$localSrcRootUri = "file://$repoRootSlash"
$finalSrcRootUri = 'https://github.com/microsoft/sarif-sdk/blob/main/'

# Repo-relative artifact path; with --srcroot above this becomes a directly
# clickable https://github.com/microsoft/sarif-sdk/blob/main/<path>.
$sampleFileRepoRelative = 'src/Sarif/Taxonomies/SampleCode.cs'
$sampleFileOnDisk       = Join-Path $PSScriptRoot 'SampleCode.cs'

# Fixed for determinism — the sample is checked in; per-run GUID drift would
# constantly dirty the working tree.
$automationGuid            = 'a7ad9ab8-1234-5678-9abc-def012345678'
$automationCorrelationGuid = '660f3001-34a8-46c5-8ad5-14b9682470ba'

Write-Host "[1/6] Opening run -> $outPath"
$initArgs = @(
    $multitool, 'emit-init-run', $outPath,
    '--tool-driver-name',              'CweSamplerScanner',
    '--tool-version',                  '0.1.0',
    '--tool-driver-semantic-version',  '0.1.0',
    '--information-uri',               'https://cwesamplerscanner.example.com/',
    '--organization',                  'Example Scanner Authority',
    '--vcp-repositoryuri',             'https://github.com/microsoft/sarif-sdk',
    '--vcp-revisionid',                '0000000000000000000000000000000000000000',
    '--vcp-branch',                    'main',
    '--srcroot',                       $localSrcRootUri,
    '--automation-guid',               $automationGuid,
    '--automation-correlation-guid',   $automationCorrelationGuid,
    '--ai-origin',                     'generated',
    '--force-overwrite'
)
& dotnet @initArgs | Out-Host
if ($LASTEXITCODE -ne 0) { throw "emit-init-run failed (exit $LASTEXITCODE)." }

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

Write-Host "[2/6] Appending $($events.Count) Result events + 1 Notification"

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

# timeUtc preset so SarifEventReplayer leaves it alone (AI2019 auto-stamp).
$notifPayload = [ordered]@{
    level   = 'note'
    message = [ordered]@{
        text     = "Analyzed $($events.Count) findings across 1 file."
        markdown = "Analyzed **$($events.Count)** findings across **1** file."
    }
    timeUtc = '2024-01-01T00:00:00.000Z'
}
$notifJson = $notifPayload | ConvertTo-Json -Compress -Depth 8
$notifJson | & dotnet $multitool add-notification $outPath | Out-Null
if ($LASTEXITCODE -ne 0) { throw "add-notification failed (exit $LASTEXITCODE)." }

Write-Host "[3/6] Finalizing (--srcroot $finalSrcRootUri --embed-text-files)"
$finalizeArgs = @(
    $multitool, 'emit-finalize', $outPath,
    '--srcroot',          $finalSrcRootUri,
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

# Write back. The SARIF was indented; preserve indentation. ConvertTo-Json
# in PowerShell 7 uses 2-space indent by default; the emit-finalize JsonTextWriter
# defaults to Indented = 2-space as well. Compare on hash, not diff.
$json = $doc | ConvertTo-Json -Depth 64
[System.IO.File]::WriteAllText($outPath, $json, [System.Text.UTF8Encoding]::new($false))

# ---------------------------------------------------------------------------
# Validate. CweSample.sarif MUST pass with 0 Errors and 0 Warnings under
# --rule-kind Sarif;AI. Two notes are accepted by design (see header).
# ---------------------------------------------------------------------------
Write-Host "[5/6] Validating CweSample.sarif (--rule-kind Sarif;AI)"
$validateReport = Join-Path $PSScriptRoot 'CweSample.validate-report.sarif'
& dotnet $multitool validate $outPath `
    --rule-kind 'Sarif;AI' `
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

$acceptedNoteRuleIds = @('SARIF2002', 'SARIF2009')
$unacceptedNotes = @($notes | Where-Object { $acceptedNoteRuleIds -notcontains $_.ruleId })

Write-Host ""
Write-Host "Validator summary: $($errors.Count) error(s), $($warnings.Count) warning(s), $($notes.Count) note(s)"
if ($notes.Count -gt 0) {
    $byRule = $notes | Group-Object -Property ruleId | Sort-Object Name
    foreach ($g in $byRule) {
        $accepted = if ($acceptedNoteRuleIds -contains $g.Name) { ' (accepted by design)' } else { '' }
        Write-Host ("  note: {0,-12} x{1}{2}" -f $g.Name, $g.Count, $accepted)
    }
}

if ($errors.Count -gt 0 -or $warnings.Count -gt 0 -or $unacceptedNotes.Count -gt 0) {
    if ($errors.Count -gt 0)        { Write-Warning ("Error rules: "   + (($errors   | Group-Object ruleId | ForEach-Object { $_.Name }) -join ', ')) }
    if ($warnings.Count -gt 0)      { Write-Warning ("Warning rules: " + (($warnings | Group-Object ruleId | ForEach-Object { $_.Name }) -join ', ')) }
    if ($unacceptedNotes.Count -gt 0) { Write-Warning ("Unaccepted notes: " + (($unacceptedNotes | Group-Object ruleId | ForEach-Object { $_.Name }) -join ', ')) }
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
Write-Host "CweSample.sarif: 0 errors, 0 warnings, $($notes.Count) note(s) ($($notes.Count - $unacceptedNotes.Count) accepted by design)."
