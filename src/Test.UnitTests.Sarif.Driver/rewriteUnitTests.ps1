param(
  [Parameter(Mandatory=$true)][string]$CoyoteVersion="",
  [Parameter(Mandatory=$true)][string]$TargetFramework
)

Write-Output "Rewrite Unit Tests with Coyote"
dotnet ../../packages/microsoft.coyote.test/$CoyoteVersion/lib/$TargetFramework/coyote.dll rewrite rewrite.coyote.json