<#
.SYNOPSIS
    Performs pre-build actions.
.DESCRIPTION
    This script performs the actions that are required before building the solution file
    src\Sarif.Sdk.sln. These actions are broken out into a separate script, rather than
    being performed inline in BuildAndTest.cmd, because AppVeyor cannot run BuildAndTest.
    AppVeyor only allows you to specify the project to build, and a script to run before
    the build step. So that is how we have factored the build scripts.
.PARAMETER NuGetVerbosity
    Specifies the amount of information for NuGet to display: quiet, normal,
    or detailed. Default=quiet
.PARAMETER NoClean
    Do not remove the outputs from the previous build.
.PARAMETER NoRestore
    Do not restore NuGet packages.
.PARAMETER NoObjectModel
    Do not rebuild the SARIF object model from the schema.
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("quiet", "normal", "detailed")]
    $NuGetVerbosity = "quiet",

    [switch]
    $NoClean,

    [switch]
    $NoRestore,

    [switch]
    $NoObjectModel
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

function Remove-BuildOutput {
    Remove-DirectorySafely $BuildRoot
    foreach ($project in $Projects.All) {
        $objDir = "$SourceRoot\$project\obj"
        Remove-DirectorySafely $objDir
    }
}

# Install-VersionConstantsFile
#
# Create a source file containing a class that specifies the NuGet package
# version number, and place it in the Sarif project directory.
#
function Install-VersionConstantsFile {
    # The name of the project in which to install VersionConstants.cs.
    $targetProjectName = "Sarif"

    # The element in build.props from which to extract the root of the namespace
    # used in VersionConstants.cs.
    $xPath = "/msbuild:Project/msbuild:PropertyGroup[@Label='Build']/msbuild:RootNamespaceBase"
    $xml = Select-Xml -Path $BuildPropsPath -Namespace $MSBuildXmlNamespaces -XPath $xPath
    $rootNamespaceBase = $xml.Node.InnerText

    $targetProjectDirectory = "$SourceRoot\$targetProjectName"
    $rootNamespace = "$rootNamespaceBase.$targetProjectName"

    & $PSScriptRoot\New-VersionConstantsFile.ps1 $targetProjectDirectory $rootNamespace
}

if (-not $NoClean) {
    Remove-BuildOutput
}

Install-VersionConstantsFile

if (-not $NoRestore) {
    Write-Information "Restoring NuGet packages for $SolutionFile..."

    dotnet restore $SourceRoot\$SolutionFile --configfile $NuGetConfigFile --packages $NuGetPackageRoot --verbosity $NuGetVerbosity
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "NuGet restore failed for $SolutionFile."
    }

    Write-Information "Restoring NuGet packages for $SampleSolutionFile..."
        & $NuGetExePath restore -ConfigFile $NuGetConfigFile -Verbosity $NuGetVerbosity -OutputDirectory $NuGetSamplesPackageRoot $SourceRoot\$SampleSolutionFile
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "NuGet restore failed for $SampleSolutionFile."
    }
}

if (-not $NoObjectModel) {
    # Generate the SARIF object model classes from the SARIF JSON schema.
    dotnet msbuild /verbosity:minimal /target:BuildAndInjectObjectModel $SourceRoot\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed`;LogFile=CodeGen.log
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "SARIF object model generation failed."
    }
}
