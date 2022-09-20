// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class TestMultithreadedAnalyzeCommand : MultithreadedAnalyzeCommandBase<TestAnalysisContext, TestAnalyzeOptions>, ITestAnalyzeCommand
    {
        public TestMultithreadedAnalyzeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
        }

        public override IEnumerable<Assembly> DefaultPluginAssemblies { get; set; }

        protected override TestAnalysisContext CreateContext(
            TestAnalyzeOptions options,
            IAnalysisLogger logger,
            RuntimeConditions runtimeErrors,
            PropertiesDictionary policy = null,
            string filePath = null)
        {
            TestAnalysisContext context = base.CreateContext(options, logger, runtimeErrors, policy, filePath);

            // TODO: investigate discrepancy in logging for existing tests when
            // using an `AggregatingLogger` as opposed to the default.
            // Debugging: Uncomment this code segument to use the `AggregatingLogger`
            // which logs content relevant to and to provoke concurrency errors.
            // This type of logger is not compatible with some existing tests.
            var aggregatingLogger = context.Logger as AggregatingLogger;

            if (aggregatingLogger != null)
            {
                aggregatingLogger.Loggers.Add(new TestMessageLogger());
            }

            if (context.Policy == null)
            {
                context.Policy = new PropertiesDictionary();
                context.Policy.SetProperty(TestRule.Behaviors, options.TestRuleBehaviors.AccessibleWithinContextOnly());
            }

            TestRuleBehaviors behaviors = context.Policy.GetProperty(TestRule.Behaviors);

            context.IsValidAnalysisTarget = !behaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsInvalid);

            context.TargetLoadException =
                behaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsCorrupted)
               ? new InvalidOperationException()
               : null;

            context.Options = options;

            return context;
        }

        protected override void ValidateOptions(TestAnalyzeOptions options, TestAnalysisContext context)
        {
            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RegardOptionsAsInvalid))
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }

            base.ValidateOptions(options, context);
        }

        public int Run(AnalyzeOptionsBase options)
        {
            int result = base.Run((TestAnalyzeOptions)options);
            this._rootContext?.Disposed.Should().BeTrue();
            return result;
        }

        protected override TestAnalysisContext DetermineApplicabilityAndAnalyze(TestAnalysisContext context, IEnumerable<Skimmer<TestAnalysisContext>> skimmers, ISet<string> disabledSkimmers)
        {
            TestRuleBehaviors behaviors = context.Policy.GetProperty(TestRule.Behaviors);

            TestRule.s_testRuleBehaviors = behaviors.AccessibleOutsideOfContextOnly();

            return base.DetermineApplicabilityAndAnalyze(context, skimmers, disabledSkimmers);
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
    }
}
