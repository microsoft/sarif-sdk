@ECHO off
SETLOCAL

if NOT exist "GeneratedKey.snk" (
sn -k GeneratedKey.snk
)

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

if NOT exist "GeneratedKey.snk" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo SCRIPT FAILED
exit /b 1

:Exit