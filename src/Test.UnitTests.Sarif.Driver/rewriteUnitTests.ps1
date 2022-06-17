param(
  [Parameter(Mandatory=$true)][string]$CoyoteVersion="",
  [Parameter(Mandatory=$true)][string]$Configuration,
  [Parameter(Mandatory=$true)][string]$TargetFramework
)

Write-Output "Rewrite Unit Tests with Coyote $CoyoteVersion, on target $TargetFramework"
if ($ENV:OS) {
    dotnet ../../packages/microsoft.coyote.test/$CoyoteVersion/lib/$TargetFramework/coyote.dll rewrite rewrite.coyote.Windows.$Configuration.json
} else {
    dotnet ../../packages/microsoft.coyote.test/$CoyoteVersion/lib/$TargetFramework/coyote.dll rewrite rewrite.coyote.nonWindows.$Configuration.json
}
