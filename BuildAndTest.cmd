call BeforeBuild.cmd

msbuild /verbosity:minimal /target:rebuild src\Everything.sln /p:"Configuration=%Configuration%" /p:"Platform=Any CPU" /filelogger /fileloggerparameters:Verbosity=detailed

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Build Nuget packages
.nuget\NuGet.exe pack .\src\Nuget\Sarif.Sdk.nuspec -Symbols -Properties id=Sarif.Sdk;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE%;configuration=%Configuration% -Verbosity Quiet -BasePath .\bld\bin\Sarif\AnyCPU_%Configuration% -OutputDirectory .\bld\bin\Nuget
.nuget\NuGet.exe pack .\src\Nuget\Sarif.Driver.nuspec -Symbols -Properties id=Sarif.Driver;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE%;configuration=%Configuration% -Verbosity Quiet -BasePath .\bld\bin\Sarif.Driver\AnyCPU_%Configuration%\ -OutputDirectory .\bld\bin\Nuget

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Run all tests
SET PASSED=true

mstest /detail:errormessage /detail:stdout /detail:errorstacktrace /detail:displaytext /detail:traceinfo /detail:outcometext /detail:spoolmessage /testContainer:bld\bin\Sarif.UnitTests\AnyCPU_%Configuration%\Sarif.UnitTests.dll | tee logs.txt
if "%ERRORLEVEL%" NEQ "0" (
set PASSED=false
)

if "%PASSED%" NEQ "true" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.Driver.UnitTests\AnyCPU_%Configuration%\Sarif.Driver.UnitTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.FunctionalTests\AnyCPU_%Configuration%\Sarif.FunctionalTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.ValidationTests\AnyCPU_%Configuration%\Sarif.ValidationTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo SCRIPT FAILED
exit /b 1

:Exit


