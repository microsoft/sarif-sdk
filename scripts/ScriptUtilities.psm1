<#
.SYNOPSIS
    Utility functions.

.DESCRIPTION
    The ScriptUtilties module exports generally useful functions.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$RepoRoot = $(Resolve-Path $PSScriptRoot\..).Path
$Platform = "AnyCPU"
$SourceRoot = "$RepoRoot\src"
$JsonSchemaPath = "$SourceRoot\Sarif\Schemata\Sarif.schema.json"
$BuildRoot = "$RepoRoot\bld"
$BinRoot = "$BuildRoot\bin"

$SarifExtension = ".sarif"

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

function Get-ProjectBinDirectory($project, $configuration) {
    "$BinRoot\${Platform}_$configuration\$project\"
}

function Write-CommandLine($exeName, $arguments) {
    Write-Verbose "$exeName $($arguments -join ' ')"
}

Export-ModuleMember -Function `
    Exit-WithFailureMessage, `
    New-DirectorySafely, `
    Remove-DirectorySafely, `
    Get-ProjectBinDirectory, `
    Write-CommandLine

Export-ModuleMember -Variable `
    RepoRoot, `
    SourceRoot, `
    JsonSchemaPath, `
    BuildRoot, `
    BinRoot, `
    SarifExtension, `
    Platform