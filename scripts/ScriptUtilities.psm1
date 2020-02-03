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
$JsonSchemaPath = "$SourceRoot\Sarif\Schemata\sarif-schema.json"
$BuildPropsPath = "$SourceRoot\build.props"
$BuildRoot = "$RepoRoot\bld"
$BinRoot = "$BuildRoot\bin"
$SolutionFile = "Sarif.Sdk.sln"
$SampleSolutionFile = "Samples\Sarif.Sdk.Sample.sln"

$MSBuildXmlNamespaces = @{ msbuild = "http://schemas.microsoft.com/developer/msbuild/2003" }

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

function Find-AndReplaceInFile($filePath, $value, $withValue) {
    $tempFilePath = "$filePath.tmp"
    if (Test-Path -Path $tempFilePath) { 
        Remove-Item $tempFilePath 
    }

    (Get-Content -Path $filePath) -replace $value, $withValue | Add-Content -Path $tempFilePath -Force
    Remove-Item $filePath
    Move-Item -Path $tempFilePath -Destination $filePath
}

Export-ModuleMember -Function `
    Exit-WithFailureMessage, `
    New-DirectorySafely, `
    Remove-DirectorySafely, `
    Get-ProjectBinDirectory, `
    Write-CommandLine, `
    Find-AndReplaceInFile

Export-ModuleMember -Variable `
    BinRoot, `
    BuildPropsPath, `
    BuildRoot, `
    JsonSchemaPath, `
    MSBuildXmlNamespaces, `
    RepoRoot, `
    Platform, `
    SampleSolutionFile, `
    SarifExtension, `
    SolutionFile, `
    SourceRoot
