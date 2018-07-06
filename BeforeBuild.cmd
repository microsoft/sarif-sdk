:: BeforeBuild.cmd
::
:: This script performs the actions that are required before building the solution file
:: src\Everything.sln. These actions are broken out into a separate script, rather than
:: being performed inline in BuildAndTest.cmd, because AppVeyor cannot run BuildAndTest.
:: AppVeyor only allows you to specify the project to build, and a script to run before
:: the build step. So that is how we have factored the build scripts.

@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

set ThisFileDir=%~dp0

call SetBuildEnvVars.cmd

set NuGetConfigFile=%ThisFileDir%src\NuGet.Config
set NuGetPackageDir=%ThisFileDir%src\packages

:: We have to restore the projects one by one, rather than restoring the entire solution,
:: because the solution includes projects that are not .NET SDK projects.
for %%i IN (%CrossPlatformProjects%) DO (
    echo Restoring NuGet packages for %%i...
    dotnet restore src\%%i\%%i.csproj --configfile %NuGetConfigFile% --verbosity quiet
        if "%ERRORLEVEL%" NEQ "0" (
            echo NuGet restore failed for project %%i.
            goto ExitFailed
    )
)

::Restore nuget packages for projects that we don't build with dotnet core.
echo Restoring NuGet packages for Sarif.Viewer.VisualStudio...
%ThisFileDir%.nuget\NuGet.exe restore src\Sarif.Viewer.VisualStudio\Sarif.Viewer.VisualStudio.csproj -ConfigFile "%NuGetConfigFile%" -OutputDirectory "%NuGetPackageDir%" -Verbosity normal
if "%ERRORLEVEL%" NEQ "0" (
    echo NuGet restore failed for project Sarif.Viewer.VisualStudio.
    goto ExitFailed
)

:: Generate the SARIF object model classes from the SARIF JSON schema.
msbuild /verbosity:minimal /target:BuildAndInjectObjectModel src\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed;LogFile=CodeGen.log
if "%ERRORLEVEL%" NEQ "0" (
    echo SARIF object model generation failed.
    goto ExitFailed
)

goto Exit

:ExitFailed
@echo BeforeBuild script failed.
Exit /B 1

:Exit