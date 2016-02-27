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


@REM immediately unlist our package
set ID=Sarif.Sdk
call %NUGET% delete %ID% %VERSION% -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo package delisting FAILED && goto Exit)

set ID=Sarif.Driver
call %NUGET% delete %ID% %VERSION% -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo package delisting FAILED && goto Exit)

:Exit