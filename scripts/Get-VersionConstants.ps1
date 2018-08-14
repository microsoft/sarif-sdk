<#
.SYNOPSIS
Extract the version number from build.props.

.PARAMETER Previous
Extract the previous version number rather than the current version number.
#>

[CmdletBinding()]
param(
    [switch]
    $Previous
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Import-Module $PSScriptRoot\ScriptUtilities.psm1 -Force

$buildPropsPath = "$SourceRoot\build.props"
$namespace = @{ msbuild = "http://schemas.microsoft.com/developer/msbuild/2003" }
$assemblyAttributesXPath = "/msbuild:Project/msbuild:PropertyGroup[@Label='AssemblyAttributes']"

function Get-VersionComponent($componentName) {
    $xPath = "$assemblyAttributesXPath/msbuild:$componentName"
    $xml = Select-Xml -Path $buildPropsPath -Namespace $namespace -XPath $xPath
    $xml.Node.InnerText
}

$PreviousNamePrefix = ""
if ($Previous) {
    $PreviousNamePrefix = "Previous"
}

$versionPrefix = Get-VersionComponent "${PreviousNamePrefix}VersionPrefix"
$versionSuffix = Get-VersionComponent "${PreviousNamePrefix}VersionSuffix"

$versionPrefix, $versionSuffix
