// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Sarif;

[assembly: AssemblyTitle("Static Analysis Results Interchange Format Library")]
[assembly: InternalsVisibleTo("Sarif.UnitTests")]
[assembly: InternalsVisibleTo("Sarif.FunctionalTests")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: AssemblyVersion(VersionConstants.FileVersion)]
[assembly: AssemblyFileVersion(VersionConstants.FileVersion)]
