// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class TestAnalyzeCommand : AnalyzeCommandBase<TestAnalysisContext, TestAnalyzeOptions>
    {
        public override IEnumerable<Assembly> DefaultPlugInAssemblies { get; set; }

        public override string Prerelease {  get { return ""; } }

        protected override TestAnalysisContext CreateContext(TestAnalyzeOptions options, IResultLogger logger, PropertyBag policy, string filePath = null)
        {
            var context = base.CreateContext(options, logger, policy, filePath);
            context.IsValidAnalysisTarget = options.RegardAnalysisTargetAsValid;
            return context;
        }
    }
}
