#Requires -RunAsAdministrator

$ErrorActionPreference = "stop"
$InformationPreference = "continue"

try {
    $vsExePath = (Get-ChildItem -Path ${env:ProgramFiles(x86)} -Filter "devenv.exe" -Recurse)[0].FullName
    $rootPath = ".\SOFTWARE\Classes"
    Push-Location
    Set-Location HKLM:
    Write-Information "Setting up the registery keys to run .SARIF files with Visual Studio by default ...."
    New-Item -Path (Join-Path $rootPath "\.sarif") -Value "SARIFFILE" -Force | Out-Null
    $sarifFilePath = Join-Path $rootPath "SARIFFILE\shell\Open"
    New-Item -Path $sarifFilePath -Value "&Open with Visual Studio Sarif Viewer" -Force | Out-Null
    New-Item -Path "$sarifFilePath\Command" -Value $vsExePath -Force | Out-Null
    Write-Information "Registery Key Setting are done"
} catch {
    Write-Information $_
} finally {
    Pop-Location
}