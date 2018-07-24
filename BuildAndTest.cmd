@echo off
powershell -ExecutionPolicy RemoteSigned -File %~dp0\scripts\BuildAndTest.ps1 %*
