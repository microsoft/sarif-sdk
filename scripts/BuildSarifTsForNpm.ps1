<#
.SYNOPSIS
    Build and stage the @microsoft/sarif and @microsoft/sarif-multitool-ts npm packages.
.DESCRIPTION
    Compiles both TypeScript packages, copies them to bld/Publish/npm-ts/, stamps the
    {version} and {version-range} placeholders, and runs `npm pack` so each is ready for
    `npm publish --access public`. Mirrors BuildMultitoolForNpm.ps1's stage-then-publish
    pattern for the .NET-backed sarif-multitool packages.
.PARAMETER Version
    The version to stamp into both package.json files. Defaults to the SDK package
    version from Get-PackageVersion (so released packages lock-step with the .NET SDK).
    For the v0.0.x publish-pipeline proof, pass -Version 0.0.1.
#>

[CmdletBinding()]
param(
    [string]
    $Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Get-PackageVersion
}
Write-Information "Stamping version $Version."

# Build order matters: @microsoft/sarif-multitool-ts compiles against @microsoft/sarif's
# source via a tsconfig path mapping, but at publish time it depends on the built dist/.
$packages = @("sarif", "sarif-multitool-ts")
$npmSourceFolder = "$RepoRoot\npm"
$npmBuildFolder  = "$BuildRoot\Publish\npm-ts"

Remove-DirectorySafely $npmBuildFolder
New-DirectorySafely $npmBuildFolder

foreach ($pkg in $packages) {
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

Write-Information "Staging packages to $npmBuildFolder..."
foreach ($pkg in $packages) {
    $src = Join-Path $npmSourceFolder $pkg
    $dst = Join-Path $npmBuildFolder $pkg
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

Write-Information "Injecting version $Version and rewriting local dependency range..."
foreach ($pkg in $packages) {
    $manifest = Join-Path $npmBuildFolder "$pkg\package.json"
    # Source manifests carry "version": "0.0.0" so `npm install` in-source sees valid
    # semver; the publish stage stamps the real version. Targeted replace so any other
    # 0.0.0 occurrence (none today) is left alone.
    Find-AndReplaceInFile $manifest '"version": "0.0.0"' "`"version`": `"$Version`""
    # @microsoft/sarif-multitool-ts depends on @microsoft/sarif via "file:../sarif" in
    # source so local `npm install` resolves it without an npm registry round-trip; the
    # publish stage rewrites that to a caret range on the same SDK version.
    Find-AndReplaceInFile $manifest '"file:../sarif"' "`"^$Version`""
}

Write-Information "Running npm pack (dry verification of package contents)..."
foreach ($pkg in $packages) {
    Push-Location (Join-Path $npmBuildFolder $pkg)
    try {
        npm pack
        if ($LASTEXITCODE -ne 0) { throw "npm pack failed for $pkg (exit $LASTEXITCODE)." }
    }
    finally { Pop-Location }
}

# To Publish:
# from [sarif-sdk]\bld\Publish\npm-ts, for each package folder, run:
#   npm login   (Once; need a token from npmjs.com with @microsoft scope publish rights)
#   npm publish --access public
#
# Publish @microsoft/sarif FIRST so @microsoft/sarif-multitool-ts's dependency resolves.
# For the initial v0.0.x pipeline proof, run this script with -Version 0.0.1.

Write-Information "$ScriptName SUCCEEDED. Tarballs staged under $npmBuildFolder."
