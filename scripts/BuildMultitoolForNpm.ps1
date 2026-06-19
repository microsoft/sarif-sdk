<#
.SYNOPSIS
    Build and stage every Sarif npm package.
.DESCRIPTION
    Builds and stages all of the project's npm packages under bld\Publish:
      * The .NET-backed @microsoft/sarif-multitool platform packages (a self-contained
        Sarif.Multitool single-file exe for win-x64, linux-x64, and osx-x64), staged to
        bld\Publish\npm.
      * The native-TypeScript @microsoft/sarif and @microsoft/sarif-multitool-ts packages,
        compiled and staged to bld\Publish\npm-ts.
    Every package is stamped with the same SARIF SDK version from Get-PackageVersion, so the
    .NET and TypeScript packages always release in lock-step. There is deliberately no
    -Version override: the version is whatever src\build.props declares.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
.PARAMETER SkipBuild
    Skip the .NET and TypeScript compilation and re-stamp the already-staged folders.
.PARAMETER NoPostClean
    Keep the intermediate single-file-exe output directories instead of deleting them.
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

# Derive roots from $PSScriptRoot rather than ScriptUtilities' exported $RepoRoot /
# $BuildRoot / $SourceRoot: Get-PackageVersion (below) runs Get-VersionConstants.ps1, which
# re-imports ScriptUtilities with -Force. That Remove-Module / re-import cycle drops the
# module's scope-local variables from this script's scope (its functions stay session-global
# and survive), leaving $RepoRoot empty. Deriving the paths here keeps them correct no matter
# when Get-PackageVersion runs.
$repoRoot   = (Resolve-Path "$PSScriptRoot\..").Path
$sourceRoot = Join-Path $repoRoot "src"
$npmSourceFolder      = Join-Path $repoRoot "npm"
$multitoolBuildFolder = Join-Path $repoRoot "bld\Publish\npm"
$tsBuildFolder        = Join-Path $repoRoot "bld\Publish\npm-ts"

$project = "Sarif.Multitool"
$projectBinDirectory = (Get-ProjectBinDirectory $project $Configuration)

# @microsoft/sarif must build before @microsoft/sarif-multitool-ts: the latter compiles
# against the former via a tsconfig path mapping and depends on it at publish time.
$tsPackages = @("sarif", "sarif-multitool-ts")

if (-not $SkipBuild) {
    # --- .NET-backed @microsoft/sarif-multitool platform packages ---
    Write-Information "Building Sarif.Multitool for Windows, Linux, and MacOS..."
    foreach ($runtime in "win-x64", "linux-x64", "osx-x64") {
        dotnet publish $sourceRoot\$project\$project.csproj -c $Configuration -f net8.0 -r $runtime --self-contained
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for runtime '$runtime' (exit code $LASTEXITCODE). Aborting before any npm folder is populated."
        }
    }

    Write-Information "Merging binaries [$projectBinDirectory] and NPM configuration [$npmSourceFolder]..."
    New-DirectorySafely $multitoolBuildFolder\
    Copy-Item -Force -Container -Recurse -Path $npmSourceFolder\* -Destination $multitoolBuildFolder\
    Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\net8.0\win-x64\* -Destination $multitoolBuildFolder\sarif-multitool-win32\
    Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\net8.0\linux-x64\* -Destination $multitoolBuildFolder\sarif-multitool-linux\
    Copy-Item -Force -Container -Recurse -Path $projectBinDirectory\Publish\net8.0\osx-x64\* -Destination $multitoolBuildFolder\sarif-multitool-darwin\

    # A silently-empty platform folder ships an npm package with no executable.
    # Verify each one carries its multitool binary before the package is considered built.
    $expectedBinaries = [ordered]@{
        "sarif-multitool-win32"  = "Sarif.Multitool.exe"
        "sarif-multitool-linux"  = "Sarif.Multitool"
        "sarif-multitool-darwin" = "Sarif.Multitool"
    }
    foreach ($folder in $expectedBinaries.Keys) {
        $binaryPath = Join-Path $multitoolBuildFolder "$folder\$($expectedBinaries[$folder])"
        if (-not (Test-Path $binaryPath)) {
            throw "Expected multitool binary not found: $binaryPath. The npm package would ship without an executable."
        }
    }

    # --- native-TypeScript @microsoft/sarif and @microsoft/sarif-multitool-ts packages ---
    Remove-DirectorySafely $tsBuildFolder
    New-DirectorySafely $tsBuildFolder

    foreach ($pkg in $tsPackages) {
        $src = Join-Path $npmSourceFolder $pkg
        Write-Information "Building $pkg..."
        Push-Location $src
        try {
            npm install --no-audit --no-fund
            if ($LASTEXITCODE -ne 0) { throw "npm install failed for $pkg (exit $LASTEXITCODE)." }
            npm run build
            if ($LASTEXITCODE -ne 0) { throw "npm run build failed for $pkg (exit $LASTEXITCODE)." }
        }
        finally { Pop-Location }
    }

    Write-Information "Staging TypeScript packages to $tsBuildFolder..."
    foreach ($pkg in $tsPackages) {
        $src = Join-Path $npmSourceFolder $pkg
        $dst = Join-Path $tsBuildFolder $pkg
        New-DirectorySafely $dst
        # Copy only what `files` in package.json declares plus the manifest itself; npm pack
        # would do this too, but staging a clean folder makes the placeholder stamping safe.
        Copy-Item -Force "$src\package.json" -Destination $dst
        Copy-Item -Force "$src\README.md"    -Destination $dst
        Copy-Item -Force -Recurse "$src\dist" -Destination $dst
        foreach ($assetDir in @("schemas", "skills", "assets")) {
            if (Test-Path "$src\$assetDir") {
                Copy-Item -Force -Recurse "$src\$assetDir" -Destination $dst
            }
        }
    }
}

# Single source of truth for the version stamped into every package, .NET and TypeScript
# alike (SARIF SDK version from src\build.props, 2.2.1 forward).
$sarifVersion = Get-PackageVersion
Write-Information "Injecting Sarif SDK version $sarifVersion for all NPM packages..."

# .NET multitool manifests carry the literal {version} token.
foreach ($package in (Get-ChildItem $multitoolBuildFolder).FullName) {
    Find-AndReplaceInFile "$package\package.json" "{version}" $sarifVersion
}

# TypeScript manifests ship "version": "0.0.0" so in-source `npm install` sees valid semver;
# stamp the real version at stage time. Targeted replace so any other 0.0.0 occurrence (none
# today) is left alone. @microsoft/sarif-multitool-ts depends on @microsoft/sarif via
# "file:../sarif" for local resolution; rewrite it to a caret range on the same SDK version.
foreach ($pkg in $tsPackages) {
    $manifest = Join-Path $tsBuildFolder "$pkg\package.json"
    Find-AndReplaceInFile $manifest '"version": "0.0.0"' "`"version`": `"$sarifVersion`""
    Find-AndReplaceInFile $manifest '"file:../sarif"' "`"^$sarifVersion`""
}

if (-not $SkipBuild) {
    Write-Information "Running npm pack (dry verification of TypeScript package contents)..."
    foreach ($pkg in $tsPackages) {
        Push-Location (Join-Path $tsBuildFolder $pkg)
        try {
            npm pack
            if ($LASTEXITCODE -ne 0) { throw "npm pack failed for $pkg (exit $LASTEXITCODE)." }
        }
        finally { Pop-Location }
    }
}

# To Publish (the release pipeline does this automatically via its npm service connection):
#   .NET multitool platform packages from bld\Publish\npm\*, and the @microsoft/sarif /
#   @microsoft/sarif-multitool-ts packages from bld\Publish\npm-ts\*.
#   Publish @microsoft/sarif before @microsoft/sarif-multitool-ts so its dependency resolves.
#   For each folder: npm publish --access public  (npm login once with an @microsoft-scoped token).

# After merging outputs, delete the other 250MB copies of the Multitool single file exes
# (saving only the bld\Publish\npm copy).
if (-not $NoPostClean) {
    Remove-DirectorySafely $projectBinDirectory\net8.0
    Remove-DirectorySafely $projectBinDirectory\Publish\net8.0
}

Write-Information "$ScriptName SUCCEEDED."
