@ECHO off
SETLOCAL
@REM Uncomment this line to update nuget.exe
@REM Doing so can break SLN build (which uses nuget.exe to
@REM create a nuget package for the SARIF SDK) so must opt-in
@REM %~dp0.nuget\NuGet.exe update -self

set Platform=Any CPU
set Configuration=Release

:NextArg
if "%1" == "" goto :EndArgs
if "%1" == "/config" (
    if not "%2" == "Debug" if not "%2" == "Release" echo error: /config must be either Debug or Release && goto :ExitFailed
    set Configuration=%2&& shift && shift && goto :NextArg
)
echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

@REM Remove existing build data
rd /s /q bld
md bld\bin\nuget

call SetCurrentVersion.cmd 

@REM Write VersionConstants files 

set SDK_VERSION_CONSTANTS=src\Sarif\VersionConstants.cs
set DRV_VERSION_CONSTANTS=src\Sarif.Driver\VersionConstants.cs
set Version=%MAJOR%.%MINOR%.%PATCH%

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

call BeforeBuild.cmd

if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

msbuild /verbosity:minimal /target:BuildObjectModel src\Sarif\Sarif.csproj /fileloggerparameters:Verbosity=detailed
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

msbuild /verbosity:minimal /target:rebuild src\Everything.sln /filelogger /fileloggerparameters:Verbosity=detailed /p:"RunBinSkim=false" /p:"BinSkimVerboseOutput=true"
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

set Platform=AnyCPU

@REM Build Nuget packages
set PackOptions=--configuration %Configuration% --no-build /p:PackageVersion=%Version% /p:Platform=%Platform%

dotnet pack .\src\Sarif\Sarif.csproj %PackOptions%
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

dotnet pack .\src\Sarif.Converters\Sarif.Converters.csproj %PackOptions%
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

dotnet pack .\src\Sarif.Driver\Sarif.Driver.csproj %PackOptions%
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

dotnet pack .\src\Sarif.Multitool\Sarif.Multitool.csproj %PackOptions%
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

@REM Run all tests

pushd .\src\Sarif.Converters.UnitTests && dotnet xunit -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.UnitTests && dotnet xunit -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.Driver.UnitTests && dotnet xunit -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.FunctionalTests && dotnet xunit -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

pushd .\src\Sarif.Multitool.FunctionalTests && dotnet xunit -nobuild -configuration Release && popd
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.3.0\tools\net452\xunit.console.x86.exe bld\bin\Sarif.ValidationTests\AnyCPU_%Configuration%\Sarif.ValidationTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

src\packages\xunit.runner.console.2.3.0\tools\net452\xunit.console.x86.exe bld\bin\Sarif.Viewer.VisualStudio.UnitTests\AnyCPU_%Configuration%\Sarif.Viewer.VisualStudio.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo.
@echo script failed
exit /b 1

:Exit