@echo off
powershell -ExecutionPolicy RemoteSigned -File %~dp0\scripts\DelistCurrentPackages.ps1 %*
