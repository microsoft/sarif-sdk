::Build initialization step

@ECHO off
SETLOCAL

SET NuGetConfigFile=%~dp0src\NuGet.Config
SET NuGetPackageDir=src\packages

md bld\bin\nuget

%~dp0.nuget\NuGet.exe restore src\Sarif.ValidationTests\Sarif.ValidationTests.csproj -ConfigFile "%NuGetConfigFile%" -OutputDirectory "%NuGetPackageDir%"
dotnet restore src\Sarif.Sdk.sln --configfile %NuGetConfigFile% --packages %NuGetPackageDir%

if "%ERRORLEVEL%" NEQ "0" (
echo NuGet restore failed.
goto ExitFailed
)

:: Generate the SARIF object model classes from the SARIF JSON schema.
msbuild /verbosity:minimal /target:BuildAndInjectObjectModel src\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed
if "%ERRORLEVEL%" NEQ "0" (
echo SARIF object model generation failed.
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo script %~n0 failed
exit /b 1

:Exit
