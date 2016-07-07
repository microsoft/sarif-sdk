::Convert files to Sarif
@ECHO off
SETLOCAL

dir 
dir src\
dir Microsoft.CodeAnalysis.Analyzers\

where /R c:\ *.pftlog
cl /? | findstr analyze
where /R c:\ *.sarif

@REMSarif.Sdk.1.5.25\tools\ConvertToSarif\ConvertToSarif.exe src\PREfastXmlSarifConverter\prefastresult.pftlog -t PREfast -p true -f true -o prefastresult.sarif

dir 
dir src\
dir bld\

goto Exit

:ExitFailed
@echo.
@echo script %~n0 failed
exit /b 1

:Exit