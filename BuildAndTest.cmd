@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Uncomment this line to update nuget.exe
rem Doing so can break SLN build (which uses nuget.exe to
rem create a nuget package for the SARIF SDK) so must opt-in
rem %~dp0.nuget\NuGet.exe update -self

set Configuration=Release
set SolutionFile=src\Everything.sln
set FullClean=false
set NoClean=false
set NoBuild=false
set NoTest=false
set NoPublish=false
set BuildTarget=rebuild
set ThisFileDir=%~dp0

:NextArg
if "%1" == "" goto :EndArgs
if "%1" == "/config" (
    if not "%2" == "Debug" if not "%2" == "Release" echo error: /config must be either Debug or Release && goto :ExitFailed
    set Configuration=%2&& shift && shift && goto :NextArg
)
if "%1" == "/sln" (
    if "%2" == "" echo error: no argument specified for /sln && goto ExitFailed
    if not exist "%2" echo error: solution file "%2" does not exist && goto ExitFailed
    set SolutionFile=%2&& shift && shift && goto :NextArg
)
if "%1" == "/fullclean" (
    set FullClean=true&& shift && goto :NextArg
)
if "%1" == "/noclean" (
    set NoClean=true&& shift && goto :NextArg
)
if "%1" == "/nobuild" (
    REM If we're not going to build, we'd better not clean.
    set NoBuild=true&& set NoClean=true&& shift && goto :NextArg
)
if "%1" == "/notest" (
    set NoTest=true&& shift && goto :NextArg
)
if "%1" == "/nopublish" (
    set NoPublish=true&& shift && goto :NextArg
)
if "%1" == "/incremental" (
    set BuildTarget=build&&set NoClean=true&& shift && goto :NextArg
)
echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

if "%NoClean%" EQU "false" (
    rem Remove existing build data
    call :Clean %FullClean%
)

call BeforeBuild.cmd
call SetBuildEnvVars.cmd

set NuGetOutputDirectory=..\..\bld\bin\nuget\

if "%NoBuild%" EQU "false" (
    msbuild /verbosity:minimal /target:%BuildTarget% /property:Configuration=%Configuration% /fileloggerparameters:Verbosity=detailed %SolutionFile%
    if "%ERRORLEVEL%" NEQ "0" (
        echo %SolutionFile%: Build failed.
        goto ExitFailed
    )
)

if "%NoTest%" EQU "false" (
    call RunTests.cmd /config %Configuration%
)

if "%NoPublish%" EQU "false" (
    call :PublishApplication Sarif.Multitool net461
    call :PublishApplication Sarif.Multitool netcoreapp2.0
)

echo SUCCESS -- so far!
echo TODO -- Finish modifying the rest of this script.
goto Exit

rem Build all NuGet packages
echo BuildPackages.cmd %Configuration% %Platform% %NuGetOutputDirectory% %Version% || goto :ExitFailed
call BuildPackages.cmd %Configuration% %Platform% %NuGetOutputDirectory% %Version% || goto :ExitFailed

rem Create layout directory of assemblies that need to be signed
call CreateLayoutDirectory.cmd .\bld\bin\ %Configuration% %Platform%

goto Exit

:Clean
ECHO Cleaning enlistment...

SET BLDDIR=bld
IF EXIST !BLDDIR! (
    ECHO Removing !BLDDIR!...
    RMDIR /S /Q !BLDDIR!
)

IF "%FullClean%" EQU "true" (
    ECHO Performing full clean...
    SET PKGDIR=src\packages
    IF EXIST !PKGDIR! (
        ECHO Removing !PKGDIR!...
        RMDIR /S /Q !PKGDIR!
    )

    FOR /D %%D IN (src\*) DO (
        SET OBJDIR=%%D\obj
        IF EXIST !OBJDIR! (
            ECHO Removing !OBJDIR!...
            RMDIR /S /Q !OBJDIR!
        )
    )
)

ECHO Done.
EXIT /B %ERRORLEVEL%

:PublishApplication
set Project=%1
set Framework=%2
echo Publishing %1 for %2...
dotnet publish %~dp0src\%Project%\%Project%.csproj --no-restore --configuration %Configuration% --framework %Framework%
Exit /B %ERRORLEVEL%

:ExitFailed
@echo Build and test did not complete successfully.
Exit /B 1

:Exit