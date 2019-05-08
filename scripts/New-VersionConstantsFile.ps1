<#
.SYNOPSIS
Uses the package construction version details within build.props to synthesize a file containing
compilation constants used to set the version attributes of the assembly being built.
#>

param(
    [Parameter(Mandatory=$true)] $outputDirectory,
    [Parameter(Mandatory=$true)] $namespace
)

$ErrorActionPreference = "Stop"

$versionPrefix, $schemaVersion, $sdkVersion = & "$PSScriptRoot\Get-VersionConstants.ps1"

$versionConstantsFileContents =
@"
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace $namespace
{
    public static class VersionConstants
    {
        public const string SarifVersion = "$versionPrefix";
        public const string SchemaVersion = "$schemaVersion";
        public const string SdkVersion = "$sdkVersion";
    }
}
"@

$outputFile = Join-Path $outputDirectory VersionConstants.cs

# We use .NET rather than the PowerShell Set-Content cmdlet because Set-Content
# intermittently fails with "Stream was not readable".
[System.IO.File]::WriteAllText($outputFile, $versionConstantsFileContents)

