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

$assemblyAttributesXPath = "/msbuild:Project/msbuild:PropertyGroup[@Label='AssemblyAttributes']"

function Get-VersionComponent($componentName) {
    $xPath = "$assemblyAttributesXPath/msbuild:$componentName"
    $xml = Select-Xml -Path $BuildPropsPath -Namespace $MSBuildXmlNamespaces -XPath $xPath
    $xml.Node.InnerText
}

$PreviousNamePrefix = ""
if ($Previous) {
    $PreviousNamePrefix = "Previous"
}

$sarifVersion = Get-VersionComponent "${PreviousNamePrefix}SarifVersion"
$schemaVersion = Get-VersionComponent "${PreviousNamePrefix}SchemaVersion"
$sdkVersion = Get-VersionComponent "${PreviousNamePrefix}SdkVersion"

$sarifVersion, $schemaVersion, $sdkVersion
