@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

:: Uncomment this line to update nuget.exe
:: Doing so can break SLN build (which uses nuget.exe to
:: create a nuget package for the SARIF SDK) so must opt-in
:: %~dp0.nuget\NuGet.exe update -self

set Configuration=Release
set SolutionFile=src\Everything.sln
set FullClean=false
set NoTest=false
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
if "%1" == "/notest" (
    set NoTest=true&& shift && goto :NextArg
)
echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

@REM Remove existing build data
CALL :Clean %FullClean%

CALL BeforeBuild.cmd
CALL SetBuildEnvVars.cmd

set NuGetOutputDirectory=..\..\bld\bin\nuget\

msbuild /verbosity:minimal /target:Rebuild /property:Configuration=%Configuration% /fileloggerparameters:Verbosity=detailed %SolutionFile%
if "%ERRORLEVEL%" NEQ "0" (
    echo %SolutionFile%: Build failed.
    goto ExitFailed
)

if "%NoTest%" EQU "false" (
    for %%i in (%CrossPlatformTestProjects%) do (
        dotnet test --no-build --no-restore src\%%i\%%i.csproj
        if "%ERRORLEVEL%" NEQ "0" (
            echo %%i: tests failed.
            goto ExitFailed
        )
    )
)

echo SUCCESS -- so far!
echo TODO -- Finish modifying the rest of this script.
goto Exit

call :CreatePublishPackage Sarif.Multitool net452
call :CreatePublishPackage Sarif.Multitool netcoreapp2.0
call :CreatePublishPackage Sarif.Multitool netstandard2.0

::Build all NuGet packages
echo BuildPackages.cmd %Configuration% %Platform% %NuGetOutputDirectory% %Version% || goto :ExitFailed
call BuildPackages.cmd %Configuration% %Platform% %NuGetOutputDirectory% %Version% || goto :ExitFailed

::Create layout directory of assemblies that need to be signed
call CreateLayoutDirectory.cmd .\bld\bin\ %Configuration% %Platform%

@REM Run all multitargeting xunit tests
call :RunMultitargetingTests Sarif Unit                 || goto :ExitFailed
call :RunMultitargetingTests Sarif Functional           || goto :ExitFailed
call :RunMultitargetingTests Sarif.Converters Unit      || goto :ExitFailed
call :RunMultitargetingTests Sarif.Driver Unit          || goto :ExitFailed
call :RunMultitargetingTests Sarif.Multitool Functional || goto :ExitFailed

::Run all non-multitargeting unit tests
src\packages\xunit.runner.console.2.3.0\tools\net452\xunit.console.x86.exe bld\bin\Sarif.ValidationTests\AnyCPU_%Configuration%\Sarif.ValidationTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.3.0\tools\net452\xunit.console.x86.exe bld\bin\Sarif.Viewer.VisualStudio.UnitTests\AnyCPU_%Configuration%\Sarif.Viewer.VisualStudio.UnitTests.dll -parallel none
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

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

:CreatePublishPackage
set Project=%1
set Framework=%2
dotnet publish %~dp0src\%Project%\%Project%.csproj --no-restore -c %Configuration% -f %Framework%
Exit /B %ERRORLEVEL%

:RunMultitargetingTests
set TestProject=%1
set TestType=%2
pushd .\src\%TestProject%.%TestType%Tests && dotnet xunit --fx-version 2.0.0 -nobuild -configuration %Configuration%
popd
if "%ERRORLEVEL%" NEQ "0" (echo %TestProject% %TestType% tests execution FAILED.)
Exit /B %ERRORLEVEL%

:ExitFailed
@echo Build and test did not complete successfully.
Exit /B 1

:Exit