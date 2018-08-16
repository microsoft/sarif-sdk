<#
.SYNOPSIS
    Performs pre-build actions.
.DESCRIPTION
    This script performs the actions that are required before building the solution file
    src\Sarif.Sdk.sln. These actions are broken out into a separate script, rather than
    being performed inline in BuildAndTest.cmd, because AppVeyor cannot run BuildAndTest.
    AppVeyor only allows you to specify the project to build, and a script to run before
    the build step. So that is how we have factored the build scripts.
.PARAMETER NoClean
    Do not remove the outputs from the previous build.
.PARAMETER NoRestore
    Do not restore NuGet packages.
.PARAMETER NoObjectModel
    Do not rebuild the SARIF object model from the schema.
#>

[CmdletBinding()]
param(
    [switch]
    $NoClean,

    [switch]
    $NoRestore,

    [switch]
    $NoObjectModel
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

function Remove-BuildOutput {
    Remove-DirectorySafely $BuildRoot
    foreach ($project in $Projects.All) {
        $objDir = "$SourceRoot\$project\obj"
        Remove-DirectorySafely $objDir
    }
}

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

if (-not $NoClean) {
    Remove-BuildOutput
}

if (-not $NoRestore) {
    $NuGetConfigFile = "$SourceRoot\NuGet.Config"

    dotnet restore $SourceRoot\Sarif.Sdk.sln --configfile $NuGetConfigFile --packages $NuGetPackageRoot --verbosity quiet

    Write-Information "Restoring NuGet packages for Sarif.Sdk.sln..."
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "NuGet restore failed."
    }
}

if (-not $NoObjectModel) {
    # Generate the SARIF object model classes from the SARIF JSON schema.
    msbuild /verbosity:minimal /target:BuildAndInjectObjectModel $SourceRoot\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed`;LogFile=CodeGen.log
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "SARIF object model generation failed."
    }
}
