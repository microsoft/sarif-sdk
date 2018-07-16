<#
.SYNOPSIS
Extract the version number from build.props.
#>

$ErrorActionPreference = "Stop"

$buildPropsPath = "$PSScriptRoot\src\build.props"
$namespace = @{ msbuild = "http://schemas.microsoft.com/developer/msbuild/2003" }
$assemblyAttributesXPath = "/msbuild:Project/msbuild:PropertyGroup[@Label='AssemblyAttributes']"

function Get-VersionComponent($componentName) {
    $xPath = "$assemblyAttributesXPath/msbuild:$componentName"
    $xml = Select-Xml -Path $buildPropsPath -Namespace $namespace -XPath $xPath
    $xml.Node.InnerText
}

$versionPrefix = Get-VersionComponent VersionPrefix
$versionSuffix = Get-VersionComponent VersionSuffix

$versionPrefix, $versionSuffix
