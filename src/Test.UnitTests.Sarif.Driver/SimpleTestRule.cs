// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Export(typeof(ReportingDescriptor)), Export(typeof(IOptionsProvider)), Export(typeof(Skimmer<TestAnalysisContext>))]
    internal class SimpleTestRule : TestRuleBase, IOptionsProvider
    {
        public override string Id => "TEST1001";

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> MessageResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TEST1001_Failed),
            nameof(SkimmerBaseTestResources.TEST1001_Pass),
            nameof(SkimmerBaseTestResources.TEST1001_Note),
            nameof(SkimmerBaseTestResources.TEST1001_Open),
            nameof(SkimmerBaseTestResources.TEST1001_Review),
            nameof(SkimmerBaseTestResources.TEST1001_Information)
        };

        public override AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
        {
            AnalysisApplicability applicability = AnalysisApplicability.ApplicableToSpecifiedTarget;
            reasonIfNotApplicable = null;

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
            if (context.Policy.GetProperty(Behaviors) == TestRuleBehaviors.LogError)
            {
                context.Logger.Log(this,
                    new Result()
                    {
                        RuleId = this.Id,
                        Level = FailureLevel.Error,
                        Message = new Message { Text = "Simple test rule message." }
                    });
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
            return new IOption[] { Behaviors };
        }

        private const string AnalyzerName = "TEST001." + nameof(SimpleTestRule);

        public static PerLanguageOption<TestRuleBehaviors> Behaviors { get; } =
            new PerLanguageOption<TestRuleBehaviors>(
                AnalyzerName, nameof(TestRuleBehaviors), defaultValue: () => { return TestRuleBehaviors.None; });

    }
}
