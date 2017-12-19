::Build initialization step

@ECHO off
SETLOCAL

SET NUGET_CONFIG_FILE=%~dp0src\NuGet.Config
SET NUGET_PACKAGE_DIR=src\packages

::Restore nuget packages
%~dp0.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio\Sarif.Viewer.VisualStudio.csproj -ConfigFile "%NUGET_CONFIG_FILE%" -OutputDirectory "%NUGET_PACKAGE_DIR%"
%~dp0.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio.UnitTests\Sarif.Viewer.VisualStudio.UnitTests.csproj -ConfigFile "%NUGET_CONFIG_FILE%" -OutputDirectory "%NUGET_PACKAGE_DIR%"
%~dp0.nuget\NuGet.exe restore src\Sarif.ValidationTests\Sarif.ValidationTests.csproj -ConfigFile "%NUGET_CONFIG_FILE%" -OutputDirectory "%NUGET_PACKAGE_DIR%"
dotnet restore src\Everything.sln --packages %NUGET_PACKAGE_DIR%

if "%ERRORLEVEL%" NEQ "0" (
echo nuget restore failed.
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
