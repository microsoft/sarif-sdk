// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class TestExportConfigurationCommand : ExportConfigurationCommandBase
    {
        public override string Prerelease { get { return ""; } }

        public override IEnumerable<Assembly> DefaultPlugInAssemblies { get; set; }
    }
}
