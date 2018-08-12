<#
.SYNOPSIS
    Performs pre-build actions.
.DESCRIPTION
    This script performs the actions that are required before building the solution file
    src\Everything.sln. These actions are broken out into a separate script, rather than
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
    foreach ($project in $Projects.New) {
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

    # Restore NuGet packages for projects that use the new VS 2017 project system.
    # We have to restore the projects one by one, rather than restoring the entire solution,
    # because the solution includes projects that do not use the VS 2017 project system.
    foreach ($project in $Projects.New) {
        Write-Information "Restoring NuGet packages for $project..."
        dotnet restore $SourceRoot\$project\$project.csproj --configfile $NuGetConfigFile --packages $NuGetPackageRoot --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            Exit-WithFailureMessage $ScriptName "NuGet restore failed for $project."
        }
    }

    # Restore nuget packages for projects that don't use the VS 2017 project system.
    foreach ($project in $Projects.Old) {
        Write-Information "Restoring NuGet packages for $project..."
        & $RepoRoot\.nuget\NuGet.exe restore $SourceRoot\$project\$project.csproj -ConfigFile "$NuGetConfigFile" -OutputDirectory "$NuGetPackageRoot" -Verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            Exit-WithFailureMessage $ScriptName "NuGet restore failed for $project."
        }
    }
}

if (-not $NoObjectModel) {
    # Generate the SARIF object model classes from the SARIF JSON schema.
    msbuild /verbosity:minimal /target:BuildAndInjectObjectModel $SourceRoot\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed`;LogFile=CodeGen.log
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "SARIF object model generation failed."
    }
}
