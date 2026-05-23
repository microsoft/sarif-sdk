# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

<#
.SYNOPSIS
    Emits a small sample SARIF log that exercises the CWE taxonomy enricher
    end-to-end via the multitool emit verbs.

.DESCRIPTION
    Convention: every taxonomy that ships with this SDK includes a
    <Taxonomy>GenerateSample.ps1 alongside its data. The sample script
    produces a runnable, self-checking demonstration of the taxonomy in
    action so that contributors, reviewers, and downstream consumers can
    see what enrichment looks like without building one from scratch.

    For CWE specifically the script:

      1. Locates the locally built Sarif.Multitool.dll under bld/bin/.
      2. Runs emit-init-run to open a fresh SARIF log.
      3. Pipes a handful of CWE-shaped Result JSON objects (and one
         Notification) into the multitool's add-result / add-notification
         verbs over stdin. The verbs validate ruleId conformance to the
         AI ruleId convention at receipt and append to the .wip.jsonl.
      4. Runs emit-finalize. CweTaxonomyEnricher hydrates every CWE-*
         ruleId into a full reportingDescriptor (name, shortDescription,
         fullDescription, helpUri, MITRE markdown).
      5. Prints a summary table of the rules the enricher populated.

    The CWE IDs in the sample are deliberately chosen to span the three
    statuses included in CweTaxonomy.DefaultStatuses
    (Stable, Draft, Incomplete) so the run also serves as a smoke test
    that the default loadout covers the surface area real-world rulesets
    cite. See CweReadme.md for the measurement behind that default.

.PARAMETER OutputDirectory
    Directory to write the sample SARIF (and its .wip.jsonl) into.
    Defaults to a unique subdirectory under $env:TEMP so the source tree
    stays clean and re-runs do not collide. Ignored when -CheckedIn is
    set.

.PARAMETER Configuration
    Build configuration whose multitool binary to invoke. Release or
    Debug. Defaults to Release.

.PARAMETER CheckedIn
    Regenerate the deterministic CweSample.sarif fixture that ships next
    to this script. Forces a stable SRCROOT, writes output next to the
    script, and suppresses the chatty per-step summary so the resulting
    file is byte-stable across machines. CI guards this file via the
    CweGenerateSampleRegressionTests fixture in Test.UnitTests.Sarif.

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1 `
        -OutputDirectory C:\temp\cwe-sample -Configuration Debug

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1 -CheckedIn
#>
[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [switch]$CheckedIn
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# When generating the checked-in fixture, suppress chatty progress and
# summary output so the script's stdout stays clean. Validation logic
# (Write-Warning + exit on a missing enrichment) still runs.
function Write-Status {
    param([string]$msg)
    if (-not $CheckedIn) { Write-Host $msg }
}

$repoRoot   = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')
$multitool  = Join-Path $repoRoot "bld/bin/AnyCPU_$Configuration/Sarif.Multitool/net8.0/Sarif.Multitool.dll"

if (-not (Test-Path $multitool)) {
    throw "Sarif.Multitool.dll not found at '$multitool'. Build the SDK in $Configuration configuration first (e.g. dotnet build src\Sarif.Multitool\Sarif.Multitool.csproj -c $Configuration)."
}

if (-not $OutputDirectory) {
    if ($CheckedIn) {
        # The checked-in fixture lives next to this script so the convention
        # "<Taxonomy>GenerateSample.ps1 produces <Taxonomy>Sample.sarif"
        # holds for every taxonomy that ships with the SDK.
        $OutputDirectory = $PSScriptRoot
    } else {
        $stamp = [System.Guid]::NewGuid().ToString('N').Substring(0, 8)
        $OutputDirectory = Join-Path $env:TEMP "cwe-sample-$stamp"
    }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$outFileName = if ($CheckedIn) { 'CweSample.sarif' } else { 'cwe-sample.sarif' }
$outPath = Join-Path $OutputDirectory $outFileName
$wipPath = "$outPath.wip.jsonl"

# SRCROOT anchors every finding's artifactLocation.uri at the Taxonomies
# folder. The sample source file (SampleCode.cs) lives next to this script,
# so every result resolves to a real file on disk. When generating the
# checked-in fixture we substitute a stable placeholder so the URI does not
# vary per machine.
if ($CheckedIn) {
    $srcRootUri = 'file:///SRCROOT/'
} else {
    $srcRootUri = ([System.Uri](Resolve-Path $PSScriptRoot).Path).AbsoluteUri
    if (-not $srcRootUri.EndsWith('/')) { $srcRootUri = "$srcRootUri/" }
}
$sampleFile = 'SampleCode.cs'

Write-Status "[1/4] Opening run -> $outPath"
$initArgs = @(
    $multitool, 'emit-init-run', $outPath,
    '--tool-driver-name', 'CweSamplerScanner',
    '--tool-version',     '0.1.0',
    '--information-uri',  'https://github.com/microsoft/sarif-sdk',
    '--srcroot',          $srcRootUri
)
# In -CheckedIn mode the previous fixture is by design overwritten on every
# run, so reviewers can diff the regenerated file against the prior commit.
if ($CheckedIn) { $initArgs += '--force-overwrite' }
$initOutput = & dotnet @initArgs
if (-not $CheckedIn -and $initOutput) { $initOutput | Out-Host }

# Each line is a SarifEvent envelope: {"v":1,"kind":"<kind>","payload":<SARIF object>}
# Lines MUST be LF-terminated. AtomicSarifWriter rejects torn (non-LF) lines on
# append-open. Do not use Add-Content; it writes CRLF on Windows.
#
# All findings point at SampleCode.cs (a checked-in, intentionally innocuous
# file). The richness of the sample comes from the SARIF structure - regions,
# rule descriptors, taxonomy enrichment - not from the source contents.
$events = @(
    @{ kind = 'result'; cwe = 'CWE-79/unescaped-view-input';            level = 'warning'; status = 'Stable';     msg = 'Possible XSS via unescaped user input in view template.';                 startLine = 18; endLine = 18 }
    @{ kind = 'result'; cwe = 'CWE-89/string-concat-query';             level = 'error';   status = 'Stable';     msg = 'SQL query built via string concatenation; parameterize.';                 startLine = 22; endLine = 22 }
    @{ kind = 'result'; cwe = 'CWE-22/untrusted-path-no-canon';         level = 'error';   status = 'Stable';     msg = 'Path constructed from untrusted input without canonicalization.';         startLine = 26; endLine = 26 }
    @{ kind = 'result'; cwe = 'CWE-798/embedded-credential';            level = 'error';   status = 'Draft';      msg = 'Hard-coded credential in production code path.';                          startLine = 30; endLine = 30 }
    @{ kind = 'result'; cwe = 'CWE-1220/missing-tenant-scope';          level = 'warning'; status = 'Incomplete'; msg = 'Authorization check missing tenant-scoped granularity.';                  startLine = 34; endLine = 34 }
    @{ kind = 'result'; cwe = 'CWE-79/dom-xss-via-sanitizer-bypass';    level = 'error';   status = 'Stable';     msg = 'DOM XSS: untrusted value reaches innerHTML after a sanitizer that does not escape the current context. Second sub-id under CWE-79; shares the base descriptor with the first finding.'; startLine = 39; endLine = 39 }
    @{ kind = 'result'; cwe = 'NOVEL-prompt-injection-via-system-message'; level = 'error'; status = '(novel)';   msg = 'Untrusted content reaches a system-role prompt at runtime, letting an attacker override tool-use policy. No CWE entry fits — emitted under the NOVEL- escape hatch.'; startLine = 44; endLine = 44 }
)

Write-Status "[2/4] Appending $($events.Count) Result events + 1 Notification via add-result / add-notification"

# Read the source file once and trim the region.startColumn to the first
# non-whitespace column of each finding's start line. The region then points
# precisely at the meaningful content; the context region built by
# InsertOptionalDataVisitor carries the surrounding indent and neighboring
# lines so a reviewer still sees how the finding sits in its file.
$sampleLines = [System.IO.File]::ReadAllLines((Join-Path $PSScriptRoot $sampleFile))

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

    $payload = @{
        ruleId  = $e.cwe
        level   = $e.level
        message = @{ text = $e.msg }
        locations = @(@{
            physicalLocation = @{
                artifactLocation = @{ uri = $sampleFile; uriBaseId = 'SRCROOT' }
                region           = @{ startLine = $e.startLine; startColumn = $startColumn; endLine = $e.endLine }
            }
        })
    }
    # Pipe the result JSON to add-result over stdin. The verb validates the
    # ruleId against the AI ruleId convention (taxonomy sub-id or NOVEL-
    # prefix) before appending — a bad ruleId fails clean WITHOUT polluting
    # the event log, and prints an AI-consumable error envelope to stderr.
    $payloadJson = $payload | ConvertTo-Json -Compress -Depth 12
    $payloadJson | & dotnet $multitool add-result $outPath | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "add-result failed for ruleId '$($e.cwe)' (exit $LASTEXITCODE)." }
}

$notifPayload = @{ level = 'note'; message = @{ text = "Analyzed $($events.Count) findings across 1 file." } }
# In -CheckedIn mode pre-populate timeUtc so SarifEventReplayer leaves it
# alone (it auto-stamps DateTime.UtcNow when absent for AI2019 timeline
# support). Without this the fixture's byte hash drifts per regeneration.
if ($CheckedIn) { $notifPayload['timeUtc'] = '2024-01-01T00:00:00.000Z' }
$notifJson = $notifPayload | ConvertTo-Json -Compress -Depth 8
$notifJson | & dotnet $multitool add-notification $outPath | Out-Null
if ($LASTEXITCODE -ne 0) { throw "add-notification failed (exit $LASTEXITCODE)." }

Write-Status "[3/4] Finalizing"
$finalizeOutput = & dotnet $multitool emit-finalize $outPath
if (-not $CheckedIn -and $finalizeOutput) { $finalizeOutput | Out-Host }

Write-Status "[4/4] Verifying enrichment"
$log = Get-Content $outPath -Raw | ConvertFrom-Json
$run = $log.runs[0]
$driver = $run.tool.driver
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

Write-Status ""
Write-Status "Sample SARIF: $outPath"
$toolLine = if ([string]::IsNullOrEmpty($toolName)) { '(missing - is your multitool DLL current?)' } else { $toolName }
if (-not [string]::IsNullOrEmpty($toolVersion)) { $toolLine = "$toolLine $toolVersion" }
Write-Status "Tool:         $toolLine"
Write-Status "Info URI:     $toolInfoUri"
Write-Status "Results:      $($run.results.Count)"
Write-Status "Rules:        $($rules.Count) (auto-registered from result.ruleId)"
if ($null -ne $artifacts) {
    $hashedCount = @($artifacts | Where-Object { $null -ne (Get-OptionalProperty $_ 'hashes') }).Count
    Write-Status "Artifacts:    $($artifacts.Count) ($hashedCount with sha-256 hash)"
}

# Probe the first result to confirm InsertOptionalDataVisitor enrichment took.
# A correctly enriched region has snippet text; the contextRegion has surrounding
# lines. Their presence is the smoke test for the always-on enrichment pass.
$firstResult = $run.results | Select-Object -First 1
if ($null -ne $firstResult) {
    $region        = Get-OptionalProperty $firstResult.locations[0].physicalLocation 'region'
    $contextRegion = Get-OptionalProperty $firstResult.locations[0].physicalLocation 'contextRegion'
    $regionSnippet        = if ($null -ne $region)        { Get-OptionalProperty $region        'snippet' } else { $null }
    $contextRegionSnippet = if ($null -ne $contextRegion) { Get-OptionalProperty $contextRegion 'snippet' } else { $null }
    $regionMark        = if ($null -ne $regionSnippet)        { '[OK]' } else { '[!!]' }
    $contextRegionMark = if ($null -ne $contextRegionSnippet) { '[OK]' } else { '[!!]' }
    Write-Status "Region snippet:        $regionMark"
    Write-Status "Context region snippet: $contextRegionMark"
}
Write-Status ""
$rules | ForEach-Object {
    $name       = Get-OptionalProperty $_ 'name'
    $helpUri    = Get-OptionalProperty $_ 'helpUri'
    $hasName    = -not [string]::IsNullOrEmpty($name)
    $hasHelpUri = -not [string]::IsNullOrEmpty($helpUri)
    $isNovel    = $_.id -like 'NOVEL-*'
    if ($isNovel) {
        # NOVEL- descriptors have no upstream taxonomy to enrich from; the
        # enricher correctly skips them. Show them as expected-bare.
        $marker = '[--]'
        $shown  = '(NOVEL: not enriched by design)'
    } else {
        $marker = if ($hasName -and $hasHelpUri) { '[OK]' } else { '[!!]' }
        $shown  = if ($hasName) { $name } else { '(not enriched)' }
    }
    "{0} {1,-10} {2}" -f $marker, $_.id, $shown
} | ForEach-Object { Write-Status $_ }

# Only CWE descriptors are expected to be enriched. NOVEL- descriptors do not
# have a taxonomy back-end, so the enricher correctly leaves them bare.
$unenriched = @($rules | Where-Object {
    $_.id -notlike 'NOVEL-*' -and [string]::IsNullOrEmpty((Get-OptionalProperty $_ 'name'))
})
if ($unenriched.Count -gt 0) {
    Write-Warning "$($unenriched.Count) CWE rule(s) were not enriched. Expected the CweTaxonomyEnricher to populate every CWE-* ruleId. Investigate."
    exit 1
}

Write-Status ""
Write-Status "All CWE rule descriptors enriched successfully."

# Verify each sub-id finding wired up the way SARIF §3.49.3 / §3.52.4 describe:
# the full hierarchical id stays on result.ruleId; result.ruleIndex points at
# the BASE descriptor so the enricher's MITRE metadata applies. Two CWE-79
# sub-ids should share descriptor index 0.
$subIdResults = @($run.results | Where-Object {
    (Get-OptionalProperty $_ 'ruleId') -and ((Get-OptionalProperty $_ 'ruleId') -like '*/*')
})
if ($subIdResults.Count -gt 0) {
    Write-Status ""
    Write-Status "Taxonomy sub-id findings (BASE/sub-id form):"
    foreach ($h in $subIdResults) {
        $hRuleId = Get-OptionalProperty $h 'ruleId'
        $hRuleIndex = Get-OptionalProperty $h 'ruleIndex'
        $baseId = $hRuleId.Split('/')[0]
        $descriptor = if ($null -ne $hRuleIndex -and $hRuleIndex -ge 0 -and $hRuleIndex -lt $rules.Count) { $rules[$hRuleIndex] } else { $null }
        $descriptorId = if ($null -ne $descriptor) { Get-OptionalProperty $descriptor 'id' } else { $null }
        $mark = if ($descriptorId -eq $baseId) { '[OK]' } else { '[!!]' }
        Write-Status ("{0} {1} -> ruleIndex {2} -> descriptor '{3}' (base '{4}')" -f $mark, $hRuleId, $hRuleIndex, $descriptorId, $baseId)
        if ($descriptorId -ne $baseId) {
            Write-Warning "Sub-id ruleId '$hRuleId' did not resolve to its base descriptor '$baseId'."
            exit 1
        }
    }
}

# Verify NOVEL- escape-hatch findings: result.ruleId stays flat (no slash),
# the descriptor is registered with the same flat id, and it is correctly
# left un-enriched.
$novelResults = @($run.results | Where-Object {
    (Get-OptionalProperty $_ 'ruleId') -and ((Get-OptionalProperty $_ 'ruleId') -like 'NOVEL-*')
})
if ($novelResults.Count -gt 0) {
    Write-Status ""
    Write-Status "NOVEL escape-hatch findings:"
    foreach ($n in $novelResults) {
        $nRuleId = Get-OptionalProperty $n 'ruleId'
        $nRuleIndex = Get-OptionalProperty $n 'ruleIndex'
        $descriptor = if ($null -ne $nRuleIndex -and $nRuleIndex -ge 0 -and $nRuleIndex -lt $rules.Count) { $rules[$nRuleIndex] } else { $null }
        $descriptorId = if ($null -ne $descriptor) { Get-OptionalProperty $descriptor 'id' } else { $null }
        $mark = if ($descriptorId -eq $nRuleId) { '[OK]' } else { '[!!]' }
        Write-Status ("{0} {1} -> ruleIndex {2} -> descriptor '{3}' (flat, no enrichment)" -f $mark, $nRuleId, $nRuleIndex, $descriptorId)
        if ($descriptorId -ne $nRuleId) {
            Write-Warning "NOVEL- ruleId '$nRuleId' did not register a flat descriptor."
            exit 1
        }
    }
}
