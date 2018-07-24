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
        Write-Verbose "Removing directory $dir..."
        Remove-Item -Force -Recurse $dir
    }
}

function New-DirectorySafely($dir) {
    if (-not (Test-Path $dir)) {
        Write-Verbose "Creating directory $dir..."
        New-Item -Type Directory $dir | Out-Null
    } else {
        Write-Verbose "Directory $dir already exists."
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
    New-DirectorySafely, `
    Remove-DirectorySafely

Export-ModuleMember -Variable `
    RepoRoot, `
    SourceRoot, `
    NuGetPackageRoot, `
    BuildRoot, `
    BinRoot