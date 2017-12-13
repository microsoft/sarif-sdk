@echo off
call SetCurrentVersion.cmd

set VERSION=%MAJOR_PREVIOUS%.%MINOR_PREVIOUS%.%PATCH_PREVIOUS%%PRERELEASE_PREVIOUS%
set NUGET=.nuget\nuget.exe
set SOURCE=https://nuget.org

if exist ..\SetNugetSarifApiKey.cmd (
call ..\SetNugetSarifApiKey.cmd
call %NUGET% SetApiKey %API_KEY% -Source %SOURCE%
)
if "%ERRORLEVEL%" NEQ "0" (echo set api key of %API_KEY% to %SOURCE% FAILED && goto Exit)

call :DelistPackage Sarif.Sdk        || goto :EOF
call :DelistPackage Sarif.Driver     || goto :EOF
call :DelistPackage Sarif.Converters || goto :EOF
call :DelistPackage Sarif.Multitool  || goto :EOF

goto :EOF

:DelistPackage
set ID=%1
call %NUGET% delete %ID% %VERSION% -Source %SOURCE%
if "%ERRORLEVEL%" NEQ "0" (echo Delisting of %ID% failed.)
Exit /B %ERRORLEVEL%

:EOF
