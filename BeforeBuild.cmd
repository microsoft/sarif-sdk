rem BeforeBuild.cmd
rem
rem This script performs the actions that are required before building the solution file
rem src\Everything.sln. These actions are broken out into a separate script, rather than
rem being performed inline in BuildAndTest.cmd, because AppVeyor cannot run BuildAndTest.
rem AppVeyor only allows you to specify the project to build, and a script to run before
rem the build step. So that is how we have factored the build scripts.

@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

set ThisFileDir=%~dp0

call SetBuildEnvVars.cmd

set NuGetConfigFile=%ThisFileDir%src\NuGet.Config
set NuGetPackageDir=%ThisFileDir%src\packages

rem Restore NuGet packages for projects that use the new VS 2017 project system.
rem We have to restore the projects one by one, rather than restoring the entire solution,
rem because the solution includes projects that do not use the VS 2017 project system.
for %%i in (%NewProjects%) do (
    echo Restoring NuGet packages for %%i...
    dotnet restore src\%%i\%%i.csproj --configfile %NuGetConfigFile% --packages %NuGetPackageDir% --verbosity quiet
        if "%ERRORLEVEL%" NEQ "0" (
            echo NuGet restore failed for project %%i.
            goto ExitFailed
    )
)

rem Restore nuget packages for projects that don't use the VS 2017 project system.
for %%i in (%OldProjects%) do (
    echo Restoring NuGet packages for %%i...
    %ThisFileDir%.nuget\NuGet.exe restore src\%%i\%%i.csproj -ConfigFile "%NuGetConfigFile%" -OutputDirectory "%NuGetPackageDir%" -Verbosity normal
    if "%ERRORLEVEL%" NEQ "0" (
        echo NuGet restore failed for project %%i.
        goto ExitFailed
    )
)

rem Generate the SARIF object model classes from the SARIF JSON schema.
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