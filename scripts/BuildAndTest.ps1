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
.PARAMETER NoFormat
    Do not format files based on dotnet-format tool.
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
    $Associate,
    
    [switch]
    $NoFormat
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$OnWindows = $Env:OS -eq 'Windows_NT'
$NonWindowsOptions = @{}

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

function Invoke-DotNetBuild($solutionFileRelativePath) {
    Write-Information "Building $solutionFileRelativePath..."

    $solutionFilePath = Join-Path $SourceRoot $solutionFileRelativePath
    & dotnet build $solutionFilePath --configuration $Configuration --verbosity $BuildVerbosity --no-incremental -bl /p:EnforceCodeStyleInBuild=true
    
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Build of $solutionFilePath failed."
    }
}

# Create a directory containing all files necessary to execute an application.
# This operation is called "publish" because it is performed by "dotnet publish".
function Publish-Application($project, $framework) {
    Write-Information "Publishing $project for $framework ..."
    dotnet publish $SourceRoot\$project\$project.csproj --no-build --configuration $Configuration --framework $framework
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Publish failed."
    }
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
                if ($Projects.Applications -contains $project -and $Frameworks.ApplicationNetFx -contains $framework) {
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


# The SARIF object model is stable. We disable autogenerating it to allow
# for strict control enforcing style guidelines from command-line builds.
#if (-not $NoObjectModel) {
#    # Generate the SARIF object model classes from the SARIF JSON schema.
#    dotnet msbuild /verbosity:minimal /target:BuildAndInjectObjectModel $SourceRoot\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed`;LogFile=CodeGen.log
#    if ($LASTEXITCODE -ne 0) {
#        Exit-WithFailureMessage $ScriptName "SARIF object model generation failed."
#    }
#}


if (-not $?) {
    Exit-WithFailureMessage $ScriptName "BeforeBuild failed."
}

if (-not $NoBuild) {
    Invoke-DotNetBuild $SolutionFile
    if ($OnWindows) {
        Invoke-DotNetBuild $sampleSolutionFile
    }
}

if (-not $NoTest) {
    if (-not $OnWindows) {
        $NonWindowsOptions = @{ "-filter" = "WindowsOnly!=true" }
    }
    & dotnet test $SourceRoot\$SolutionFile --no-build --configuration $Configuration @NonWindowsOptions
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Tests failed."
    }
}

if (-not $NoPublish -and $OnWindows) {  # Can't publish on non-windows due to not building net4x assets
    foreach ($project in $Projects.Applications) {
        foreach ($framework in $Frameworks.Application) {
            Publish-Application $project $framework
        }
    }
}

if (-not $NoSigningDirectory) {
    New-SigningDirectory
}

if (-not $NoPackage -and $OnWindows) { # Can't package on non-windows due to not building net4x assets
    & dotnet pack $SourceRoot\$SolutionFile --no-build --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage $ScriptName "Package failed."
    }
}

if (-not $NoFormat) {
    dotnet tool update --global dotnet-format --version 4.1.131201
    dotnet-format --folder --exclude .\src\Sarif\Autogenerated\
}

if ($Associate) {
    Set-SarifFileAssociationRegistrySettings
}

Write-Information "$ScriptName SUCCEEDED."