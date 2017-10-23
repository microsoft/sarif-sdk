@ECHO off
SETLOCAL

pushd .\src\Sarif.Converters.UnitTests && dotnet xunit -appveyor -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.UnitTests && dotnet xunit -appveyor -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.Driver.UnitTests && dotnet xunit -appveyor -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.FunctionalTests && dotnet xunit -appveyor -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.Multitool.FunctionalTests && dotnet xunit -appveyor -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.2.0\tools\xunit.console.x86.exe -appveyor bld\bin\Sarif.ValidationTests\AnyCPU_%Configuration%\Sarif.ValidationTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.2.0\tools\xunit.console.x86.exe -appveyor bld\bin\Sarif.Viewer.VisualStudio.UnitTests\AnyCPU_%Configuration%\Sarif.Viewer.VisualStudio.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo script failed
exit /b 1

:Exit