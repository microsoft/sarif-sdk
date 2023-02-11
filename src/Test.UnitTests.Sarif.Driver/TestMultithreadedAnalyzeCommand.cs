// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class TestMultithreadedAnalyzeCommand : MultithreadedAnalyzeCommandBase<TestAnalysisContext, TestAnalyzeOptions>, ITestAnalyzeCommand
    {
        public TestMultithreadedAnalyzeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
        }

        public override IEnumerable<Assembly> DefaultPluginAssemblies { get; set; }

        public override TestAnalysisContext InitializeContextFromOptions(TestAnalyzeOptions options, ref TestAnalysisContext context)
        {
            context ??= new TestAnalysisContext();
            context.Policy ??= new PropertiesDictionary();
            context.Policy.SetProperty(TestRule.Behaviors, options.TestRuleBehaviors.AccessibleWithinContextOnly());

            context = base.InitializeContextFromOptions(options, ref context);

            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RegardOptionsAsInvalid))
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
                ThrowExitApplicationException(ExitReason.InvalidCommandLineOption);
            }

            return context;
        }

        protected override TestAnalysisContext CreateContext(TestAnalyzeOptions options, IAnalysisLogger logger, RuntimeConditions runtimeErrors, IFileSystem fileSystem = null, PropertiesDictionary policy = null)
        {
            TestAnalysisContext context = base.CreateContext(options, logger, runtimeErrors, fileSystem, policy);
            TestRuleBehaviors behaviors = context.Policy.GetProperty(TestRule.Behaviors);
            context.IsValidAnalysisTarget = !behaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsInvalid);

            context.RuntimeException =
                behaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsCorrupted)
               ? new InvalidOperationException()
               : null;

            return context;
        }

        protected override void ValidateOptions(TestAnalyzeOptions options, TestAnalysisContext context)
        {
            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RegardOptionsAsInvalid))
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
                ThrowExitApplicationException(ExitReason.InvalidCommandLineOption);
            }

            base.ValidateOptions(options, context);
        }

        public int Run(AnalyzeOptionsBase options, ref TestAnalysisContext context)
        {
            int result = base.Run((TestAnalyzeOptions)options, ref context);
            context.Should().NotBeNull();
            context.Disposed.Should().BeTrue();
            return result;
        }

        protected override TestAnalysisContext DetermineApplicabilityAndAnalyze(TestAnalysisContext context, IEnumerable<Skimmer<TestAnalysisContext>> skimmers, ISet<string> disabledSkimmers)
        {
            TestRuleBehaviors behaviors = context.Policy.GetProperty(TestRule.Behaviors);

            TestRule.s_testRuleBehaviors = behaviors.AccessibleOutsideOfContextOnly();

            return base.DetermineApplicabilityAndAnalyze(context, skimmers, disabledSkimmers);
        }

        protected override void ProcessBaseline(IAnalysisContext context)
        {
            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RaiseExceptionProcessingBaseline))
            {
                context.RuntimeErrors |= RuntimeConditions.ExceptionProcessingBaseline;
                ThrowExitApplicationException(ExitReason.ExceptionProcessingBaseline);
            }

            base.ProcessBaseline(context);
        }

        public new void CheckIncompatibleRules(IEnumerable<Skimmer<TestAnalysisContext>> skimmers, TestAnalysisContext context, ISet<string> disabledSkimmers)
        {
            base.CheckIncompatibleRules(skimmers, context, disabledSkimmers);
        }
    }
}
