<#
.SYNOPSIS
Update the expected output files for the SarifCli functional tests.

.DESCRIPTION
This script runs "SarifCli validate" on each input log file in the SarifCli
functional tests TestData directory. For each file foo.sarif, it produces
an output file foo_Expected.sarif, overwriting any existing file of that
name in the TestData directory.

.PARAMETERS
    Configuration
        The build configuration. Default: Debug
#>

param([string] $Configuration = "Debug")

$ErrorActionPreference = "Stop"

$SarifExtension = ".sarif"
$ExpectedSuffix = "_Expected$SarifExtension"

$sdkPath = Join-Path -Resolve $PSScriptRoot "..\.."
$sarifCliExe = Join-Path $sdkPath "bld\bin\SarifCli\AnyCPU_$Configuration\SarifCli.exe"
$jsonSchemaPath = Join-Path $sdkPath "src\Sarif\Schemata\Sarif.schema.json"
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
    & $sarifCliExe validate $inputPath -o $outputPath -j $jsonSchemaPath
}

function Update-RuleDirectory($ruleDirectoryPath) {
    $inputFiles = Get-InputFiles $ruleDirectoryPath
    $inputFiles | ForEach-Object { Update-ExpectedFile $_ }
}

$ruleDirectoryNames = Get-ChildItem -Path $testDataPath -Directory | Foreach-Object { $_.Name }
$ruleDirectoryNames | ForEach-Object {
    $ruleDirectoryPath = [IO.Path]::Combine($testDataPath, $_)
    Update-RuleDirectory $ruleDirectoryPath
}