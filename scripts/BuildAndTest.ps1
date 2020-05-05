<#
.SYNOPSIS
    Build, test, and package the SARIF SDK.
.DESCRIPTION
    Builds the SARIF SDK for multiple target frameworks, runs the tests, and creates
    NuGet packages.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
.PARAMETER BuildVerbosity
    Specifies the amount of information for MSBuild to display: quiet, minimal,
    normal, detailed, or diagnostic. Default=minimal
.PARAMETER NuGetVerbosity
    Specifies the amount of information for NuGet to display: quiet, normal,
    or detailed. Default=quiet
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
.PARAMETER NoSigningDirectory
    Do not create a directory containing the binaries that need to be signed.
.PARAMETER Associate
    Associate SARIF files with Visual Studio.
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release",

    [string]
    [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
    $BuildVerbosity = "minimal",

    [string]
    [ValidateSet("quiet", "normal", "detailed")]
    $NuGetVerbosity = "quiet",

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
    $NoSigningDirectory,

    [switch]
    $Associate
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

function Invoke-DotNetBuild($solutionFileRelativePath) {
    Write-Information "Building $solutionFileRelativePath..."

    $solutionFilePath = Join-Path $SourceRoot $solutionFileRelativePath
    & dotnet build  $solutionFilePath -c $Configuration -v $BuildVerbosity -maxcpucount:1 --no-incremental
    
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Build of $solutionFilePath failed."
    }
}

function Invoke-Build {
    Invoke-DotNetBuild $SolutionFile
}

function Invoke-BuildSample {
    Invoke-DotNetBuild $sampleSolutionFile
}

# Create a directory containing all files necessary to execute an application.
# This operation is called "publish" because it is performed by "dotnet publish".
function Publish-Application($project, $framework) {
    Write-Information "Publishing $project for $framework ..."
    dotnet publish $SourceRoot\$project\$project.csproj --no-restore --configuration $Configuration --framework $framework
}

# Create a directory populated with the binaries that need to be signed.
function New-SigningDirectory {
    Write-Information "Copying files to signing directory..."
    $SigningDirectory = "$BinRoot\Signing"

    foreach ($framework in $Frameworks.All) {
        New-DirectorySafely $SigningDirectory\$framework
    }

    foreach ($project in $Projects.Products) {
        $projectBinDirectory = (Get-ProjectBinDirectory $project $configuration)

        foreach ($framework in $Frameworks.All) {
            $sourceDirectory = "$projectBinDirectory\$framework"
            $destinationDirectory = "$SigningDirectory\$framework"

            if (Test-Path $sourceDirectory) {

                # Everything we copy is a DLL, _except_ that application projects built for
                # NetFX have a .exe extension.
                $fileExtension = ".dll"
                if ($Projects.Applications -contains $project -and $Frameworks.NetFx -contains $framework) {
                    $fileExtension = ".exe"
                }

                $fileToCopy = "$sourceDirectory\$project$fileExtension"
                Copy-Item -Force -Path $fileToCopy -Destination $destinationDirectory
            }
        }
    }
}

# Create registry settings to open SARIF files in Visual Studio by default.
# If the SARIF Viewer extension is not installed, the file will be opened by the JSON editor.
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

& $PSScriptRoot\BeforeBuild.ps1 -NuGetVerbosity $NuGetVerbosity -NoClean:$NoClean -NoRestore:$NoRestore -NoObjectModel:$NoObjectModel
if (-not $?) {
    Exit-WithFailureMessage $ScriptName "BeforeBuild failed."
}

if (-not $NoBuild) {
    Invoke-Build
    Invoke-BuildSample
}

if (-not $NoTest) {
    & $PSScriptRoot\Run-Tests.ps1 $Configuration
    if (-not $?) {
        Exit-WithFailureMessage $ScriptName "RunTests failed."
    }
}

if (-not $NoPublish) {
    foreach ($project in $Projects.Applications) {
        foreach ($framework in $Frameworks.Application) {
            Publish-Application $project $framework
        }
    }
}

if (-not $NoSigningDirectory) {
    New-SigningDirectory
}

if (-not $NoPackage) {
    New-NuGetPackages $Configuration $Projects
}

if ($Associate) {
    Set-SarifFileAssociationRegistrySettings
}

Write-Information "$ScriptName SUCCEEDED."