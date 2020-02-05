<#
.SYNOPSIS
    Package the SARIF SDK using binaries from the signing directory.
.DESCRIPTION
    Builds the SARIF SDK NuGet Packages from the signing directory after
    they have been signed.
.PARAMETER Configuration
    The build configuration: Release or Debug. Default=Release
#>

[CmdletBinding()]
param(
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration="Release"
)

$ScriptName = $([io.Path]::GetFileNameWithoutExtension($PSCommandPath))

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1
Import-Module -Force $PSScriptRoot\NuGetUtilities.psm1

# Copy signed binaries back into the normal directory structure.
function Copy-FromSigningDirectory {
    Write-Information "Copying files to signing directory..."
    $SigningDirectory = "$BinRoot\Signing"

    foreach ($project in $Projects.Products) {
        $projectBinDirectory = (Get-ProjectBinDirectory $project $configuration)

        foreach ($framework in $Frameworks.All) {
            $sourceDirectory = "$SigningDirectory\$framework"
            $destinationDirectory = "$projectBinDirectory\$framework"

            # Everything we copy is a DLL, _except_ that application projects built for
            # NetFX have a .exe extension.
            $fileExtension = ".dll"
            if ($Projects.Applications -contains $project -and $Frameworks.NetFx -contains $framework) {
                $fileExtension = ".exe"
            }

            $fileToCopy = "$sourceDirectory\$project$fileExtension"
            if (Test-Path $fileToCopy) {
                Write-Information "$fileToCopy $destinationDirectory"
                Copy-Item -Force -Path $fileToCopy -Destination $destinationDirectory
            }
        }
    }
}

Copy-FromSigningDirectory

New-NuGetPackages $Configuration $Projects