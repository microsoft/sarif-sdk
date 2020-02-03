<#
.SYNOPSIS
    Build, and package the Sarif Multitool NPM package.
.DESCRIPTION
    Builds the Sarif Multitool NPM package, including building the .NET Core 3.0 single-file-exe of the Multitool for all supported platforms.
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