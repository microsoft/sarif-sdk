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
$SolutionFile = "Sarif.Sdk.sln"
$SampleSolutionFile = "Samples\Sarif.Sdk.Sample.sln"

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

function Show-ErrorInformation($error) {
    Write-Information "Entering Show-ErrorInformation"
    Write-Information "Argument is of type $($error.GetType())"

    $message = $error.ToString() + "`n"

    $exception = $error.Exception
    while ($exception) {
        Write-Information "Exception exists."
        $message += $exception.Message + "`n`n"
        $message += "Stack trace:`n"
        $message += $exception.StackTrace + "`n`n"
        $exception = $exception.InnerException
        if ($exception) {
            $message += "Inner exception:`n"
        }
    }

    Write-Information $message
}

Export-ModuleMember -Function `
    Exit-WithFailureMessage, `
    Get-ProjectBinDirectory, `
    New-DirectorySafely, `
    Remove-DirectorySafely, `
    Show-ErrorInformation, `
    Write-CommandLine

Export-ModuleMember -Variable `
    BinRoot, `
    BuildRoot, `
    JsonSchemaPath, `
    RepoRoot, `
    Platform, `
    SampleSolutionFile, `
    SarifExtension, `
    SolutionFile, `
    SourceRoot `
