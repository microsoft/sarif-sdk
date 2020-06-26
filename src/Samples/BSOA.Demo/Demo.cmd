@ECHO OFF
SET SampleFileName=CodeAsData.50M
IF NOT "%1"=="" (SET SampleFileName=%1)

SET SampleFolderPath=C:\Download\Demo\V2
SET SampleFilePath=%SampleFolderPath%\Inputs\%SampleFileName%.sarif
SET SampleOutBsoaPath=%SampleFolderPath%\Out\%SampleFileName%.bsoa
SET SampleOutJsonPath=%SampleFolderPath%\Out\%SampleFileName%.sarif

CLS
PUSHD "%~dp0"

SDK_v2.3.0\BSOA.Demo.exe load "%SampleFilePath%"
::bin\SDK_v2.3.1\BSOA.Demo.exe load "%SampleFilePath%"

bin\Release\netcoreapp3.1\BSOA.Demo.exe loadandsave "%SampleFilePath%" "%SampleOutBsoaPath%" 4

bin\Release\netcoreapp3.1\BSOA.Demo.exe load "%SampleOutBsoaPath%" "%SampleOutJsonPath%" 10

::bin\Release\netcoreapp3.1\BSOA.Demo.exe diagnostics "%SampleOutBsoaPath%"

ECHO.
POPD
