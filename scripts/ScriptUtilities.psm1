<#
.SYNOPSIS
    Utility functions.

.DESCRIPTION
    The ScriptUtilties module exports generally useful functions to PowerShell
    scripts in the SARIF SDK.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

function Remove-DirectorySafely($dir) {
    if (Test-Path $dir) {
        Write-Verbose "Removing directory $dir..."
        Remove-Item -Force -Recurse $dir
    }
}

function New-DirectorySafely($dir) {
    if (-not (Test-Path $dir)) {
        Write-Verbose "Creating directory $dir..."
        New-Item -Type Directory $dir | Out-Null
    } else {
        Write-Verbose "Directory $dir already exists."
    }
}

function Exit-WithFailureMessage($scriptName, $message) {
    Write-Information "${scriptName}: $message"
    Write-Information "$scriptName FAILED."
    exit 1
}

# NuGet Package Creation section
function New-NuGetPackageFromProjectFile($Configuration, $project, $version) {
    $PackageOutputDirectory = "$BinRoot\NuGet\$Configuration"
    $projectFile = "$SourceRoot\$project\$project.csproj"

    $arguments =
        "pack", $projectFile,
        "--configuration", $Configuration,
        "--no-build", "--no-restore",
        "--include-source", "--include-symbols",
        "-p:Platform=$Platform",
        "--output", $PackageOutputDirectory

    Write-Debug "dotnet $($arguments -join ' ')"

    dotnet $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "$project NuGet package creation failed."
    }
}

function New-NuGetPackageFromNuspecFile($Configuration, $project, $version, $suffix = "") {
    $PackageOutputDirectory = "$BinRoot\NuGet\$Configuration"
    $nuspecFile = "$SourceRoot\NuGet\$project.nuspec"

    $arguments=
        "pack", $nuspecFile,
        "-Symbols",
        "-Properties", "configuration=$Configuration;version=$version",
        "-Verbosity", "Quiet",
        "-BasePath", ".\",
        "-OutputDirectory", $PackageOutputDirectory

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

function New-NuGetPackages($Configuration, $Projects) {
    $versionPrefix, $versionSuffix = & $PSScriptRoot\Get-VersionConstants.ps1
    $version = "$versionPrefix-$versionSuffix"

    # We can build the NuGet packages for library projects directly from their
    # project file.
    foreach ($project in $Projects.NewLibrary) {
        New-NuGetPackageFromProjectFile $Configuration $project $version
    }

    # Unfortunately, application projects like MultiTool need to include things
    # that are not specified in the project file, so their packages still require
    # a .nuspec file.
    foreach ($project in $Projects.NewApplication) {
        New-NuGetPackageFromNuSpecFile $Configuration $project $version
    }
}

$RepoRoot = $(Resolve-Path $PSScriptRoot\..).Path
$Platform = "AnyCPU"
$SourceRoot = "$RepoRoot\src"
$NuGetPackageRoot = "$SourceRoot\packages"
$JsonSchemaPath = "$SourceRoot\Sarif\Schemata\Sarif.schema.json"
$BuildRoot = "$RepoRoot\bld"
$BinRoot = "$BuildRoot\bin"
$SarifExtension = ".sarif"

Export-ModuleMember -Function `
    Exit-WithFailureMessage, `
    New-DirectorySafely, `
    Remove-DirectorySafely, `
    New-NuGetPackages, `
    New-NuGetPackageFromProjectFile, `
    New-NuGetPackageFromNuSpecFile

Export-ModuleMember -Variable `
    RepoRoot, `
    SourceRoot, `
    NuGetPackageRoot, `
    JsonSchemaPath, `
    BuildRoot, `
    BinRoot, `
    SarifExtension, `
    Platform, `
    PackageOutputDirectory