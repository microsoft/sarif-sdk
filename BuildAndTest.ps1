# Requires -Version 5.0

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

function Set-MSBuildPath {
    $vsPath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\"
    $msBuildpaths = Get-ChildItem -Path $vsPath -Filter "MSBuild.exe" -Recurse
    $rootPath = $msBuildpaths[0].DirectoryName
    $env:Path += ";$rootPath"
}

function Run-Build {
    $proc = start-process .\BuildAndTest.cmd -PassThru
    $proc.WaitForExit()
    $exitCode = $proc.ExitCode
    $proc.Dispose()
    if ($exitCode -ne 0) {
        throw "BuildFialed. See msbuild.log file"
    }   
}

function  Install-SarifExtension {
    $vsixInstallerPaths = Get-ChildItem $PSScriptRoot "*.vsix" -Recurse
    Start-Process $vsixInstallerPaths[0].FullName -PassThru
}

function Set-RegistrySettings {
    $path = "$PSScriptRoot\RegistrySettings.ps1"
    Start-Process powershell.exe -ArgumentList "-File $path" -Verb RunAs | Out-Null
}

try {
    Set-MSBuildPath
    Run-Build
    Install-SarifExtension
    Set-RegistrySettings
} catch {
    Write-Information $_
}