@echo off
call SetCurrentVersion.cmd

if "%PRERELEASE%" EQU "-developer" (
echo Attempt to push working bits. Fix prerelease value and rebuild && goto Exit)
)

set VERSION=%MAJOR%.%MINOR%.%PATCH%%PRERELEASE%
set NUGET=.nuget\nuget.exe
set SOURCE=https://nuget.org

if exist ..\SetNugetSarifKey.cmd (
call ..\SetNugetSarifKey.cmd
call %NUGET% SetApiKey %API_KEY% -Source %SOURCE%
)
if "%ERRORLEVEL%" NEQ "0" (echo set api key of %API_KEY% to %SOURCE% FAILED && goto Exit)

call :PublishPackage Sarif.Sdk        || goto :EOF
call :PublishPackage Sarif.Converters || goto :EOF
call :PublishPackage Sarif.Driver     || goto :EOF
call :PublishPackage Sarif.Multitool  || goto :EOF

goto :EOF

:PublishPackage
set ID=%1
set PRERELEASE=%2
set PACKAGE_ROOT=.\bld\bin\nuget\%ID%.%VERSION%%PRERELEASE%

call %NUGET% push %PACKAGE_ROOT%.symbols.nupkg -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo Push of %ID% to %SOURCE% failed.)
Exit /B %ERRORLEVEL%
:EOF
