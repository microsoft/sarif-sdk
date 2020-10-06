@ECHO OFF
SET SamplePath=C:\Download\Customer\Quick
SET Old="%~dp0\Legacy\bin\Release\netcoreapp3.1\BSOA.Demo.Legacy.exe"
SET New="%~dp0\BSOA\bin\Release\netcoreapp3.1\BSOA.Demo.BSOA.exe"

%Old% benchmarks %SamplePath%
%New% benchmarks %SamplePath%\Out