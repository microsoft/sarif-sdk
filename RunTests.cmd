:: RunTests.cmd
::
:: This script runs the tests in each test project. This is done in a separate script, 
:: rather than inline in BuildAndTest.cmd, because AppVeyor cannot run BuildAndTest.
:: AppVeyor runs the tests by invoking a separate script, and this is it.

@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

call SetBuildEnvVars.cmd

for %%i in (%CrossPlatformTestProjects%) do (
    echo Running tests in %%i...
    dotnet test --no-build --no-restore src\%%i\%%i.csproj
    if "%ERRORLEVEL%" NEQ "0" (
        echo %%i: tests failed.
        goto ExitFailed
    )
)

:ExitFailed
@echo Tests did not complete successfully.
exit /B 1

:Exit