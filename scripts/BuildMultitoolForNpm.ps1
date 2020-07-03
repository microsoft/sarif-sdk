<#
.SYNOPSIS
    Build and package the Sarif Multitool NPM package.
.DESCRIPTION
    Builds the Sarif.Multitool NPM package, including building the .NET Core 3.1 single-file-exe of the Multitool for all supported platforms.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release",

    [switch]
    $SkipBuild,

    [switch]
    $NoPostClean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

$project = "Sarif.Multitool"
$projectBinDirectory = (Get-ProjectBinDirectory $project $Configuration)
$npmSourceFolder = "$RepoRoot\npm"
$npmBuildFolder = "$BuildRoot\Publish\npm"

if (-not $SkipBuild) {
    Write-Information "Building Sarif.Multitool for Windows, Linux, and MacOS..."
    foreach ($runtime in "win-x64", "linux-x64", "osx-x64") {
        dotnet publish $SourceRoot\$project\$project.csproj -c $Configuration -f netcoreapp3.1 -r $runtime
    }

    Write-Information "Merging binaries [$projectBinDirectory] and NPM configuration [$npmSourceFolder]..."
    New-DirectorySafely $npmBuildFolder\
    Copy-Item -Force -Container -Recurse -Path $npmSourceFolder\* -Destination $npmBuildFolder\
    Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\netcoreapp3.1\win-x64\* -Destination $npmBuildFolder\sarif-multitool-win32\
    Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\netcoreapp3.1\linux-x64\* -Destination $npmBuildFolder\sarif-multitool-linux\
    Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\netcoreapp3.1\osx-x64\* -Destination $npmBuildFolder\sarif-multitool-darwin\
}

# Match SARIF SDK version (from 2.2.1 forward).
$sarifVersion = Get-PackageVersion

Write-Information "Injecting Sarif SDK version $sarifVersion for NPM Packages..."
foreach ($package in (Get-ChildItem $npmBuildFolder).FullName) {
    Find-AndReplaceInFile "$package\package.json" "{version}" $sarifVersion
}


# To Publish:
# from [sarif-sdk]\bld\bin\AnyCPU_Release\Sarif.Multitool\npm, run:
#  npm login   (Once; need a token from npmjs.com)
# for each sarif-multitool folder...
#  Update the version numbers, if desired.
#  npm publish --access public

# After merging outputs, delete the other 250MB copies of the Multitool single file exes (saving only the bld\Publish\npm copy)
if (-not $NoPostClean) {
    Remove-DirectorySafely $projectBinDirectory\netcoreapp3.1
    Remove-DirectorySafely $projectBinDirectory\Publish\netcoreapp3.1
}

Write-Information "$ScriptName SUCCEEDED."