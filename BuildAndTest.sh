#!/bin/bash
#ECHO off
#SETLOCAL
# Uncomment this line to update nuget.exe
# Doing so can break SLN build (which uses nuget.exe to
# create a nuget package for the SARIF SDK) so must opt-in
# %~dp0.nuget/NuGet.exe update -self

Platform="Any CPU"
Configuration="Release"

Exit ()
{
    echo
    echo script failed
    return 1
}

CheckArgs () {
    if [[ $1 == "" ]]; then
        return
    elif [[ $1 == "/config" ]]; then
        if [[ $2 != "Debug" ]] && [[ $2 != "Release" ]]; then
            echo error: /config must be either Debug or Release
            return 1
        else
            set Configuration=$2
            shift 2
            NextArg
            return $?
        fi
    else
        echo Unrecognized option $1
        return 1
    fi
}

CheckArgs
if [[ $? != 0 ]]; then Exit; return $?; fi;

# Remove existing build data
rm -r bld
#rm -rf bld
mkdir -p bld/bin/nuget

source ./SetCurrentVersion.sh

# Write VersionConstants files 
SDK_VERSION_CONSTANTS="./src/Sarif/VersionConstants.cs"
DRV_VERSION_CONSTANTS="./src/Sarif.Driver/VersionConstants.cs"

# Rewrite VersionConstants.cs
echo "// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT"          >  $SDK_VERSION_CONSTANTS
echo "// license. See LICENSE file in the project root for full license information."   >> $SDK_VERSION_CONSTANTS
echo "namespace Microsoft.CodeAnalysis.Sarif"                                           >> $SDK_VERSION_CONSTANTS
echo "{"                                                                                >> $SDK_VERSION_CONSTANTS
echo "    public static class VersionConstants"                                         >> $SDK_VERSION_CONSTANTS
echo "    {"                                                                            >> $SDK_VERSION_CONSTANTS
echo "        public const string Prerelease = \"$PRERELEASE\";"                        >> $SDK_VERSION_CONSTANTS
echo "        public const string AssemblyVersion = \"$MAJOR.$MINOR.$PATCH\";"          >> $SDK_VERSION_CONSTANTS
echo "        public const string FileVersion = \"$MAJOR.$MINOR.$PATCH\" + \".0\";"     >> $SDK_VERSION_CONSTANTS
echo "        public const string Version = AssemblyVersion + Prerelease;"              >> $SDK_VERSION_CONSTANTS
echo "    }"                                                                            >> $SDK_VERSION_CONSTANTS
echo "}"                                                                                >> $SDK_VERSION_CONSTANTS

# Rewrite VersionConstants.cs
echo "// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT"          >  $DRV_VERSION_CONSTANTS
echo "// license. See LICENSE file in the project root for full license information."   >> $DRV_VERSION_CONSTANTS
echo "namespace Microsoft.CodeAnalysis.Sarif.Driver"                                    >> $DRV_VERSION_CONSTANTS
echo "{"                                                                                >> $DRV_VERSION_CONSTANTS
echo "    public static class VersionConstants"                                         >> $DRV_VERSION_CONSTANTS
echo "    {"                                                                            >> $DRV_VERSION_CONSTANTS
echo "        public const string Prerelease = \"$PRERELEASE\";"                        >> $DRV_VERSION_CONSTANTS
echo "        public const string AssemblyVersion = \"$MAJOR.$MINOR.$PATCH\";"          >> $DRV_VERSION_CONSTANTS
echo '        public const string FileVersion = AssemblyVersion + ".0";'                >> $DRV_VERSION_CONSTANTS
echo "        public const string Version = AssemblyVersion + Prerelease;"              >> $DRV_VERSION_CONSTANTS
echo "    }"                                                                            >> $DRV_VERSION_CONSTANTS
echo "}"                                                                                >> $DRV_VERSION_CONSTANTS

source ./BeforeBuild.sh
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet build src/Sarif.Sdk.sln -f netcoreapp2.0
if [ $? != 0 ]; then Exit; return $?; fi;

# Build Nuget packages
.nuget/NuGet.exe pack ./src/Nuget/Sarif.Sdk.nuspec -Symbols -Properties id=Sarif.Sdk;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE%;configuration=%Configuration% -Verbosity Quiet -BasePath ./bld/bin/Sarif/AnyCPU_%Configuration% -OutputDirectory ./bld/bin/Nuget
.nuget/NuGet.exe pack ./src/Nuget/Sarif.Driver.nuspec -Symbols -Properties id=Sarif.Driver;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE%;configuration=%Configuration% -Verbosity Quiet -BasePath ./bld/bin/Sarif.Driver/AnyCPU_%Configuration%/ -OutputDirectory ./bld/bin/Nuget
if [ $? != 0 ]; then Exit; return $?; fi;

# Run all tests
dotnet xunit bld/bin/Sarif.Converters.UnitTests/AnyCPU_%Configuration%/Sarif.Converters.UnitTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/Sarif.UnitTests/AnyCPU_%Configuration%/Sarif.UnitTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/Sarif.UnitTests/AnyCPU_%Configuration%/Sarif.UnitTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/Sarif.Driver.UnitTests/AnyCPU_%Configuration%/Sarif.Driver.UnitTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/Sarif.FunctionalTests/AnyCPU_%Configuration%/Sarif.FunctionalTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/Sarif.ValidationTests/AnyCPU_%Configuration%/Sarif.ValidationTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/Sarif.Viewer.VisualStudio.UnitTests/AnyCPU_%Configuration%/Sarif.Viewer.VisualStudio.UnitTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

dotnet xunit bld/bin/SarifCli.FunctionalTests/AnyCPU_%Configuration%/SarifCli.FunctionalTests.dll
if [ $? != 0 ]; then Exit; return $?; fi;

echo script passed