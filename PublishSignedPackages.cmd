@echo on
call SetCurrentVersion.cmd

set VERSION=%MAJOR%.%MINOR%.%PATCH%%PRERELEASE%
set NUGET=.nuget\nuget.exe
set SOURCE=https://nuget.org

if exist ..\SetNugetSarifKey.cmd (
call ..\SetNugetSarifKey.cmd
call %NUGET% SetApiKey %API_KEY% -Source %SOURCE%
)
if "%ERRORLEVEL%" NEQ "0" (echo set api key of %API_KEY% to %SOURCE% FAILED && goto Exit)

@REM Publish SDK
set ID=Sarif.Sdk
set PACKAGE_ROOT=bld\bin\nuget\%ID%.%VERSION%

call %NUGET% push %PACKAGE_ROOT%.nupkg -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo push to %SOURCE% FAILED && goto Exit)

@REM Publish Driver
set ID=Sarif.Driver
set PACKAGE_ROOT=bld\bin\nuget\%ID%.%VERSION%

call %NUGET% push %PACKAGE_ROOT%.nupkg -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo push to %SOURCE% FAILED && goto Exit)

:Exit
