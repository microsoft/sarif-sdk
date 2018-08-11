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
$PackageSource = "https://nuget.org"
$PackageOutputDirectoryRoot = Join-Path $BinRoot NuGet

function Get-CurrentVersion {
    $versionPrefix, $versionSuffix = & $PSScriptRoot\Get-VersionConstants.ps1
    $version = $versionPrefix
    if ($versionSuffix)
    {
        $version += "-$versionSuffix"
    }

    $version
}

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

    Write-Information "  Successfully created package '$BinRoot\NuGet\$Configuration\$Project.$version.nupkg'."
}

function New-NuGetPackages($configuration, $projects) {
    $version = Get-CurrentVersion

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

function Get-NuGetApiKey {
    # We temporarily implement this function by parsing the key from the file
    # SetNuGetJSarifApiKey.cmd, which is expected to exist in the parent
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

    $arguments = "SetApiKey", $apiKey, "-Source", $PackageSource
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

    $version = Get-CurrentVersion
    foreach ($project in $Projects.New) {
        Hide-NuGetPackage $project $version
    }
}

Export-ModuleMember -Function `
    Hide-NuGetPackages, `
    New-NuGetPackages

Export-ModuleMember -Variable `
    NuGetPackageRoot
