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

function New-NuGetPackageFromNuspecFile($configuration, $project, $version, $suffix = "") {
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

# Get the packaging directory name.
function Get-PackageDirectoryName($configuration) {
    Join-Path $PackageOutputDirectoryRoot $configuration
}

function Get-ProjectBinDirectory($project, $configuration)
{
    "$BinRoot\${Platform}_$configuration\$project\"
}

$RepoRoot = $(Resolve-Path $PSScriptRoot\..).Path
$Platform = "AnyCPU"
$SourceRoot = "$RepoRoot\src"
$NuGetPackageRoot = "$SourceRoot\packages"
$JsonSchemaPath = "$SourceRoot\Sarif\Schemata\Sarif.schema.json"
$BuildRoot = "$RepoRoot\bld"
$BinRoot = "$BuildRoot\bin"
$PackageOutputDirectoryRoot = Join-Path $BinRoot NuGet

$SarifExtension = ".sarif"

Export-ModuleMember -Function `
    Exit-WithFailureMessage, `
    New-DirectorySafely, `
    Remove-DirectorySafely, `
    New-NuGetPackages, `
    New-NuGetPackageFromProjectFile, `
    New-NuGetPackageFromNuSpecFile, `
    Get-PackageDirectoryName, `
    Get-ProjectBinDirectory

Export-ModuleMember -Variable `
    RepoRoot, `
    SourceRoot, `
    NuGetPackageRoot, `
    JsonSchemaPath, `
    BuildRoot, `
    BinRoot, `
    SarifExtension, `
    Platform