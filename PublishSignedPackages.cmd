@echo on
call SetCurrentVersion.cmd

set ID=Sarif.Sdk
set VERSION=%MAJOR%.%MINOR%.%PATCH%%PRERELEASE%
set PACKAGE_ROOT=%DROP%Nuget\%ID%.%VERSION%
set NUGET=.nuget\nuget.exe
set SOURCE=https://nuget.org

echo %PACKAGE_ROOT%

if exist ..\SetNugetSarifKey.cmd (
call ..\SetNugetSarifKey.cmd
call %NUGET% SetApiKey %API_KEY% -Source %SOURCE%
)
if "%ERRORLEVEL%" NEQ "0" (echo set api key of %API_KEY% to %SOURCE% FAILED && goto Exit)

call %NUGET% push %PACKAGE_ROOT%.nupkg -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo push to %SOURCE% FAILED && goto Exit)

@REM immediately unlist our package
call %NUGET% delete %ID% %VERSION% -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo package delisting FAILED && goto Exit)

@REM call %NUGET%push %PACKAGE_ROOT%.symbols.nupkg -Source https://nuget.smbsrc.net/
if "%ERRORLEVEL%" NEQ "0" (echo push to symbsource.org FAILED goto Exit)

:Exit
