@echo off
SETLOCAL

set Root=%1

if "%Root%" EQU "" (
set Root=e:
)

::Refresh test files from a local BinSkim enlistment
echo Copying BinSkim SARIF v2 files

for %%i in (*.sarif) do call :CopyFile %%i || goto :Exit

goto :Exit

:CopyFile
set File=%root%\src\binskim\src\Test.FunctionalTests.BinSkim.Driver\BaselineTestsData\Expected\%1
copy %file% .

if "%ERRORLEVEL%" NEQ "0" (echo %File% copy FAILED.)
Exit /B %ERRORLEVEL%

:Exit