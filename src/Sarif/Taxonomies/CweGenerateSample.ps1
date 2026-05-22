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
      3. Appends a handful of CWE-shaped Result and Notification events
         to the .wip.jsonl as raw JSONL (no SDK in the data path) to
         demonstrate that the on-disk envelope is a publicly authorable
         contract.
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
    stays clean and re-runs do not collide.

.PARAMETER Configuration
    Build configuration whose multitool binary to invoke. Release or
    Debug. Defaults to Release.

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1

.EXAMPLE
    pwsh src/Sarif/Taxonomies/CweGenerateSample.ps1 `
        -OutputDirectory C:\temp\cwe-sample -Configuration Debug
#>
[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot   = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')
$multitool  = Join-Path $repoRoot "bld/bin/AnyCPU_$Configuration/Sarif.Multitool/net8.0/Sarif.Multitool.dll"

if (-not (Test-Path $multitool)) {
    throw "Sarif.Multitool.dll not found at '$multitool'. Build the SDK in $Configuration configuration first (e.g. dotnet build src\Sarif.Multitool\Sarif.Multitool.csproj -c $Configuration)."
}

if (-not $OutputDirectory) {
    $stamp = [System.Guid]::NewGuid().ToString('N').Substring(0, 8)
    $OutputDirectory = Join-Path $env:TEMP "cwe-sample-$stamp"
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$outPath = Join-Path $OutputDirectory 'cwe-sample.sarif'
$wipPath = "$outPath.wip.jsonl"

Write-Host "[1/4] Opening run -> $outPath"
& dotnet $multitool emit-init-run $outPath `
    --tool-driver-name 'CweSamplerScanner' `
    --tool-version    '0.1.0' `
    --information-uri 'https://github.com/microsoft/sarif-sdk' `
    --srcroot         'file:///D:/src/sarif-sdk/' | Out-Host

# Each line is a SarifEvent envelope: {"v":1,"kind":"<kind>","payload":<SARIF object>}
# Lines MUST be LF-terminated. AtomicSarifWriter rejects torn (non-LF) lines on
# append-open. Do not use Add-Content; it writes CRLF on Windows.
$events = @(
    @{ kind = 'result'; cwe = 'CWE-79';   level = 'warning'; status = 'Stable';     msg = 'Possible XSS via unescaped user input in view template.';                  uri = 'src/Web/Home.cshtml';            line = 42 }
    @{ kind = 'result'; cwe = 'CWE-89';   level = 'error';   status = 'Stable';     msg = 'SQL query built via string concatenation; parameterize.';                 uri = 'src/Data/UserRepo.cs';           line = 117 }
    @{ kind = 'result'; cwe = 'CWE-22';   level = 'error';   status = 'Stable';     msg = 'Path constructed from untrusted input without canonicalization.';        uri = 'src/Io/ArchiveExtractor.cs';     line = 88 }
    @{ kind = 'result'; cwe = 'CWE-798';  level = 'error';   status = 'Draft';      msg = 'Hard-coded credential in production code path.';                          uri = 'src/Auth/LegacyClient.cs';       line = 14 }
    @{ kind = 'result'; cwe = 'CWE-1220'; level = 'warning'; status = 'Incomplete'; msg = 'Authorization check missing tenant-scoped granularity.';                  uri = 'src/Authz/TenantPolicy.cs';      line = 51 }
)

Write-Host "[2/4] Appending $($events.Count) Result events + 1 Notification as raw JSONL"
foreach ($e in $events) {
    $payload = @{
        ruleId  = $e.cwe
        level   = $e.level
        message = @{ text = $e.msg }
        locations = @(@{
            physicalLocation = @{
                artifactLocation = @{ uri = $e.uri; uriBaseId = 'SRCROOT' }
                region           = @{ startLine = $e.line }
            }
        })
    }
    $line = (@{ v = 1; kind = $e.kind; payload = $payload } | ConvertTo-Json -Compress -Depth 12)
    [System.IO.File]::AppendAllText($wipPath, $line + "`n")
}

$notif = @{ v = 1; kind = 'notification'; payload = @{ level = 'note'; message = @{ text = "Analyzed $($events.Count) findings across $(($events.uri | Sort-Object -Unique).Count) files." } } } | ConvertTo-Json -Compress -Depth 8
[System.IO.File]::AppendAllText($wipPath, $notif + "`n")

Write-Host "[3/4] Finalizing"
& dotnet $multitool emit-finalize $outPath | Out-Host

Write-Host "[4/4] Verifying enrichment"
$log = Get-Content $outPath -Raw | ConvertFrom-Json
$run = $log.runs[0]
$rules = $run.tool.driver.rules

Write-Host ""
Write-Host "Sample SARIF: $outPath"
Write-Host "Results:      $($run.results.Count)"
Write-Host "Rules:        $($rules.Count) (auto-registered from result.ruleId)"
Write-Host ""
$rules | ForEach-Object {
    $hasName    = -not [string]::IsNullOrEmpty($_.name)
    $hasHelpUri = $_.PSObject.Properties.Match('helpUri').Count -gt 0 -and -not [string]::IsNullOrEmpty($_.helpUri)
    $marker     = if ($hasName -and $hasHelpUri) { '[OK]' } else { '[!!]' }
    $name       = if ($hasName) { $_.name } else { '(not enriched)' }
    "{0} {1,-10} {2}" -f $marker, $_.id, $name
} | ForEach-Object { Write-Host $_ }

$unenriched = @($rules | Where-Object { [string]::IsNullOrEmpty($_.name) })
if ($unenriched.Count -gt 0) {
    Write-Warning "$($unenriched.Count) rule(s) were not enriched. Expected the CweTaxonomyEnricher to populate every CWE-* ruleId. Investigate."
    exit 1
}

Write-Host ""
Write-Host "All CWE rule descriptors enriched successfully."
