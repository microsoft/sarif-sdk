@ECHO off
SETLOCAL
@REM Uncomment this line to update nuget.exe
@REM Doing so can break SLN build (which uses nuget.exe to
@REM create a nuget package for the SARIF SDK) so must opt-in
@REM %~dp0.nuget\NuGet.exe update -self

set Platform=AnyCPU
set Configuration=Release

:NextArg
if "%1" == "" goto :EndArgs
if "%1" == "/config" (
    if not "%2" == "Debug" if not "%2" == "Release" echo error: /config must be either Debug or Release && goto :ExitFailed
    set Configuration=%2&& shift && shift && goto :NextArg
)
echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

@REM Remove existing build data
if exist bld (rd /s /q bld)
set NuGetOutputDirectory=..\..\bld\bin\nuget\

call SetCurrentVersion.cmd 

call WriteVersionConstants.cmd

call BeforeBuild.cmd

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

msbuild /verbosity:minimal /target:rebuild src\Everything.sln /filelogger /fileloggerparameters:Verbosity=detailed /p:AutoGenerateBindingRedirects=false
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

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