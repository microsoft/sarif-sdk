@echo off
powershell -ExecutionPolicy RemoteSigned -File %~dp0BuildAndTest.ps1 %*
