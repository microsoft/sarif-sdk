<#
.SYNOPSIS
    Runs tests in all test projects.

.DESCRIPTION
    This script runs the tests in each test project. This is done in a separate script,
    rather than inline in BuildAndTest.ps1, because AppVeyor cannot run BuildAndTest.
    AppVeyor runs the tests by invoking a separate script, and this is it.

.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release

.PARAMETER AppVeyor
    True if the tests are running in AppVeyor.
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release",

    [switch]
    $AppVeyor
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

$ReporterOption = $null
if ($AppVeyor) {
    $ReporterOption = "-appveyor"
}

$TestRunnerRootPath = "$NuGetPackageRoot\xunit.runner.console\2.3.1\tools\"

foreach ($project in $Projects.Tests) {
    foreach ($framework in $Frameworks.Application) {
        Write-Information "Running tests in ${project}: $framework..."
        Push-Location $BinRoot\${Platform}_$Configuration\$project\$framework
        $dll = "$project" + ".dll"
        if ($framework -eq "netcoreapp2.0") {
            & dotnet ${TestRunnerRootPath}netcoreapp2.0\xunit.console.dll $dll $ReporterOption
        } else {
            & ${TestRunnerRootPath}net452\xunit.console.exe $dll $ReporterOption
        }
        if ($LASTEXITCODE -ne 0) {
            Pop-Location
            Exit-WithFailureMessage $ScriptName "${project}: tests failed."
        }
        Pop-Location
    }
}
