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

$NugetExePath = "$RepoRoot\.nuget\NuGet.exe"
$NuGetPackageRoot = "$SourceRoot\packages"
$NuGetSamplesPackageRoot = "$SourceRoot\Samples\packages"
$NuGetConfigFile = "$RepoRoot\NuGet.Config"

$PackageSource = "https://nuget.org"
$PackageOutputDirectoryRoot = Join-Path $BuildRoot "Publish\NuGet"

function Get-PackageVersion([switch]$previous) {
    $versionPrefix, $schemaVersion, $stableSarifVersion = & $PSScriptRoot\Get-VersionConstants.ps1 -Previous:$previous
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
        "-Symbols",
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

    $packageFilePath = Join-Path -Resolve $BinRoot ..\Publish\NuGet\$Configuration\$Project.$version.nupkg

    Write-Information "  Successfully created package '$packageFilePath'."
}

function New-NuGetPackages($configuration, $projects) {
    $version = Get-PackageVersion

    # We can build the NuGet packages for library projects directly from their
    # project file.
    foreach ($project in $projects.Libraries) {
        New-NuGetPackageFromProjectFile $configuration $project $version
    }

    # We create both a DotnetTool package and a Dependency package from each application (Exe)
    # project. Users need a DotnetTool package so they can install it and run it from the command
    # line. They need a Dependency package so they can add a NuGet reference to their project and
    # invoke the package's public APIs. If you try to add a NuGet reference to a DotnetTool package,
    # you get the error message "Package '<packageId>' has a package type 'DotnetTool' that is not
    # supported by project '<project>'."
    #
    # It is possible to create a single package that declares multiple package types, but when
    # you try to add a NuGet reference to such a package, you get the error message "Package
    # '<packageId>' has multiple package types, which is not supported."
    #
    # We create the Dependency package from the project file. We can't create the DotnetTool
    # package form the project file for two reasons:
    #
    # 1. The tool package is standalone and requires dependent binaries that are not specified
    #    in the project file.
    # 2. It's not possible (AFAIK) to create multiple packages from the same project file.
    #
    # So we author a .nuspec file for projects that require DotnetTool packages.
    foreach ($project in $Projects.Applications) {
        New-NuGetPackageFromProjectFile $configuration $project $version
        New-NuGetPackageFromNuSpecFile $configuration $project $version
    }
}

function Get-NuGetApiKey {
    # We temporarily implement this function by parsing the key from the file
    # SetNuGetSarifApiKey.cmd, which is expected to exist in the parent
    # directory of the developer's sarif-sdk enlistment.
    # In future, we should get this key from the Azure Key Vault.
    $apiKeyPath = Join-Path $PSScriptRoot ..\..\SetNuGetSarifApiKey.cmd
    if (-not (Test-Path $apiKeyPath)) {
        Exit-WithFailureMessage NuGetUtilties "API key file $apiKeyPath does not exist."
    }

    # Everything that isn't a double quote, followed by a double quote, followed
    # by the key (everything up to the next double quote).
    $pattern = '^[^"]*"(?<key>[^"]*)'

    $firstLine = [IO.File]::ReadAllLines($apiKeyPath)[0]
    if ($firstLine -match $pattern) {
        $matches["key"]
    } else {
        Exit-WithFailureMessage NuGetUtilties "API key file $apiKeyPath does not contain a key."
    }
}

function Set-NuGetApiKey {
    $apiKey = Get-NuGetApiKey

    $arguments = "SetApiKey", $apiKey, "-Source", $PackageSource, "-Verbosity", "quiet"
    Write-CommandLine $NuGetExePath $arguments

    & $NugetExePath $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Could not set NuGet API key."
    }
}

function Hide-NuGetPackage($project, $version) {
    $arguments = "delete", $project, $version, "-Source", $PackageSource
    Write-CommandLine $NuGetExePath $arguments

    & $NugetExePath $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Could not delist NuGet package $project $version."
    }
}

function Hide-NuGetPackages {
    Set-NuGetApiKey

    $version = Get-PackageVersion -Previous
    foreach ($project in $Projects.Products) {
        Hide-NuGetPackage $project $version
    }
}

Export-ModuleMember -Function `
    Hide-NuGetPackages, `
    New-NuGetPackages, `
    Get-PackageVersion

Export-ModuleMember -Variable `
    NuGetConfigFile, `
    NuGetExePath, `
    NuGetPackageRoot, `
    NuGetSamplesPackageRoot
