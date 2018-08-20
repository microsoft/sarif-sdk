<#
.SYNOPSIS
    Package the SARIF SDK from a custom signing directory.
.DESCRIPTION
    Builds the SARIF SDK NuGet Packages from a custom signing directory after
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

Import-Module -Force $PSScriptRoot\ScriptUtilities.psm1
Import-Module -Force $PSScriptRoot\Projects.psm1

# Copy signed binaries back into the normal directory structure.
function CopyFromSigningDirectory {
    Write-Information "Copying files to signing directory..."
    $SigningDirectory = "$BinRoot\Signing"

    foreach ($project in $Projects.NewProduct) {
        $projectBinDirectory = "$BinRoot\${Platform}_$Configuration\$project\"

        foreach ($framework in $Frameworks.All) {
            $sourceDirectory = "$SigningDirectory\$framework"
            $destinationDirectory = "$projectBinDirectory\$framework"

            # Everything we copy is a DLL, _except_ that application projects built for
            # NetFX have a .exe extension.
            $fileExtension = ".dll"
            if ($Projects.NewApplication -contains $project -and $Frameworks.NetFx -contains $framework) {
                $fileExtension = ".exe"
            }

            $fileToCopy = "$sourceDirectory\$project$fileExtension"
            if (Test-Path $fileToCopy) {
                Write-Information "$fileToCopy $destinationDirectory"
                Copy-Item -Force -Path $fileToCopy -Destination $destinationDirectory
            }
        }
    }

    # Copy the viewer. Its name doesn't fit the pattern binary name == project name,
    # so we copy it by hand.
    foreach ($framework in $Frameworks.NetFX) {
        Copy-Item -Force -Path $SigningDirectory\$framework\Microsoft.Sarif.Viewer.dll -Destination $BinRoot\${Platform}_$Configuration\Sarif.Viewer.VisualStudio\Microsoft.Sarif.Viewer.dll
    }
}

CopyFromSigningDirectory

New-NuGetPackages $Configuration $Projects