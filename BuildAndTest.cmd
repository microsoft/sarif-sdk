@ECHO false
SETLOCAL
@REM Uncomment this line to update nuget.exe
@REM Doing so can break SLN build (which uses nuget.exe to
@REM create a nuget package for binskim) so must opt-in
@REM %~dp0.nuget\NuGet.exe update -self

set MAJOR=1
set MINOR=4
set PATCH=8
set PRERELEASE=-beta

set VERSION_CONSTANTS=src\StaticAnalysisResultsInterchangeFormat\VersionConstants.cs

rd /s /q bld

@REM Rewrite VersionConstants.cs
echo // Copyright (c) Microsoft. All rights reserved. Licensed under the MIT         > %VERSION_CONSTANTS%
echo // license. See LICENSE file in the project root for full license information. >> %VERSION_CONSTANTS%
echo namespace Microsoft.CodeAnalysis.Sarif                                         >> %VERSION_CONSTANTS%
echo {                                                                              >> %VERSION_CONSTANTS%
echo     public static class VersionConstants                                       >> %VERSION_CONSTANTS%
echo     {                                                                          >> %VERSION_CONSTANTS%
echo         public const string Prerelease = "%PRERELEASE%";                       >> %VERSION_CONSTANTS%
echo         public const string AssemblyVersion = "%MAJOR%.%MINOR%.%PATCH%";       >> %VERSION_CONSTANTS%
echo         public const string FileVersion = AssemblyVersion + ".0";              >> %VERSION_CONSTANTS%
echo         public const string Version = AssemblyVersion + Prerelease;            >> %VERSION_CONSTANTS%
echo     }                                                                          >> %VERSION_CONSTANTS%
echo  }                                                                             >> %VERSION_CONSTANTS%

%~dp0.nuget\NuGet.exe restore src\Sarif.Sdk.sln 
msbuild /verbosity:minimal /target:rebuild src\Sarif.Sdk.sln /p:Configuration=Release 

md bld\bin\nuget

.nuget\NuGet.exe pack .\src\Nuget\Sarif.Sdk.nuspec -Symbols -Properties id=Sarif.Sdk;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\StaticAnalysisResultsInterchangeFormat\AnyCPU_Release -OutputDirectory .\bld\bin\Nuget

SET PASSED=true

mstest /testContainer:bld\bin\StaticAnalysisResultsInterchangeFormat.Test.Functional\AnyCPU_Release\StaticAnalysisResultsInterchangeFormat.Test.Functional.dll
if "%ERRORLEVEL%" NEQ "0" (
set PASSED=false
)

mstest /testContainer:bld\bin\StaticAnalysisResultsInterchangeFormat.Test.Unit\AnyCPU_Release\StaticAnalysisResultsInterchangeFormat.Test.Unit.dll
if "%ERRORLEVEL%" NEQ "0" (
set PASSED=false
)

if "%PASSED%" NEQ "true" (
@echo
@echo Tests FAILED.
)