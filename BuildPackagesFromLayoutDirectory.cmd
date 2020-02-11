::Build NuGet packages step
@ECHO off
SETLOCAL

set BinaryOutputDirectory=%1
set Configuration=%1
set Platform=%2

if "%BinaryOutputDirectory%" EQU "" (
set BinaryOutputDirectory=.\bld\bin\
)

if "%Configuration%" EQU "" (
set Configuration=Release
)

if "%Platform%" EQU "" (
set Platform=AnyCpu
)

set BinaryOutputDirectory=%BinaryOutputDirectory%\%Platform%_%Configuration%
set LayoutForSigningDirectory=%BinaryOutputDirectory%\..\LayoutForSigning

:: Copy all multitargeted assemblies to their locations
call :CopyFilesForMultitargeting Sarif.dll            || goto :ExitFailed
call :CopyFilesForMultitargeting Sarif.Converters.dll || goto :ExitFailed
call :CopyFilesForMultitargeting Sarif.Driver.dll     || goto :ExitFailed
call :CopyFilesForMultitargeting Sarif.Multitool.exe  || goto :ExitFailed

:: Copy viewer dll to net452
xcopy /Y %LayoutForSigningDirectory%\net452\Microsoft.Sarif.Viewer.dll %BinaryOutputDirectory%\..\Sarif.Viewer.VisualStudio\%Platform%_%Configuration%\

call SetCurrentVersion.cmd
set Version=%MAJOR%.%MINOR%.%PATCH%%PRERELEASE%
set NuGetOutputDirectory=..\..\bld\bin\nuget\
call BuildPackages.cmd %Configuration% %Platform% %NuGetOutputDirectory% %Version% || goto :ExitFailed

goto :Exit

:CopyFilesForMultitargeting
xcopy /Y %LayoutForSigningDirectory%\net452\%1 %BinaryOutputDirectory%\net452\

:: For .NET core, .exes are renamed to .dlls due to packaging conventions
xcopy /Y %LayoutForSigningDirectory%\netcoreapp2.1\%~n1.dll %BinaryOutputDirectory%\netcoreapp2.1\
xcopy /Y %LayoutForSigningDirectory%\netstandard2.1\%~n1.dll %BinaryOutputDirectory%\netstandard2.1\

if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed.)
Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Build NuGet packages from layout directory step failed.
exit /b 1

:Exit