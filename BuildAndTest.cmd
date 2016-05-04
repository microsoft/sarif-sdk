@ECHO off
SETLOCAL
@REM Uncomment this line to update nuget.exe
@REM Doing so can break SLN build (which uses nuget.exe to
@REM create a nuget package for binskim) so must opt-in
@REM %~dp0.nuget\NuGet.exe update -self

@REM Remove existing build data
rd /s /q bld
md bld\bin\nuget

call SetCurrentVersion.cmd 

set Platform=AnyCPU
set Configuration=Release

@REM Write VersionConstants files 

set SDK_VERSION_CONSTANTS=src\Sarif\VersionConstants.cs
set DRV_VERSION_CONSTANTS=src\Sarif.Driver\VersionConstants.cs

@REM Rewrite VersionConstants.cs
echo // Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        >  %SDK_VERSION_CONSTANTS%
echo // license. See LICENSE file in the project root for full license information. >> %SDK_VERSION_CONSTANTS%
echo namespace Microsoft.CodeAnalysis.Sarif                                         >> %SDK_VERSION_CONSTANTS%
echo {                                                                              >> %SDK_VERSION_CONSTANTS%
echo     public static class VersionConstants                                       >> %SDK_VERSION_CONSTANTS%
echo     {                                                                          >> %SDK_VERSION_CONSTANTS%
echo         public const string Prerelease = "%PRERELEASE%";                       >> %SDK_VERSION_CONSTANTS%
echo         public const string AssemblyVersion = "%MAJOR%.%MINOR%.%PATCH%";       >> %SDK_VERSION_CONSTANTS%
echo         public const string FileVersion = "%MAJOR%.%MINOR%.%PATCH%" + ".0";    >> %SDK_VERSION_CONSTANTS%
echo         public const string Version = AssemblyVersion + Prerelease;            >> %SDK_VERSION_CONSTANTS%
echo     }                                                                          >> %SDK_VERSION_CONSTANTS%
echo  }                                                                             >> %SDK_VERSION_CONSTANTS%

@REM Rewrite VersionConstants.cs
echo // Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        >  %DRV_VERSION_CONSTANTS%
echo // license. See LICENSE file in the project root for full license information. >> %DRV_VERSION_CONSTANTS%
echo namespace Microsoft.CodeAnalysis.Sarif.Driver                                  >> %DRV_VERSION_CONSTANTS%
echo {                                                                              >> %DRV_VERSION_CONSTANTS%
echo     public static class VersionConstants                                       >> %DRV_VERSION_CONSTANTS%
echo     {                                                                          >> %DRV_VERSION_CONSTANTS%
echo         public const string Prerelease = "%PRERELEASE%";                       >> %DRV_VERSION_CONSTANTS%
echo         public const string AssemblyVersion = "%MAJOR%.%MINOR%.%PATCH%";       >> %DRV_VERSION_CONSTANTS%
echo         public const string FileVersion = AssemblyVersion + ".0";              >> %DRV_VERSION_CONSTANTS%
echo         public const string Version = AssemblyVersion + Prerelease;            >> %DRV_VERSION_CONSTANTS%
echo     }                                                                          >> %DRV_VERSION_CONSTANTS%
echo  }                                                                             >> %DRV_VERSION_CONSTANTS%

if NOT exist "GeneratedKey.snk" (
sn -k GeneratedKey.snk
)

@REM Build all code
%~dp0.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

msbuild /verbosity:minimal /target:rebuild src\Everything.sln /p:"Configuration=Release" /p:"Platform=Any CPU" /filelogger /fileloggerparameters:Verbosity=detailed

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Build Nuget packages
.nuget\NuGet.exe pack .\src\Nuget\Sarif.Sdk.nuspec -Symbols -Properties id=Sarif.Sdk;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\Sarif\AnyCPU_Release -OutputDirectory .\bld\bin\Nuget
.nuget\NuGet.exe pack .\src\Nuget\Sarif.Driver.nuspec -Symbols -Properties id=Sarif.Driver;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% -Verbosity Quiet -BasePath .\bld\bin\Sarif.Driver\AnyCPU_Release\ -OutputDirectory .\bld\bin\Nuget

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Run all tests
SET PASSED=true

mstest /testContainer:bld\bin\Sarif.UnitTests\AnyCPU_Release\Sarif.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
set PASSED=false
)

if "%PASSED%" NEQ "true" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.Driver.UnitTests\AnyCPU_Release\Sarif.Driver.UnitTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe bld\bin\Sarif.FunctionalTests\AnyCPU_Release\Sarif.FunctionalTests.dll

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo SCRIPT FAILED

:Exit


