// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Resources;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk;

namespace Microsoft.CodeAnalysis.Sarif
{
    // Test rule for provoking various behaviors designed to increase code coverage. This rule can be configured
    // via explicitly passed configuration, by injecting test behaviors into a thread static variable, or
    // implicitly via the name of the scan targets.
    [Export(typeof(ReportingDescriptor)), Export(typeof(IOptionsProvider)), Export(typeof(Skimmer<TestAnalysisContext>))]
    internal class TestRule : TestRuleBase, IOptionsProvider
    {
        [ThreadStatic]
        internal static TestRuleBehaviors s_testRuleBehaviors;

        public TestRule()
        {
            if (s_testRuleBehaviors.HasFlag(TestRuleBehaviors.RaiseExceptionInvokingConstructor))
            {
                throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingConstructor));
            }
        }

        private const string TestRuleId = "TEST1001";

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> MessageResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TEST1001_Failed),
            nameof(SkimmerBaseTestResources.TEST1001_Pass),
            nameof(SkimmerBaseTestResources.TEST1001_Note),
            nameof(SkimmerBaseTestResources.TEST1001_Open),
            nameof(SkimmerBaseTestResources.TEST1001_Review),
            nameof(SkimmerBaseTestResources.TEST1001_Information),
            nameof(SkimmerBaseTestResources.NotApplicable_InvalidMetadata)
        };

        public override SupportedPlatform SupportedPlatforms
        {
            get
            {
                if (s_testRuleBehaviors == TestRuleBehaviors.TreatPlatformAsInvalid)
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
                if (s_testRuleBehaviors == TestRuleBehaviors.RaiseExceptionAccessingId)
                {
                    throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionAccessingId));
                }
                return _id ?? TestRuleId;
            }
            set
            {
                _id = value;
            }
        }

        private string _name;
        public override string Name
        {
            get { return _name ?? base.Name; }
            set { _name = value; }
        }

        public override void Initialize(TestAnalysisContext context)
        {
            if (s_testRuleBehaviors == TestRuleBehaviors.RaiseExceptionInvokingInitialize)
            {
                throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingInitialize));
            }
        }

        public override AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
        {
            AnalysisApplicability applicability = AnalysisApplicability.ApplicableToSpecifiedTarget;
            reasonIfNotApplicable = null;

            if (s_testRuleBehaviors == TestRuleBehaviors.RaiseExceptionInvokingCanAnalyze)
            {
                throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingCanAnalyze));
            }

            if (context.Options.RegardAnalysisTargetAsNotApplicable)
            {
                reasonIfNotApplicable = "testing NotApplicableToSpecifiedTarget";
                return AnalysisApplicability.NotApplicableToSpecifiedTarget;
            }

            string fileName = Path.GetFileName(context.TargetUri.LocalPath);

            if (fileName.Contains("NotApplicable"))
            {
                reasonIfNotApplicable = "test was configured to find target not applicable.";
                applicability = AnalysisApplicability.NotApplicableToSpecifiedTarget;
            }

            return applicability;
        }

        public override MultiformatMessageString FullDescription { get { return new MultiformatMessageString { Text = "This is the full description for TST1001" }; } }

        public override void Analyze(TestAnalysisContext context)
        {
            TestRuleBehaviors testRuleBehaviors = context.Policy.GetProperty(Behaviors);

            // Currently, we only allow test rule behavior either be passed by context
            // or injected via static data, not by both mechanisms.
            (s_testRuleBehaviors == 0 || testRuleBehaviors == 0).Should().BeTrue();

            if (testRuleBehaviors == TestRuleBehaviors.None)
            {
                testRuleBehaviors = s_testRuleBehaviors;
            };

            switch (testRuleBehaviors)
            {
                case TestRuleBehaviors.RaiseExceptionInvokingAnalyze:
                {
                    throw new InvalidOperationException(nameof(TestRuleBehaviors.RaiseExceptionInvokingAnalyze));
                }

                case TestRuleBehaviors.RaiseTargetParseError:
                {
                    Errors.LogTargetParseError(
                        context,
                        new Region
                        {
                            StartLine = 42,
                            StartColumn = 54
                        },
                        "Could not parse target.");
                    break;
                }

                case TestRuleBehaviors.LogError:
                {
                    context.Logger.Log(this,
                        new Result
                        {
                            RuleId = this.Id,
                            Level = FailureLevel.Error,
                            Message = new Message { Text = "Simple test rule message." }
                        });
                    break;
                }

                default:
                {
                    break;
                }
            }

            string fileName = Path.GetFileName(context.TargetUri.LocalPath);

            if (fileName.Contains(nameof(FailureLevel.Error)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(FailureLevel.Error, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Failed),
                    context.TargetUri.GetFileName()));
            }
            if (fileName.Contains(nameof(FailureLevel.Warning)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(FailureLevel.Warning, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Failed),
                    context.TargetUri.GetFileName()));
            }
            if (fileName.Contains(nameof(FailureLevel.Note)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(FailureLevel.Note, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Note),
                    context.TargetUri.GetFileName()));
            }
            else if (fileName.Contains(nameof(ResultKind.Pass)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(ResultKind.Pass, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Pass),
                    context.TargetUri.GetFileName()));
            }
            else if (fileName.Contains(nameof(ResultKind.Review)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(ResultKind.Review, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Review),
                    context.TargetUri.GetFileName()));
            }
            else if (fileName.Contains(nameof(ResultKind.Open)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(ResultKind.Open, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Open),
                    context.TargetUri.GetFileName()));

            }
            else if (fileName.Contains(nameof(ResultKind.Informational)))
            {
                context.Logger.Log(this,
                    RuleUtilities.BuildResult(ResultKind.Informational, context, null,
                    nameof(SkimmerBaseTestResources.TEST1001_Information),
                    context.TargetUri.GetFileName()));
            }
        }

        public IEnumerable<IOption> GetOptions()
        {
            return new IOption[] { Behaviors, UnusedOption };
        }

        private const string AnalyzerName = TestRuleId + "." + nameof(TestRule);

        public static PerLanguageOption<TestRuleBehaviors> Behaviors { get; } =
            new PerLanguageOption<TestRuleBehaviors>(
                AnalyzerName, nameof(TestRuleBehaviors), defaultValue: () => { return TestRuleBehaviors.None; });

        public static PerLanguageOption<bool> UnusedOption { get; } =
            new PerLanguageOption<bool>(
                AnalyzerName, nameof(TestRuleBehaviors), defaultValue: () => { return true; });

    }
}
