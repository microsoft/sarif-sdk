// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
#pragma warning disable CS0618
    public class TestAnalyzeCommand : AnalyzeCommandBase<TestAnalysisContext, TestAnalyzeOptions>, ITestAnalyzeCommand
#pragma warning restore CS0618
    {
        public TestAnalyzeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
        }

        public override IEnumerable<Assembly> DefaultPluginAssemblies { get; set; }

        public int Run(AnalyzeOptionsBase options)
        {
            return base.Run((TestAnalyzeOptions)options);
        }

        protected override TestAnalysisContext CreateContext(
            TestAnalyzeOptions options,
            IAnalysisLogger logger,
            RuntimeConditions runtimeErrors,
            PropertiesDictionary policy = null,
            string filePath = null)
        {
            TestAnalysisContext context = base.CreateContext(options, logger, runtimeErrors, policy, filePath);
            context.Policy.SetProperty(TestRule.Behaviors, options.TestRuleBehaviors);

            context.IsValidAnalysisTarget =
                !context.Policy
                .GetProperty(TestRule.Behaviors)
                .HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsInvalid);

            if (options.TestRuleBehaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsCorrupted))
            {
                context.TargetLoadException = new InvalidOperationException();
            }

            context.Options = options;
            return context;
        }

        protected override void ValidateOptions(TestAnalysisContext context, TestAnalyzeOptions options)
        {
            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RegardOptionsAsInvalid))
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }

            base.ValidateOptions(context, options);
        }

        protected override void ProcessBaseline(IAnalysisContext context, TestAnalyzeOptions options, IFileSystem fileSystem)
        {
            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RaiseExceptionProcessingBaseline))
            {
                context.RuntimeErrors |= RuntimeConditions.ExceptionProcessingBaseline;
                ThrowExitApplicationException((TestAnalysisContext)context, ExitReason.ExceptionProcessingBaseline);
            }

            base.ProcessBaseline(context, options, fileSystem);
        }

        public override int Run(TestAnalyzeOptions options)
        {
            int result = base.Run(options);
            this._rootContext?.Disposed.Should().BeTrue();
            return result;
        }

        public new void CheckIncompatibleRules(IEnumerable<Skimmer<TestAnalysisContext>> skimmers, TestAnalysisContext context, ISet<string> disabledSkimmers)
        {
            base.CheckIncompatibleRules(skimmers, context, disabledSkimmers);
        }
    }
}
