@ECHO OFF
SET SampleFolderPath=C:\Download\Demo\V2
SET SampleFilePath=%SampleFolderPath%\Inputs\CodeAsData.50M.sarif
SET SampleOutBsoaPath=%SampleFolderPath%\Out\CodeAsData.50M.bsoa
SET SampleOutJsonPath=%SampleFolderPath%\Out\CodeAsData.50M.sarif

CLS
PUSHD "%~dp0"

bin\SDK_v2.3.0\BSOA.Demo.exe load "%SampleFilePath%"

bin\SDK_v2.3.1\BSOA.Demo.exe load "%SampleFilePath%"

bin\Release\netcoreapp3.1\BSOA.Demo.exe loadandsave "%SampleFilePath%" "%SampleOutBsoaPath%" 1

bin\Release\netcoreapp3.1\BSOA.Demo.exe load "%SampleOutBsoaPath%" "%SampleOutJsonPath%" 10

::bin\Release\netcoreapp3.1\BSOA.Demo.exe diagnostics "%SampleOutBsoaPath%"

ECHO.
POPD
