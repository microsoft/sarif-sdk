<#
.SYNOPSIS
    Delists currently visible NuGet packages.
.DESCRIPTION
    Whenever we bump the SDK version number, we publish new packages. Then we
    need to "delist" the packages from the previous version. ("hide" is really
    what it amounts to, and "delete" is what the NuGet command line calls it.)
#>


Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module $PSScriptRoot\NuGetUtilities.psm1

Hide-NuGetPackages