<#
.SYNOPSIS
Updates the submodule with the latest sources from the remote.
After running this script, commit and push to the server.
#>

$ErrorActionPreference = "Stop"

& git submodule update --remote