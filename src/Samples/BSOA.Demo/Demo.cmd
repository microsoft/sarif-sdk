@ECHO OFF
SET SamplePath=C:\Download\Customer
SET Old="%~dp0\Legacy\bin\Release\netcoreapp3.1\BSOA.Demo.Legacy.exe"
SET New="%~dp0\BSOA\bin\Release\netcoreapp3.1\BSOA.Demo.BSOA.exe"

%Old% load %SamplePath%
%New% load %SamplePath%

%New% load %SamplePath%\Out

%Old% objectmodeloverhead %SamplePath%
%New% objectmodeloverhead %SamplePath%\Out