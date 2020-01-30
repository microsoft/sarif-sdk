<#
.SYNOPSIS
    Build, and package the Sarif Multitool NPM package.
.DESCRIPTION
    Builds the Sarif Multitool NPM package, including building the .NET Core 3.0 single-file-exe of the Multitool for all supported platforms.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

$project = "Sarif.Multitool"
$projectBinDirectory = (Get-ProjectBinDirectory $project $Configuration)
$npmSourceFolder = "$RepoRoot\npm"
$npmBuildFolder = "$projectBinDirectory\npm"

Write-Information "Building Sarif.Multitool for Windows, Linux, and MacOS..."
dotnet publish $SourceRoot\$project\$project.csproj -c $Configuration -f netcoreapp3.0 /p:TargetFrameworks=netcoreapp3.0 -r win-x64
dotnet publish $SourceRoot\$project\$project.csproj -c $Configuration -f netcoreapp3.0 /p:TargetFrameworks=netcoreapp3.0 -r linux-x64
dotnet publish $SourceRoot\$project\$project.csproj -c $Configuration -f netcoreapp3.0 /p:TargetFrameworks=netcoreapp3.0 -r osx-x64

Write-Information "Merging binaries [$projectBinDirectory] and NPM configuration [$npmSourceFolder]..."
New-Item -ItemType Directory -Path $npmBuildFolder\
Copy-Item -Force -Container -Recurse -Path $npmSourceFolder\* -Destination $npmBuildFolder\
Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\netcoreapp3.0\win-x64\* -Destination $npmBuildFolder\sarif-multitool-win32\
Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\netcoreapp3.0\linux-x64\* -Destination $npmBuildFolder\sarif-multitool-linux\
Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\netcoreapp3.0\osx-x64\* -Destination $npmBuildFolder\sarif-multitool-darwin\

# To Publish:
# from [sarif-sdk]\bld\bin\AnyCPU_Release\Sarif.Multitool\npm, run:
#  npm login   (Once; need a token from npmjs.com)
# for each sarif-multitool folder...
#  Update the version numbers, if desired.
#  npm publish --access public

Write-Information "$ScriptName SUCCEEDED."