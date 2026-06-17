// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class TestMultithreadedAnalyzeCommand : MultithreadedAnalyzeCommandBase<TestAnalysisContext, TestAnalyzeOptions>, ITestAnalyzeCommand
    {
        private readonly HttpClientWrapper _httpClientWrapper;

        public TestMultithreadedAnalyzeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
            TestRule.s_testRuleBehaviors = 0;
        }

        public TestMultithreadedAnalyzeCommand(IFileSystem fileSystem, HttpClientWrapper httpClientWrapper)
            : base(fileSystem)
        {
            _httpClientWrapper = httpClientWrapper;
            TestRule.s_testRuleBehaviors = 0;
        }

        public override TestAnalysisContext InitializeGlobalContextFromOptions(TestAnalyzeOptions options, ref TestAnalysisContext context)
        {
            context ??= new TestAnalysisContext();
            context.Policy ??= new PropertiesDictionary();

            context = base.InitializeGlobalContextFromOptions(options, ref context);

            if (options.TestRuleBehaviors != null)
            {
                context.Policy.SetProperty(TestRule.Behaviors, options.TestRuleBehaviors.Value);
            }

            if (context.Policy.GetProperty(TestRule.Behaviors).HasFlag(TestRuleBehaviors.RegardOptionsAsInvalid))
            {
                context.RuntimeErrors |= RuntimeConditions.InvalidCommandLineOption;
                ThrowExitApplicationException(ExitReason.InvalidCommandLineOption);
            }

            return context;
        }

        protected override TestAnalysisContext CreateScanTargetContext(TestAnalysisContext globalContext)
        {
            globalContext = base.CreateScanTargetContext(globalContext);
            TestRuleBehaviors behaviors = globalContext.Policy.GetProperty(TestRule.Behaviors);
            globalContext.IsValidAnalysisTarget = !behaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsInvalid);

            globalContext.RuntimeExceptions =
                behaviors.HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsCorrupted)
               ? new List<Exception>(new[] { new InvalidOperationException() })
               : null;

            return globalContext;
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

        protected override ISet<Skimmer<TestAnalysisContext>> CreateSkimmers(TestAnalysisContext context)
        {
            TestRule.s_testRuleBehaviors = context.Policy.GetProperty(TestRule.Behaviors);
            return base.CreateSkimmers(context);
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

            TestRule.s_testRuleBehaviors = behaviors;

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

        protected override HttpClientWrapper GetHttpClientWrapper()
        {
            return _httpClientWrapper ?? base.GetHttpClientWrapper();
        }
    }
}
