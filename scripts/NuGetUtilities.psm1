<#
.SYNOPSIS
    NuGet utility functions.

.DESCRIPTION
    The NuGetUtilities module exports functions for performing NuGet actions.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Import-Module $PSScriptRoot\ScriptUtilities.psm1
Import-Module $PSScriptRoot\Projects.psm1

$NugetExePath = "nuget"
if ($ENV:OS) {
    $NugetExePath = "$RepoRoot\.nuget\NuGet.exe"
}
$NuGetPackageRoot = Join-Path (Join-Path $RepoRoot "bld") "packages"
$NuGetSamplesPackageRoot = Join-Path (Join-Path $SourceRoot "samples") "packages"
$NuGetConfigFile = Join-Path $RepoRoot "NuGet.Config"

$PackageOutputDirectoryRoot = Join-Path (Join-Path $BuildRoot "Publish") "NuGet"

function Get-PackageVersion() {
    $versionPrefix, $schemaVersion, $stableSarifVersion = & $PSScriptRoot\Get-VersionConstants.ps1
    $versionPrefix
}

function Get-PackageDirectoryName($configuration) {
    Join-Path $PackageOutputDirectoryRoot $configuration
}

function New-NuGetPackageFromProjectFile($configuration, $project, $version) {
    $projectFile = "$SourceRoot\$project\$project.csproj"

	# Note that we override the standard version construct via the VersionPrefix and 
	# VersionSuffix properties so that we can specify an additional suffix that's only
	# applied to the package.
    $arguments =
        "pack", $projectFile,
        "--configuration", $configuration,
        "--no-build", "--no-restore",
        "--include-source", "--include-symbols",
        "-p:Platform=$Platform",
        "--output", (Get-PackageDirectoryName $configuration),
		"-p:Version=$version"

    Write-CommandLine dotnet $arguments

    dotnet $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "$project NuGet package creation failed."
    }
}

function New-NuGetPackageFromNuSpecFile($configuration, $project, $version, $suffix = "") {
    $nuspecFile = "$SourceRoot\NuGet\$project.nuspec"

    $arguments=
        "pack", $nuspecFile,
        "-Properties", "platform=$Platform;configuration=$configuration;version=$version",
        "-Verbosity", "Quiet",
        "-BasePath", ".\",
        "-OutputDirectory", (Get-PackageDirectoryName $configuration)

    if ($suffix -ne "") {
        $arguments += "-Suffix", $Suffix
    }

    Write-CommandLine $NuGetExePath $arguments

    & $NuGetExePath $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "$project NuGet package creation failed."
    }

    Write-Information "  Successfully created package '$BinRoot\..\Publish\NuGet\$Configuration\$Project.$version.nupkg'."
}

function New-NuGetPackages($configuration, $projects) {
    $version = Get-PackageVersion

    # We can build the NuGet packages for library projects directly from their
    # project file.
    foreach ($project in $projects.Libraries) {
        New-NuGetPackageFromProjectFile $configuration $project $version
    }

    # Unfortunately, application projects like MultiTool need to include things
    # that are not specified in the project file, so their packages still require
    # a .nuspec file.
    foreach ($project in $Projects.Applications) {
        New-NuGetPackageFromNuSpecFile $configuration $project $version
    }
}

Export-ModuleMember -Function `
    New-NuGetPackages, `
    Get-PackageVersion

Export-ModuleMember -Variable `
    NuGetConfigFile, `
    NuGetExePath, `
    NuGetPackageRoot, `
    NuGetSamplesPackageRoot
