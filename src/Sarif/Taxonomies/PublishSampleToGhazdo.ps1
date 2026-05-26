<#
.SYNOPSIS
    Publishes a SARIF file to GitHub Advanced Security for Azure DevOps (GHAzDO).

.DESCRIPTION
    POSTs the gzipped SARIF body to the GHAzDO SARIFs ingestion endpoint. The
    target organization / project / repository are derived from the SARIF
    file itself, specifically:

        runs[0].versionControlProvenance[0].repositoryUri

    The Personal Access Token must be supplied via the ADO_PAT environment
    variable; the script will not look for token files or prompt. The PAT
    needs Advanced Security (Read & Write) scope on the target organization.

    Endpoint host is `https://advsec.dev.azure.com` with a fallback to
    `https://dev.azure.com` on 404. The body is gzip-compressed in memory
    and sent with `Content-Type: application/octet-stream`. The Content-
    Encoding header is deliberately NOT set: the server reads the body as
    raw bytes and manually gunzips it, so a protocol-level decompression
    hint would trigger double-decompression and a 400 JsonReaderException.

    Supported VCP URI forms (the regexes accept an optional .git suffix):
        https://dev.azure.com/{org}/{project}/_git/{repo}
        https://{org}.visualstudio.com/{project}/_git/{repo}

.PARAMETER SarifPath
    Path to the SARIF file to publish. Its runs[0].versionControlProvenance[0]
    .repositoryUri must be a recognized Azure DevOps repository URL.

.PARAMETER ApiVersion
    GHAzDO SARIFs API version. Default '7.2-preview.1'.

.EXAMPLE
    $env:ADO_PAT = '<your-pat>'
    pwsh ./PublishSampleToGhazdo.ps1 -SarifPath ./CweGHAzDoSample.sarif
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $SarifPath,
    [string] $ApiVersion = '7.2-preview.1'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$pat = $env:ADO_PAT
if ([string]::IsNullOrWhiteSpace($pat)) {
    throw "ADO_PAT environment variable is not set. Generate a PAT with 'Advanced Security (Read & Write)' scope and set it before invoking this script, e.g.: `$env:ADO_PAT = '<pat>'."
}

if (-not (Test-Path $SarifPath)) { throw "SARIF file not found: $SarifPath" }

function ConvertTo-AdoTarget {
    param([string]$repositoryUri)
    $u = $repositoryUri -replace '\.git$',''
    if ($u -match '^https?://(?:[^@/]+@)?dev\.azure\.com/([^/]+)/([^/]+)/_git/([^/?#]+)/?$') {
        return [pscustomobject]@{
            Organization = $Matches[1]
            Project      = [System.Uri]::UnescapeDataString($Matches[2])
            Repository   = [System.Uri]::UnescapeDataString($Matches[3])
        }
    }
    if ($u -match '^https?://([^.]+)\.visualstudio\.com/([^/]+)/_git/([^/?#]+)/?$') {
        return [pscustomobject]@{
            Organization = $Matches[1]
            Project      = [System.Uri]::UnescapeDataString($Matches[2])
            Repository   = [System.Uri]::UnescapeDataString($Matches[3])
        }
    }
    throw "Could not parse an Azure DevOps repository URL from '$repositoryUri'. Supported forms: 'https://dev.azure.com/{org}/{project}/_git/{repo}' and 'https://{org}.visualstudio.com/{project}/_git/{repo}'."
}

# StrictMode-safe property access: $obj.Foo throws if Foo doesn't exist;
# $obj.PSObject.Properties['Foo'] returns $null (the indexer is case-
# insensitive on PSMemberInfoCollection).
$sarifJson = Get-Content $SarifPath -Raw | ConvertFrom-Json
$runsProp = $sarifJson.PSObject.Properties['runs']
if (-not $runsProp -or -not $runsProp.Value -or $runsProp.Value.Count -eq 0) {
    throw "SARIF has no runs[]: $SarifPath"
}
$run = $runsProp.Value[0]
$vcpProp = $run.PSObject.Properties['versionControlProvenance']
if (-not $vcpProp -or -not $vcpProp.Value -or $vcpProp.Value.Count -eq 0) {
    throw "SARIF runs[0] has no versionControlProvenance[]; cannot determine the publish target. Re-emit with --vcp-repositoryuri pointing at the target Azure DevOps repo, e.g.: --vcp-repositoryuri https://dev.azure.com/{org}/{project}/_git/{repo}."
}
$uriProp = $vcpProp.Value[0].PSObject.Properties['repositoryUri']
if (-not $uriProp -or [string]::IsNullOrWhiteSpace($uriProp.Value)) {
    throw "SARIF runs[0].versionControlProvenance[0].repositoryUri is missing or empty."
}
$repoUri = $uriProp.Value

$target = ConvertTo-AdoTarget $repoUri
Write-Host ("Target derived from SARIF runs[0].versionControlProvenance[0].repositoryUri:")
Write-Host ("  {0}" -f $repoUri)
Write-Host ("  -> org='{0}', project='{1}', repo='{2}'" -f $target.Organization, $target.Project, $target.Repository)

# Gzip the body in memory. We pass raw bytes (no Content-Encoding header) so
# the server's manual gunzip is the sole decompression step.
$rawBytes = [System.IO.File]::ReadAllBytes((Resolve-Path $SarifPath).Path)
$memStream = New-Object System.IO.MemoryStream
$gzipStream = New-Object System.IO.Compression.GZipStream($memStream, [System.IO.Compression.CompressionLevel]::Optimal, $true)
try {
    $gzipStream.Write($rawBytes, 0, $rawBytes.Length)
} finally {
    $gzipStream.Dispose()
}
$compressedBytes = $memStream.ToArray()
$memStream.Dispose()
Write-Host ("Uploading: $SarifPath ({0:n0} raw bytes -> {1:n0} gzip bytes)" -f $rawBytes.Length, $compressedBytes.Length)

$authHeader = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
$candidateHosts = @('advsec.dev.azure.com', 'dev.azure.com')
$response = $null
foreach ($adoHost in $candidateHosts) {
    $url = "https://$adoHost/$($target.Organization)/$($target.Project)/_apis/alert/repositories/$($target.Repository)/sarifs?api-version=$ApiVersion"
    Write-Host "POST $url"
    $response = Invoke-WebRequest -Uri $url -Method Post -Body $compressedBytes -ContentType 'application/octet-stream' -Headers @{ Authorization = $authHeader } -SkipHttpErrorCheck
    Write-Host ("Status: {0}" -f [int]$response.StatusCode)
    if ([int]$response.StatusCode -ne 404) { break }
    Write-Host "(404 — falling back to next host.)"
}

Write-Host "Body:"
Write-Host $response.Content

if ([int]$response.StatusCode -ge 400) {
    throw "Publish failed with HTTP $([int]$response.StatusCode)."
}
