@ECHO OFF
SET SampleFileName=CodeAsData.50M
IF NOT "%1"=="" (SET SampleFileName=%1)

SET SampleFolderPath=C:\Download\Demo\V2
SET SampleFilePath=%SampleFolderPath%\Inputs\%SampleFileName%.sarif
SET SampleOutBsoaPath=%SampleFolderPath%\Out\%SampleFileName%.bsoa
SET SampleOutJsonPath=%SampleFolderPath%\Out\%SampleFileName%.sarif

CLS
PUSHD "%~dp0"

bin\Release\netcoreapp3.1\BSOA.Demo.exe loadandsave "%SampleFilePath%" "%SampleOutBsoaPath%" 1

bin\Release\netcoreapp3.1\BSOA.Demo.exe load "%SampleOutBsoaPath%" "%SampleOutJsonPath%" 5

::bin\Release\netcoreapp3.1\BSOA.Demo.exe diagnostics "%SampleOutBsoaPath%"

ECHO.
POPD
