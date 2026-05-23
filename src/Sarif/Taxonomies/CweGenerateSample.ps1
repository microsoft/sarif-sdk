# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

<#
.SYNOPSIS
    Emits CweSample.sarif — a deterministic, fully-enriched SARIF fixture
    that demonstrates the CWE taxonomy enricher end-to-end via the multitool
    emit verbs.

.DESCRIPTION
    Convention: every taxonomy that ships with this SDK includes a
    <Taxonomy>GenerateSample.ps1 alongside its data, and that script
    produces a checked-in <Taxonomy>Sample.sarif fixture next to itself.
    The script has one job: deterministically (re)generate the fixture
    so reviewers can diff it like any other source artifact and CI can
    assert it stays in sync with the emit chain.

    For CWE specifically the script:

      1. Locates the locally built Sarif.Multitool.dll under bld/bin/.
      2. Runs emit-init-run with a real local SRCROOT pointing at this
         repository's working tree, so InsertOptionalDataVisitor can read
         SampleCode.cs at finalize time and populate every value-add the
         emit chain provides:
           * artifact sha-256 hashes,
           * region.snippet,
           * contextRegion.snippet,
           * charOffset / charLength (comprehensive region properties).
      3. Pipes 7 CWE-shaped Result JSON objects and 1 Notification into
         the multitool's add-result / add-notification verbs over stdin.
         The verbs validate ruleId conformance to the AI ruleId convention
         (BASE/sub-id or NOVEL-<sub-id>) at receipt.
      4. Runs emit-finalize with --srcroot https://github.com/microsoft/sarif-sdk/blob/main/
         so the shipped fixture anchors at a portable, host-independent URL
         that resolves directly to file content on GitHub. The visitor's
         file reads above used the local SRCROOT; the swap happens after
         enrichment so both contracts hold.
      5. Prints a summary table verifying the enricher hydrated every
         CWE-* descriptor (and correctly left NOVEL- bare).

    Determinism notes:
      * notification.timeUtc is pre-populated so SarifEventReplayer does
        not auto-stamp DateTime.UtcNow (its AI2019 timeline support).
      * SampleCode.cs is pinned to LF via .gitattributes so snippets,
        contextRegion text, and the artifact sha-256 are byte-identical
        across Windows / Linux / macOS checkouts.
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

$repoRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')).Path
$multitool  = Join-Path $repoRoot "bld/bin/AnyCPU_$Configuration/Sarif.Multitool/net8.0/Sarif.Multitool.dll"

if (-not (Test-Path $multitool)) {
    throw "Sarif.Multitool.dll not found at '$multitool'. Build the SDK in $Configuration configuration first (e.g. dotnet build src\Sarif.Multitool\Sarif.Multitool.csproj -c $Configuration)."
}

$outPath = Join-Path $PSScriptRoot 'CweSample.sarif'
$wipPath = "$outPath.wip.jsonl"

# During emit, SRCROOT must be a real local file:// URI so the visitor can
# resolve src/Sarif/Taxonomies/SampleCode.cs and populate snippets/hashes.
# emit-finalize --srcroot below swaps this to the canonical GitHub URL.
$localSrcRootUri = ([System.Uri]$repoRoot).AbsoluteUri
if (-not $localSrcRootUri.EndsWith('/')) { $localSrcRootUri = "$localSrcRootUri/" }
$finalSrcRootUri = 'https://github.com/microsoft/sarif-sdk/blob/main/'

# Artifact URIs are repo-relative (paired with SRCROOT base id) so the final
# SRCROOT swap produces directly-clickable GitHub blob URLs:
#   https://github.com/microsoft/sarif-sdk/blob/main/src/Sarif/Taxonomies/SampleCode.cs
$sampleFileRepoRelative = 'src/Sarif/Taxonomies/SampleCode.cs'
$sampleFileOnDisk       = Join-Path $PSScriptRoot 'SampleCode.cs'

Write-Host "[1/4] Opening run -> $outPath"
$initArgs = @(
    $multitool, 'emit-init-run', $outPath,
    '--tool-driver-name', 'CweSamplerScanner',
    '--tool-version',     '0.1.0',
    '--information-uri',  'https://github.com/microsoft/sarif-sdk',
    '--srcroot',          $localSrcRootUri,
    '--force-overwrite'
)
& dotnet @initArgs | Out-Host
if ($LASTEXITCODE -ne 0) { throw "emit-init-run failed (exit $LASTEXITCODE)." }

# Each line piped into add-result / add-notification is a fully-formed SARIF
# Result or Notification JSON object. AI orchestrators have these objects in
# hand already (code flows, thread flows, fixes etc.) so the verbs accept
# the whole shape and round-trip it verbatim — they do NOT model individual
# fields as CLI flags.
#
# All findings point at SampleCode.cs (a checked-in, intentionally innocuous
# file). The richness of the sample comes from the SARIF structure — regions,
# enriched rule descriptors, taxonomy wiring — not from defects in the file.
$events = @(
    @{ kind = 'result'; cwe = 'CWE-79/unescaped-view-input';            level = 'warning'; status = 'Stable';     msg = 'Possible XSS via unescaped user input in view template.';                 startLine = 18; endLine = 18 }
    @{ kind = 'result'; cwe = 'CWE-89/string-concat-query';             level = 'error';   status = 'Stable';     msg = 'SQL query built via string concatenation; parameterize.';                 startLine = 22; endLine = 22 }
    @{ kind = 'result'; cwe = 'CWE-22/untrusted-path-no-canon';         level = 'error';   status = 'Stable';     msg = 'Path constructed from untrusted input without canonicalization.';         startLine = 26; endLine = 26 }
    @{ kind = 'result'; cwe = 'CWE-798/embedded-credential';            level = 'error';   status = 'Draft';      msg = 'Hard-coded credential in production code path.';                          startLine = 30; endLine = 30 }
    @{ kind = 'result'; cwe = 'CWE-1220/missing-tenant-scope';          level = 'warning'; status = 'Incomplete'; msg = 'Authorization check missing tenant-scoped granularity.';                  startLine = 34; endLine = 34 }
    @{ kind = 'result'; cwe = 'CWE-79/dom-xss-via-sanitizer-bypass';    level = 'error';   status = 'Stable';     msg = 'DOM XSS: untrusted value reaches innerHTML after a sanitizer that does not escape the current context. Second sub-id under CWE-79; shares the base descriptor with the first finding.'; startLine = 39; endLine = 39 }
    @{ kind = 'result'; cwe = 'NOVEL-prompt-injection-via-system-message'; level = 'error'; status = '(novel)';   msg = 'Untrusted content reaches a system-role prompt at runtime, letting an attacker override tool-use policy. No CWE entry fits — emitted under the NOVEL- escape hatch.'; startLine = 44; endLine = 44 }
)

Write-Host "[2/4] Appending $($events.Count) Result events + 1 Notification via add-result / add-notification"

# Read SampleCode.cs once and trim the region.startColumn to the first
# non-whitespace column of each finding's start line. The region then points
# precisely at the meaningful content; the context region built by
# InsertOptionalDataVisitor carries the surrounding indent and neighboring
# lines so a reviewer still sees how the finding sits in its file.
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

    $payload = @{
        ruleId  = $e.cwe
        level   = $e.level
        message = @{ text = $e.msg }
        locations = @(@{
            physicalLocation = @{
                artifactLocation = @{ uri = $sampleFileRepoRelative; uriBaseId = 'SRCROOT' }
                region           = @{ startLine = $e.startLine; startColumn = $startColumn; endLine = $e.endLine }
            }
        })
    }
    $payloadJson = $payload | ConvertTo-Json -Compress -Depth 12
    $payloadJson | & dotnet $multitool add-result $outPath | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "add-result failed for ruleId '$($e.cwe)' (exit $LASTEXITCODE)." }
}

# Pre-populate timeUtc so SarifEventReplayer leaves it alone (it auto-stamps
# DateTime.UtcNow when absent for AI2019 timeline support). Without this the
# fixture's byte hash drifts per regeneration.
$notifPayload = @{
    level   = 'note'
    message = @{ text = "Analyzed $($events.Count) findings across 1 file." }
    timeUtc = '2024-01-01T00:00:00.000Z'
}
$notifJson = $notifPayload | ConvertTo-Json -Compress -Depth 8
$notifJson | & dotnet $multitool add-notification $outPath | Out-Null
if ($LASTEXITCODE -ne 0) { throw "add-notification failed (exit $LASTEXITCODE)." }

Write-Host "[3/4] Finalizing (rewriting SRCROOT to $finalSrcRootUri)"
$finalizeArgs = @(
    $multitool, 'emit-finalize', $outPath,
    '--srcroot', $finalSrcRootUri
)
& dotnet @finalizeArgs | Out-Host
if ($LASTEXITCODE -ne 0) { throw "emit-finalize failed (exit $LASTEXITCODE)." }

Write-Host "[4/4] Verifying enrichment"
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
$srcRootUri  = Get-OptionalProperty (Get-OptionalProperty $run.originalUriBaseIds 'SRCROOT') 'uri'

Write-Host ""
Write-Host "Sample SARIF: $outPath"
$toolLine = if ([string]::IsNullOrEmpty($toolName)) { '(missing - is your multitool DLL current?)' } else { $toolName }
if (-not [string]::IsNullOrEmpty($toolVersion)) { $toolLine = "$toolLine $toolVersion" }
Write-Host "Tool:         $toolLine"
Write-Host "Info URI:     $toolInfoUri"
Write-Host "SRCROOT URI:  $srcRootUri"
Write-Host "Results:      $($run.results.Count)"
Write-Host "Rules:        $($rules.Count) (auto-registered from result.ruleId)"
if ($null -ne $artifacts) {
    $hashedCount = @($artifacts | Where-Object { $null -ne (Get-OptionalProperty $_ 'hashes') }).Count
    Write-Host "Artifacts:    $($artifacts.Count) ($hashedCount with sha-256 hash)"
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
    Write-Host "Region snippet:        $regionMark"
    Write-Host "Context region snippet: $contextRegionMark"
}
Write-Host ""
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
} | ForEach-Object { Write-Host $_ }

# Only CWE descriptors are expected to be enriched. NOVEL- descriptors do not
# have a taxonomy back-end, so the enricher correctly leaves them bare.
$unenriched = @($rules | Where-Object {
    $_.id -notlike 'NOVEL-*' -and [string]::IsNullOrEmpty((Get-OptionalProperty $_ 'name'))
})
if ($unenriched.Count -gt 0) {
    Write-Warning "$($unenriched.Count) CWE rule(s) were not enriched. Expected the CweTaxonomyEnricher to populate every CWE-* ruleId. Investigate."
    exit 1
}

Write-Host ""
Write-Host "All CWE rule descriptors enriched successfully."

# Verify each sub-id finding wired up the way SARIF §3.49.3 / §3.52.4 describe:
# the full hierarchical id stays on result.ruleId; result.ruleIndex points at
# the BASE descriptor so the enricher's MITRE metadata applies. Two CWE-79
# sub-ids should share descriptor index 0.
$subIdResults = @($run.results | Where-Object {
    (Get-OptionalProperty $_ 'ruleId') -and ((Get-OptionalProperty $_ 'ruleId') -like '*/*')
})
if ($subIdResults.Count -gt 0) {
    Write-Host ""
    Write-Host "Taxonomy sub-id findings (BASE/sub-id form):"
    foreach ($h in $subIdResults) {
        $hRuleId = Get-OptionalProperty $h 'ruleId'
        $hRuleIndex = Get-OptionalProperty $h 'ruleIndex'
        $baseId = $hRuleId.Split('/')[0]
        $descriptor = if ($null -ne $hRuleIndex -and $hRuleIndex -ge 0 -and $hRuleIndex -lt $rules.Count) { $rules[$hRuleIndex] } else { $null }
        $descriptorId = if ($null -ne $descriptor) { Get-OptionalProperty $descriptor 'id' } else { $null }
        $mark = if ($descriptorId -eq $baseId) { '[OK]' } else { '[!!]' }
        Write-Host ("{0} {1} -> ruleIndex {2} -> descriptor '{3}' (base '{4}')" -f $mark, $hRuleId, $hRuleIndex, $descriptorId, $baseId)
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
    Write-Host ""
    Write-Host "NOVEL escape-hatch findings:"
    foreach ($n in $novelResults) {
        $nRuleId = Get-OptionalProperty $n 'ruleId'
        $nRuleIndex = Get-OptionalProperty $n 'ruleIndex'
        $descriptor = if ($null -ne $nRuleIndex -and $nRuleIndex -ge 0 -and $nRuleIndex -lt $rules.Count) { $rules[$nRuleIndex] } else { $null }
        $descriptorId = if ($null -ne $descriptor) { Get-OptionalProperty $descriptor 'id' } else { $null }
        $mark = if ($descriptorId -eq $nRuleId) { '[OK]' } else { '[!!]' }
        Write-Host ("{0} {1} -> ruleIndex {2} -> descriptor '{3}' (flat, no enrichment)" -f $mark, $nRuleId, $nRuleIndex, $descriptorId)
        if ($descriptorId -ne $nRuleId) {
            Write-Warning "NOVEL- ruleId '$nRuleId' did not register a flat descriptor."
            exit 1
        }
    }
}
