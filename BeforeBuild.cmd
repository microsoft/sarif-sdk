::Configuration for AppVeyor build platform

@ECHO off
SETLOCAL

if NOT exist "GeneratedKey.snk" (
sn -k GeneratedKey.snk
)

if "%ERRORLEVEL%" NEQ "0" (
echo COMMAND sn -k FAILED
goto ExitFailed
)

if NOT exist "GeneratedKey.snk" (
echo GeneratedKey.snk NOT FOUND
goto ExitFailed
)

@REM Restore nuget packages
%~dp0.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

if "%ERRORLEVEL%" NEQ "0" (
echo NUGET RESTORE FAILED
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo SCRIPT FAILED
exit /b 1

:Exit