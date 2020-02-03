<#
.SYNOPSIS
    Delete test outputs from bld\bin to reduce published content size.
.DESCRIPTION
    Deletes all test project outputs from bld\bin; reduces published size by ~450 MB.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

Write-Information "Removing Test Outputs before Publish..."
foreach ($project in $Projects.Tests) {
    $projectBinDirectory = (Get-ProjectBinDirectory $project $configuration)
    Remove-DirectorySafely $projectBinDirectory
}

Write-Information "$ScriptName SUCCEEDED."