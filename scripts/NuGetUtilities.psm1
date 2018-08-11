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

$NuGetPackageRoot = "$SourceRoot\packages"
$PackageOutputDirectoryRoot = Join-Path $BinRoot NuGet

function Get-PackageDirectoryName($configuration) {
    Join-Path $PackageOutputDirectoryRoot $configuration
}

function New-NuGetPackageFromProjectFile($configuration, $project, $version) {
    $projectFile = "$SourceRoot\$project\$project.csproj"

    $arguments =
        "pack", $projectFile,
        "--configuration", $configuration,
        "--no-build", "--no-restore",
        "--include-source", "--include-symbols",
        "-p:Platform=$Platform",
        "--output", (Get-PackageDirectoryName $configuration)

    Write-Debug "dotnet $($arguments -join ' ')"

    dotnet $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "$project NuGet package creation failed."
    }
}

function New-NuGetPackageFromNuSpecFile($configuration, $project, $version, $suffix = "") {
    $nuspecFile = "$SourceRoot\NuGet\$project.nuspec"

    $arguments=
        "pack", $nuspecFile,
        "-Symbols",
        "-Properties", "platform=$Platform;configuration=$configuration;version=$version",
        "-Verbosity", "Quiet",
        "-BasePath", ".\",
        "-OutputDirectory", (Get-PackageDirectoryName $configuration)

    if ($suffix -ne "") {
        $arguments += "-Suffix", $Suffix
    }

    $nugetExePath = "$RepoRoot\.nuget\NuGet.exe"

    Write-Debug "$nuGetExePath $($arguments -join ' ')"

    &$nuGetExePath $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "$project NuGet package creation failed."
    }

    Write-Information "  Successfully created package '$BinRoot\NuGet\$Configuration\$Project.$version.nupkg'."
}

function New-NuGetPackages($configuration, $projects) {
    $versionPrefix, $versionSuffix = & $PSScriptRoot\Get-VersionConstants.ps1
    if ($versionSuffix)
    {
        $version = "$versionPrefix-$versionSuffix"
    }

    # We can build the NuGet packages for library projects directly from their
    # project file.
    foreach ($project in $projects.NewLibrary) {
        New-NuGetPackageFromProjectFile $configuration $project $version
    }

    # Unfortunately, application projects like MultiTool need to include things
    # that are not specified in the project file, so their packages still require
    # a .nuspec file.
    foreach ($project in $Projects.NewApplication) {
        New-NuGetPackageFromNuSpecFile $configuration $project $version
    }
}

Export-ModuleMember -Function `
    New-NuGetPackages

Export-ModuleMember -Variable `
    NuGetPackageRoot
