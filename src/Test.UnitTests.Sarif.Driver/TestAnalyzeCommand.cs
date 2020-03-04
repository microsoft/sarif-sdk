// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class TestAnalyzeCommand : AnalyzeCommandBase<TestAnalysisContext, TestAnalyzeOptions>
    {
        public TestAnalyzeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
        }

        public override IEnumerable<Assembly> DefaultPlugInAssemblies { get; set; }

        protected override TestAnalysisContext CreateContext(
            TestAnalyzeOptions options,
            IAnalysisLogger logger,
            RuntimeConditions runtimeErrors,
            string filePath = null)
        {
            TestAnalysisContext context = base.CreateContext(options, logger, runtimeErrors, filePath);
            context.IsValidAnalysisTarget = options.RegardAnalysisTargetAsValid;
            context.TargetLoadException = options.RegardAnalysisTargetAsCorrupted ? new InvalidOperationException() : null;
            context.Options = options;
            return context;
        }

        protected override void ValidateOptions(TestAnalysisContext context, TestAnalyzeOptions options)
        {
            if (options.RegardOptionsAsInvalid)
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }

            base.ValidateOptions(context, options);
        }
    }
}
