@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION
SET SampleFolderPath=C:\Download\Demo\V2

PUSHD "%~dp0"

FOR /F "delims=" %%C IN ('dir /b %SampleFolderPath%\Inputs\*.sarif') DO (
  bin\Release\netcoreapp3.1\BSOA.Demo.exe loadandsave "%SampleFolderPath%\Inputs\%%C" "%SampleFolderPath%\Out\%%~nC.bsoa" 1
)

ECHO.
POPD
