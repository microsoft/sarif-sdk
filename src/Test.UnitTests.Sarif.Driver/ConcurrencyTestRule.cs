// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This test class exists to provide an additional check that can be disabled via context, etc. It has no configurable behavior.
    /// </summary>
    [Export(typeof(ReportingDescriptor)), Export(typeof(IOptionsProvider)), Export(typeof(Skimmer<TestAnalysisContext>))]
    internal class ConcurrencyTestRule : TestRuleBase, IOptionsProvider
    {
        [ThreadStatic]
        internal static TestRuleBehaviors s_TestRuleBehaviors;

        private const string ConcurrencyTestRuleId = "TEST1009";

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> MessageResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TEST1009_Failed),
            nameof(SkimmerBaseTestResources.TEST1009_Pass),
            nameof(SkimmerBaseTestResources.NotApplicable_InvalidMetadata)
        };

        public override SupportedPlatform SupportedPlatforms
        {
            get
            {
                if (s_TestRuleBehaviors.HasFlag(TestRuleBehaviors.TreatPlatformAsInvalid))
                {
                    return SupportedPlatform.Unknown;
                }
                return SupportedPlatform.All;
            }
        }

        private string _id;
        public override string Id
        {
            get
            {
                if (s_TestRuleBehaviors == TestRuleBehaviors.RaiseExceptionAccessingId)
                {
                    throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionAccessingId));
                }
                return _id ?? ConcurrencyTestRuleId;
            }
            set
            {
                _id = value;
            }
        }

        private string _name;
        public override string Name
        {
            get
            {
                if (s_TestRuleBehaviors == TestRuleBehaviors.RaiseExceptionAccessingName)
                {
                    throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionAccessingId));
                }
                return _name ?? base.Name;
            }
            set
            {
                _name = value;
            }
        }

        public override void Initialize(TestAnalysisContext context)
        {
            if (context.Policy.GetProperty(Behaviors).HasFlag(TestRuleBehaviors.RaiseExceptionInvokingInitialize))
            {
                throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingInitialize));
            }
        }

        public override AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
        {
            AnalysisApplicability applicability = AnalysisApplicability.ApplicableToSpecifiedTarget;
            reasonIfNotApplicable = null;

            if (context.Policy.GetProperty(Behaviors).HasFlag(TestRuleBehaviors.RaiseExceptionInvokingCanAnalyze))
            {
                throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingCanAnalyze));
            }

            if (context.Policy.GetProperty(Behaviors).HasFlag(TestRuleBehaviors.RegardAnalysisTargetAsNotApplicable))
            {
                reasonIfNotApplicable = "testing NotApplicableToSpecifiedTarget";
                return AnalysisApplicability.NotApplicableToSpecifiedTarget;
            }

            return applicability;
        }

        public override MultiformatMessageString FullDescription { get { return new MultiformatMessageString { Text = "This is the full description for TST1001" }; } }

        public override void Analyze(TestAnalysisContext context)
        {
            // We do not access the static test rule behaviors here. We also want to 
            // ensure this data is only set with flags (if any) that are legal for 
            // this property.
            (s_TestRuleBehaviors & s_TestRuleBehaviors.AccessibleOutsideOfContextOnly())
                .Should().Be(s_TestRuleBehaviors);

            // Now we'll make sure the context test rule behaviors are restricted
            // to settings that are legal to pass in a context object.
            TestRuleBehaviors TestRuleBehaviors = context.Policy.GetProperty(Behaviors);
            (TestRuleBehaviors & TestRuleBehaviors.AccessibleWithinContextOnly())
                .Should().Be(TestRuleBehaviors);

            switch (TestRuleBehaviors)
            {
                case TestRuleBehaviors.LogError:
                {
                    uint errorsCount = context.Policy.GetProperty(ErrorsCount);
                    for (int j = 0;  j < 100;  j++ )
                    {
                        for (uint i = 0; i < errorsCount; i++)
                        {
                            context.Logger.Log(this,
                                new Result
                                {
                                    RuleId = this.Id,
                                    Level = FailureLevel.Error,
                                    Message = new Message { Text = "Simple test rule message." }
                                });

                            Thread.Sleep(1);
                        }
                    }
                    break;
                }

                default:
                {
                    break;
                }
            }
        }

        public static void RaiseExceptionViaReflection()
        {
            throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingAnalyze));
        }

        public IEnumerable<IOption> GetOptions()
        {
            return new IOption[] { Behaviors, UnusedOption, ErrorsCount };
        }

        private const string AnalyzerName = ConcurrencyTestRuleId + "." + nameof(ConcurrencyTestRule);

        public static PerLanguageOption<uint> ErrorsCount { get; } =
            new PerLanguageOption<uint>(
                AnalyzerName, nameof(ErrorsCount), defaultValue: () => { return 1; });

        public static PerLanguageOption<TestRuleBehaviors> Behaviors { get; } =
            new PerLanguageOption<TestRuleBehaviors>(
                AnalyzerName, nameof(TestRuleBehaviors), defaultValue: () => { return TestRuleBehaviors.None; });

        public static PerLanguageOption<bool> UnusedOption { get; } =
            new PerLanguageOption<bool>(
                AnalyzerName, nameof(UnusedOption), defaultValue: () => { return true; });
    }
}
