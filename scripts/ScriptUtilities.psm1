<#
.SYNOPSIS
    Utility functions.

.DESCRIPTION
    The ScriptUtilties module exports generally useful functions to PowerShell
    scripts in the SARIF SDK.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

function Remove-DirectorySafely($dir) {
    if (Test-Path $dir) {
        Write-Verbose "Removing $dir"
        Remove-Item -Force -Recurse $dir
    }
}

function Exit-WithFailureMessage($scriptName, $message) {
    Write-Information "${scriptName}: $message"
    Write-Information "$scriptName FAILED."
    exit 1
}

$RepoRoot = $(Resolve-Path $PSScriptRoot\..).Path
$SourceRoot = "$RepoRoot\src"
$NuGetPackageRoot = "$SourceRoot\packages"
$BuildRoot = "$RepoRoot\bld"
$BinRoot = "$BuildRoot\bin"

Export-ModuleMember -Function `
    Exit-WithFailureMessage, `
    Remove-DirectorySafely

Export-ModuleMember -Variable `
    RepoRoot, `
    SourceRoot, `
    NuGetPackageRoot, `
    BuildRoot, `
    BinRoot