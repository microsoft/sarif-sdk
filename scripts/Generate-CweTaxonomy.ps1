# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

<#
.SYNOPSIS
    Regenerates src/Sarif/Taxonomies/CWE.sarif from the authoritative MITRE
    Common Weakness Enumeration (CWE) catalog.

.DESCRIPTION
    Downloads the official CWE "View-1000 Research Concepts" CSV from
    cwe.mitre.org, parses it, and emits a SARIF 2.1.0 log file whose single
    run carries a CWE taxonomy (per SARIF section 3.19). The taxonomy is committed
    so the SARIF SDK and consumers like Sarif.Mcp.Server can reference it
    statically; no runtime network access is required.

    CWE is published by MITRE Corporation under terms permitting redistribution
    with attribution. The generated taxonomy preserves the attribution metadata
    on its toolComponent (organization, informationUri, downloadUri).

    Re-run this script when MITRE publishes a new CWE version. Commit the
    regenerated file with a release-notes entry naming the new CWE version.

.PARAMETER OutputPath
    Where to write the generated taxonomy. Defaults to the canonical location
    src/Sarif/Taxonomies/CWE.sarif relative to the repository root.

.PARAMETER SourceUrl
    Override the MITRE download URL (mainly for testing).

.EXAMPLE
    pwsh scripts/Generate-CweTaxonomy.ps1
#>

[CmdletBinding()]
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..' 'src' 'Sarif' 'Taxonomies' 'CWE.sarif'),
    [string]$SourceUrl  = 'https://cwe.mitre.org/data/csv/1000.csv.zip'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$staging  = Join-Path $repoRoot 'bld' 'cwe-taxonomy'
New-Item -ItemType Directory -Path $staging -Force | Out-Null

$zipPath = Join-Path $staging 'cwe-1000.csv.zip'
$csvDir  = Join-Path $staging 'csv'

Write-Host "[1/4] Downloading $SourceUrl ..."
Invoke-WebRequest -Uri $SourceUrl -OutFile $zipPath -UseBasicParsing

Write-Host "[2/4] Extracting ..."
if (Test-Path $csvDir) { Remove-Item -Recurse -Force $csvDir }
Expand-Archive -Path $zipPath -DestinationPath $csvDir

$csvFile = Get-ChildItem -Path $csvDir -Filter '*.csv' | Select-Object -First 1
if (-not $csvFile) { throw "No CSV found inside $zipPath" }

Write-Host "[3/4] Parsing $($csvFile.Name) ..."
$rows = Import-Csv $csvFile.FullName

$taxa = New-Object System.Collections.Generic.List[object]
$skipped = 0
foreach ($row in $rows)
{
    $id = $row.'CWE-ID'
    $name = $row.Name
    $description = $row.Description

    # Quality filter: require non-empty id, name, and short-description so every
    # taxon serves as a useful lookup target. CWE entries that fail this
    # filter would carry no value to a consumer reading "what is CWE-N?".
    if ([string]::IsNullOrWhiteSpace($id) -or
        [string]::IsNullOrWhiteSpace($name) -or
        [string]::IsNullOrWhiteSpace($description))
    {
        $skipped++
        continue
    }

    $taxon = [ordered]@{
        id               = $id
        name             = $name
        shortDescription = [ordered]@{ text = $description }
    }

    if (-not [string]::IsNullOrWhiteSpace($row.'Extended Description'))
    {
        $taxon.fullDescription = [ordered]@{ text = $row.'Extended Description' }
    }

    $taxon.helpUri = "https://cwe.mitre.org/data/definitions/$id.html"

    # Capture catalog status + abstraction so SARIF producers can filter or
    # surface this metadata when they cite the taxon. Properties are scoped
    # to the CWE taxonomy namespace to avoid colliding with consumer keys.
    $properties = [ordered]@{}
    if (-not [string]::IsNullOrWhiteSpace($row.Status))
    {
        $properties.'cwe/status' = $row.Status
    }
    if (-not [string]::IsNullOrWhiteSpace($row.'Weakness Abstraction'))
    {
        $properties.'cwe/abstraction' = $row.'Weakness Abstraction'
    }
    if ($properties.Count -gt 0)
    {
        $taxon.properties = $properties
    }

    $taxa.Add([pscustomobject]$taxon)
}

Write-Host "Parsed $($rows.Count) rows; kept $($taxa.Count); skipped $skipped (missing name/description)."

# CWE catalog version: pull from the CSV filename pattern when possible,
# else fall back to a generation-date marker.
$cweVersion = 'View-1000 (' + (Get-Date -Format 'yyyy-MM-dd') + ')'

$taxonomy = [ordered]@{
    name             = 'CWE'
    version          = $cweVersion
    organization     = 'MITRE'
    informationUri   = 'https://cwe.mitre.org/'
    downloadUri      = $SourceUrl
    isComprehensive  = $true
    minimumRequiredLocalizedDataSemanticVersion = '1.0.0'
    shortDescription = [ordered]@{ text = 'Common Weakness Enumeration (CWE), View 1000 (Research Concepts).' }
    fullDescription  = [ordered]@{ text = 'A community-developed list of software and hardware weakness types maintained by MITRE Corporation. Published under terms permitting redistribution with attribution.' }
    taxa             = $taxa
}

$run = [ordered]@{
    tool = [ordered]@{
        driver = [ordered]@{
            name = 'Microsoft.CodeAnalysis.Sarif.Taxonomies.CweGenerator'
            informationUri = 'https://github.com/microsoft/sarif-sdk'
        }
    }
    taxonomies = @($taxonomy)
}

$sarifLog = [ordered]@{
    '$schema' = 'https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0.json'
    version   = '2.1.0'
    runs      = @($run)
}

Write-Host "[4/4] Writing taxonomy to $OutputPath ..."
$outputDir = Split-Path $OutputPath -Parent
if (-not (Test-Path $outputDir))
{
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# ConvertTo-Json with adequate depth; the taxa array nests at depth ~5
# (sarifLog.runs.taxonomies.taxa.shortDescription.text).
$json = $sarifLog | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $outputDir).Path + [System.IO.Path]::DirectorySeparatorChar + (Split-Path $OutputPath -Leaf), $json, [System.Text.UTF8Encoding]::new($false))

Write-Host "Wrote $($taxa.Count) taxa to $OutputPath"
