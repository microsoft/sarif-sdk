<#
.SYNOPSIS
Update the expected output files for the Sarif.Multitool functional tests.

.DESCRIPTION
This script runs "Sarif.Multitool validate" on each input log file in the SarifCli
functional tests TestData directory. For each file foo.sarif, it produces
an output file foo_Expected.sarif, overwriting any existing file of that
name in the TestData directory.

.PARAMETER Configuration
    The configuration of the build that produced Sarif.Multitool.exe.
    Default: Debug

.PARAMETER RuleName
    The name of the rule whose files are to be regenerated.
    Default: $null, meaning "all rules"
#>

param(
    [string] $Configuration = "Debug",
    [string] $RuleName = $null
)

$ErrorActionPreference = "Stop"

Import-Module -Force ..\..\scripts\ScriptUtilities.psm1

$ExpectedSuffix = "_Expected$SarifExtension"

$multiToolExePath = "$BinRoot\Sarif.Multitool\AnyCPU_$Configuration\net461\Sarif.Multitool.exe"
$testDataPath = Join-Path $PSScriptRoot "TestData"

function Test-IsInputFile($fileName) {
    -not $fileName.EndsWith($ExpectedSuffix)
}

function Get-InputFiles($ruleDirectoryPath) {
    $inputFiles = Get-ChildItem -Path $ruleDirectoryPath -File | Where-Object { Test-IsInputFile $_.Name }
    $inputFiles | ForEach-Object { [IO.Path]::Combine($ruleDirectoryPath, $_.Name) }
}

function Update-ExpectedFile($inputPath) {
    $outputPath = $inputPath.Replace($SarifExtension, $ExpectedSuffix)
    & $multiToolExePath validate $inputPath --output $outputPath --json-schema $JsonSchemaPath --pretty-print --force --verbose
}

function Update-RuleDirectory($ruleDirectoryPath) {
    $inputFiles = Get-InputFiles $ruleDirectoryPath
    $inputFiles | ForEach-Object { Update-ExpectedFile $_ }
}

if ($RuleName -eq $null) {
    $ruleDirectoryNames = Get-ChildItem -Path $testDataPath -Directory | Foreach-Object { $_.Name }
} else {
    $ruleDirectoryNames = @($RuleName)
}

$ruleDirectoryNames | ForEach-Object {
    $ruleDirectoryPath = [IO.Path]::Combine($testDataPath, $_)
    Update-RuleDirectory $ruleDirectoryPath
}