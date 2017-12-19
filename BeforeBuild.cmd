::Build initialization step

@ECHO off
SETLOCAL

::Restore nuget packages
%~dp0.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio\Sarif.Viewer.VisualStudio.csproj -ConfigFile "%~dp0src\NuGet.Config" -OutputDirectory "src\packages"
%~dp0.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio.UnitTests\Sarif.Viewer.VisualStudio.UnitTests.csproj -ConfigFile "%~dp0src\NuGet.Config" -OutputDirectory "src\packages"
%~dp0.nuget\NuGet.exe restore src\Sarif.ValidationTests\Sarif.ValidationTests.csproj -ConfigFile "%~dp0src\NuGet.Config" -OutputDirectory "src\packages"
dotnet restore src\Everything.sln --packages src\packages

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
