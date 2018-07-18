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
