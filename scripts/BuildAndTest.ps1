<#
.SYNOPSIS
    Build, test, and package the SARIF SDK.
.DESCRIPTION
    Builds the SARIF SDK for multiple target frameworks, runs the tests, and creates
    NuGet packages.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
.PARAMETER NoClean
    Do not remove the outputs from the previous build.
.PARAMETER NoRestore
    Do not restore NuGet packages.
.PARAMETER NoObjectModel
    Do not rebuild the SARIF object model from the schema.
.PARAMETER NoBuild
    Do not build.
.PARAMETER NoTest
    Do not run tests.
.PARAMETER NoPackage
    Do not create NuGet packages.
.PARAMETER NoPublish
    Do not run dotnet publish, which creates a layout directory.
.PARAMETER Install
    Install the VSIX.
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release",

    [switch]
    $NoClean,

    [switch]
    $NoRestore,

    [switch]
    $NoObjectModel,

    [switch]
    $NoBuild,

    [switch]
    $NoTest,

    [switch]
    $NoPackage,

    [switch]
    $NoPublish,

    [switch]
    $Install
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

$SolutionFile = "$SourceRoot\Everything.sln"
$Platform = "AnyCPU"
$BuildTarget = "Rebuild"
$PackageOutputDirectory = "$BinRoot\NuGet\$Configuration"

function Remove-BuildOutput {
    Remove-DirectorySafely $BuildRoot
    foreach ($project in $Projects.New) {
        $objDir = "$SourceRoot\$project\obj"
        Remove-DirectorySafely $objDir
    }
}

function Invoke-Build {
    Write-Information "Building $SolutionFile..."
    msbuild /verbosity:minimal /target:$BuildTarget /property:Configuration=$Configuration /fileloggerparameters:Verbosity=detailed $SolutionFile
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Build failed."
    }
}

# Create a directory containing all files necessary to execute an application.
# This operation is called "publish" because it is performed by "dotnet publish".
function Publish-Application($project, $framework) {
    Write-Information "Publishing $project for $framework ..."
    dotnet publish $SourceRoot\$project\$project.csproj --no-restore --configuration $Configuration --framework $framework
}
function  Install-SarifExtension {
    $vsixInstallerPaths = Get-ChildItem $BinRoot "*.vsix" -Recurse
    if (-not $vsixInstallerPaths) {
        Exit-WithFailureMessage $ScriptName "Cannot install VSIX: .vsix file was not found."
    }

    Write-Information "Launching VSIX installer..."
    & $vsixInstallerPaths[0].FullName
}

# Create registry settings to open SARIF files in Visual Studio by default.
function Set-SarifFileAssociationRegistrySettings {
    # You need to be Admin to modify the registry, so create the settings by
    # running a separate script, elevated ("-Verb RunAs").
    $path = "$PSScriptRoot\RegistrySettings.ps1"
    Write-Information "Creating registry settings to associate SARIF files with Visual Studio..."
    $proc = Start-Process powershell.exe -ArgumentList "-File $path" -Verb RunAs -PassThru
    $proc.WaitForExit()
    $exitCode = $proc.ExitCode
    $proc.Dispose()
    if ($exitCode -ne 0) {
        Exit-WithFailureMessage $ScriptName "Failed to create registry settings ($exitCode)."
    }
}

if (-not $NoClean) {
    Remove-BuildOutput
}

& $PSScriptRoot\BeforeBuild.ps1 -NoRestore:$NoRestore -NoObjectModel:$NoObjectModel
if (-not $?) {
    Exit-WithFailureMessage $ScriptName "BeforeBuild failed."
}

if (-not $NoBuild) {
    Invoke-Build
}

if (-not $NoTest) {
    & $PSScriptRoot\Run-Tests.ps1
    if (-not $?) {
        Exit-WithFailureMessage $ScriptName "RunTests failed."
    }
}

if (-not $NoPublish) {
    foreach ($project in $Projects.NewApplication) {
        foreach ($framework in $Frameworks) {
            Publish-Application $project $framework
        }
    }
}

if ($Install) {
    Install-SarifExtension
    Set-SarifFileAssociationRegistrySettings
}

Write-Information "TODO: Build NuGet packages and create layout directory."
