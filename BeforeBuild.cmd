@ECHO off
SETLOCAL

if NOT exist "GeneratedKey.snk" (
sn -k GeneratedKey.snk
)

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

if NOT exist "GeneratedKey.snk" (
goto ExitFailed
)

@REM Build all code
%~dp0.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

goto Exit

:ExitFailed
@echo.
@echo SCRIPT FAILED
exit /b 1

:Exit