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
%~dp0.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio\Sarif.Viewer.VisualStudio.csproj -OutputDirectory "src\packages"
%~dp0.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio.UnitTests\Sarif.Viewer.VisualStudio.UnitTests.csproj -OutputDirectory "src\packages"
%~dp0.nuget\NuGet.exe restore src\Sarif.ValidationTests\Sarif.ValidationTests.csproj -OutputDirectory "src\packages"
dotnet restore src\Everything.sln --packages src\packages

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
