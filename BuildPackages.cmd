::Build NuGet packages step
@ECHO off
SETLOCAL

set Configuration=%1
set Platform=%2
set NuGetOutputDirectory=%3
set Version=%4

::Build release packages
call :BuildNuGetPackageFromCsproj Sarif            %Version% || goto :ExitFailed
call :BuildNuGetPackageFromCsproj Sarif.Converters %Version% || goto :ExitFailed

::Build pre-release packages
call :BuildNuGetPackageFromCsproj Sarif.Driver    %Version%-beta || goto :ExitFailed

::Build Multitool package from nuspec
call :BuildNuGetPackageFromNuspec Sarif.Multitool %Version% beta || goto :ExitFailed

goto Exit

:BuildNuGetPackageFromCsproj
set NuGetProject=%1
set PackOptions=--configuration %Configuration% --no-build -p:Platform=%Platform% -o %NuGetOutputDirectory% --include-source --include-symbols -p:PackageVersion=%2
echo .
echo dotnet pack %~p0src\%NuGetProject%\%NuGetProject%.csproj %PackOptions%
dotnet pack %~p0src\%NuGetProject%\%NuGetProject%.csproj %PackOptions%
if "%ERRORLEVEL%" NEQ "0" (echo %NuGetProject% NuGet package creation FAILED.)
Exit /B %ERRORLEVEL%

:BuildNuGetPackageFromNuspec
set NuGetProject=%1
set PackOptions=-Symbols -Properties configuration=%Configuration%;version=%2 -Verbosity Quiet -BasePath .\ -OutputDirectory .\bld\bin\Nuget
set Suffix=%3
if "%Suffix%" NEQ "" (
set Suffix=-Suffix %Suffix%
)
.nuget\NuGet.exe pack .\src\Nuget\%NuGetProject%.nuspec %PackOptions% %Suffix%
if "%ERRORLEVEL%" NEQ "0" (echo %NuGetProject% NuGet package creation FAILED.)
Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Build NuGet packages step failed.
exit /b 1

:Exit