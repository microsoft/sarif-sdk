@ECHO OFF
SETLOCAL

SET SampleFolderPath=C:\Download\Demo\V2
SET SampleFileName=CodeAsData
IF NOT "%1"=="" (SET SampleFileName=%1)

SET SarifToCsv="%~dp0..\SarifToCsv\bin\Release\netcoreapp3.1\SarifToCsv.exe"
SET SarifToCsv.Old="%~dp0SarifToCsv_SDK_v2.0\SarifToCsv.exe"

SET SarifMultitool="%~dp0..\..\..\bld\bin\AnyCPU_Release\Sarif.Multitool\netcoreapp3.0\Sarif.Multitool.exe"
SET SarifMultitool.Old="%~dp0SarifMultitool_SDK_v2.3.0\Sarif.Multitool.exe"

CALL :setESC

ECHO.
ECHO %ESC%[31mOLD: SarifToCsv %SampleFolderPath%\Inputs\%SampleFileName%.sarif %ESC%[0m
%SarifToCsv.Old% %SampleFolderPath%\Inputs\%SampleFileName%.sarif

ECHO.
ECHO %ESC%[92mNEW: SarifToCsv %SampleFolderPath%\Out\%SampleFileName%.bsoa %ESC%[0m
%SarifToCsv% %SampleFolderPath%\Out\%SampleFileName%.bsoa

ECHO.
ECHO %ESC%[31mOLD: Sarif.Multitool query %SampleFolderPath%\Inputs\%SampleFileName%.sarif -e "ruleId : 433" %ESC%[0m
%SarifMultitool.Old% query %SampleFolderPath%\Inputs\%SampleFileName%.sarif -e "ruleId : 433"

ECHO.
ECHO %ESC%[92mNEW: Sarif.Multitool query %SampleFolderPath%\Out\%SampleFileName%.bsoa -e "ruleId : 433" %ESC%[0m
%SarifMultitool% query %SampleFolderPath%\Out\%SampleFileName%.bsoa -e "ruleId : 433"

ECHO %ESC%[0m

:setESC
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
  set ESC=%%b
  exit /B 0
)
exit /B 0