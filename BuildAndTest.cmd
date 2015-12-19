@ECHO off
SETLOCAL
@REM Uncomment this line to update nuget.exe
@REM Doing so can break SLN build (which uses nuget.exe to
@REM create a nuget package for binskim) so must opt-in
@REM %~dp0.nuget\NuGet.exe update -self

@REM Remove existing build data
rd /s /q bld
md bld\bin\nuget

@REM Set versions for SDK and Driver
set SDK_MAJOR=1
set SDK_MINOR=4
set SDK_PATCH=13

set DRV_MAJOR=1
set DRV_MINOR=0
set DRV_PATCH=13

set PRERELEASE=-beta

@REM Write VersionConstants files 

set SDK_VERSION_CONSTANTS=src\Sarif\VersionConstants.cs
set DRV_VERSION_CONSTANTS=src\Sarif.Driver\VersionConstants.cs

@REM Rewrite VersionConstants.cs
echo // Copyright (c) Microsoft. All rights reserved. Licensed under the MIT              >  %SDK_VERSION_CONSTANTS%
echo // license. See LICENSE file in the project root for full license information.       >> %SDK_VERSION_CONSTANTS%
echo namespace Microsoft.CodeAnalysis.Sarif                                               >> %SDK_VERSION_CONSTANTS%
echo {                                                                                    >> %SDK_VERSION_CONSTANTS%
echo     public static class VersionConstants                                             >> %SDK_VERSION_CONSTANTS%
echo     {                                                                                >> %SDK_VERSION_CONSTANTS%
echo         public const string Prerelease = "%PRERELEASE%";                             >> %SDK_VERSION_CONSTANTS%
echo         public const string AssemblyVersion = "%SDK_MAJOR%.%SDK_MINOR%.%SDK_PATCH%"; >> %SDK_VERSION_CONSTANTS%
echo         public const string FileVersion = AssemblyVersion + ".0";                    >> %SDK_VERSION_CONSTANTS%
echo         public const string Version = AssemblyVersion + Prerelease;                  >> %SDK_VERSION_CONSTANTS%
echo     }                                                                                >> %SDK_VERSION_CONSTANTS%
echo  }                                                                                   >> %SDK_VERSION_CONSTANTS%

@REM Rewrite VersionConstants.cs
echo // Copyright (c) Microsoft. All rights reserved. Licensed under the MIT               > %DRV_VERSION_CONSTANTS%
echo // license. See LICENSE file in the project root for full license information.       >> %DRV_VERSION_CONSTANTS%
echo namespace Microsoft.CodeAnalysis.Sarif.Driver                                        >> %DRV_VERSION_CONSTANTS%
echo {                                                                                    >> %DRV_VERSION_CONSTANTS%
echo     public static class VersionConstants                                             >> %DRV_VERSION_CONSTANTS%
echo     {                                                                                >> %DRV_VERSION_CONSTANTS%
echo         public const string Prerelease = "%PRERELEASE%";                             >> %DRV_VERSION_CONSTANTS%
echo         public const string AssemblyVersion = "%DRV_MAJOR%.%DRV_MINOR%.%DRV_PATCH%"; >> %DRV_VERSION_CONSTANTS%
echo         public const string FileVersion = AssemblyVersion + ".0";                    >> %DRV_VERSION_CONSTANTS%
echo         public const string Version = AssemblyVersion + Prerelease;                  >> %DRV_VERSION_CONSTANTS%
echo     }                                                                                >> %DRV_VERSION_CONSTANTS%
echo  }                                                                                   >> %DRV_VERSION_CONSTANTS%

@REM Build all code
%~dp0.nuget\NuGet.exe restore src\Sarif.Sdk.sln 
msbuild /verbosity:minimal /target:rebuild src\Sarif.Sdk.sln /p:Configuration=Release 

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Build Nuget packages
.nuget\NuGet.exe pack .\src\Nuget\Sarif.Sdk.nuspec -Symbols -Properties id=Sarif.Sdk;major=%SDK_MAJOR%;minor=%SDK_MINOR%;patch=%SDK_PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\Sarif\AnyCPU_Release -OutputDirectory .\bld\bin\Nuget
.nuget\NuGet.exe pack .\src\Nuget\Sarif.Driver.nuspec -Symbols -Properties id=Sarif.Driver;major=%DRV_MAJOR%;minor=%DRV_MINOR%;patch=%DRV_PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\Sarif.Driver\AnyCPU_Release\ -OutputDirectory .\bld\bin\Nuget

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Run all tests
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

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.Driver.UnitTests\AnyCPU_Release\Sarif.Driver.UnitTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo  
@echo SCRIPT FAILED

:Exit


