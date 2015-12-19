@ECHO off
SETLOCAL
@REM Uncomment this line to update nuget.exe
@REM Doing so can break SLN build (which uses nuget.exe to
@REM create a nuget package for binskim) so must opt-in
@REM %~dp0.nuget\NuGet.exe update -self

@REM 
@REM SDK BUILD
@REM 
set MAJOR=1
set MINOR=4
set PATCH=10
set PRERELEASE=-beta

set VERSION_CONSTANTS=src\Sarif\VersionConstants.cs

rd /s /q bld
md bld\bin\nuget

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

.nuget\NuGet.exe pack .\src\Nuget\Sarif.Sdk.nuspec -Symbols -Properties id=Sarif.Sdk;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\Sarif\AnyCPU_Release -OutputDirectory .\bld\bin\Nuget

SET PASSED=true

mstest /testContainer:bld\bin\Sarif.FunctionalTests\AnyCPU_Release\Sarif.FunctionalTests.dll
if "%ERRORLEVEL%" NEQ "0" (
set PASSED=false
)

mstest /testContainer:bld\bin\Sarif.UnitTests\AnyCPU_Release\Sarif.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
set PASSED=false
)

if "%PASSED%" NEQ "true" (
goto ExitFailed
)

@REM 
@REM SDK DRIVER BUILD
@REM 

set MAJOR=1
set MINOR=0
set PATCH=9
set PRERELEASE=-beta

set VERSION_CONSTANTS=src\Sarif.Driver\VersionConstants.cs

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

.nuget\NuGet.exe pack .\src\Nuget\Sarif.Driver.nuspec -Symbols -Properties id=Sarif.Driver;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\Sarif.Driver\AnyCPU_Release\ -OutputDirectory .\bld\bin\Nuget

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.Driver.UnitTests\AnyCPU_Release\Sarif.Driver.UnitTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo  
@echo SCRIPT FAILED

:Exit


