// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AnalyzeTestCommand : MultithreadedAnalyzeCommandBase<AnalyzeTestContext, AnalyzeTestOptions>
    {
        public override IEnumerable<Assembly> DefaultPlugInAssemblies 
        { 
            get => new Assembly[] { this.GetType().Assembly }; 
            set => base.DefaultPlugInAssemblies = value; 
        }
    }
}
