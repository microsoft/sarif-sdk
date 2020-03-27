<#
.SYNOPSIS
Creates and syncs the submodule.
This script does not have to be rerun. It's provided only as a reference to know
the command parameters used to create the submodule.
#>

$ErrorActionPreference = "Stop"

$submoduleUrl = 'https://smurff@dev.azure.com/smurff/Smurff/_git/Smurff'
$submoduleFolder = 'extensions'

Push-Location "$PSScriptRoot\.."

# Create the submodule in the root of the repo.
Write-Host "Uncomment the lines to run the script"
# & git submodule add -b master "$submoduleUrl" "$submoduleFolder"
# & git submodule update --init --recursive

Pop-Location
