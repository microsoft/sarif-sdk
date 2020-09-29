﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExportValidationDocumentationCommand : ExportRulesDocumentationCommandBase<SarifValidationContext>
    {
        public override IEnumerable<Assembly> DefaultPlugInAssemblies => new Assembly[] {
            this.GetType().Assembly
        };
    }
}
