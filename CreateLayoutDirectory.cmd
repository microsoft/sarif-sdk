::Build NuGet packages step
@ECHO off
SETLOCAL

set BinaryOutputDirectory=%1
set Configuration=%2
set Platform=%3

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

if not exist %LayoutForSigningDirectory% (md %LayoutForSigningDirectory%)
if not exist %LayoutForSigningDirectory%\net452 (md %LayoutForSigningDirectory%\net452)
if not exist %LayoutForSigningDirectory%\netcoreapp2.0 (md %LayoutForSigningDirectory%\netcoreapp2.0)
if not exist %LayoutForSigningDirectory%\netstandard2.0 (md %LayoutForSigningDirectory%\netstandard2.0)

call :CopyFilesForMultitargeting Sarif.dll            || goto :ExitFailed
call :CopyFilesForMultitargeting Sarif.Converters.dll || goto :ExitFailed
call :CopyFilesForMultitargeting Sarif.Driver.dll     || goto :ExitFailed
call :CopyFilesForMultitargeting Sarif.Multitool.exe  || goto :ExitFailed

:: Copy viewer dll to net452
xcopy /Y %BinaryOutputDirectory%\..\Sarif.Viewer.VisualStudio\%Platform%_%Configuration%\Microsoft.Sarif.Viewer.dll %LayoutForSigningDirectory%\net452

goto :Exit

:CopyFilesForMultitargeting
xcopy /Y %BinaryOutputDirectory%\net452\%1 %LayoutForSigningDirectory%\net452

:: For .NET core, .exes are renamed to .dlls due to packaging conventions
xcopy /Y %BinaryOutputDirectory%\netcoreapp2.0\%~n1.dll  %LayoutForSigningDirectory%\netcoreapp2.0 
xcopy /Y %BinaryOutputDirectory%\netstandard2.0\%~n1.dll %LayoutForSigningDirectory%\netstandard2.0

if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed.)
Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Create layout directory step failed.
exit /b 1

:Exit