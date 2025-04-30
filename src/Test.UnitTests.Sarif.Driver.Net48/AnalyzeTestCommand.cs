// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Test.UnitTests.Sarif.Driver.Net48
{
    public class AnalyzeTestCommand : MultithreadedAnalyzeCommandBase<AnalyzeTestContext, AnalyzeTestOptions>
    {
        public override IEnumerable<Assembly> DefaultPluginAssemblies
        {
            get => new Assembly[] { this.GetType().Assembly };
            set => base.DefaultPluginAssemblies = value;
        }
    }
}
