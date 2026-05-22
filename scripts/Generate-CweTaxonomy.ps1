# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

<#
.SYNOPSIS
    Regenerates the per-status CWE taxonomy artifacts under src/Sarif/Taxonomies/
    from the authoritative MITRE Common Weakness Enumeration (CWE) XML catalog.

.DESCRIPTION
    Downloads cwec_latest.xml.zip from cwe.mitre.org, partitions the entries by
    Status (Stable, Draft, Incomplete, Deprecated, Obsolete), and emits two
    artifacts per status:

      CWE.<Status>.sarif       - SARIF 2.1.0 taxonomy with verbatim help content
                                 (Description + Extended_Description + Common
                                 Consequences + Potential Mitigations) embedded
                                 in reportingDescriptor.help.markdown.

      CWE.<Status>.brief.md    - Compact markdown table (ID, Name, Abstraction,
                                 Parent, Description) sized for AI prompt
                                 context-window injection.

    Both forms are committed and embedded as resources in Sarif.dll. No runtime
    network access is required.

    CWE is published by MITRE Corporation under terms permitting redistribution
    with attribution. The generated taxonomies preserve attribution metadata.

.PARAMETER OutputDirectory
    Target directory. Defaults to src/Sarif/Taxonomies/ at the repository root.

.PARAMETER SourceUrl
    Override the MITRE download URL (primarily for testing).

.EXAMPLE
    pwsh scripts/Generate-CweTaxonomy.ps1
#>
[CmdletBinding()]
param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..' 'src' 'Sarif' 'Taxonomies'),
    [string]$SourceUrl       = 'https://cwe.mitre.org/data/xml/cwec_latest.xml.zip'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Statuses we emit. We always emit all five even if a status is empty in the
# current catalog (CWE 4.20 currently has 0 Obsolete entries) so consumer
# logic uniformly handles the matrix.
$AllStatuses = @('Stable', 'Draft', 'Incomplete', 'Deprecated', 'Obsolete')

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$staging  = Join-Path $repoRoot 'bld' 'cwe-taxonomy'
New-Item -ItemType Directory -Path $staging -Force | Out-Null

$zipPath = Join-Path $staging 'cwec_latest.xml.zip'
Write-Host "[1/4] Downloading $SourceUrl ..."
Invoke-WebRequest -Uri $SourceUrl -OutFile $zipPath -UseBasicParsing

Write-Host "[2/4] Extracting ..."
Expand-Archive -Path $zipPath -DestinationPath $staging -Force
$xmlFile = Get-ChildItem -Path $staging -Filter 'cwec_v*.xml' | Select-Object -First 1
if (-not $xmlFile) { throw "No CWE XML found in $staging" }
$versionMatch = [regex]::Match($xmlFile.Name, 'cwec_v([\d.]+)\.xml')
$cweVersion = if ($versionMatch.Success) { $versionMatch.Groups[1].Value } else { '0.0' }

Write-Host "[3/4] Parsing $($xmlFile.Name) (CWE v$cweVersion) ..."
$xml = [xml](Get-Content $xmlFile.FullName -Raw)
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace('c', 'http://cwe.mitre.org/cwe-7')

function Get-CleanText {
    param($node)
    if ($null -eq $node) { return '' }
    if ($node -is [string]) { return $node.Trim() }
    return ($node.InnerText -replace '\s+', ' ').Trim()
}

function Has-Property {
    param($obj, [string]$name)
    if ($null -eq $obj) { return $false }
    return [bool]($obj.PSObject.Properties.Match($name).Count)
}

function Get-View1000Parent {
    param($w)
    if (-not (Has-Property $w 'Related_Weaknesses') -or -not $w.Related_Weaknesses) { return $null }
    if (-not (Has-Property $w.Related_Weaknesses 'Related_Weakness')) { return $null }
    $rels = @($w.Related_Weaknesses.Related_Weakness)
    $candidates = $rels | Where-Object {
        $_.Nature -eq 'ChildOf' -and $_.View_ID -eq '1000'
    }
    if (-not $candidates -or $candidates.Count -eq 0) { return $null }
    $primary = $candidates | Where-Object { (Has-Property $_ 'Ordinal') -and $_.Ordinal -eq 'Primary' } | Select-Object -First 1
    $parent  = if ($primary) { $primary } else { $candidates[0] }
    return "CWE-$($parent.CWE_ID)"
}

function Build-HelpMarkdown {
    param($w)
    $sb = [System.Text.StringBuilder]::new()

    # Description
    if (Has-Property $w 'Description') {
        $desc = Get-CleanText $w.Description
        if ($desc) {
            $null = $sb.AppendLine('## Description').AppendLine()
            $null = $sb.AppendLine($desc).AppendLine()
        }
    }

    # Extended Description
    if (Has-Property $w 'Extended_Description') {
        $ext = Get-CleanText $w.Extended_Description
        if ($ext) {
            $null = $sb.AppendLine('## Extended Description').AppendLine()
            $null = $sb.AppendLine($ext).AppendLine()
        }
    }

    # Common Consequences
    if ((Has-Property $w 'Common_Consequences') -and $w.Common_Consequences -and (Has-Property $w.Common_Consequences 'Consequence')) {
        $consequences = @($w.Common_Consequences.Consequence)
        if ($consequences.Count -gt 0) {
            $null = $sb.AppendLine('## Common Consequences').AppendLine()
            foreach ($c in $consequences) {
                $scopeText  = if ((Has-Property $c 'Scope')  -and $c.Scope)  { (@($c.Scope)  -join ', ') } else { '' }
                $impactText = if ((Has-Property $c 'Impact') -and $c.Impact) { (@($c.Impact) -join ', ') } else { '' }
                $line = '- '
                if ($scopeText)  { $line += "**Scope**: $scopeText. " }
                if ($impactText) { $line += "**Impact**: $impactText." }
                $null = $sb.AppendLine($line.TrimEnd())
                if ((Has-Property $c 'Note') -and $c.Note) {
                    $noteText = Get-CleanText $c.Note
                    if ($noteText) { $null = $sb.AppendLine("  - $noteText") }
                }
            }
            $null = $sb.AppendLine()
        }
    }

    # Potential Mitigations
    if ((Has-Property $w 'Potential_Mitigations') -and $w.Potential_Mitigations -and (Has-Property $w.Potential_Mitigations 'Mitigation')) {
        $mitigations = @($w.Potential_Mitigations.Mitigation)
        if ($mitigations.Count -gt 0) {
            $null = $sb.AppendLine('## Potential Mitigations').AppendLine()
            $i = 1
            foreach ($m in $mitigations) {
                $phaseText    = if ((Has-Property $m 'Phase')    -and $m.Phase)    { (@($m.Phase) -join ', ') } else { '' }
                $strategyText = if ((Has-Property $m 'Strategy') -and $m.Strategy) { $m.Strategy }            else { '' }
                $tagParts = @()
                if ($phaseText)    { $tagParts += "Phase: $phaseText" }
                if ($strategyText) { $tagParts += "Strategy: $strategyText" }
                $tag = if ($tagParts) { '*' + ($tagParts -join '; ') + '*' } else { '' }
                $descText = if (Has-Property $m 'Description') { Get-CleanText $m.Description } else { '' }
                $line = "$i. "
                if ($tag) { $line += "$tag. " }
                $line += $descText
                $null = $sb.AppendLine($line)
                $i++
            }
            $null = $sb.AppendLine()
        }
    }

    return $sb.ToString().TrimEnd()
}

# Bucket Weaknesses by Status.
$bucketed = @{}
foreach ($s in $AllStatuses) { $bucketed[$s] = New-Object System.Collections.Generic.List[object] }

$weaknesses = $xml.SelectNodes('//c:Weaknesses/c:Weakness', $ns)
$skipped = 0
foreach ($w in $weaknesses) {
    $status = $w.Status
    if (-not $bucketed.ContainsKey($status)) { $skipped++; continue }
    $bucketed[$status].Add($w)
}
Write-Host "Parsed $($weaknesses.Count) weaknesses; skipped $skipped (unrecognized status)."
foreach ($s in $AllStatuses) {
    Write-Host ("  {0,-11} {1,4} entries" -f $s, $bucketed[$s].Count)
}

Write-Host "[4/4] Writing consolidated artifacts to $OutputDirectory ..."
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

# Build one flat list of all items, sorted by numeric ID for stable, browsable output.
$allItems = New-Object System.Collections.Generic.List[object]
foreach ($s in $AllStatuses) {
    foreach ($w in $bucketed[$s]) { $allItems.Add($w) }
}
$allItems = $allItems | Sort-Object { [int]$_.ID }

# ---- One SARIF taxonomy containing all 969 entries ----
$taxa = New-Object System.Collections.Generic.List[object]
foreach ($w in $allItems) {
    $id = "CWE-$($w.ID)"
    $name = $w.Name
    $shortText = if (Has-Property $w 'Description') { Get-CleanText $w.Description } else { '' }

    $taxon = [ordered]@{
        id   = $id
        name = $name
        shortDescription = [ordered]@{ text = $shortText }
    }

    if (Has-Property $w 'Extended_Description') {
        $ext = Get-CleanText $w.Extended_Description
        if ($ext) { $taxon.fullDescription = [ordered]@{ text = $ext } }
    }

    $help = Build-HelpMarkdown $w
    if ($help) {
        $taxon.help = [ordered]@{
            text     = $shortText
            markdown = $help
        }
    }
    $taxon.helpUri = "https://cwe.mitre.org/data/definitions/$($w.ID).html"

    $props = [ordered]@{
        'cwe/status'      = $w.Status
        'cwe/abstraction' = if (Has-Property $w 'Abstraction') { $w.Abstraction } else { '' }
    }
    $parent = Get-View1000Parent $w
    if ($parent) { $props.'cwe/parent' = $parent }
    $taxon.properties = $props

    $taxa.Add([pscustomobject]$taxon)
}

$statusCounts = ($AllStatuses | ForEach-Object { "$_=$($bucketed[$_].Count)" }) -join ', '

$taxonomy = [ordered]@{
    name             = 'CWE'
    version          = $cweVersion
    organization     = 'MITRE'
    informationUri   = 'https://cwe.mitre.org/'
    downloadUri      = $SourceUrl
    isComprehensive  = $true
    minimumRequiredLocalizedDataSemanticVersion = '1.0.0'
    shortDescription = [ordered]@{ text = "MITRE Common Weakness Enumeration (CWE). $($allItems.Count) entries from cwec_v$cweVersion.xml." }
    fullDescription  = [ordered]@{ text = "Complete snapshot of the MITRE CWE catalog. Each taxon carries cwe/status ($statusCounts), cwe/abstraction, and (where applicable) cwe/parent (canonical ChildOf parent under CWE View 1000 / Research Concepts)." }
    taxa             = $taxa
}

$run = [ordered]@{
    tool = [ordered]@{
        driver = [ordered]@{
            name           = 'Microsoft.CodeAnalysis.Sarif.Taxonomies.CweGenerator'
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

$sarifPath = Join-Path $OutputDirectory 'CWE.sarif'
$json = $sarifLog | ConvertTo-Json -Depth 50
[System.IO.File]::WriteAllText($sarifPath, $json, [System.Text.UTF8Encoding]::new($false))

# ---- One brief markdown table containing all 969 entries (Status column added) ----
$sb = [System.Text.StringBuilder]::new()
$null = $sb.AppendLine('# CWE').AppendLine()
$null = $sb.AppendLine("Compact one-row-per-entry index of the MITRE CWE catalog (cwec_v$cweVersion.xml). $($allItems.Count) entries (Stable $($bucketed['Stable'].Count), Draft $($bucketed['Draft'].Count), Incomplete $($bucketed['Incomplete'].Count), Deprecated $($bucketed['Deprecated'].Count), Obsolete $($bucketed['Obsolete'].Count)). Sorted by numeric ID.").AppendLine()
$null = $sb.AppendLine('Designed for AI prompt-context injection: full id form (`CWE-N`), name, abstraction level (Pillar/Class/Base/Variant/Compound), MITRE maturity status, parent in View 1000 (Research Concepts), and the verbatim MITRE Description.').AppendLine()
$null = $sb.AppendLine('| ID | Name | Abstraction | Status | Parent | Description |')
$null = $sb.AppendLine('|----|------|-------------|--------|--------|-------------|')

foreach ($w in $allItems) {
    $idCell     = "CWE-$($w.ID)"
    $nameCell   = (($w.Name -replace '\|', '\|') -replace "`r?`n", ' ').Trim()
    $absCell    = if (Has-Property $w 'Abstraction') { $w.Abstraction } else { '' }
    $statusCell = $w.Status
    $parent     = Get-View1000Parent $w
    $parentCell = if ($parent) { $parent } else { '' }
    $descSrc    = if (Has-Property $w 'Description') { Get-CleanText $w.Description } else { '' }
    $descCell   = ($descSrc -replace '\|', '\|') -replace "`r?`n", ' '
    $null = $sb.AppendLine("| $idCell | $nameCell | $absCell | $statusCell | $parentCell | $descCell |")
}

$briefPath = Join-Path $OutputDirectory 'CWE.brief.md'
[System.IO.File]::WriteAllText($briefPath, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))

Write-Host ("  -> CWE.sarif ({0:N0} bytes) / CWE.brief.md ({1:N0} bytes) -- {2:N0} taxa" -f `
    (Get-Item $sarifPath).Length, (Get-Item $briefPath).Length, $allItems.Count)

Write-Host "Done."


