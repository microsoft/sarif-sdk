::Build initialization step

@ECHO off
SETLOCAL

if NOT exist "GeneratedKey.snk" (
sn -k GeneratedKey.snk
)

if "%ERRORLEVEL%" NEQ "0" (
echo command sn -k failed
goto ExitFailed
)

if NOT exist "GeneratedKey.snk" (
echo GeneratedKey.snk not found
goto ExitFailed
)

::Restore nuget packages
%~dp0.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

if "%ERRORLEVEL%" NEQ "0" (
echo nuget restore failed
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo script %~n0 failed
exit /b 1

:Exit
